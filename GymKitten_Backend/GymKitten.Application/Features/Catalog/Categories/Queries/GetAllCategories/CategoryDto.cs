namespace GymKitten.Application.Features.Catalog.Categories.Queries.GetAllCategories;

public sealed record CategoryItemDto(
    Guid CategoryId,
    Guid? ParentCategoryId,
    string Name,
    string Slug,
    string? Description,
    int DisplayOrder,
    DateTime CreatedAt);
