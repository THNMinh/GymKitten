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
    private readonly IUnitOfWork _unitOfWork;
    private readonly IVnPayService _vnPayService;
    private readonly IMomoService _momoService;
    private readonly IOrderAutoCancelService _orderAutoCancelService;

    public CheckoutCommandHandler(
        IUserContext userContext,
        IProductVariantRepository productVariantRepository,
        IInventoryRepository inventoryRepository,
        IOrderRepository orderRepository,
        IPaymentTransactionRepository paymentTransactionRepository,
        IUnitOfWork unitOfWork,
        IVnPayService vnPayService,
        IMomoService momoService,
        IOrderAutoCancelService orderAutoCancelService)
    {
        _userContext = userContext;
        _productVariantRepository = productVariantRepository;
        _inventoryRepository = inventoryRepository;
        _orderRepository = orderRepository;
        _paymentTransactionRepository = paymentTransactionRepository;
        _unitOfWork = unitOfWork;
        _vnPayService = vnPayService;
        _momoService = momoService;
        _orderAutoCancelService = orderAutoCancelService;
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

        // 1. Validate stock availability
        foreach (var item in request.Items)
        {
            var inventory = inventoryItems.FirstOrDefault(i => i.Variantid == item.VariantId);
            var availableStock = inventory != null ? (inventory.Quantityonhand - inventory.Quantityreserved) : 0;

            if (availableStock < item.Quantity)
            {
                return Result.Failure<CheckoutCommandResponse>(OrderErrors.InsufficientStock);
            }
        }

        // 2. Reserve stock
        foreach (var item in request.Items)
        {
            var inventory = inventoryItems.FirstOrDefault(i => i.Variantid == item.VariantId);
            if (inventory != null)
            {
                inventory.Quantityreserved += item.Quantity;
                inventory.Updatedat = DateTime.UtcNow;
                _inventoryRepository.Update(inventory);
            }
        }

        // 3. Create Order & Items
        decimal totalAmount = 0;
        var orderItems = new List<Orderitem>();
        var orderId = Guid.NewGuid();
        var orderCode = $"GK-{DateTime.UtcNow:yyMMddHHmmss}-{Random.Shared.Next(100, 999)}";

        foreach (var item in request.Items)
        {
            var variant = await _productVariantRepository.GetByIdAsync(item.VariantId, cancellationToken);
            if (variant is null)
            {
                return Result.Failure<CheckoutCommandResponse>(ProductVariantErrors.NotFound);
            }

            var itemTotal = variant.Price * item.Quantity;
            totalAmount += itemTotal;

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

        var paymentMethodUpper = request.PaymentMethod.Trim().ToUpper();

        var order = new Domain.Entities.Order
        {
            Orderid = orderId,
            Ordercode = orderCode,
            Userid = _userContext.UserId,
            Shippingaddress = request.ShippingAddress,
            Subtotal = totalAmount,
            Shippingfee = 0,
            Discountamount = 0,
            Totalamount = totalAmount,
            Currentstatus = "Pending",
            Paymentmethod = paymentMethodUpper,
            Paymentstatus = "Unpaid",
            Customernote = request.CustomerNote,
            Createdat = DateTime.UtcNow,
            Updatedat = DateTime.UtcNow,
            Orderitems = orderItems
        };

        await _orderRepository.AddAsync(order, cancellationToken);

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

        return Result.Success(new CheckoutCommandResponse(
            order.Orderid,
            order.Ordercode,
            order.Totalamount,
            order.Currentstatus,
            order.Paymentstatus,
            paymentUrl));
    }
}
