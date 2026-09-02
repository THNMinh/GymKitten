using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Admin.Coupons.Commands.UpdateCoupon;

public sealed record UpdateCouponCommand(
    Guid CouponId,
    string Code,
    string DiscountType,
    decimal DiscountValue,
    decimal MinOrderValue,
    decimal? MaxDiscountAmount,
    int? UsageLimit,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive) : ICommand<Result<UpdateCouponResponse>>;

public sealed record UpdateCouponResponse(Guid CouponId);
