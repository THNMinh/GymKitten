using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Auth.Register;

public sealed record RegisterCustomerCommand(
    string FullName,
    string Email,
    string Password) : ICommand<Result<RegisterCustomerResponse>>;
