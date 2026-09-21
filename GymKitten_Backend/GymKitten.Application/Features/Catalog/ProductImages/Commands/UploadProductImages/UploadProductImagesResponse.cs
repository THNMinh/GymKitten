namespace GymKitten.Application.Features.Catalog.ProductImages.Commands.UploadProductImages;

public record ProductImageDto(
    Guid ImageId,
    Guid ProductId,
    Guid? VariantId,
    string ImageUrl,
    int DisplayOrder,
    bool IsPrimary,
    string? ColorName = null,
    string? ColorHex = null);

public sealed record UploadProductImagesResponse(
    Guid ProductId,
    List<ProductImageDto> Images);
