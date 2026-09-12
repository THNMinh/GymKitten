using GymKitten.Domain.Entities;

namespace GymKitten.Application.Abstractions.Repositories;

public interface ISystemLogRepository
{
    Task AddAsync(Systemlog log, CancellationToken cancellationToken = default);

    Task<(IEnumerable<Systemlog> Logs, int TotalCount)> SearchSystemLogsAsync(
        string? action,
        string? logLevel,
        Guid? userId,
        DateTime? fromDate,
        DateTime? toDate,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<(Systemlog? Log, string? UserEmail)> GetByIdAsync(
        Guid logId,
        CancellationToken cancellationToken = default);
}
