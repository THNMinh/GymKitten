using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Catalog.SizeGuides.Queries.GetProductSizeGuide;

public sealed record GetProductSizeGuideQuery(Guid ProductId) : IQuery<Result<GetProductSizeGuideResponse>>;
