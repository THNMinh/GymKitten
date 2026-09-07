using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Data;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Common;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Notifications.Commands.MarkAsRead;

public sealed class MarkNotificationAsReadCommandHandler
    : ICommandHandler<MarkNotificationAsReadCommand, Result>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;

    public MarkNotificationAsReadCommandHandler(
        INotificationRepository notificationRepository,
        IUserContext userContext,
        IUnitOfWork unitOfWork)
    {
        _notificationRepository = notificationRepository;
        _userContext = userContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        MarkNotificationAsReadCommand request,
        CancellationToken cancellationToken)
    {
        if (!_userContext.UserId.HasValue)
        {
            return Result.Failure(UserErrors.Unauthorized);
        }

        var notification = await _notificationRepository.GetByIdAndUserIdAsync(
            request.NotificationId,
            _userContext.UserId.Value,
            cancellationToken);

        if (notification is null)
        {
            return Result.Failure(NotificationErrors.NotFound);
        }

        if (!notification.Isread)
        {
            notification.Isread = true;
            notification.Readat = DateTime.UtcNow;
            notification.Updatedat = DateTime.UtcNow;

            _notificationRepository.Update(notification);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }
}
