using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Common;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Notifications.Commands.MarkAllAsRead;

public sealed class MarkAllNotificationsAsReadCommandHandler
    : ICommandHandler<MarkAllNotificationsAsReadCommand, Result>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUserContext _userContext;

    public MarkAllNotificationsAsReadCommandHandler(
        INotificationRepository notificationRepository,
        IUserContext userContext)
    {
        _notificationRepository = notificationRepository;
        _userContext = userContext;
    }

    public async Task<Result> Handle(
        MarkAllNotificationsAsReadCommand request,
        CancellationToken cancellationToken)
    {
        if (!_userContext.UserId.HasValue)
        {
            return Result.Failure(UserErrors.Unauthorized);
        }

        await _notificationRepository.MarkAllAsReadByUserIdAsync(
            _userContext.UserId.Value,
            cancellationToken);

        return Result.Success();
    }
}
