using GymKitten.Domain.Common;

namespace GymKitten.Domain.Errors;

public static class NotificationErrors
{
    public static readonly Error NotFound = new(
        "Notification.NotFound",
        "The requested notification was not found.");

    public static readonly Error Forbidden = new(
        "Notification.Forbidden",
        "You do not have permission to access or modify this notification.");
}
