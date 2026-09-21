using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Application.Features.Catalog.ProductImages.Commands.UploadProductImages;
using GymKitten.Application.Features.Catalog.ProductVariants.Dtos;
using GymKitten.Domain.Common;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Catalog.Products.Queries.GetProductById;

public sealed class GetProductByIdQueryHandler
    : IQueryHandler<GetProductByIdQuery, Result<ProductDetailDto>>
{
    private readonly IProductRepository _productRepository;

    public GetProductByIdQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<ProductDetailDto>> Handle(
        GetProductByIdQuery request,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetProductWithImagesAsync(request.ProductId, cancellationToken);
        if (product is null)
        {
            return Result.Failure<ProductDetailDto>(ProductErrors.NotFound);
        }

        var imageDtos = product.Productimages
            .OrderBy(img => img.Displayorder)
            .Select(img => new ProductImageDto(
                img.Imageid,
                img.Productid,
                img.Variantid,
                img.Imageurl,
                img.Displayorder,
                img.Isprimary,
                img.Variant?.Colorname,
                img.Variant?.Colorhex))
            .ToList();

        var variantDtos = product.Productvariants
            .OrderBy(v => v.Colorname)
            .ThenBy(v => v.Size)
            .Select(v =>
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
            })
            .ToList();

        var activeReviews = product.Productreviews.Where(r => r.Deletedat == null).ToList();
        var avgRating = activeReviews.Any()
            ? Math.Round(activeReviews.Average(r => r.Rating), 1)
            : 0.0;
        var reviewCount = activeReviews.Count;

        var detailDto = new ProductDetailDto(
            product.Productid,
            product.Categoryid,
            product.Name,
            product.Slug,
            product.Description,
            product.Fittype,
            product.Gender,
            product.Isactive,
            product.Createdat,
            imageDtos,
            variantDtos,
            avgRating,
            reviewCount);

        return Result.Success(detailDto);
    }
}
