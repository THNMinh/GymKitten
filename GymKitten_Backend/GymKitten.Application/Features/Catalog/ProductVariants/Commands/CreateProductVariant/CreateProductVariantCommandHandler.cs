using GymKitten.Application.Abstractions.Data;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Common;
using GymKitten.Domain.Entities;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Catalog.ProductVariants.Commands.CreateProductVariant;

public sealed class CreateProductVariantCommandHandler
    : ICommandHandler<CreateProductVariantCommand, Result<CreateProductVariantResponse>>
{
    private readonly IProductRepository _productRepository;
    private readonly IProductVariantRepository _productVariantRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductVariantCommandHandler(
        IProductRepository productRepository,
        IProductVariantRepository productVariantRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _productVariantRepository = productVariantRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CreateProductVariantResponse>> Handle(
        CreateProductVariantCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Verify parent product exists
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
        {
            return Result.Failure<CreateProductVariantResponse>(ProductErrors.NotFound);
        }

        // 2. Check SKU uniqueness
        var skuExists = await _productVariantRepository.ExistsBySkuAsync(request.Sku.Trim(), cancellationToken);
        if (skuExists)
        {
            return Result.Failure<CreateProductVariantResponse>(ProductVariantErrors.SkuAlreadyExists);
        }

        // 3. Create entity
        var now = DateTime.UtcNow;
        var variant = new Productvariant
        {
            Variantid = Guid.NewGuid(),
            Productid = request.ProductId,
            Sku = request.Sku.Trim(),
            Colorname = request.ColorName.Trim(),
            Colorhex = request.ColorHex?.Trim(),
            Size = request.Size.Trim(),
            Price = request.Price,
            Originalprice = request.OriginalPrice,
            Weightgrams = request.WeightGrams,
            Createdat = now,
            Updatedat = now
        };

        await _productVariantRepository.AddAsync(variant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateProductVariantResponse(variant.Variantid));
    }
}
