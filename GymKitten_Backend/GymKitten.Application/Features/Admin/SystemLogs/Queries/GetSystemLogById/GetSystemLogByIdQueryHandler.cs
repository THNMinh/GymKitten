using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Common;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Admin.SystemLogs.Queries.GetSystemLogById;

public sealed class GetSystemLogByIdQueryHandler
    : IQueryHandler<GetSystemLogByIdQuery, Result<SystemLogDetailDto>>
{
    private readonly IUserContext _userContext;
    private readonly ISystemLogRepository _systemLogRepository;

    public GetSystemLogByIdQueryHandler(
        IUserContext userContext,
        ISystemLogRepository systemLogRepository)
    {
        _userContext = userContext;
        _systemLogRepository = systemLogRepository;
    }

    public async Task<Result<SystemLogDetailDto>> Handle(
        GetSystemLogByIdQuery request,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(_userContext.Role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Failure<SystemLogDetailDto>(SystemLogErrors.Forbidden);
        }

        var (log, userEmail) = await _systemLogRepository.GetByIdAsync(request.LogId, cancellationToken);
        if (log is null)
        {
            return Result.Failure<SystemLogDetailDto>(SystemLogErrors.NotFound);
        }

        var dto = new SystemLogDetailDto(
            log.Logid,
            log.Userid,
            userEmail,
            log.Action,
            log.Message,
            log.Loglevel,
            log.Ipaddress,
            log.Useragent,
            log.Createdat,
            log.Updatedat);

        return Result.Success(dto);
    }
}
