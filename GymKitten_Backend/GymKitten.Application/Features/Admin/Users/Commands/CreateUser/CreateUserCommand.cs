using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Features.Admin.Users.DTOs;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Admin.Users.Commands.CreateUser;

public sealed record CreateUserCommand(
    string Email,
    string Password,
    string? FullName,
    string? Phone,
    string Role = "Customer",
    bool IsActive = true) : ICommand<Result<AdminUserItemDto>>;
