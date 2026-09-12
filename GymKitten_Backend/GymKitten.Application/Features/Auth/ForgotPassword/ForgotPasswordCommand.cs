using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Auth.ForgotPassword;

public sealed record ForgotPasswordCommand(string Email) : ICommand<Result>;
