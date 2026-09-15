using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Data;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Common;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Catalog.ProductVariants.Commands.UpdateProductVariant;

public sealed class UpdateProductVariantCommandHandler
    : ICommandHandler<UpdateProductVariantCommand, Result<UpdateProductVariantResponse>>
{
    private readonly IUserContext _userContext;
    private readonly IProductVariantRepository _productVariantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProductVariantCommandHandler(
        IUserContext userContext,
        IProductVariantRepository productVariantRepository,
        IUnitOfWork unitOfWork)
    {
        _userContext = userContext;
        _productVariantRepository = productVariantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UpdateProductVariantResponse>> Handle(
        UpdateProductVariantCommand request,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(_userContext.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<UpdateProductVariantResponse>(UserErrors.Forbidden);
        }

        // 1. Fetch variant
        var variant = await _productVariantRepository.GetByIdAsync(request.VariantId, cancellationToken);
        if (variant is null)
        {
            return Result.Failure<UpdateProductVariantResponse>(ProductVariantErrors.NotFound);
        }

        // 2. Check SKU uniqueness if SKU changed
        if (!string.IsNullOrWhiteSpace(request.Sku) && variant.Sku != request.Sku.Trim())
        {
            var skuExists = await _productVariantRepository.ExistsBySkuExcludingIdAsync(request.Sku.Trim(), request.VariantId, cancellationToken);
            if (skuExists)
            {
                return Result.Failure<UpdateProductVariantResponse>(ProductVariantErrors.SkuAlreadyExists);
            }
            variant.Sku = request.Sku.Trim();
        }

        // 3. Check duplicate color + size if either changed
        var newColor = !string.IsNullOrWhiteSpace(request.ColorName) ? request.ColorName.Trim() : variant.Colorname;
        var newSize = !string.IsNullOrWhiteSpace(request.Size) ? request.Size.Trim() : variant.Size;

        if (!string.Equals(newColor, variant.Colorname, StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(newSize, variant.Size, StringComparison.OrdinalIgnoreCase))
        {
            var isDuplicate = await _productVariantRepository.ExistsByProductColorAndSizeAsync(
                variant.Productid,
                newColor,
                newSize,
                variant.Variantid,
                cancellationToken);
            if (isDuplicate)
            {
                return Result.Failure<UpdateProductVariantResponse>(ProductVariantErrors.DuplicateColorAndSize);
            }
        }

        // 3. Update properties if provided
        if (!string.IsNullOrWhiteSpace(request.ColorName))
        {
            variant.Colorname = request.ColorName.Trim();
        }

        if (request.ColorHex != null)
        {
            variant.Colorhex = request.ColorHex.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.Size))
        {
            variant.Size = request.Size.Trim();
        }

        if (request.Price.HasValue)
        {
            if (request.Price.Value <= 0 || request.Price.Value > 1000000000m)
            {
                return Result.Failure<UpdateProductVariantResponse>(ProductVariantErrors.InvalidPrice);
            }
            variant.Price = request.Price.Value;
        }

        if (request.OriginalPrice.HasValue)
        {
            if (request.OriginalPrice.Value > 1000000000m)
            {
                return Result.Failure<UpdateProductVariantResponse>(ProductVariantErrors.InvalidPrice);
            }
            variant.Originalprice = request.OriginalPrice.Value;
        }

        if (request.WeightGrams.HasValue)
        {
            variant.Weightgrams = request.WeightGrams.Value;
        }

        variant.Updatedat = DateTime.UtcNow;

        _productVariantRepository.Update(variant);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new UpdateProductVariantResponse(variant.Variantid));
    }
}
