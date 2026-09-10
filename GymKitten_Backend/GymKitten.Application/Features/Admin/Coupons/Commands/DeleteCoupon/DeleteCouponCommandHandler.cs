using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Data;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Application.Abstractions.Services;
using GymKitten.Domain.Common;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Admin.Coupons.Commands.DeleteCoupon;

public sealed class DeleteCouponCommandHandler
    : ICommandHandler<DeleteCouponCommand, Result>
{
    private readonly IUserContext _userContext;
    private readonly ICouponRepository _couponRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISystemLogService? _systemLogService;

    public DeleteCouponCommandHandler(
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

    public async Task<Result> Handle(
        DeleteCouponCommand request,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(_userContext.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure(UserErrors.Forbidden);
        }

        var coupon = await _couponRepository.GetByIdAsync(request.CouponId, cancellationToken);
        if (coupon is null || coupon.Deletedat != null)
        {
            return Result.Failure(CouponErrors.NotFound);
        }

        // Soft Delete: Mark DeletedAt and set IsActive = false so DB records & historical order statistics remain intact
        coupon.Deletedat = DateTime.UtcNow;
        coupon.Isactive = false;
        coupon.Updatedat = DateTime.UtcNow;

        _couponRepository.Update(coupon);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (_systemLogService != null)
        {
            await _systemLogService.LogAsync(
                "DeleteCoupon",
                $"Admin {_userContext.Email ?? "Unknown"} deleted (deactivated) coupon '{coupon.Code}' (ID: {coupon.Couponid}).",
                "Information",
                cancellationToken: cancellationToken);
        }

        return Result.Success();
    }
}
