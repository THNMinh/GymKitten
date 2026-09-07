using GymKitten.Domain.Common;

namespace GymKitten.Domain.Events;

public sealed record OrderStatusChangedDomainEvent(
    Guid OrderId,
    Guid UserId,
    string OrderCode,
    string NewStatus,
    string StatusDescription
) : IDomainEvent;
