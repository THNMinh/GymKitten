using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Data;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Common;
using GymKitten.Domain.Entities;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Admin.Coupons.Commands.CreateCoupon;

public sealed class CreateCouponCommandHandler
    : ICommandHandler<CreateCouponCommand, Result<CreateCouponResponse>>
{
    private readonly IUserContext _userContext;
    private readonly ICouponRepository _couponRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCouponCommandHandler(
        IUserContext userContext,
        ICouponRepository couponRepository,
        IUnitOfWork unitOfWork)
    {
        _userContext = userContext;
        _couponRepository = couponRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CreateCouponResponse>> Handle(
        CreateCouponCommand request,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(_userContext.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<CreateCouponResponse>(UserErrors.Forbidden);
        }

        if (string.IsNullOrWhiteSpace(request.Code))
        {
            return Result.Failure<CreateCouponResponse>(CouponErrors.NotFound);
        }

        var normalizedCode = request.Code.Trim().ToUpper();
        var codeExists = await _couponRepository.ExistsByCodeAsync(normalizedCode, cancellationToken);
        if (codeExists)
        {
            return Result.Failure<CreateCouponResponse>(CouponErrors.CodeAlreadyExists);
        }

        var discountTypeNorm = request.DiscountType.Trim();
        if (!discountTypeNorm.Equals("Percentage", StringComparison.OrdinalIgnoreCase) &&
            !discountTypeNorm.Equals("FixedAmount", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<CreateCouponResponse>(CouponErrors.InvalidDiscountType);
        }

        var now = DateTime.UtcNow;
        var coupon = new Coupon
        {
            Couponid = Guid.NewGuid(),
            Code = normalizedCode,
            Discounttype = discountTypeNorm.Equals("Percentage", StringComparison.OrdinalIgnoreCase) ? "Percentage" : "FixedAmount",
            Discountvalue = request.DiscountValue,
            Minordervalue = request.MinOrderValue,
            Maxdiscountamount = request.MaxDiscountAmount,
            Usagelimit = request.UsageLimit,
            Usedcount = 0,
            Startdate = request.StartDate,
            Enddate = request.EndDate,
            Isactive = request.IsActive,
            Createdat = now,
            Updatedat = now
        };

        await _couponRepository.AddAsync(coupon, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new CreateCouponResponse(coupon.Couponid));
    }
}
