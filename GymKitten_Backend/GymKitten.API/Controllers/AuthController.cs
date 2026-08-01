using GymKitten.Application.Features.Auth.Login;
using GymKitten.Application.Features.Auth.RefreshToken;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GymKitten.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Authentication Failed",
                Detail = result.Error.Message,
                Type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                Extensions = { { "code", result.Error.Code } }
            });
        }

        return Ok(result.Value);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken(
        [FromBody] RefreshTokenCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Token Refresh Failed",
                Detail = result.Error.Message,
                Type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                Extensions = { { "code", result.Error.Code } }
            });
        }

        return Ok(result.Value);
    }
}
