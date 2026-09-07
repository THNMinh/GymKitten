using GymKitten.Application.Abstractions.Messaging;
using GymKitten.Domain.Common;

namespace GymKitten.Application.Features.Notifications.Commands.MarkAsRead;

public sealed record MarkNotificationAsReadCommand(Guid NotificationId) : ICommand<Result>;
