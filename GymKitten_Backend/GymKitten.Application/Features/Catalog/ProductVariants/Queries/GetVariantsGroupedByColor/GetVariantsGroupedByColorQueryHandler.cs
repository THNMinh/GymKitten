using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Application.Features.Catalog.ProductVariants.Dtos;
using GymKitten.Domain.Common;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Catalog.ProductVariants.Queries.GetVariantsGroupedByColor;

public sealed class GetVariantsGroupedByColorQueryHandler
    : IQueryHandler<GetVariantsGroupedByColorQuery, Result<List<ProductColorGroupDto>>>
{
    private readonly IProductRepository _productRepository;
    private readonly IProductVariantRepository _productVariantRepository;

    public GetVariantsGroupedByColorQueryHandler(
        IProductRepository productRepository,
        IProductVariantRepository productVariantRepository)
    {
        _productRepository = productRepository;
        _productVariantRepository = productVariantRepository;
    }

    public async Task<Result<List<ProductColorGroupDto>>> Handle(
        GetVariantsGroupedByColorQuery request,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);
        if (product is null)
        {
            return Result.Failure<List<ProductColorGroupDto>>(ProductErrors.NotFound);
        }

        var variants = await _productVariantRepository.GetByProductIdAsync(request.ProductId, cancellationToken);

        var groups = variants
            .GroupBy(v => v.Colorname.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(g =>
            {
                var first = g.First();
                var variantDtos = g.Select(v =>
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

                return new ProductColorGroupDto(
                    ColorName: first.Colorname,
                    ColorHex: first.Colorhex,
                    RepresentativeVariantId: first.Variantid,
                    TotalAvailableStock: variantDtos.Sum(v => v.Available),
                    AvailableSizes: variantDtos.Select(v => v.Size).Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
                    Variants: variantDtos
                );
            })
            .ToList();

        return Result.Success(groups);
    }
}
