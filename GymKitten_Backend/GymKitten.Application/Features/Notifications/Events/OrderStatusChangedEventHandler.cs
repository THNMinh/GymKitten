using GymKitten.Application.Abstractions.Data;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Application.Abstractions.Services;
using GymKitten.Domain.Entities;
using GymKitten.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GymKitten.Application.Features.Notifications.Events;

public sealed class OrderStatusChangedEventHandler : INotificationHandler<OrderStatusChangedDomainEvent>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly INotificationHubService _notificationHubService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OrderStatusChangedEventHandler> _logger;

    public OrderStatusChangedEventHandler(
        INotificationRepository notificationRepository,
        INotificationHubService notificationHubService,
        IUnitOfWork unitOfWork,
        ILogger<OrderStatusChangedEventHandler> logger)
    {
        _notificationRepository = notificationRepository;
        _notificationHubService = notificationHubService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(
        OrderStatusChangedDomainEvent notificationEvent,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Handling OrderStatusChangedDomainEvent for OrderId: {OrderId}, UserId: {UserId}, Status: {Status}",
            notificationEvent.OrderId, notificationEvent.UserId, notificationEvent.NewStatus);

        // 1. Save Notification record to Database
        var notificationEntity = new Notification
        {
            Notificationid = Guid.NewGuid(),
            Userid = notificationEvent.UserId,
            Title = $"Cập nhật đơn hàng #{notificationEvent.OrderCode}",
            Content = notificationEvent.StatusDescription,
            Type = "Order",
            Isread = false,
            Targeturl = $"/orders/{notificationEvent.OrderId}",
            Createdat = DateTime.UtcNow,
            Updatedat = DateTime.UtcNow
        };

        await _notificationRepository.AddAsync(notificationEntity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 2. Dispatch real-time notification to SignalR Hub
        var payload = new
        {
            NotificationId = notificationEntity.Notificationid,
            Title = notificationEntity.Title,
            Content = notificationEntity.Content,
            Type = notificationEntity.Type,
            TargetUrl = notificationEntity.Targeturl,
            CreatedAt = notificationEntity.Createdat,
            IsRead = false,
            OrderId = notificationEvent.OrderId,
            OrderCode = notificationEvent.OrderCode,
            NewStatus = notificationEvent.NewStatus.ToString()
        };

        await _notificationHubService.SendNotificationToUserAsync(
            notificationEvent.UserId,
            payload,
            cancellationToken);
    }
}
