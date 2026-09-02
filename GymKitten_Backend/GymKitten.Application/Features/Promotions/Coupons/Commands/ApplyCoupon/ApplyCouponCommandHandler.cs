using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Common;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Promotions.Coupons.Commands.ApplyCoupon;

public sealed class ApplyCouponCommandHandler
    : ICommandHandler<ApplyCouponCommand, Result<ApplyCouponResponse>>
{
    private readonly IUserContext _userContext;
    private readonly ICouponRepository _couponRepository;

    public ApplyCouponCommandHandler(
        IUserContext userContext,
        ICouponRepository couponRepository)
    {
        _userContext = userContext;
        _couponRepository = couponRepository;
    }

    public async Task<Result<ApplyCouponResponse>> Handle(
        ApplyCouponCommand request,
        CancellationToken cancellationToken)
    {
        if (!_userContext.UserId.HasValue)
        {
            return Result.Failure<ApplyCouponResponse>(UserErrors.Unauthorized);
        }

        if (string.IsNullOrWhiteSpace(request.Code))
        {
            return Result.Failure<ApplyCouponResponse>(CouponErrors.NotFound);
        }

        var coupon = await _couponRepository.GetByCodeAsync(request.Code, cancellationToken);
        if (coupon is null || coupon.Deletedat != null)
        {
            return Result.Failure<ApplyCouponResponse>(CouponErrors.NotFound);
        }

        if (!coupon.Isactive)
        {
            return Result.Failure<ApplyCouponResponse>(CouponErrors.Inactive);
        }

        var now = DateTime.UtcNow;
        if (now < coupon.Startdate || now > coupon.Enddate)
        {
            return Result.Failure<ApplyCouponResponse>(CouponErrors.Expired);
        }

        if (coupon.Usagelimit.HasValue && coupon.Usedcount >= coupon.Usagelimit.Value)
        {
            return Result.Failure<ApplyCouponResponse>(CouponErrors.UsageLimitReached);
        }

        if (request.OrderSubtotal < coupon.Minordervalue)
        {
            return Result.Failure<ApplyCouponResponse>(CouponErrors.MinOrderValueNotMet);
        }

        decimal discountAmount = 0;
        if (coupon.Discounttype.Equals("Percentage", StringComparison.OrdinalIgnoreCase))
        {
            var calc = request.OrderSubtotal * (coupon.Discountvalue / 100m);
            if (coupon.Maxdiscountamount.HasValue)
            {
                calc = Math.Min(calc, coupon.Maxdiscountamount.Value);
            }
            discountAmount = Math.Round(calc, 2);
        }
        else if (coupon.Discounttype.Equals("FixedAmount", StringComparison.OrdinalIgnoreCase))
        {
            discountAmount = Math.Min(request.OrderSubtotal, coupon.Discountvalue);
        }

        var finalAmount = Math.Max(0, request.OrderSubtotal - discountAmount);

        var response = new ApplyCouponResponse(
            coupon.Code,
            coupon.Discounttype,
            coupon.Discountvalue,
            discountAmount,
            finalAmount,
            "Coupon applied successfully.");

        return Result.Success(response);
    }
}
