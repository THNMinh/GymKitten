namespace GymKitten.Application.Features.Catalog.ProductVariants.Dtos;

public sealed record ProductVariantDto(
    Guid VariantId,
    Guid ProductId,
    string Sku,
    string ColorName,
    string? ColorHex,
    string Size,
    decimal Price,
    decimal? OriginalPrice,
    int? WeightGrams,
    int Available = 10);
