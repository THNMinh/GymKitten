using GymKitten.Domain.Entities;

namespace GymKitten.Application.Abstractions.Repositories;

public interface INotificationRepository
{
    Task<(IEnumerable<Notification> Notifications, int TotalCount)> SearchNotificationsByUserIdAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<int> CountUnreadByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<Notification?> GetByIdAndUserIdAsync(
        Guid notificationId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task MarkAllAsReadByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Notification notification,
        CancellationToken cancellationToken = default);

    void Update(Notification notification);
}
