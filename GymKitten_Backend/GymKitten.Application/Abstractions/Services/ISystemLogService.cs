namespace GymKitten.Application.Abstractions.Services;

public interface ISystemLogService
{
    Task LogAsync(
        string action,
        string message,
        string logLevel = "Information",
        Guid? userId = null,
        CancellationToken cancellationToken = default);
}
