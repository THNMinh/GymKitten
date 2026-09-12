namespace GymKitten.Application.Features.Auth.VerifyEmail;

public sealed record VerifyEmailResponse(string AccessToken, string RefreshToken);
