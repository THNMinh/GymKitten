using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymKitten.Infrastructure.Repositories;

public sealed class SystemLogRepository : ISystemLogRepository
{
    private readonly GymkittenContext _context;

    public SystemLogRepository(GymkittenContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Systemlog log, CancellationToken cancellationToken = default)
    {
        await _context.Systemlogs.AddAsync(log, cancellationToken);
    }

    public async Task<(IEnumerable<Systemlog> Logs, int TotalCount)> SearchSystemLogsAsync(
        string? action,
        string? logLevel,
        Guid? userId,
        DateTime? fromDate,
        DateTime? toDate,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Systemlogs
            .AsNoTracking()
            .Where(l => l.Deletedat == null);

        if (!string.IsNullOrWhiteSpace(action))
        {
            var actionClean = action.Trim();
            query = query.Where(l => EF.Functions.ILike(l.Action, $"%{actionClean}%"));
        }

        if (!string.IsNullOrWhiteSpace(logLevel))
        {
            var levelClean = logLevel.Trim();
            query = query.Where(l => EF.Functions.ILike(l.Loglevel, levelClean));
        }

        if (userId.HasValue)
        {
            query = query.Where(l => l.Userid == userId.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(l => l.Createdat >= fromDate.Value.ToUniversalTime());
        }

        if (toDate.HasValue)
        {
            query = query.Where(l => l.Createdat <= toDate.Value.ToUniversalTime());
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(l => l.Createdat)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(Systemlog? Log, string? UserEmail)> GetByIdAsync(
        Guid logId,
        CancellationToken cancellationToken = default)
    {
        var log = await _context.Systemlogs
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Logid == logId && l.Deletedat == null, cancellationToken);

        if (log is null)
        {
            return (null, null);
        }

        string? userEmail = null;
        if (log.Userid.HasValue)
        {
            userEmail = await _context.Users
                .AsNoTracking()
                .Where(u => u.Userid == log.Userid.Value)
                .Select(u => u.Email)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return (log, userEmail);
    }
}
