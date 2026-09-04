namespace GymKitten.API.Requests;

public sealed record RecommendSizeRequest(
    Guid? ProductId,
    double? HeightCm,
    double? WeightKg,
    double? ChestCm,
    double? WaistCm);

public sealed record CreateSizeGuideRequest(
    string Size,
    string? ChestCm,
    string? WaistCm,
    string? HipsCm,
    string? HeightRangeCm);

public sealed record UpdateSizeGuideRequest(
    string Size,
    string? ChestCm,
    string? WaistCm,
    string? HipsCm,
    string? HeightRangeCm);
