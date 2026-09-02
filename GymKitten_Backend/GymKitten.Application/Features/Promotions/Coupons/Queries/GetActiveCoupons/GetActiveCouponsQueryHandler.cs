using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Promotions.Coupons.Queries.GetActiveCoupons;

public sealed class GetActiveCouponsQueryHandler
    : IQueryHandler<GetActiveCouponsQuery, Result<GetActiveCouponsResponse>>
{
    private readonly ICouponRepository _couponRepository;

    public GetActiveCouponsQueryHandler(ICouponRepository couponRepository)
    {
        _couponRepository = couponRepository;
    }

    public async Task<Result<GetActiveCouponsResponse>> Handle(
        GetActiveCouponsQuery request,
        CancellationToken cancellationToken)
    {
        var activeCoupons = await _couponRepository.GetActiveCouponsAsync(DateTime.UtcNow, cancellationToken);

        var dtos = activeCoupons.Select(c => new ActiveCouponDto(
            c.Couponid,
            c.Code,
            c.Discounttype,
            c.Discountvalue,
            c.Minordervalue,
            c.Maxdiscountamount,
            c.Usagelimit,
            c.Usedcount,
            c.Startdate,
            c.Enddate
        )).ToList();

        return Result.Success(new GetActiveCouponsResponse(dtos));
    }
}
