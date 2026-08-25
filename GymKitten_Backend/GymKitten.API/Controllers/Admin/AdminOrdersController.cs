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
    public async Task<IActionResult> ShipOrder(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var command = new ShipOrderCommand(orderId);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Ship Order Failed",
                Detail = result.Error.Message,
                Extensions = { { "code", result.Error.Code } }
            });
        }

        return Ok(result.Value);
    }
}
