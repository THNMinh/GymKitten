using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Data;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Application.Abstractions.Services;
using GymKitten.Domain.Common;
using GymKitten.Domain.Entities;
using GymKitten.Domain.Enums;
using GymKitten.Domain.Errors;
using GymKitten.Domain.Events;
using MediatR;

namespace GymKitten.Application.Features.Order.Commands.CancelMyOrder;

public sealed class CancelMyOrderCommandHandler
    : ICommandHandler<CancelMyOrderCommand, Result<CancelMyOrderResponse>>
{
    private readonly IUserContext _userContext;
    private readonly IOrderRepository _orderRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IOrderTrackingRepository _orderTrackingRepository;
    private readonly IInventoryTransactionRepository _inventoryTransactionRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationHubService _notificationHubService;
    private readonly IPublisher _publisher;
    private readonly ISystemLogService _systemLogService;
    private readonly IUnitOfWork _unitOfWork;

    public CancelMyOrderCommandHandler(
        IUserContext userContext,
        IOrderRepository orderRepository,
        IInventoryRepository inventoryRepository,
        IOrderTrackingRepository orderTrackingRepository,
        IInventoryTransactionRepository inventoryTransactionRepository,
        INotificationRepository notificationRepository,
        INotificationHubService notificationHubService,
        IPublisher publisher,
        ISystemLogService systemLogService,
        IUnitOfWork unitOfWork)
    {
        _userContext = userContext;
        _orderRepository = orderRepository;
        _inventoryRepository = inventoryRepository;
        _orderTrackingRepository = orderTrackingRepository;
        _inventoryTransactionRepository = inventoryTransactionRepository;
        _notificationRepository = notificationRepository;
        _notificationHubService = notificationHubService;
        _publisher = publisher;
        _systemLogService = systemLogService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CancelMyOrderResponse>> Handle(
        CancelMyOrderCommand request,
        CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdWithItemsAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return Result.Failure<CancelMyOrderResponse>(OrderErrors.NotFound);
        }

        if (order.Userid.HasValue && order.Userid != _userContext.UserId)
        {
            return Result.Failure<CancelMyOrderResponse>(OrderErrors.AccessDenied);
        }

        var isPending = order.Currentstatus.Equals(OrderStatusExtensions.Pending, StringComparison.OrdinalIgnoreCase);
        var isProcessing = order.Currentstatus.Equals(OrderStatusExtensions.Processing, StringComparison.OrdinalIgnoreCase);

        if (!isPending && !isProcessing)
        {
            return Result.Failure<CancelMyOrderResponse>(OrderErrors.CannotCancelNonPendingOrder);
        }

        order.Currentstatus = OrderStatusExtensions.Cancelled;
        order.Updatedat = DateTime.UtcNow;
        _orderRepository.Update(order);

        // Release inventory reservation
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

                var customer = _userContext.Email ?? "Customer";
                var refText = $"Cancel #{order.Ordercode} | Customer: {customer}";
                if (refText.Length > 100) refText = refText[..100];

                var cancelTxn = new Inventorytransaction
                {
                    Transactionid = Guid.NewGuid(),
                    Variantid = item.Variantid,
                    Quantitychange = -item.Quantity,
                    Type = "Reserve",
                    Referenceid = refText,
                    Createdat = DateTime.UtcNow,
                    Updatedat = DateTime.UtcNow
                };
                await _inventoryTransactionRepository.AddAsync(cancelTxn, cancellationToken);
            }
        }

        // Add Tracking History Timeline entry
        var tracking = new Ordertrackinghistory
        {
            Trackingid = Guid.NewGuid(),
            Orderid = order.Orderid,
            Status = OrderStatusExtensions.Cancelled,
            Title = "Order Cancelled by Customer",
            Description = "Customer requested order cancellation.",
            Location = "Customer Portal",
            Timestamp = DateTime.UtcNow,
            Updatedby = "Customer",
            Createdat = DateTime.UtcNow,
            Updatedat = DateTime.UtcNow
        };
        await _orderTrackingRepository.AddAsync(tracking, cancellationToken);

        // 1. Create and save customer notification if user is authenticated
        var customerIdentifier = _userContext.Email ?? "Khách hàng";
        if (order.Userid.HasValue)
        {
            var customerNotification = new Notification
            {
                Notificationid = Guid.NewGuid(),
                Userid = order.Userid.Value,
                Title = $"Hủy đơn hàng #{order.Ordercode} thành công",
                Content = $"Đơn hàng #{order.Ordercode} của bạn đã được hủy thành công. Tồn kho đã được giải phóng.",
                Type = "Order",
                Isread = false,
                Targeturl = "/account",
                Createdat = DateTime.UtcNow,
                Updatedat = DateTime.UtcNow
            };
            await _notificationRepository.AddAsync(customerNotification, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 2. Dispatch real-time SignalR notification to all Admins
        var adminPayload = new
        {
            NotificationId = Guid.NewGuid(),
            Title = $"Khách hàng hủy đơn #{order.Ordercode}",
            Content = $"Khách hàng ({customerIdentifier}) đã hủy đơn #{order.Ordercode}. Tồn kho đã được hoàn lại.",
            Type = "Order",
            TargetUrl = "/admin/orders",
            CreatedAt = DateTime.UtcNow,
            IsRead = false,
            OrderId = order.Orderid,
            OrderCode = order.Ordercode,
            NewStatus = OrderStatusExtensions.Cancelled
        };
        await _notificationHubService.SendNotificationToAdminsAsync(adminPayload, cancellationToken);

        // 3. Publish domain event to notify customer & log system action
        if (order.Userid.HasValue)
        {
            await _publisher.Publish(new OrderStatusChangedDomainEvent(
                order.Orderid,
                order.Userid.Value,
                order.Ordercode ?? order.Orderid.ToString()[..8],
                OrderStatusExtensions.Cancelled,
                $"Đơn hàng #{order.Ordercode} của bạn đã được hủy thành công."
            ), cancellationToken);
        }

        await _systemLogService.LogAsync(
            "CancelOrder",
            $"Customer cancelled order #{order.Ordercode}. Released reserved inventory and notified admin.",
            "Information",
            _userContext.UserId,
            cancellationToken);

        return Result.Success(new CancelMyOrderResponse(
            order.Orderid,
            order.Currentstatus,
            "Order cancelled successfully."));
    }
}
