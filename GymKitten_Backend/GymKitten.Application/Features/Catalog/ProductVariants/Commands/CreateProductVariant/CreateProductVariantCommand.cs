using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Catalog.ProductVariants.Commands.CreateProductVariant;

public sealed record CreateProductVariantCommand(
    Guid ProductId,
    string Sku,
    string ColorName,
    string? ColorHex,
    string Size,
    decimal Price,
    decimal? OriginalPrice = null,
    int? WeightGrams = null) : ICommand<Result<CreateProductVariantResponse>>;
