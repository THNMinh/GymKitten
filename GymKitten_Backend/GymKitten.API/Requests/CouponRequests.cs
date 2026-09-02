namespace GymKitten.API.Requests;

public sealed record ApplyCouponRequest(
    string Code,
    decimal OrderSubtotal);

public sealed record CreateCouponRequest(
    string Code,
    string DiscountType,
    decimal DiscountValue,
    decimal MinOrderValue,
    decimal? MaxDiscountAmount,
    int? UsageLimit,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive = true);

public sealed record UpdateCouponRequest(
    string Code,
    string DiscountType,
    decimal DiscountValue,
    decimal MinOrderValue,
    decimal? MaxDiscountAmount,
    int? UsageLimit,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive);
