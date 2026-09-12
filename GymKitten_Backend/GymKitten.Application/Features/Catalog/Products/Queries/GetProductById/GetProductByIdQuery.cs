using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Features.Catalog.ProductImages.Commands.UploadProductImages;
using GymKitten.Application.Features.Catalog.ProductVariants.Dtos;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Catalog.Products.Queries.GetProductById;

public record ProductDetailDto(
    Guid ProductId,
    Guid CategoryId,
    string Name,
    string Slug,
    string? Description,
    string? FitType,
    string Gender,
    bool IsActive,
    DateTime CreatedAt,
    List<ProductImageDto> Images,
    List<ProductVariantDto> Variants);

public sealed record GetProductByIdQuery(Guid ProductId) : IQuery<Result<ProductDetailDto>>;
