namespace GymKitten.Application.Features.Catalog.Categories.Queries.GetAllCategories;

public sealed record GetAllCategoriesResponse(
    List<CategoryItemDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);
