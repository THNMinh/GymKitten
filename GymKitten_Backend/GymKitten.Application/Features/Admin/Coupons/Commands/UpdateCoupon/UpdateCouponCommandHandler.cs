using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Data;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Application.Abstractions.Services;
using GymKitten.Domain.Common;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Admin.Coupons.Commands.UpdateCoupon;

public sealed class UpdateCouponCommandHandler
    : ICommandHandler<UpdateCouponCommand, Result<UpdateCouponResponse>>
{
    private readonly IUserContext _userContext;
    private readonly ICouponRepository _couponRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISystemLogService? _systemLogService;

    public UpdateCouponCommandHandler(
        IUserContext userContext,
        ICouponRepository couponRepository,
        IUnitOfWork unitOfWork,
        ISystemLogService? systemLogService = null)
    {
        _userContext = userContext;
        _couponRepository = couponRepository;
        _unitOfWork = unitOfWork;
        _systemLogService = systemLogService;
    }

    public async Task<Result<UpdateCouponResponse>> Handle(
        UpdateCouponCommand request,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(_userContext.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<UpdateCouponResponse>(UserErrors.Forbidden);
        }

        var coupon = await _couponRepository.GetByIdAsync(request.CouponId, cancellationToken);
        if (coupon is null || coupon.Deletedat != null)
        {
            return Result.Failure<UpdateCouponResponse>(CouponErrors.NotFound);
        }

        var normalizedCode = request.Code.Trim().ToUpper();
        if (coupon.Code.ToUpper() != normalizedCode)
        {
            var codeExists = await _couponRepository.ExistsByCodeExcludingIdAsync(normalizedCode, request.CouponId, cancellationToken);
            if (codeExists)
            {
                return Result.Failure<UpdateCouponResponse>(CouponErrors.CodeAlreadyExists);
            }
            coupon.Code = normalizedCode;
        }

        var discountTypeNorm = request.DiscountType.Trim();
        if (!discountTypeNorm.Equals("Percentage", StringComparison.OrdinalIgnoreCase) &&
            !discountTypeNorm.Equals("FixedAmount", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<UpdateCouponResponse>(CouponErrors.InvalidDiscountType);
        }

        coupon.Discounttype = discountTypeNorm.Equals("Percentage", StringComparison.OrdinalIgnoreCase) ? "Percentage" : "FixedAmount";
        coupon.Discountvalue = request.DiscountValue;
        coupon.Minordervalue = request.MinOrderValue;
        coupon.Maxdiscountamount = request.MaxDiscountAmount;
        coupon.Usagelimit = request.UsageLimit;
        coupon.Startdate = request.StartDate;
        coupon.Enddate = request.EndDate;
        coupon.Isactive = request.IsActive;
        coupon.Updatedat = DateTime.UtcNow;

        _couponRepository.Update(coupon);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (_systemLogService != null)
        {
            await _systemLogService.LogAsync(
                "UpdateCoupon",
                $"Admin {_userContext.Email ?? "Unknown"} updated coupon '{coupon.Code}' (ID: {coupon.Couponid}). Active: {coupon.Isactive}.",
                "Information",
                cancellationToken: cancellationToken);
        }

        return Result.Success(new UpdateCouponResponse(coupon.Couponid));
    }
}
