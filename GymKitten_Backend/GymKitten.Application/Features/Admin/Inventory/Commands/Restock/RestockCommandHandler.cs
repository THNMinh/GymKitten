using GymKitten.Application.Abstractions.Data;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Common;
using GymKitten.Domain.Entities;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Admin.Inventory.Commands.Restock;

public sealed class RestockCommandHandler
    : ICommandHandler<RestockCommand, Result<RestockCommandResponse>>
{
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IProductVariantRepository _productVariantRepository;
    private readonly IInventoryTransactionRepository _inventoryTransactionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RestockCommandHandler(
        IInventoryRepository inventoryRepository,
        IProductVariantRepository productVariantRepository,
        IInventoryTransactionRepository inventoryTransactionRepository,
        IUnitOfWork unitOfWork)
    {
        _inventoryRepository = inventoryRepository;
        _productVariantRepository = productVariantRepository;
        _inventoryTransactionRepository = inventoryTransactionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<RestockCommandResponse>> Handle(
        RestockCommand request,
        CancellationToken cancellationToken)
    {
        var inventory = await _inventoryRepository.GetByVariantIdAsync(request.VariantId, cancellationToken);

        if (inventory is null)
        {
            // Verify variant exists
            var variant = await _productVariantRepository.GetByIdAsync(request.VariantId, cancellationToken);
            if (variant is null)
            {
                return Result.Failure<RestockCommandResponse>(ProductVariantErrors.NotFound);
            }

            // Upsert: Create new Inventoryitem for existing variant without inventory
            inventory = new Inventoryitem
            {
                Inventoryid = Guid.NewGuid(),
                Variantid = request.VariantId,
                Quantityonhand = request.QuantityAdded,
                Quantityreserved = 0,
                Safetystock = 5,
                Rowversion = 1,
                Createdat = DateTime.UtcNow,
                Updatedat = DateTime.UtcNow
            };

            await _inventoryRepository.AddAsync(inventory, cancellationToken);
        }
        else
        {
            // Add quantity to existing physical stock
            inventory.Quantityonhand += request.QuantityAdded;
            inventory.Updatedat = DateTime.UtcNow;
            _inventoryRepository.Update(inventory);
        }

        // Audit transaction
        var transaction = new Inventorytransaction
        {
            Transactionid = Guid.NewGuid(),
            Variantid = request.VariantId,
            Quantitychange = request.QuantityAdded,
            Type = "Restock",
            Referenceid = request.Note,
            Createdat = DateTime.UtcNow,
            Updatedat = DateTime.UtcNow
        };

        await _inventoryTransactionRepository.AddAsync(transaction, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new RestockCommandResponse(
            inventory.Variantid,
            inventory.Quantityonhand,
            inventory.Quantityreserved));
    }
}
