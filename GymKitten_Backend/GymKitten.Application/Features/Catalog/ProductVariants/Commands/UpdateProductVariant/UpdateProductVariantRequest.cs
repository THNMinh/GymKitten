namespace GymKitten.Application.Features.Catalog.ProductVariants.Commands.UpdateProductVariant;

public sealed record UpdateProductVariantRequest(
    string? Sku = null,
    string? ColorName = null,
    string? ColorHex = null,
    string? Size = null,
    decimal? Price = null,
    decimal? OriginalPrice = null,
    int? WeightGrams = null);
