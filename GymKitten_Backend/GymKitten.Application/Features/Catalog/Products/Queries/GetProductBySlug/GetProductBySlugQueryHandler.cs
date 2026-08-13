using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Application.Features.Catalog.ProductImages.Commands.UploadProductImages;
using GymKitten.Application.Features.Catalog.ProductVariants.Dtos;
using GymKitten.Domain.Common;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Catalog.Products.Queries.GetProductBySlug;

public sealed class GetProductBySlugQueryHandler
    : IQueryHandler<GetProductBySlugQuery, Result<GetProductBySlugResponse>>
{
    private readonly IProductRepository _productRepository;

    public GetProductBySlugQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<GetProductBySlugResponse>> Handle(
        GetProductBySlugQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Slug))
        {
            return Result.Failure<GetProductBySlugResponse>(ProductErrors.NotFound);
        }

        var product = await _productRepository.GetBySlugAsync(request.Slug.Trim().ToLower(), cancellationToken);

        if (product is null || !product.Isactive)
        {
            return Result.Failure<GetProductBySlugResponse>(ProductErrors.NotFound);
        }

        var images = product.Productimages
            .OrderBy(img => img.Displayorder)
            .Select(img => new ProductImageDto(
                img.Imageid,
                img.Productid,
                img.Variantid,
                img.Imageurl,
                img.Displayorder,
                img.Isprimary))
            .ToList();

        var variants = product.Productvariants
            .OrderBy(v => v.Colorname)
            .ThenBy(v => v.Size)
            .Select(v => new ProductVariantDto(
                v.Variantid,
                v.Productid,
                v.Sku,
                v.Colorname,
                v.Colorhex,
                v.Size,
                v.Price,
                v.Originalprice,
                v.Weightgrams,
                v.Inventoryitem != null ? Math.Max(0, v.Inventoryitem.Quantityonhand - v.Inventoryitem.Quantityreserved) : 10))
            .ToList();

        var response = new GetProductBySlugResponse(
            product.Productid,
            product.Categoryid,
            product.Name,
            product.Slug,
            product.Description,
            product.Fittype,
            product.Gender,
            product.Isactive,
            product.Createdat,
            images,
            variants);

        return Result.Success(response);
    }
}
