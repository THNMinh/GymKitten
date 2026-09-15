using System;

namespace GymKitten.Application.Features.Admin.Users.DTOs;

public record AdminUserItemDto(
    Guid UserId,
    string Email,
    string? FullName,
    string? Phone,
    string Role,
    bool IsEmailVerified,
    bool IsActive,
    string? AvatarUrl,
    DateTime CreatedAt,
    DateTime UpdatedAt);
