using GymKitten.Domain.Entities;

namespace GymKitten.Application.Abstractions.Auth;

public interface IJwtProvider
{
    /// <summary>
    /// Generates a JWT access token for the given user.
    /// Returns the token string and the JwtId (jti claim) for refresh token binding.
    /// </summary>
    (string Token, string JwtId) GenerateAccessToken(User user);

    /// <summary>
    /// Generates a cryptographically secure random refresh token string.
    /// </summary>
    string GenerateRefreshToken();
}
