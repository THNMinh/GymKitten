using GymKitten.API.Extensions;
using GymKitten.API.Requests;
using GymKitten.Application.Features.Auth.Login;
using GymKitten.Application.Features.Auth.RefreshToken;
using GymKitten.Application.Features.Auth.Register;
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

    [HttpPost("register")]
    public async Task<IResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RegisterCustomerCommand(
            request.FullName,
            request.Email,
            request.Password);

        var result = await _sender.Send(command, cancellationToken);
        // return result.MatchCreated(res => $"/api/users/{res.UserId}");
        return result.MatchOk();
    }

    [HttpPost("login")]
    public async Task<IResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var command = new LoginCommand(request.Email, request.Password);
        var result = await _sender.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpPost("refresh-token")]
    public async Task<IResult> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RefreshTokenCommand(request.AccessToken, request.RefreshToken);
        var result = await _sender.Send(command, cancellationToken);
        return result.MatchOk();
    }
}
