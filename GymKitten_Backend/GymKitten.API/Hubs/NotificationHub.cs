using System.Security.Claims;
using GymKitten.Infrastructure.Realtime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace GymKitten.API.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    private readonly ConnectionMapping _connectionMapping;
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(
        ConnectionMapping connectionMapping,
        ILogger<NotificationHub> logger)
    {
        _connectionMapping = connectionMapping;
        _logger = logger;
    }

    private Guid CurrentUserId
    {
        get
        {
            var claim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? Context.User?.FindFirst("userId")?.Value
                ?? Context.User?.FindFirst("sub")?.Value;

            if (Guid.TryParse(claim, out var userId))
            {
                return userId;
            }

            throw new HubException("Unauthorized: Valid user identifier claim not found");
        }
    }

    public override async Task OnConnectedAsync()
    {
        var userId = CurrentUserId;
        _connectionMapping.Add(userId, Context.ConnectionId);

        await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");

        _logger.LogInformation(
            "📥 [SignalR Connected] UserId: {UserId} | ConnectionId: {ConnectionId}",
            userId, Context.ConnectionId);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            var userId = CurrentUserId;
            _connectionMapping.Remove(userId, Context.ConnectionId);

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");

            _logger.LogInformation(
                "📥 [SignalR Disconnected] UserId: {UserId} | ConnectionId: {ConnectionId}",
                userId, Context.ConnectionId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error while cleaning up connection in OnDisconnectedAsync");
        }

        await base.OnDisconnectedAsync(exception);
    }
}
