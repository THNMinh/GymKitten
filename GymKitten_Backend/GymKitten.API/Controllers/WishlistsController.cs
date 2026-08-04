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
    public async Task<IActionResult> ToggleWishlist(
        [FromBody] ToggleWishlistCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Toggle Wishlist Failed",
                Detail = result.Error.Message,
                Extensions = { { "code", result.Error.Code } }
            });
        }

        return Ok(result.Value);
    }

    [HttpDelete("products/{productId:guid}")]
    public async Task<IActionResult> RemoveWishlist(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var command = new RemoveWishlistCommand(productId);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Remove Wishlist Failed",
                Detail = result.Error.Message,
                Extensions = { { "code", result.Error.Code } }
            });
        }

        return NoContent();
    }

    [HttpGet]
    public async Task<IActionResult> GetMyWishlist(
        [FromQuery] GetMyWishlistQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(query, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Get Wishlist Failed",
                Detail = result.Error.Message,
                Extensions = { { "code", result.Error.Code } }
            });
        }

        return Ok(result.Value);
    }
}
