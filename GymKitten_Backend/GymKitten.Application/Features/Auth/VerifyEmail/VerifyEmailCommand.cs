using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Auth.VerifyEmail;

public sealed record VerifyEmailCommand(string Email, string OtpCode) : ICommand<Result<VerifyEmailResponse>>;
