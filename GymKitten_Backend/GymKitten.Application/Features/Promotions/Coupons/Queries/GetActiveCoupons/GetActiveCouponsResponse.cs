namespace GymKitten.Application.Features.Promotions.Coupons.Queries.GetActiveCoupons;

public sealed record GetActiveCouponsResponse(List<ActiveCouponDto> Items);

public sealed record ActiveCouponDto(
    Guid CouponId,
    string Code,
    string DiscountType,
    decimal DiscountValue,
    decimal MinOrderValue,
    decimal? MaxDiscountAmount,
    int? UsageLimit,
    int UsedCount,
    DateTime StartDate,
    DateTime EndDate);
