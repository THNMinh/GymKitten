namespace GymKitten.Application.Features.Catalog.SizeGuides.Queries.GetProductSizeGuide;

public sealed record GetProductSizeGuideResponse(
    Guid ProductId,
    List<SizeGuideDto> Items);

public sealed record SizeGuideDto(
    Guid GuideId,
    string Size,
    string? ChestCm,
    string? WaistCm,
    string? HipsCm,
    string? HeightRangeCm);
