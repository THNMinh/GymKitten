using GymKitten.API.Extensions;
using GymKitten.Application.Features.Admin.Orders.Commands.ShipOrder;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GymKitten.API.Controllers.Admin;

[ApiController]
[Route("api/admin/orders")]
public class AdminOrdersController : ControllerBase
{
    private readonly ISender _sender;

    public AdminOrdersController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPut("{orderId:guid}/ship")]
    public async Task<IResult> ShipOrder(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ShipOrderCommand(orderId), cancellationToken);
        return result.MatchOk();
    }
}
