using GymKitten.Application.Abstractions.Data;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Common;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Catalog.ProductVariants.Commands.UpdateProductVariant;

public sealed class UpdateProductVariantCommandHandler
    : ICommandHandler<UpdateProductVariantCommand, Result<UpdateProductVariantResponse>>
{
    private readonly IProductVariantRepository _productVariantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProductVariantCommandHandler(
        IProductVariantRepository productVariantRepository,
        IUnitOfWork unitOfWork)
    {
        _productVariantRepository = productVariantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UpdateProductVariantResponse>> Handle(
        UpdateProductVariantCommand request,
        CancellationToken cancellationToken)
    {
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

        if (request.Price.HasValue && request.Price.Value > 0)
        {
            variant.Price = request.Price.Value;
        }

        if (request.OriginalPrice.HasValue)
        {
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
