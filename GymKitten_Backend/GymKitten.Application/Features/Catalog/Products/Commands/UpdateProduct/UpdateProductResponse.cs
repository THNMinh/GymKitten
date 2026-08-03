namespace GymKitten.Application.Features.Catalog.Products.Commands.UpdateProduct;

public sealed record UpdateProductResponse(
    Guid ProductId,
    string Name,
    string Slug,
    bool IsActive);
