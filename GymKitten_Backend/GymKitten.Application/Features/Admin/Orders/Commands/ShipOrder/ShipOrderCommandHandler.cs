using GymKitten.Application.Abstractions.Data;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Common;
using GymKitten.Domain.Entities;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Admin.Orders.Commands.ShipOrder;

public sealed class ShipOrderCommandHandler
    : ICommandHandler<ShipOrderCommand, Result<ShipOrderCommandResponse>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IInventoryTransactionRepository _inventoryTransactionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ShipOrderCommandHandler(
        IOrderRepository orderRepository,
        IInventoryRepository inventoryRepository,
        IInventoryTransactionRepository inventoryTransactionRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _inventoryRepository = inventoryRepository;
        _inventoryTransactionRepository = inventoryTransactionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ShipOrderCommandResponse>> Handle(
        ShipOrderCommand request,
        CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdWithItemsAsync(request.OrderId, cancellationToken);
        if (order is null)
        {
            return Result.Failure<ShipOrderCommandResponse>(OrderErrors.NotFound);
        }

        if (order.Currentstatus.Equals("Cancelled", StringComparison.OrdinalIgnoreCase) ||
            order.Currentstatus.Equals("Shipped", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<ShipOrderCommandResponse>(OrderErrors.InvalidStatus);
        }

        order.Currentstatus = "Shipped";
        order.Updatedat = DateTime.UtcNow;
        _orderRepository.Update(order);

        // Deduct physical inventory stock & release reserved stock
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

                var transaction = new Inventorytransaction
                {
                    Transactionid = Guid.NewGuid(),
                    Variantid = item.Variantid,
                    Quantitychange = -item.Quantity,
                    Type = "OrderShipped",
                    Referenceid = order.Orderid.ToString(),
                    Createdat = DateTime.UtcNow,
                    Updatedat = DateTime.UtcNow
                };

                await _inventoryTransactionRepository.AddAsync(transaction, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new ShipOrderCommandResponse(order.Orderid, order.Currentstatus));
    }
}
