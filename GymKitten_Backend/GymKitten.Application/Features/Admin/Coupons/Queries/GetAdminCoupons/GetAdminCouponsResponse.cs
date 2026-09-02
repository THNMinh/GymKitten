namespace GymKitten.Application.Features.Admin.Coupons.Queries.GetAdminCoupons;

public sealed record GetAdminCouponsResponse(
    List<AdminCouponDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);

public sealed record AdminCouponDto(
    Guid CouponId,
    string Code,
    string DiscountType,
    decimal DiscountValue,
    decimal MinOrderValue,
    decimal? MaxDiscountAmount,
    int? UsageLimit,
    int UsedCount,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);
