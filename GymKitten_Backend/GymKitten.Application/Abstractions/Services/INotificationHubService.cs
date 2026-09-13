namespace GymKitten.Application.Abstractions.Services;

public interface INotificationHubService
{
    Task SendNotificationToUserAsync(Guid userId, object payload, CancellationToken cancellationToken = default);
    Task SendNotificationToAdminsAsync(object payload, CancellationToken cancellationToken = default);
    Task BroadcastNotificationAsync(object payload, CancellationToken cancellationToken = default);
}
