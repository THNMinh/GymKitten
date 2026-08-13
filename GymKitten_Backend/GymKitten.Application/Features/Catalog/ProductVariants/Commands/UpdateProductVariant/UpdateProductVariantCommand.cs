using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Catalog.ProductVariants.Commands.UpdateProductVariant;

public sealed record UpdateProductVariantCommand(
    Guid VariantId = default,
    string? Sku = null,
    string? ColorName = null,
    string? ColorHex = null,
    string? Size = null,
    decimal? Price = null,
    decimal? OriginalPrice = null,
    int? WeightGrams = null) : ICommand<Result<UpdateProductVariantResponse>>;
