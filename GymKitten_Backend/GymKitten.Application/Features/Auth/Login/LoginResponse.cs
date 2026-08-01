namespace GymKitten.Application.Features.Auth.Login;

public sealed record LoginResponse(string AccessToken, string RefreshToken);
