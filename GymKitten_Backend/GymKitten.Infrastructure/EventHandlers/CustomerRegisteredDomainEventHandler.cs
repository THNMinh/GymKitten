using GymKitten.Application.Abstractions.Auth;
using GymKitten.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GymKitten.Infrastructure.EventHandlers;

public sealed class CustomerRegisteredDomainEventHandler
    : INotificationHandler<CustomerRegisteredDomainEvent>
{
    private readonly IEmailJobService _emailJobService;
    private readonly ILogger<CustomerRegisteredDomainEventHandler> _logger;

    public CustomerRegisteredDomainEventHandler(
        IEmailJobService emailJobService,
        ILogger<CustomerRegisteredDomainEventHandler> logger)
    {
        _emailJobService = emailJobService;
        _logger = logger;
    }

    public Task Handle(
        CustomerRegisteredDomainEvent notification,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling CustomerRegisteredDomainEvent for user {UserId} ({Email})",
            notification.UserId, notification.Email);

        _emailJobService.EnqueueSendOtpEmail(notification.Email, notification.OtpCode);

        return Task.CompletedTask;
    }
}
