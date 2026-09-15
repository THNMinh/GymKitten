namespace GymKitten.API.Requests;

public sealed record CreateAdminUserRequest(
    string Email,
    string Password,
    string? FullName,
    string? Phone,
    string Role = "Customer",
    bool IsActive = true);

public sealed record UpdateAdminUserRequest(
    string? FullName,
    string? Phone,
    string? Role,
    bool? IsActive,
    bool? IsEmailVerified);
