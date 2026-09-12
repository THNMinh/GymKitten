using GymKitten.Application.Abstractions.Auth;
using GymKitten.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GymKitten.Infrastructure.EventHandlers;

public sealed class ForgotPasswordDomainEventHandler : INotificationHandler<ForgotPasswordDomainEvent>
{
    private readonly IEmailJobService _emailJobService;
    private readonly ILogger<ForgotPasswordDomainEventHandler> _logger;

    public ForgotPasswordDomainEventHandler(
        IEmailJobService emailJobService,
        ILogger<ForgotPasswordDomainEventHandler> logger)
    {
        _emailJobService = emailJobService;
        _logger = logger;
    }

    public Task Handle(ForgotPasswordDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling ForgotPasswordDomainEvent for email {Email}", notification.Email);
        _emailJobService.EnqueueSendResetPasswordEmail(notification.Email, notification.NewPassword);
        return Task.CompletedTask;
    }
}
