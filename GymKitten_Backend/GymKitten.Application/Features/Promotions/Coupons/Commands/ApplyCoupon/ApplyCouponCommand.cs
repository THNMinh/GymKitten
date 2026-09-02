using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Promotions.Coupons.Commands.ApplyCoupon;

public sealed record ApplyCouponCommand(
    string Code,
    decimal OrderSubtotal) : ICommand<Result<ApplyCouponResponse>>;

public sealed record ApplyCouponResponse(
    string Code,
    string DiscountType,
    decimal DiscountValue,
    decimal DiscountAmount,
    decimal FinalAmount,
    string Message);
