namespace GymKitten.Application.Features.Catalog.ProductVariants.Dtos;

public sealed record ProductColorGroupDto(
    string ColorName,
    string? ColorHex,
    Guid RepresentativeVariantId,
    int TotalAvailableStock,
    List<string> AvailableSizes,
    List<ProductVariantDto> Variants);
