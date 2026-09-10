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

namespace GymKitten.Application.Features.Admin.Orders.Commands.UpdateOrderStatus;

public sealed class UpdateOrderStatusCommandHandler
    : ICommandHandler<UpdateOrderStatusCommand, Result<UpdateOrderStatusResponse>>
{
    private readonly IUserContext _userContext;
    private readonly IOrderRepository _orderRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IInventoryTransactionRepository _inventoryTransactionRepository;
    private readonly IOrderTrackingRepository _orderTrackingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublisher _publisher;
    private readonly ISystemLogService _systemLogService;

    public UpdateOrderStatusCommandHandler(
        IUserContext userContext,
        IOrderRepository orderRepository,
        IInventoryRepository inventoryRepository,
        IInventoryTransactionRepository inventoryTransactionRepository,
        IOrderTrackingRepository orderTrackingRepository,
        IUnitOfWork unitOfWork,
        IPublisher publisher,
        ISystemLogService systemLogService)
    {
        _userContext = userContext;
        _orderRepository = orderRepository;
        _inventoryRepository = inventoryRepository;
        _inventoryTransactionRepository = inventoryTransactionRepository;
        _orderTrackingRepository = orderTrackingRepository;
        _unitOfWork = unitOfWork;
        _publisher = publisher;
        _systemLogService = systemLogService;
    }

    public async Task<Result<UpdateOrderStatusResponse>> Handle(
        UpdateOrderStatusCommand request,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(_userContext.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<UpdateOrderStatusResponse>(UserErrors.Forbidden);
        }

        var order = await _orderRepository.GetByIdWithItemsAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return Result.Failure<UpdateOrderStatusResponse>(OrderErrors.NotFound);
        }

        var newStatusUpper = request.Status.Trim();
        var previousStatus = order.Currentstatus;

        // 1. Shipped Transition: Deduct physical stock Quantityonhand & Quantityreserved
        if (newStatusUpper.Equals(OrderStatusExtensions.Shipped, StringComparison.OrdinalIgnoreCase) &&
            !previousStatus.Equals(OrderStatusExtensions.Shipped, StringComparison.OrdinalIgnoreCase))
        {
            var variantIds = order.Orderitems.Select(i => i.Variantid).ToList();
            var inventoryItems = await _inventoryRepository.GetByVariantIdsAsync(variantIds, cancellationToken);

            foreach (var item in order.Orderitems)
            {
                var inventory = inventoryItems.FirstOrDefault(i => i.Variantid == item.Variantid);
                if (inventory != null)
                {
                    inventory.Quantityonhand = Math.Max(0, inventory.Quantityonhand - item.Quantity);
                    inventory.Quantityreserved = Math.Max(0, inventory.Quantityreserved - item.Quantity);
                    inventory.Updatedat = DateTime.UtcNow;
                    _inventoryRepository.Update(inventory);

                    var performer = _userContext.Email ?? "Admin";
                    var refText = $"Order #{order.Ordercode} | Admin: {performer}";
                    if (refText.Length > 100) refText = refText[..100];

                    var auditTxn = new Inventorytransaction
                    {
                        Transactionid = Guid.NewGuid(),
                        Variantid = item.Variantid,
                        Quantitychange = -item.Quantity,
                        Type = "Export",
                        Referenceid = refText,
                        Createdat = DateTime.UtcNow,
                        Updatedat = DateTime.UtcNow
                    };
                    await _inventoryTransactionRepository.AddAsync(auditTxn, cancellationToken);
                }
            }
        }
        // 2. Delivered Transition: Mark COD as Paid
        else if (newStatusUpper.Equals(OrderStatusExtensions.Delivered, StringComparison.OrdinalIgnoreCase))
        {
            if (order.Paymentmethod.Equals("COD", StringComparison.OrdinalIgnoreCase))
            {
                order.Paymentstatus = "Paid";
            }
        }
        // 3. Cancelled Transition: Release reserved stock
        else if (newStatusUpper.Equals(OrderStatusExtensions.Cancelled, StringComparison.OrdinalIgnoreCase) &&
                 !previousStatus.Equals(OrderStatusExtensions.Cancelled, StringComparison.OrdinalIgnoreCase))
        {
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

                    var performer = _userContext.Email ?? "Admin";
                    var refText = $"Cancel #{order.Ordercode} | Admin: {performer}";
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
        }

        order.Currentstatus = newStatusUpper;
        order.Updatedat = DateTime.UtcNow;
        _orderRepository.Update(order);

        // Add Tracking Timeline History Record
        var tracking = new Ordertrackinghistory
        {
            Trackingid = Guid.NewGuid(),
            Orderid = order.Orderid,
            Status = newStatusUpper,
            Title = !string.IsNullOrWhiteSpace(request.Title) ? request.Title : $"Order status updated to {newStatusUpper}",
            Description = request.Description,
            Location = request.Location ?? "Warehouse / Distribution Hub",
            Timestamp = DateTime.UtcNow,
            Updatedby = "Admin",
            Createdat = DateTime.UtcNow,
            Updatedat = DateTime.UtcNow
        };
        await _orderTrackingRepository.AddAsync(tracking, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (order.Userid.HasValue)
        {
            await _publisher.Publish(new OrderStatusChangedDomainEvent(
                order.Orderid,
                order.Userid.Value,
                order.Ordercode ?? order.Orderid.ToString()[..8],
                order.Currentstatus,
                $"Đơn hàng #{order.Ordercode} của bạn đã đổi trạng thái thành: {order.Currentstatus}"
            ), cancellationToken);
        }

        await _systemLogService.LogAsync(
            "UpdateOrderStatus",
            $"Updated order #{order.Ordercode} status from '{previousStatus}' to '{newStatusUpper}'. Admin: {_userContext.Email ?? "Admin"}",
            "Information",
            _userContext.UserId,
            cancellationToken);

        return Result.Success(new UpdateOrderStatusResponse(
            order.Orderid,
            order.Currentstatus,
            $"Order status updated successfully to {order.Currentstatus}."));
    }
}
