using GymKitten.Domain.Common;

namespace GymKitten.Domain.Events;

public sealed record CustomerRegisteredDomainEvent(
    Guid UserId,
    string Email,
    string OtpCode) : IDomainEvent;
