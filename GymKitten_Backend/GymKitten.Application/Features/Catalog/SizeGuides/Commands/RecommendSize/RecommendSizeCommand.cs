using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Catalog.SizeGuides.Commands.RecommendSize;

public sealed record RecommendSizeCommand(
    Guid? ProductId,
    double? HeightCm,
    double? WeightKg,
    double? ChestCm,
    double? WaistCm) : ICommand<Result<RecommendSizeResponse>>;

public sealed record RecommendSizeResponse(
    string RecommendedSize,
    string Confidence,
    string Explanation);
