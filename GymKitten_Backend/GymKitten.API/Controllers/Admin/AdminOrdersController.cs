using GymKitten.API.Extensions;
using GymKitten.API.Requests;
using GymKitten.Application.Features.Admin.Orders.Commands.ShipOrder;
using GymKitten.Application.Features.Admin.Orders.Commands.UpdateOrderStatus;
using GymKitten.Application.Features.Admin.Orders.Queries.GetAdminOrders;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymKitten.API.Controllers.Admin;

[ApiController]
[Authorize(Roles = "Admin,admin")]
[Route("api/admin/orders")]
public class AdminOrdersController : ControllerBase
{
    private readonly ISender _sender;

    public AdminOrdersController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IResult> GetAdminOrders(
        [FromQuery] GetAdminOrdersQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpPut("{orderId:guid}/status")]
    public async Task<IResult> UpdateOrderStatus(
        Guid orderId,
        [FromBody] UpdateOrderStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateOrderStatusCommand(
            orderId,
            request.Status,
            request.Title,
            request.Description,
            request.Location);

        var result = await _sender.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpPut("{orderId:guid}/ship")]
    public async Task<IResult> ShipOrder(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateOrderStatusCommand(
            orderId,
            "Shipped",
            "Order Shipped",
            "Package has been dispatched for delivery.",
            "Central Distribution Hub");

        var result = await _sender.Send(command, cancellationToken);
        return result.MatchOk();
    }
}
