namespace GymKitten.Application.Features.Catalog.Products.Commands.UpdateProduct;

public sealed record UpdateProductRequest(
    Guid CategoryId,
    string Name,
    string Slug,
    string? Description,
    string? FitType,
    string Gender,
    bool IsActive);
