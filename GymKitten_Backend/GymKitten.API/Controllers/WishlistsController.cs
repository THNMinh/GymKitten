using GymKitten.API.Extensions;
using GymKitten.API.Requests;
using GymKitten.Application.Features.SocialProof.Wishlists.Commands.RemoveWishlist;
using GymKitten.Application.Features.SocialProof.Wishlists.Commands.ToggleWishlist;
using GymKitten.Application.Features.SocialProof.Wishlists.Queries.GetMyWishlist;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymKitten.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class WishlistsController : ControllerBase
{
    private readonly ISender _sender;

    public WishlistsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("toggle")]
    public async Task<IResult> ToggleWishlist(
        [FromBody] ToggleWishlistRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ToggleWishlistCommand(request.ProductId);
        var result = await _sender.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpDelete("products/{productId:guid}")]
    public async Task<IResult> RemoveWishlist(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new RemoveWishlistCommand(productId), cancellationToken);
        return result.MatchOk();
    }

    [HttpGet]
    public async Task<IResult> GetMyWishlist(
        [FromQuery] GetMyWishlistQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(query, cancellationToken);
        return result.MatchOk();
    }
}
