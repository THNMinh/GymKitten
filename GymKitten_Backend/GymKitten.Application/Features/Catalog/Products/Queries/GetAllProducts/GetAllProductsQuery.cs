using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Catalog.Products.Queries.GetAllProducts;

public record ProductListDto(
    Guid ProductId,
    Guid CategoryId,
    string Name,
    string Slug,
    string? Description,
    string? FitType,
    string Gender,
    bool IsActive,
    DateTime CreatedAt);

public record PagedResult<T>(
    List<T> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public bool HasNextPage => Page * PageSize < TotalCount;
    public bool HasPreviousPage => Page > 1;
}

public sealed record GetAllProductsQuery(
    int Page = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    Guid? CategoryId = null) : IQuery<Result<PagedResult<ProductListDto>>>;
