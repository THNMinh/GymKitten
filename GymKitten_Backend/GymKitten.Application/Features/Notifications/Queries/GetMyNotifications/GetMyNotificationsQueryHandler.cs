using GymKitten.Application.Abstractions.Auth;
using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Application.Abstractions.Repositories;
using GymKitten.Domain.Common;
using GymKitten.Domain.Errors;

namespace GymKitten.Application.Features.Notifications.Queries.GetMyNotifications;

public sealed class GetMyNotificationsQueryHandler
    : IQueryHandler<GetMyNotificationsQuery, Result<GetMyNotificationsResponse>>
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUserContext _userContext;

    public GetMyNotificationsQueryHandler(
        INotificationRepository notificationRepository,
        IUserContext userContext)
    {
        _notificationRepository = notificationRepository;
        _userContext = userContext;
    }

    public async Task<Result<GetMyNotificationsResponse>> Handle(
        GetMyNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_userContext.UserId.HasValue)
        {
            return Result.Failure<GetMyNotificationsResponse>(UserErrors.Unauthorized);
        }

        var userId = _userContext.UserId.Value;
        var page = request.Page <= 0 ? 1 : request.Page;
        var pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

        var (notifications, totalCount) = await _notificationRepository.SearchNotificationsByUserIdAsync(
            userId, page, pageSize, cancellationToken);

        var unreadCount = await _notificationRepository.CountUnreadByUserIdAsync(userId, cancellationToken);

        var dtos = notifications.Select(n => new NotificationDto(
            n.Notificationid,
            n.Title,
            n.Content,
            n.Type,
            n.Isread,
            n.Targeturl,
            n.Readat,
            n.Createdat
        ));

        return Result.Success(new GetMyNotificationsResponse(
            dtos,
            unreadCount,
            totalCount,
            page,
            pageSize));
    }
}
