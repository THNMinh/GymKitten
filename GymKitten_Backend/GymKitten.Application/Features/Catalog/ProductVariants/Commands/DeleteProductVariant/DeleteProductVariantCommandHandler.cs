using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Data;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Common;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Catalog.ProductVariants.Commands.DeleteProductVariant;

public sealed class DeleteProductVariantCommandHandler
    : ICommandHandler<DeleteProductVariantCommand, Result>
{
    private readonly IUserContext _userContext;
    private readonly IProductVariantRepository _productVariantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProductVariantCommandHandler(
        IUserContext userContext,
        IProductVariantRepository productVariantRepository,
        IUnitOfWork unitOfWork)
    {
        _userContext = userContext;
        _productVariantRepository = productVariantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        DeleteProductVariantCommand request,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(_userContext.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure(UserErrors.Forbidden);
        }

        var variant = await _productVariantRepository.GetByIdAsync(request.VariantId, cancellationToken);
        if (variant is null)
        {
            return Result.Failure(ProductVariantErrors.NotFound);
        }

        // Check if variant has orders - prevent deletion if active order references exist
        var hasOrders = await _productVariantRepository.HasOrdersAsync(variant.Variantid, cancellationToken);
        if (hasOrders)
        {
            return Result.Failure(ProductVariantErrors.CannotDeleteWithOrders);
        }

        // Soft delete variant and release original SKU
        var now = DateTime.UtcNow;
        variant.Deletedat = now;
        variant.Updatedat = now;
        variant.Sku = $"{variant.Sku}__deleted_{now.Ticks}";

        if (variant.Inventoryitem != null)
        {
            variant.Inventoryitem.Deletedat = now;
            variant.Inventoryitem.Updatedat = now;
        }

        _productVariantRepository.Update(variant);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
