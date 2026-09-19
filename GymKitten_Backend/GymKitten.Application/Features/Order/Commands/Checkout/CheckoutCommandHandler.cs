using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Data;
using GymKitten.Application.Abstractions.Jobs;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Application.Abstractions.Services;
using GymKitten.Domain.Common;
using GymKitten.Domain.Entities;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Order.Commands.Checkout;

public sealed class CheckoutCommandHandler
    : ICommandHandler<CheckoutCommand, Result<CheckoutCommandResponse>>
{
    private readonly IUserContext _userContext;
    private readonly IProductVariantRepository _productVariantRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentTransactionRepository _paymentTransactionRepository;
    private readonly ICouponRepository _couponRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IVnPayService _vnPayService;
    private readonly IMomoService _momoService;
    private readonly IOrderAutoCancelService _orderAutoCancelService;
    private readonly IInventoryTransactionRepository _inventoryTransactionRepository;
    private readonly INotificationHubService _notificationHubService;

    public CheckoutCommandHandler(
        IUserContext userContext,
        IProductVariantRepository productVariantRepository,
        IInventoryRepository inventoryRepository,
        IOrderRepository orderRepository,
        IPaymentTransactionRepository paymentTransactionRepository,
        ICouponRepository couponRepository,
        IUnitOfWork unitOfWork,
        IVnPayService vnPayService,
        IMomoService momoService,
        IOrderAutoCancelService orderAutoCancelService,
        IInventoryTransactionRepository inventoryTransactionRepository,
        INotificationHubService notificationHubService)
    {
        _userContext = userContext;
        _productVariantRepository = productVariantRepository;
        _inventoryRepository = inventoryRepository;
        _orderRepository = orderRepository;
        _paymentTransactionRepository = paymentTransactionRepository;
        _couponRepository = couponRepository;
        _unitOfWork = unitOfWork;
        _vnPayService = vnPayService;
        _momoService = momoService;
        _orderAutoCancelService = orderAutoCancelService;
        _inventoryTransactionRepository = inventoryTransactionRepository;
        _notificationHubService = notificationHubService;
    }

    public async Task<Result<CheckoutCommandResponse>> Handle(
        CheckoutCommand request,
        CancellationToken cancellationToken)
    {
        if (request.Items is null || request.Items.Count == 0)
        {
            return Result.Failure<CheckoutCommandResponse>(OrderErrors.EmptyCart);
        }

        var variantIds = request.Items.Select(i => i.VariantId).Distinct().ToList();
        var inventoryItems = await _inventoryRepository.GetByVariantIdsAsync(variantIds, cancellationToken);

        // 1. Validate product active status & stock availability
        foreach (var item in request.Items)
        {
            var variant = await _productVariantRepository.GetByIdAsync(item.VariantId, cancellationToken);
            if (variant is null)
            {
                return Result.Failure<CheckoutCommandResponse>(ProductVariantErrors.NotFound);
            }

            if (variant.Product is null || !variant.Product.Isactive || variant.Product.Deletedat != null)
            {
                return Result.Failure<CheckoutCommandResponse>(ProductErrors.InactiveOrUnavailable);
            }

            var inventory = inventoryItems.FirstOrDefault(i => i.Variantid == item.VariantId);
            var availableStock = inventory != null ? (inventory.Quantityonhand - inventory.Quantityreserved) : 0;

            if (availableStock < item.Quantity)
            {
                return Result.Failure<CheckoutCommandResponse>(OrderErrors.InsufficientStock);
            }
        }

        var orderId = Guid.NewGuid();
        var orderCode = $"GK-{DateTime.UtcNow:yyMMddHHmmss}-{Random.Shared.Next(100, 999)}";
        var customer = _userContext.Email ?? "Guest";

        // 2. Reserve stock & record inventory transactions
        foreach (var item in request.Items)
        {
            var inventory = inventoryItems.FirstOrDefault(i => i.Variantid == item.VariantId);
            if (inventory != null)
            {
                inventory.Quantityreserved += item.Quantity;
                inventory.Updatedat = DateTime.UtcNow;
                _inventoryRepository.Update(inventory);

                var refText = $"Order #{orderCode} | Customer: {customer}";
                if (refText.Length > 100) refText = refText[..100];

                var reserveTxn = new Inventorytransaction
                {
                    Transactionid = Guid.NewGuid(),
                    Variantid = item.VariantId,
                    Quantitychange = item.Quantity,
                    Type = "Reserve",
                    Referenceid = refText,
                    Createdat = DateTime.UtcNow,
                    Updatedat = DateTime.UtcNow
                };
                await _inventoryTransactionRepository.AddAsync(reserveTxn, cancellationToken);
            }
        }

        // 3. Create Order & Items
        decimal subtotal = 0;
        var orderItems = new List<Orderitem>();

        foreach (var item in request.Items)
        {
            var variant = await _productVariantRepository.GetByIdAsync(item.VariantId, cancellationToken);
            if (variant is null)
            {
                return Result.Failure<CheckoutCommandResponse>(ProductVariantErrors.NotFound);
            }

            var itemTotal = variant.Price * item.Quantity;
            subtotal += itemTotal;

            orderItems.Add(new Orderitem
            {
                Orderitemid = Guid.NewGuid(),
                Orderid = orderId,
                Variantid = variant.Variantid,
                Sku = variant.Sku,
                Productname = $"{variant.Colorname} - {variant.Size}",
                Unitprice = variant.Price,
                Quantity = item.Quantity,
                Totalprice = itemTotal,
                Createdat = DateTime.UtcNow,
                Updatedat = DateTime.UtcNow
            });
        }

        // 4. Validate and apply coupon discount if provided
        decimal discountAmount = 0;
        Coupon? appliedCoupon = null;

        if (!string.IsNullOrWhiteSpace(request.CouponCode))
        {
            var now = DateTime.UtcNow;
            appliedCoupon = await _couponRepository.GetByCodeAsync(request.CouponCode, cancellationToken);

            if (appliedCoupon is null || appliedCoupon.Deletedat != null)
            {
                return Result.Failure<CheckoutCommandResponse>(CouponErrors.NotFound);
            }

            if (!appliedCoupon.Isactive)
            {
                return Result.Failure<CheckoutCommandResponse>(CouponErrors.Inactive);
            }

            if (now < appliedCoupon.Startdate || now > appliedCoupon.Enddate)
            {
                return Result.Failure<CheckoutCommandResponse>(CouponErrors.Expired);
            }

            if (appliedCoupon.Usagelimit.HasValue && appliedCoupon.Usedcount >= appliedCoupon.Usagelimit.Value)
            {
                return Result.Failure<CheckoutCommandResponse>(CouponErrors.UsageLimitReached);
            }

            if (subtotal < appliedCoupon.Minordervalue)
            {
                return Result.Failure<CheckoutCommandResponse>(CouponErrors.MinOrderValueNotMet);
            }

            if (appliedCoupon.Discounttype.Equals("Percentage", StringComparison.OrdinalIgnoreCase))
            {
                var calc = subtotal * (appliedCoupon.Discountvalue / 100m);
                if (appliedCoupon.Maxdiscountamount.HasValue)
                {
                    calc = Math.Min(calc, appliedCoupon.Maxdiscountamount.Value);
                }
                discountAmount = Math.Round(calc, 2);
            }
            else if (appliedCoupon.Discounttype.Equals("FixedAmount", StringComparison.OrdinalIgnoreCase))
            {
                discountAmount = Math.Min(subtotal, appliedCoupon.Discountvalue);
            }
        }

        decimal finalTotalAmount = Math.Max(0, subtotal - discountAmount);
        var paymentMethodUpper = request.PaymentMethod.Trim().ToUpper();

        var order = new Domain.Entities.Order
        {
            Orderid = orderId,
            Ordercode = orderCode,
            Userid = _userContext.UserId,
            Shippingaddress = request.ShippingAddress,
            Subtotal = subtotal,
            Shippingfee = 0,
            Discountamount = discountAmount,
            Totalamount = finalTotalAmount,
            Currentstatus = "Pending",
            Paymentmethod = paymentMethodUpper,
            Paymentstatus = "Unpaid",
            Customernote = request.CustomerNote,
            Createdat = DateTime.UtcNow,
            Updatedat = DateTime.UtcNow,
            Orderitems = orderItems
        };

        await _orderRepository.AddAsync(order, cancellationToken);

        // Record coupon usage & update coupon count if applied
        if (appliedCoupon != null)
        {
            appliedCoupon.Usedcount += 1;
            appliedCoupon.Updatedat = DateTime.UtcNow;
            _couponRepository.Update(appliedCoupon);

            var couponUsage = new Couponusage
            {
                Usageid = Guid.NewGuid(),
                Couponid = appliedCoupon.Couponid,
                Userid = _userContext.UserId ?? Guid.Empty,
                Orderid = orderId,
                Usedat = DateTime.UtcNow,
                Createdat = DateTime.UtcNow,
                Updatedat = DateTime.UtcNow
            };
            await _couponRepository.AddUsageAsync(couponUsage, cancellationToken);
        }

        string? paymentUrl = null;

        if (paymentMethodUpper == "VNPAY")
        {
            var transactionId = Guid.NewGuid();
            var paymentResult = _vnPayService.CreatePaymentUrl(order.Totalamount, $"Payment for order {order.Ordercode}");
            paymentUrl = paymentResult.Url;

            var transaction = new Paymenttransaction
            {
                Transactionid = transactionId,
                Orderid = order.Orderid,
                Gateway = "VNPay",
                Gatewaytransactionid = paymentResult.TxnRef,
                Amount = order.Totalamount,
                Status = "Pending",
                Createdat = DateTime.UtcNow,
                Updatedat = DateTime.UtcNow
            };

            await _paymentTransactionRepository.AddAsync(transaction, cancellationToken);

            // Schedule auto-cancel after 15 minutes if unpaid
            _orderAutoCancelService.ScheduleAutoCancel(order.Orderid, TimeSpan.FromMinutes(15));
        }
        else if (paymentMethodUpper == "MOMO")
        {
            var momoResponse = await _momoService.CreatePaymentAsync(order.Orderid, order.Totalamount, $"Payment for Order {order.Ordercode}", cancellationToken);

            if (momoResponse is null || momoResponse.ResultCode != 0 || string.IsNullOrEmpty(momoResponse.PayUrl))
            {
                return Result.Failure<CheckoutCommandResponse>(new Error(
                    "MoMo.PaymentFailed",
                    string.IsNullOrWhiteSpace(momoResponse?.Message) ? "Failed to create MoMo payment URL." : momoResponse.Message));
            }

            paymentUrl = momoResponse.PayUrl;

            var transaction = new Paymenttransaction
            {
                Transactionid = Guid.NewGuid(),
                Orderid = order.Orderid,
                Gateway = "MoMo",
                Gatewaytransactionid = null,
                Amount = order.Totalamount,
                Status = "Pending",
                Createdat = DateTime.UtcNow,
                Updatedat = DateTime.UtcNow
            };

            await _paymentTransactionRepository.AddAsync(transaction, cancellationToken);

            // Schedule auto-cancel after 15 minutes if unpaid
            _orderAutoCancelService.ScheduleAutoCancel(order.Orderid, TimeSpan.FromMinutes(15));
        }
        else if (paymentMethodUpper == "COD")
        {
            order.Currentstatus = "Processing";
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (paymentMethodUpper == "COD")
        {
            var customerName = !string.IsNullOrWhiteSpace(_userContext.Email)
                ? _userContext.Email.Split('@')[0]
                : "Khách hàng";

            var adminNotificationPayload = new
            {
                orderId = order.Orderid,
                orderCode = order.Ordercode,
                customerName = customerName,
                customerEmail = _userContext.Email ?? "guest@gymkitten.com",
                totalAmount = order.Totalamount,
                paymentMethod = order.Paymentmethod,
                paymentStatus = order.Paymentstatus,
                itemCount = order.Orderitems.Sum(i => i.Quantity),
                createdAt = order.Createdat,
                targetUrl = "/admin/orders"
            };

            await _notificationHubService.SendNewOrderPlacedToAdminsAsync(adminNotificationPayload, cancellationToken);
        }

        return Result.Success(new CheckoutCommandResponse(
            order.Orderid,
            order.Ordercode,
            order.Totalamount,
            order.Currentstatus,
            order.Paymentstatus,
            paymentUrl));
    }
}
