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

public sealed record VerifyEmailRequest(
    string Email,
    string OtpCode);

public sealed record ResendOtpRequest(
    string Email);

public sealed record ForgotPasswordRequest(
    string Email);

public sealed record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword);

