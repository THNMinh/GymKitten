using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Data;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Application.Abstractions.Services;
using GymKitten.Domain.Common;
using GymKitten.Domain.Entities;
using GymKitten.Domain.Enums;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Order.Commands.CancelMyOrder;

public sealed class CancelMyOrderCommandHandler
    : ICommandHandler<CancelMyOrderCommand, Result<CancelMyOrderResponse>>
{
    private readonly IUserContext _userContext;
    private readonly IOrderRepository _orderRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IOrderTrackingRepository _orderTrackingRepository;
    private readonly IInventoryTransactionRepository _inventoryTransactionRepository;
    private readonly ISystemLogService _systemLogService;
    private readonly IUnitOfWork _unitOfWork;

    public CancelMyOrderCommandHandler(
        IUserContext userContext,
        IOrderRepository orderRepository,
        IInventoryRepository inventoryRepository,
        IOrderTrackingRepository orderTrackingRepository,
        IInventoryTransactionRepository inventoryTransactionRepository,
        ISystemLogService systemLogService,
        IUnitOfWork unitOfWork)
    {
        _userContext = userContext;
        _orderRepository = orderRepository;
        _inventoryRepository = inventoryRepository;
        _orderTrackingRepository = orderTrackingRepository;
        _inventoryTransactionRepository = inventoryTransactionRepository;
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

        if (!order.Currentstatus.Equals(OrderStatusExtensions.Pending, StringComparison.OrdinalIgnoreCase))
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

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _systemLogService.LogAsync(
            "CancelOrder",
            $"Customer cancelled order #{order.Ordercode}. Released reserved inventory.",
            "Information",
            _userContext.UserId,
            cancellationToken);

        return Result.Success(new CancelMyOrderResponse(
            order.Orderid,
            order.Currentstatus,
            "Order cancelled successfully."));
    }
}
