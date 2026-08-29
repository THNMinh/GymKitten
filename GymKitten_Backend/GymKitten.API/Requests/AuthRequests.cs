namespace GymKitten.API.Requests;

public sealed record RegisterRequest(
    string FullName,
    string Email,
    string Password);

public sealed record LoginRequest(
    string Email,
    string Password);

public sealed record RefreshTokenRequest(
    string AccessToken,
    string RefreshToken);
