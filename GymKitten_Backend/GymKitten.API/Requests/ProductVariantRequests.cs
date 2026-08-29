namespace GymKitten.API.Requests;

public sealed record CreateProductVariantRequest(
    Guid ProductId,
    string Sku,
    string ColorName,
    string ColorHex,
    string Size,
    decimal Price,
    decimal OriginalPrice,
    int WeightGrams);

public sealed record UpdateProductVariantRequest(
    string? Sku = null,
    string? ColorName = null,
    string? ColorHex = null,
    string? Size = null,
    decimal? Price = null,
    decimal? OriginalPrice = null,
    int? WeightGrams = null);
