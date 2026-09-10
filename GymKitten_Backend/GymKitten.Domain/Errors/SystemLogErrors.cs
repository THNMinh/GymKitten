using GymKitten.Domain.Common;

namespace GymKitten.Domain.Errors;

public static class SystemLogErrors
{
    public static readonly Error NotFound = new(
        "SystemLog.NotFound",
        "The requested system log entry was not found.");

    public static readonly Error Forbidden = new(
        "SystemLog.Forbidden",
        "You do not have permission to access system audit logs.");
}
