using System;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Features.Admin.Users.DTOs;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Admin.Users.Commands.UpdateUser;

public sealed record UpdateUserCommand(
    Guid UserId,
    string? FullName = null,
    string? Phone = null,
    string? Role = null,
    bool? IsActive = null,
    bool? IsEmailVerified = null) : ICommand<Result<AdminUserItemDto>>;
