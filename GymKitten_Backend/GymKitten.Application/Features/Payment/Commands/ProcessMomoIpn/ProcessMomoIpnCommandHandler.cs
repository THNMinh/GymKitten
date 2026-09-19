using GymKitten.Application.Abstractions.Data;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Application.Abstractions.Services;
using GymKitten.Domain.Common;
using GymKitten.Domain.Entities;
using GymKitten.Domain.Enums;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Payment.Commands.ProcessMomoIpn;

public sealed class ProcessMomoIpnCommandHandler
    : ICommandHandler<ProcessMomoIpnCommand, Result<ProcessMomoIpnCommandResponse>>
{
    private readonly IMomoService _momoService;
    private readonly IPaymentTransactionRepository _paymentTransactionRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IOrderTrackingRepository _orderTrackingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationHubService _notificationHubService;

    public ProcessMomoIpnCommandHandler(
        IMomoService momoService,
        IPaymentTransactionRepository paymentTransactionRepository,
        IOrderRepository orderRepository,
        IInventoryRepository inventoryRepository,
        IOrderTrackingRepository orderTrackingRepository,
        IUnitOfWork unitOfWork,
        INotificationHubService notificationHubService)
    {
        _momoService = momoService;
        _paymentTransactionRepository = paymentTransactionRepository;
        _orderRepository = orderRepository;
        _inventoryRepository = inventoryRepository;
        _orderTrackingRepository = orderTrackingRepository;
        _unitOfWork = unitOfWork;
        _notificationHubService = notificationHubService;
    }

    public async Task<Result<ProcessMomoIpnCommandResponse>> Handle(
        ProcessMomoIpnCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Security Check: Reconstruct raw hash string & verify signature via IMomoService
        var isSignatureValid = _momoService.VerifyIpnSignature(
            request.Signature,
            request.Amount,
            request.ExtraData ?? string.Empty,
            request.Message ?? string.Empty,
            request.OrderId,
            request.OrderInfo ?? string.Empty,
            request.OrderType ?? string.Empty,
            request.PartnerCode,
            request.PayType ?? string.Empty,
            request.RequestId,
            request.ResponseTime,
            request.ResultCode,
            request.TransId);

        if (!isSignatureValid)
        {
            return Result.Failure<ProcessMomoIpnCommandResponse>(PaymentErrors.InvalidSignature);
        }

        // 2. Parse OrderId & Fetch Transaction
        if (!Guid.TryParse(request.OrderId, out var orderId))
        {
            return Result.Failure<ProcessMomoIpnCommandResponse>(OrderErrors.NotFound);
        }

        var transaction = await _paymentTransactionRepository.GetByOrderIdAndGatewayAsync(orderId, "MoMo", cancellationToken);
        if (transaction is null)
        {
            return Result.Failure<ProcessMomoIpnCommandResponse>(PaymentErrors.TransactionNotFound);
        }

        // 3. Idempotency Check
        if (transaction.Status != "Pending")
        {
            return Result.Success(new ProcessMomoIpnCommandResponse(true, "Already processed"));
        }

        var order = await _orderRepository.GetByIdWithItemsAsync(orderId, cancellationToken);

        // 4. Handle Result Code
        if (request.ResultCode == 0)
        {
            // Payment Success
            transaction.Status = "Success";
            transaction.Gatewaytransactionid = request.TransId.ToString();
            transaction.Paymentdate = DateTime.UtcNow;
            transaction.Updatedat = DateTime.UtcNow;
            _paymentTransactionRepository.Update(transaction);

            if (order != null)
            {
                order.Paymentstatus = "Paid";
                order.Currentstatus = "Processing";
                order.Updatedat = DateTime.UtcNow;
                _orderRepository.Update(order);

                // Add Order Tracking Timeline History
                var tracking = new Ordertrackinghistory
                {
                    Trackingid = Guid.NewGuid(),
                    Orderid = order.Orderid,
                    Status = OrderStatusExtensions.Processing,
                    Title = "Thanh toán MoMo thành công",
                    Description = "Hệ thống đã nhận thanh toán thành công qua ví MoMo.",
                    Location = "Cổng thanh toán MoMo",
                    Timestamp = DateTime.UtcNow,
                    Updatedby = "System/MoMo",
                    Createdat = DateTime.UtcNow,
                    Updatedat = DateTime.UtcNow
                };
                await _orderTrackingRepository.AddAsync(tracking, cancellationToken);
            }
        }
        else
        {
            // Payment Failed
            transaction.Status = "Failed";
            transaction.Updatedat = DateTime.UtcNow;
            _paymentTransactionRepository.Update(transaction);

            if (order != null)
            {
                order.Currentstatus = "Cancelled";
                order.Updatedat = DateTime.UtcNow;
                _orderRepository.Update(order);

                // Release inventory lock
                var variantIds = order.Orderitems.Select(i => i.Variantid).ToList();
                var inventoryItems = await _inventoryRepository.GetByVariantIdsAsync(variantIds, cancellationToken);

                foreach (var item in order.Orderitems)
                {
                    var inventory = inventoryItems.FirstOrDefault(i => i.Variantid == item.Variantid);
                    if (inventory != null)
                    {
                        inventory.Quantityreserved = Math.Max(0, inventory.Quantityreserved - item.Quantity);
                        inventory.Updatedat = DateTime.UtcNow;
                        _inventoryRepository.Update(inventory);
                    }
                }

                // Add Order Tracking Timeline History
                var tracking = new Ordertrackinghistory
                {
                    Trackingid = Guid.NewGuid(),
                    Orderid = order.Orderid,
                    Status = OrderStatusExtensions.Cancelled,
                    Title = "Thanh toán MoMo thất bại",
                    Description = string.IsNullOrWhiteSpace(request.Message) ? "Giao dịch qua MoMo không thành công." : request.Message,
                    Location = "Cổng thanh toán MoMo",
                    Timestamp = DateTime.UtcNow,
                    Updatedby = "System/MoMo",
                    Createdat = DateTime.UtcNow,
                    Updatedat = DateTime.UtcNow
                };
                await _orderTrackingRepository.AddAsync(tracking, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (request.ResultCode == 0 && order != null)
        {
            var customerName = order.User?.Fullname;
            if (string.IsNullOrWhiteSpace(customerName))
            {
                customerName = !string.IsNullOrWhiteSpace(order.User?.Email)
                    ? order.User.Email.Split('@')[0]
                    : "Khách hàng";
            }

            var adminNotificationPayload = new
            {
                orderId = order.Orderid,
                orderCode = order.Ordercode,
                customerName = customerName,
                customerEmail = order.User?.Email ?? "customer@gymkitten.com",
                totalAmount = order.Totalamount,
                paymentMethod = order.Paymentmethod,
                paymentStatus = order.Paymentstatus,
                itemCount = order.Orderitems.Sum(i => i.Quantity),
                createdAt = order.Createdat,
                targetUrl = "/admin/orders"
            };

            await _notificationHubService.SendNewOrderPlacedToAdminsAsync(adminNotificationPayload, cancellationToken);
        }

        return Result.Success(new ProcessMomoIpnCommandResponse(true, request.ResultCode == 0 ? "Payment Success" : "Payment Failed"));
    }
}
