namespace GymKitten.API.Requests;

public sealed record CreateProductRequest(
    Guid CategoryId,
    string Name,
    string Slug,
    string? Description,
    string? FitType,
    string Gender);

public sealed record UpdateProductRequest(
    Guid CategoryId,
    string Name,
    string Slug,
    string? Description,
    string? FitType,
    string Gender,
    bool IsActive);
