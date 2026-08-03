namespace GymKitten.Application.Features.Catalog.Categories.Commands.UpdateCategory;

public sealed record UpdateCategoryResponse(
    Guid CategoryId,
    string Name,
    string Slug);
