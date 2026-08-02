namespace GymKitten.Application.Features.Auth.Register;

public sealed record RegisterCustomerResponse(
    Guid UserId,
    string Email,
    string FullName,
    string Role,
    DateTime CreatedAt);
