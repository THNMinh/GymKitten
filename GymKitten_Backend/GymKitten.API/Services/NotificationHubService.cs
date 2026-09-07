using System.Text.Json;
using GymKitten.API.Hubs;
using GymKitten.Application.Abstractions.Services;
using GymKitten.Infrastructure.Realtime;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace GymKitten.API.Services;

public sealed class NotificationHubService : INotificationHubService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ConnectionMapping _connectionMapping;
    private readonly ILogger<NotificationHubService> _logger;

    public NotificationHubService(
        IHubContext<NotificationHub> hubContext,
        ConnectionMapping connectionMapping,
        ILogger<NotificationHubService> logger)
    {
        _hubContext = hubContext;
        _connectionMapping = connectionMapping;
        _logger = logger;
    }

    public async Task SendNotificationToUserAsync(
        Guid userId,
        object payload,
        CancellationToken cancellationToken = default)
    {
        var targetGroup = $"user_{userId}";

        _logger.LogInformation(
            "🚀 [SignalR Outbound] Event: ReceiveNotification | Target: Group {TargetGroup} | Payload: {Payload}",
            targetGroup, JsonSerializer.Serialize(payload));

        await _hubContext.Clients
            .Group(targetGroup)
            .SendAsync("ReceiveNotification", payload, cancellationToken);
    }
}
