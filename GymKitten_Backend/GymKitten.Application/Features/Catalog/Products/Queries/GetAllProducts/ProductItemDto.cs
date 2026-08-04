namespace GymKitten.Application.Features.Catalog.Products.Queries.GetAllProducts;

public sealed record ProductItemDto(
    Guid ProductId,
    string Name,
    string Slug,
    decimal Price,
    string Gender,
    string? PrimaryImageUrl,
    DateTime CreatedAt);
