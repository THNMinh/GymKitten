using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Features.Auth.Login;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Auth.RefreshToken;

public sealed record RefreshTokenCommand(string AccessToken, string RefreshToken)
    : ICommand<Result<LoginResponse>>;
