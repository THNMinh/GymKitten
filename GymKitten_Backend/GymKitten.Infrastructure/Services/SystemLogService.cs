using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Data;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Application.Abstractions.Services;
using GymKitten.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace GymKitten.Infrastructure.Services;

public sealed class SystemLogService : ISystemLogService
{
    private readonly ISystemLogRepository _systemLogRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUserContext _userContext;
    private readonly ILogger<SystemLogService> _logger;

    public SystemLogService(
        ISystemLogRepository systemLogRepository,
        IUnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor,
        IUserContext userContext,
        ILogger<SystemLogService> logger)
    {
        _systemLogRepository = systemLogRepository;
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
        _userContext = userContext;
        _logger = logger;
    }

    public async Task LogAsync(
        string action,
        string message,
        string logLevel = "Information",
        Guid? userId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var ipAddress = httpContext?.Request?.Headers["X-Forwarded-For"].FirstOrDefault()
                ?? httpContext?.Connection?.RemoteIpAddress?.ToString();

            var userAgent = httpContext?.Request?.Headers["User-Agent"].ToString();
            var resolvedUserId = userId ?? _userContext.UserId;

            var log = new Systemlog
            {
                Logid = Guid.NewGuid(),
                Userid = resolvedUserId,
                Loglevel = logLevel,
                Action = action.Length > 100 ? action[..100] : action,
                Message = message,
                Ipaddress = ipAddress != null && ipAddress.Length > 50 ? ipAddress[..50] : ipAddress,
                Useragent = userAgent,
                Createdat = DateTime.UtcNow,
                Updatedat = DateTime.UtcNow
            };

            await _systemLogRepository.AddAsync(log, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write system audit log for action: {Action}", action);
        }
    }
}
