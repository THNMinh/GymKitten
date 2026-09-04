using GymKitten.Domain.Common;

namespace GymKitten.Domain.Errors;

public static class SizeGuideErrors
{
    public static readonly Error NotFound = new(
        "SizeGuide.NotFound",
        "The requested size guide entry was not found.");

    public static readonly Error NoGuidesAvailable = new(
        "SizeGuide.NoGuidesAvailable",
        "No size guide data is configured for this product.");
}
