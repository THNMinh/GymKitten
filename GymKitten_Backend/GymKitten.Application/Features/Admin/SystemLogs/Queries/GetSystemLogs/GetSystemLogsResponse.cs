namespace GymKitten.Application.Features.Admin.SystemLogs.Queries.GetSystemLogs;

public sealed record SystemLogDto(
    Guid LogId,
    Guid? UserId,
    string Action,
    string Message,
    string LogLevel,
    string? IpAddress,
    string? UserAgent,
    DateTime CreatedAt);

public sealed record GetSystemLogsResponse(
    IEnumerable<SystemLogDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);
