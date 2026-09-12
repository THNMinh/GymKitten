using GymKitten.Application.Features.Catalog.ProductImages.Commands.UploadProductImages;
using GymKitten.Application.Features.Catalog.ProductVariants.Dtos;

namespace GymKitten.Application.Features.Catalog.Products.Queries.GetProductBySlug;

public sealed record GetProductBySlugResponse(
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
    List<ProductVariantDto> Variants,
    double AverageRating,
    int ReviewCount);
