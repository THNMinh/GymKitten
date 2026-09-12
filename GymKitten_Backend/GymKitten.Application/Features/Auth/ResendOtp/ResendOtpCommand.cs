using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Auth.ResendOtp;

public sealed record ResendOtpCommand(string Email) : ICommand<Result>;
