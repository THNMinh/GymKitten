using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Admin.SystemLogs.Queries.GetSystemLogById;

public sealed record GetSystemLogByIdQuery(Guid LogId) : IQuery<Result<SystemLogDetailDto>>;

public sealed record SystemLogDetailDto(
    Guid LogId,
    Guid? UserId,
    string? UserEmail,
    string Action,
    string Message,
    string LogLevel,
    string? IpAddress,
    string? UserAgent,
    DateTime CreatedAt,
    DateTime UpdatedAt);
