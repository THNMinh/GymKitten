using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Auth.Login;

public sealed record LoginCommand(string Email, string Password)
    : ICommand<Result<LoginResponse>>;
