namespace GymKitten.Application.Features.Catalog.Products.Queries.GetAllProducts;

public sealed record GetAllProductsResponse(
    List<ProductItemDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);
