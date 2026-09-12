using GymKitten.Application.Abstractions.Auth;
using GymKitten.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GymKitten.Infrastructure.EventHandlers;

public sealed class ResendOtpDomainEventHandler : INotificationHandler<ResendOtpDomainEvent>
{
    private readonly IEmailJobService _emailJobService;
    private readonly ILogger<ResendOtpDomainEventHandler> _logger;

    public ResendOtpDomainEventHandler(
        IEmailJobService emailJobService,
        ILogger<ResendOtpDomainEventHandler> logger)
    {
        _emailJobService = emailJobService;
        _logger = logger;
    }

    public Task Handle(ResendOtpDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling ResendOtpDomainEvent for user {UserId} ({Email})", notification.UserId, notification.Email);
        _emailJobService.EnqueueSendOtpEmail(notification.Email, notification.OtpCode);
        return Task.CompletedTask;
    }
}
