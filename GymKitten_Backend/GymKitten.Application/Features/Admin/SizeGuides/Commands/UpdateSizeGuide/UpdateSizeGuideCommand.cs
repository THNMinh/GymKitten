using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Admin.SizeGuides.Commands.UpdateSizeGuide;

public sealed record UpdateSizeGuideCommand(
    Guid GuideId,
    string Size,
    string? ChestCm,
    string? WaistCm,
    string? HipsCm,
    string? HeightRangeCm) : ICommand<Result<UpdateSizeGuideResponse>>;

public sealed record UpdateSizeGuideResponse(Guid GuideId);
