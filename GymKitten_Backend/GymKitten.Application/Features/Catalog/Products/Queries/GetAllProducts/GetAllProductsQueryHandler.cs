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
            var primaryImage = p.Productimages
                .OrderByDescending(img => img.Variantid == null)
                .ThenByDescending(img => img.Isprimary)
                .ThenBy(img => img.Displayorder)
                .FirstOrDefault();

            return new ProductItemDto(
                p.Productid,
                p.Name,
                p.Slug,
                p.Productvariants.Any() ? p.Productvariants.Min(v => v.Price) : 0m,
                p.Gender,
                primaryImage?.Imageurl,
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
