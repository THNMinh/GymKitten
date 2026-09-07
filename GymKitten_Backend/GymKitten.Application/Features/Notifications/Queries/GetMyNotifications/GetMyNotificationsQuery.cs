using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Notifications.Queries.GetMyNotifications;

public sealed record GetMyNotificationsQuery(int Page = 1, int PageSize = 10)
    : IQuery<Result<GetMyNotificationsResponse>>;
