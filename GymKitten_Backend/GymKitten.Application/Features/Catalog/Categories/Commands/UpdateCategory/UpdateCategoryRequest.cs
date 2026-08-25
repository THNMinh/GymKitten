namespace GymKitten.Application.Features.Catalog.Categories.Commands.UpdateCategory;

public sealed record UpdateCategoryRequest(
    Guid? ParentCategoryId,
    string Name,
    string Slug,
    string? Description,
    int DisplayOrder);
