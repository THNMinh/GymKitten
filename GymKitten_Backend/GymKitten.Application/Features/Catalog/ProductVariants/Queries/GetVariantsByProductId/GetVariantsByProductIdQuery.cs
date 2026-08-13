using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Features.Catalog.ProductVariants.Dtos;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Catalog.ProductVariants.Queries.GetVariantsByProductId;

public sealed record GetVariantsByProductIdQuery(Guid ProductId)
    : IQuery<Result<List<ProductVariantDto>>>;
