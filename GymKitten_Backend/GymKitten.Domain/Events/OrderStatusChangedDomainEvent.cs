using GymKitten.Domain.Common;

namespace GymKitten.Domain.Events;

public sealed record OrderStatusChangedDomainEvent(
    Guid OrderId,
    Guid UserId,
    string OrderCode,
    string NewStatus,
    string StatusDescription,
    Guid? TrackingId = null,
    string? Title = null,
    string? Description = null,
    string? Location = null,
    DateTime? Timestamp = null,
    string? UpdatedBy = null
) : IDomainEvent;
