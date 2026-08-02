namespace GymKitten.Application.Abstractions.Auth;

public interface IRedisOtpStore
{
    /// <summary>
    /// Stores an OTP code associated with a user, with the given expiration TTL.
    /// </summary>
    Task StoreAsync(Guid userId, string otpCode, TimeSpan expiration, CancellationToken ct = default);
}
