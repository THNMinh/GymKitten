using GymKitten.API.Extensions;
using GymKitten.API.Requests;
using GymKitten.Application.Features.Promotions.Coupons.Commands.ApplyCoupon;
using GymKitten.Application.Features.Promotions.Coupons.Queries.GetActiveCoupons;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymKitten.API.Controllers;

[ApiController]
[Route("api/coupons")]
public class CouponsController : ControllerBase
{
    private readonly ISender _sender;

    public CouponsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("active")]
    public async Task<IResult> GetActiveCoupons(CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new GetActiveCouponsQuery(), cancellationToken);
        return result.MatchOk();
    }

    [HttpPost("apply")]
    [Authorize]
    public async Task<IResult> ApplyCoupon(
        [FromBody] ApplyCouponRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new ApplyCouponCommand(request.Code, request.OrderSubtotal);
        var result = await _sender.Send(command, cancellationToken);
        return result.MatchOk();
    }
}
