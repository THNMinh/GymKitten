using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Admin.Coupons.Commands.CreateCoupon;

public sealed record CreateCouponCommand(
    string Code,
    string DiscountType,
    decimal DiscountValue,
    decimal MinOrderValue,
    decimal? MaxDiscountAmount,
    int? UsageLimit,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive = true) : ICommand<Result<CreateCouponResponse>>;

public sealed record CreateCouponResponse(Guid CouponId);
