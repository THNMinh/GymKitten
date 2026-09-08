using System;
using System.Collections.Generic;

namespace GymKitten.Application.Features.Catalog.Products.Queries.GetAllProducts;

public sealed record ProductVariantItemDto(
    Guid VariantId,
    string Sku,
    string ColorName,
    string? ColorHex,
    string Size,
    decimal Price,
    decimal? OriginalPrice,
    int StockQuantity,
    bool IsAvailable,
    string? VariantImageUrl);

public sealed record ProductImageItemDto(
    Guid ImageId,
    string ImageUrl,
    bool IsPrimary,
    int DisplayOrder,
    Guid? VariantId);

public sealed record ProductItemDto(
    Guid ProductId,
    Guid CategoryId,
    string? CategoryName,
    string Name,
    string Slug,
    string? Description,
    string? FitType,
    string Gender,
    bool IsActive,
    decimal Price,
    string? PrimaryImageUrl,
    string? SecondaryImageUrl,
    double AverageRating,
    int ReviewCount,
    List<ProductImageItemDto> Images,
    List<ProductVariantItemDto> Variants,
    DateTime CreatedAt);
