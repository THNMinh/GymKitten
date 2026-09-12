using GymKitten.Domain.Common;

namespace GymKitten.Domain.Events;

public sealed record ForgotPasswordDomainEvent(
    string Email,
    string NewPassword) : IDomainEvent;
