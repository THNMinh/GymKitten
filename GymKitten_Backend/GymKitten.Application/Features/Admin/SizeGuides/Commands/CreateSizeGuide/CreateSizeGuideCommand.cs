using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Admin.SizeGuides.Commands.CreateSizeGuide;

public sealed record CreateSizeGuideCommand(
    Guid ProductId,
    string Size,
    string? ChestCm,
    string? WaistCm,
    string? HipsCm,
    string? HeightRangeCm) : ICommand<Result<CreateSizeGuideResponse>>;

public sealed record CreateSizeGuideResponse(Guid GuideId);
