using GymKitten.API.Extensions;
using GymKitten.Application.Features.Order.Commands.CancelMyOrder;
using GymKitten.Application.Features.Order.Queries.GetMyOrders;
using GymKitten.Application.Features.Order.Queries.GetOrderById;
using GymKitten.Application.Features.Order.Queries.GetOrderTracking;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymKitten.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly ISender _sender;

    public OrdersController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("my-orders")]
    public async Task<IResult> GetMyOrders(
        [FromQuery] GetMyOrdersQuery query,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("{orderId:guid}")]
    public async Task<IResult> GetOrderById(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new GetOrderByIdQuery(orderId), cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("{orderId:guid}/tracking")]
    public async Task<IResult> GetOrderTracking(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new GetOrderTrackingQuery(orderId), cancellationToken);
        return result.MatchOk();
    }

    [HttpPut("{orderId:guid}/cancel")]
    public async Task<IResult> CancelMyOrder(
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new CancelMyOrderCommand(orderId), cancellationToken);
        return result.MatchOk();
    }
}
