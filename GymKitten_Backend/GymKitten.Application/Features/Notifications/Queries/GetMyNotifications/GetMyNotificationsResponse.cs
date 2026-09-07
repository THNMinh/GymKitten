namespace GymKitten.Application.Features.Notifications.Queries.GetMyNotifications;

public sealed record NotificationDto(
    Guid NotificationId,
    string Title,
    string Content,
    string Type,
    bool IsRead,
    string? TargetUrl,
    DateTime? ReadAt,
    DateTime CreatedAt);

public sealed record GetMyNotificationsResponse(
    IEnumerable<NotificationDto> Notifications,
    int UnreadCount,
    int TotalCount,
    int Page,
    int PageSize);
