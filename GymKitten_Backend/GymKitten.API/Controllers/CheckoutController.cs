using GymKitten.API.Extensions;
using GymKitten.API.Requests;
using GymKitten.Application.Features.Order.Commands.Checkout;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymKitten.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CheckoutController : ControllerBase
{
    private readonly ISender _sender;

    public CheckoutController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [Authorize]
    public async Task<IResult> Checkout(
        [FromBody] CheckoutRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new CheckoutCommand(
            request.Items.Select(i => new CheckoutItemDto(i.VariantId, i.Quantity)).ToList(),
            request.ShippingAddress,
            request.PaymentMethod,
            request.CustomerNote,
            request.CouponCode);

        var result = await _sender.Send(command, cancellationToken);
        return result.MatchOk();
    }
}
