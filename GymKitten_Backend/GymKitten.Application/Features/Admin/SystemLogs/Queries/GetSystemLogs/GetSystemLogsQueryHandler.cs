using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Common;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Admin.SystemLogs.Queries.GetSystemLogs;

public sealed class GetSystemLogsQueryHandler
    : IQueryHandler<GetSystemLogsQuery, Result<GetSystemLogsResponse>>
{
    private readonly IUserContext _userContext;
    private readonly ISystemLogRepository _systemLogRepository;

    public GetSystemLogsQueryHandler(
        IUserContext userContext,
        ISystemLogRepository systemLogRepository)
    {
        _userContext = userContext;
        _systemLogRepository = systemLogRepository;
    }

    public async Task<Result<GetSystemLogsResponse>> Handle(
        GetSystemLogsQuery request,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(_userContext.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<GetSystemLogsResponse>(SystemLogErrors.Forbidden);
        }

        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 20 : request.PageSize;

        var (logs, totalCount) = await _systemLogRepository.SearchSystemLogsAsync(
            request.Action,
            request.LogLevel,
            request.UserId,
            request.FromDate,
            request.ToDate,
            page,
            pageSize,
            cancellationToken);

        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var dtos = logs.Select(l => new SystemLogDto(
            l.Logid,
            l.Userid,
            l.Action,
            l.Message,
            l.Loglevel,
            l.Ipaddress,
            l.Useragent,
            l.Createdat
        ));

        return Result.Success(new GetSystemLogsResponse(
            dtos,
            totalCount,
            page,
            pageSize,
            totalPages));
    }
}
