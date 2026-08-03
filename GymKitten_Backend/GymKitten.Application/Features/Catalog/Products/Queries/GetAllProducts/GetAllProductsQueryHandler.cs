using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Catalog.Products.Queries.GetAllProducts;

public sealed class GetAllProductsQueryHandler
    : IQueryHandler<GetAllProductsQuery, Result<PagedResult<ProductListDto>>>
{
    private readonly IProductRepository _productRepository;

    public GetAllProductsQueryHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<PagedResult<ProductListDto>>> Handle(
        GetAllProductsQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize;

        var products = await _productRepository.GetAllPagedAsync(
            page, pageSize, request.SearchTerm, request.CategoryId, cancellationToken);

        var totalCount = await _productRepository.GetTotalCountAsync(
            request.SearchTerm, request.CategoryId, cancellationToken);

        var dtos = products.Select(p => new ProductListDto(
            p.Productid,
            p.Categoryid,
            p.Name,
            p.Slug,
            p.Description,
            p.Fittype,
            p.Gender,
            p.Isactive,
            p.Createdat)).ToList();

        var pagedResult = new PagedResult<ProductListDto>(dtos, page, pageSize, totalCount);

        return Result.Success(pagedResult);
    }
}
