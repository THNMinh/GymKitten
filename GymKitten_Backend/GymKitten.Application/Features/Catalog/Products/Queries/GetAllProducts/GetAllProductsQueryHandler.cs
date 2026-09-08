using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Catalog.Products.Queries.GetAllProducts;

public sealed class GetAllProductsQueryHandler
    : IQueryHandler<GetAllProductsQuery, Result<GetAllProductsResponse>>
{
    private readonly IProductRepository _productRepository;

    public GetAllProductsQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<GetAllProductsResponse>> Handle(
        GetAllProductsQuery request,
        CancellationToken cancellationToken)
    {
        var (products, total) = await _productRepository.SearchProductsAsync(
            request.SearchName,
            request.Gender,
            request.FitType,
            request.CategoryId,
            request.CategorySlug,
            request.IsActive,
            request.Colors,
            request.Sizes,
            request.MinPrice,
            request.MaxPrice,
            request.Page,
            request.PageSize,
            cancellationToken);

        var items = products.Select(p =>
        {
            var imagesOrdered = p.Productimages
                .OrderByDescending(img => img.Isprimary)
                .ThenBy(img => img.Displayorder)
                .ToList();

            var primaryImage = imagesOrdered.FirstOrDefault();
            var secondaryImage = imagesOrdered.Skip(1).FirstOrDefault() ?? primaryImage;

            var imageDtos = imagesOrdered.Select(img => new ProductImageItemDto(
                img.Imageid,
                img.Imageurl,
                img.Isprimary,
                img.Displayorder,
                img.Variantid)).ToList();

            var variantDtos = p.Productvariants
                .OrderBy(v => v.Colorname)
                .ThenBy(v => v.Size)
                .Select(v =>
                {
                    var stock = v.Inventoryitem != null
                        ? Math.Max(0, v.Inventoryitem.Quantityonhand - v.Inventoryitem.Quantityreserved)
                        : 10;

                    var vImg = p.Productimages.FirstOrDefault(img => img.Variantid == v.Variantid)?.Imageurl
                             ?? primaryImage?.Imageurl;

                    return new ProductVariantItemDto(
                        v.Variantid,
                        v.Sku,
                        v.Colorname,
                        v.Colorhex,
                        v.Size,
                        v.Price,
                        v.Originalprice,
                        stock,
                        stock > 0,
                        vImg);
                }).ToList();

            var activeReviews = p.Productreviews.Where(r => r.Deletedat == null).ToList();
            var avgRating = activeReviews.Any()
                ? Math.Round(activeReviews.Average(r => r.Rating), 1)
                : 0.0;
            var reviewCount = activeReviews.Count;

            var minPrice = variantDtos.Any() ? variantDtos.Min(v => v.Price) : 0m;

            return new ProductItemDto(
                p.Productid,
                p.Categoryid,
                p.Category?.Name,
                p.Name,
                p.Slug,
                p.Description,
                p.Fittype,
                p.Gender,
                p.Isactive,
                minPrice,
                primaryImage?.Imageurl,
                secondaryImage?.Imageurl,
                avgRating,
                reviewCount,
                imageDtos,
                variantDtos,
                p.Createdat);
        }).ToList();

        var totalPages = (int)Math.Ceiling(total / (double)request.PageSize);

        var response = new GetAllProductsResponse(
            items,
            total,
            request.Page,
            request.PageSize,
            totalPages);

        return Result.Success(response);
    }
}
