using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymKitten.Infrastructure.Repositories;

public sealed class NotificationRepository : INotificationRepository
{
    private readonly GymkittenContext _context;

    public NotificationRepository(GymkittenContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<Notification> Notifications, int TotalCount)> SearchNotificationsByUserIdAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Notifications
            .AsNoTracking()
            .Where(n => n.Userid == userId && n.Deletedat == null)
            .OrderByDescending(n => n.Createdat);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<int> CountUnreadByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Notifications
            .AsNoTracking()
            .CountAsync(n => n.Userid == userId && !n.Isread && n.Deletedat == null, cancellationToken);
    }

    public async Task<Notification?> GetByIdAndUserIdAsync(
        Guid notificationId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Notifications
            .FirstOrDefaultAsync(
                n => n.Notificationid == notificationId && n.Userid == userId && n.Deletedat == null,
                cancellationToken);
    }

    public async Task MarkAllAsReadByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        await _context.Notifications
            .Where(n => n.Userid == userId && !n.Isread && n.Deletedat == null)
            .ExecuteUpdateAsync(s => s
                .SetProperty(n => n.Isread, true)
                .SetProperty(n => n.Readat, now)
                .SetProperty(n => n.Updatedat, now),
                cancellationToken);
    }

    public async Task AddAsync(
        Notification notification,
        CancellationToken cancellationToken = default)
    {
        await _context.Notifications.AddAsync(notification, cancellationToken);
    }

    public void Update(Notification notification)
    {
        _context.Notifications.Update(notification);
    }
}
