using GymKitten.Application.Abstractions.Data;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Application.Abstractions.Services;
using GymKitten.Domain.Common;
using GymKitten.Domain.Entities;
using GymKitten.Domain.Enums;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Payment.Commands.ProcessMomoReturn;

public sealed class ProcessMomoReturnCommandHandler
    : ICommandHandler<ProcessMomoReturnCommand, Result>
{
    private readonly IPaymentTransactionRepository _paymentTransactionRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IOrderTrackingRepository _orderTrackingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationHubService _notificationHubService;

    public ProcessMomoReturnCommandHandler(
        IPaymentTransactionRepository paymentTransactionRepository,
        IOrderRepository orderRepository,
        IInventoryRepository inventoryRepository,
        IOrderTrackingRepository orderTrackingRepository,
        IUnitOfWork unitOfWork,
        INotificationHubService notificationHubService)
    {
        _paymentTransactionRepository = paymentTransactionRepository;
        _orderRepository = orderRepository;
        _inventoryRepository = inventoryRepository;
        _orderTrackingRepository = orderTrackingRepository;
        _unitOfWork = unitOfWork;
        _notificationHubService = notificationHubService;
    }

    public async Task<Result> Handle(
        ProcessMomoReturnCommand request,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(request.OrderId, out var orderId))
        {
            return Result.Failure(OrderErrors.NotFound);
        }

        var transaction = await _paymentTransactionRepository.GetByOrderIdAndGatewayAsync(orderId, "MoMo", cancellationToken);
        if (transaction is null)
        {
            return Result.Failure(PaymentErrors.TransactionNotFound);
        }

        // Idempotency: If already completed or failed, do nothing
        if (transaction.Status != "Pending")
        {
            return Result.Success();
        }

        var order = await _orderRepository.GetByIdWithItemsAsync(orderId, cancellationToken);

        if (request.ResultCode == 0)
        {
            // Payment Success
            transaction.Status = "Success";
            if (request.TransId.HasValue)
            {
                transaction.Gatewaytransactionid = request.TransId.Value.ToString();
            }
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
            // Payment Failed / Cancelled
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

        return Result.Success();
    }
}
