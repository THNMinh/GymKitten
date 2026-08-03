namespace GymKitten.Application.Features.Catalog.ProductImages.Commands.UploadProductImages;

public record ProductImageDto(
    Guid ImageId,
    Guid ProductId,
    Guid? VariantId,
    string ImageUrl,
    int DisplayOrder,
    bool IsPrimary);

public sealed record UploadProductImagesResponse(
    Guid ProductId,
    List<ProductImageDto> Images);
