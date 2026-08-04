using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Catalog.Products.Queries.GetAllProducts;

public sealed record GetAllProductsQuery(
    string? SearchName = null,
    string? Gender = null,
    string? FitType = null,
    Guid? CategoryId = null,
    bool? IsActive = null,
    int Page = 1,
    int PageSize = 10) : IQuery<Result<GetAllProductsResponse>>;
