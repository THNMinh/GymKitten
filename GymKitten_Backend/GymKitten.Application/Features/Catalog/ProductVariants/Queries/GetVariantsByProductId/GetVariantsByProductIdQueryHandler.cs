using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Application.Features.Catalog.ProductVariants.Dtos;
using GymKitten.Domain.Common;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Catalog.ProductVariants.Queries.GetVariantsByProductId;

public sealed class GetVariantsByProductIdQueryHandler
    : IQueryHandler<GetVariantsByProductIdQuery, Result<List<ProductVariantDto>>>
{
    private readonly IProductRepository _productRepository;
    private readonly IProductVariantRepository _productVariantRepository;

    public GetVariantsByProductIdQueryHandler(
        IProductRepository productRepository,
        IProductVariantRepository productVariantRepository)
    {
        _productRepository = productRepository;
        _productVariantRepository = productVariantRepository;
    }

    public async Task<Result<List<ProductVariantDto>>> Handle(
        GetVariantsByProductIdQuery request,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
        {
            return Result.Failure<List<ProductVariantDto>>(ProductErrors.NotFound);
        }

        var variants = await _productVariantRepository.GetByProductIdAsync(request.ProductId, cancellationToken);

        var dtos = variants.Select(v =>
        {
            var stock = v.Inventoryitem != null
                ? Math.Max(0, v.Inventoryitem.Quantityonhand - v.Inventoryitem.Quantityreserved)
                : 0;
            return new ProductVariantDto(
                v.Variantid,
                v.Productid,
                v.Sku,
                v.Colorname,
                v.Colorhex,
                v.Size,
                v.Price,
                v.Originalprice,
                v.Weightgrams,
                Available: stock,
                AvailableStock: stock);
        }).ToList();

        return Result.Success(dtos);
    }
}
