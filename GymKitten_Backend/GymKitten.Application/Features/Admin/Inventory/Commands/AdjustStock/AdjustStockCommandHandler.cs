using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Data;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Application.Abstractions.Services;
using GymKitten.Domain.Common;
using GymKitten.Domain.Entities;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Admin.Inventory.Commands.AdjustStock;

public sealed class AdjustStockCommandHandler
    : ICommandHandler<AdjustStockCommand, Result<AdjustStockCommandResponse>>
{
    private readonly IUserContext _userContext;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IProductVariantRepository _productVariantRepository;
    private readonly IInventoryTransactionRepository _inventoryTransactionRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISystemLogService _systemLogService;

    public AdjustStockCommandHandler(
        IUserContext userContext,
        IInventoryRepository inventoryRepository,
        IProductVariantRepository productVariantRepository,
        IInventoryTransactionRepository inventoryTransactionRepository,
        IUnitOfWork unitOfWork,
        ISystemLogService systemLogService)
    {
        _userContext = userContext;
        _inventoryRepository = inventoryRepository;
        _productVariantRepository = productVariantRepository;
        _inventoryTransactionRepository = inventoryTransactionRepository;
        _unitOfWork = unitOfWork;
        _systemLogService = systemLogService;
    }

    public async Task<Result<AdjustStockCommandResponse>> Handle(
        AdjustStockCommand request,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(_userContext.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<AdjustStockCommandResponse>(UserErrors.Forbidden);
        }

        var inventory = await _inventoryRepository.GetByVariantIdAsync(request.VariantId, cancellationToken);
        int diff;

        if (inventory is null)
        {
            // Verify variant exists
            var variant = await _productVariantRepository.GetByIdAsync(request.VariantId, cancellationToken);
            if (variant is null)
            {
                return Result.Failure<AdjustStockCommandResponse>(ProductVariantErrors.NotFound);
            }

            // Upsert: Create new Inventoryitem for existing variant without inventory
            diff = request.NewQuantityOnHand;
            inventory = new Inventoryitem
            {
                Inventoryid = Guid.NewGuid(),
                Variantid = request.VariantId,
                Quantityonhand = request.NewQuantityOnHand,
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
            diff = request.NewQuantityOnHand - inventory.Quantityonhand;
            inventory.Quantityonhand = request.NewQuantityOnHand;
            inventory.Updatedat = DateTime.UtcNow;
            _inventoryRepository.Update(inventory);
        }

        var performer = _userContext.Email ?? "Admin";
        var refText = !string.IsNullOrWhiteSpace(request.Reason)
            ? $"{request.Reason} | By: {performer}"
            : $"By: {performer}";
        if (refText.Length > 100) refText = refText[..100];

        var transaction = new Inventorytransaction
        {
            Transactionid = Guid.NewGuid(),
            Variantid = request.VariantId,
            Quantitychange = diff,
            Type = "Adjust",
            Referenceid = refText,
            Createdat = DateTime.UtcNow,
            Updatedat = DateTime.UtcNow
        };

        await _inventoryTransactionRepository.AddAsync(transaction, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _systemLogService.LogAsync(
            "AdjustStock",
            $"Adjusted variant {request.VariantId} stock to {request.NewQuantityOnHand} (change: {diff}). Performer: {performer}",
            "Information",
            _userContext.UserId,
            cancellationToken);

        return Result.Success(new AdjustStockCommandResponse(
            inventory.Variantid,
            inventory.Quantityonhand,
            inventory.Quantityreserved));
    }
}
