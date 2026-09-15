using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Features.Catalog.ProductVariants.Dtos;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Catalog.ProductVariants.Queries.GetVariantsGroupedByColor;

public sealed record GetVariantsGroupedByColorQuery(Guid ProductId)
    : IQuery<Result<List<ProductColorGroupDto>>>;
