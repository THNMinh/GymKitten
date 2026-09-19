using GymKitten.API.Extensions;
using GymKitten.API.Requests;
using GymKitten.Application.Features.Auth.ChangePassword;
using GymKitten.Application.Features.Auth.ForgotPassword;
using GymKitten.Application.Features.Auth.Login;
using GymKitten.Application.Features.Auth.RefreshToken;
using GymKitten.Application.Features.Auth.Register;
using GymKitten.Application.Features.Auth.ResendOtp;
using GymKitten.Application.Features.Auth.VerifyEmail;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymKitten.API.Controllers;

[ApiController]
[Route("api/auth")]
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

    [HttpPost("verify-email")]
    public async Task<IResult> VerifyEmail(
        [FromBody] VerifyEmailRequest request,
        CancellationToken cancellationToken)
    {
        var command = new VerifyEmailCommand(request.Email, request.OtpCode);
        var result = await _sender.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpPost("resend-otp")]
    public async Task<IResult> ResendOtp(
        [FromBody] ResendOtpRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ResendOtpCommand(request.Email);
        var result = await _sender.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpPost("forgot-password")]
    public async Task<IResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ForgotPasswordCommand(request.Email);
        var result = await _sender.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ChangePasswordCommand(request.CurrentPassword, request.NewPassword);
        var result = await _sender.Send(command, cancellationToken);
        return result.MatchOk();
    }
}

