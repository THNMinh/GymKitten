using GymKitten.API.Extensions;
using GymKitten.API.Requests;
using GymKitten.Application.Features.Cart.Commands.AddToCart;
using GymKitten.Application.Features.Cart.Commands.ClearCart;
using GymKitten.Application.Features.Cart.Commands.RemoveCartItem;
using GymKitten.Application.Features.Cart.Commands.UpdateCartItem;
using GymKitten.Application.Features.Cart.Queries.GetCart;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymKitten.API.Controllers;

[ApiController]
[Authorize]
[Route("api/cart")]
public class CartController : ControllerBase
{
    private readonly ISender _sender;

    public CartController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IResult> GetCart(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetCartQuery(), cancellationToken);
        return result.MatchOk();
    }

    [HttpPost("items")]
    public async Task<IResult> AddToCart(
        [FromBody] AddToCartRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AddToCartCommand(request.VariantId, request.Quantity);
        var result = await _sender.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpPut("items/{variantId:guid}")]
    public async Task<IResult> UpdateCartItem(
        Guid variantId,
        [FromBody] UpdateCartItemRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCartItemCommand(variantId, request.Quantity);
        var result = await _sender.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpDelete("items/{variantId:guid}")]
    public async Task<IResult> RemoveCartItem(
        Guid variantId,
        CancellationToken cancellationToken)
    {
        var command = new RemoveCartItemCommand(variantId);
        var result = await _sender.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpDelete]
    public async Task<IResult> ClearCart(CancellationToken cancellationToken)
    {
        var command = new ClearCartCommand();
        var result = await _sender.Send(command, cancellationToken);
        return result.MatchOk();
    }
}
