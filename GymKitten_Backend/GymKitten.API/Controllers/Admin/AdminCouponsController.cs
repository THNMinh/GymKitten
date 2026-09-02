using GymKitten.API.Extensions;
using GymKitten.API.Requests;
using GymKitten.Application.Features.Admin.Coupons.Commands.CreateCoupon;
using GymKitten.Application.Features.Admin.Coupons.Commands.DeleteCoupon;
using GymKitten.Application.Features.Admin.Coupons.Commands.UpdateCoupon;
using GymKitten.Application.Features.Admin.Coupons.Queries.GetAdminCoupons;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymKitten.API.Controllers.Admin;

[ApiController]
[Route("api/admin/coupons")]
[Authorize(Roles = "Admin,admin")]
public class AdminCouponsController : ControllerBase
{
    private readonly ISender _sender;

    public AdminCouponsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IResult> GetAdminCoupons(
        [FromQuery] string? code,
        [FromQuery] string? discountType,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAdminCouponsQuery(code, discountType, isActive, page, pageSize);
        var result = await _sender.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpPost]
    public async Task<IResult> CreateCoupon(
        [FromBody] CreateCouponRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateCouponCommand(
            request.Code,
            request.DiscountType,
            request.DiscountValue,
            request.MinOrderValue,
            request.MaxDiscountAmount,
            request.UsageLimit,
            request.StartDate,
            request.EndDate,
            request.IsActive);

        var result = await _sender.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpPut("{id:guid}")]
    public async Task<IResult> UpdateCoupon(
        Guid id,
        [FromBody] UpdateCouponRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateCouponCommand(
            id,
            request.Code,
            request.DiscountType,
            request.DiscountValue,
            request.MinOrderValue,
            request.MaxDiscountAmount,
            request.UsageLimit,
            request.StartDate,
            request.EndDate,
            request.IsActive);

        var result = await _sender.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IResult> DeleteCoupon(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new DeleteCouponCommand(id), cancellationToken);
        return result.MatchOk();
    }
}
