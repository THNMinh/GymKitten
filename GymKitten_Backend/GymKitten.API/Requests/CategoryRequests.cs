namespace GymKitten.API.Requests;

public sealed record CreateCategoryRequest(
    Guid? ParentCategoryId,
    string Name,
    string Slug,
    string? Description,
    int DisplayOrder);

public sealed record UpdateCategoryRequest(
    Guid? ParentCategoryId,
    string Name,
    string Slug,
    string? Description,
    int DisplayOrder);
