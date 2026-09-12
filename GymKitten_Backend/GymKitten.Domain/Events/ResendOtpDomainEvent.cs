using GymKitten.Domain.Common;

namespace GymKitten.Domain.Events;

public sealed record ResendOtpDomainEvent(
    Guid UserId,
    string Email,
    string OtpCode) : IDomainEvent;
