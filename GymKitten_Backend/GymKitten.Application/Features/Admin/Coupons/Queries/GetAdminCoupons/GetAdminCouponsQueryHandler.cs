using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Common;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Admin.Coupons.Queries.GetAdminCoupons;

public sealed class GetAdminCouponsQueryHandler
    : IQueryHandler<GetAdminCouponsQuery, Result<GetAdminCouponsResponse>>
{
    private readonly IUserContext _userContext;
    private readonly ICouponRepository _couponRepository;

    public GetAdminCouponsQueryHandler(
        IUserContext userContext,
        ICouponRepository couponRepository)
    {
        _userContext = userContext;
        _couponRepository = couponRepository;
    }

    public async Task<Result<GetAdminCouponsResponse>> Handle(
        GetAdminCouponsQuery request,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(_userContext.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<GetAdminCouponsResponse>(UserErrors.Forbidden);
        }

        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

        var (items, totalCount) = await _couponRepository.SearchAdminCouponsAsync(
            request.Code,
            request.DiscountType,
            request.IsActive,
            page,
            pageSize,
            cancellationToken);

        var dtos = items.Select(c => new AdminCouponDto(
            c.Couponid,
            c.Code,
            c.Discounttype,
            c.Discountvalue,
            c.Minordervalue,
            c.Maxdiscountamount,
            c.Usagelimit,
            c.Usedcount,
            c.Startdate,
            c.Enddate,
            c.Isactive,
            c.Createdat,
            c.Updatedat
        )).ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var response = new GetAdminCouponsResponse(
            dtos,
            totalCount,
            page,
            pageSize,
            totalPages);

        return Result.Success(response);
    }
}
