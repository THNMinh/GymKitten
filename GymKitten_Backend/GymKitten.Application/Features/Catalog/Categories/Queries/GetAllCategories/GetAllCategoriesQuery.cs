using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Catalog.Categories.Queries.GetAllCategories;

public sealed record GetAllCategoriesQuery(
    string? SearchName = null,
    Guid? ParentCategoryId = null,
    int Page = 1,
    int PageSize = 10) : IQuery<Result<GetAllCategoriesResponse>>;
