using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Admin.SystemLogs.Queries.GetSystemLogs;

public sealed record GetSystemLogsQuery(
    string? Action = null,
    string? LogLevel = null,
    Guid? UserId = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int Page = 1,
    int PageSize = 20) : IQuery<Result<GetSystemLogsResponse>>;
