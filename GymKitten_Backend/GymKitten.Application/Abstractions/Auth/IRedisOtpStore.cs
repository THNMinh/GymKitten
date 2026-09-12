namespace GymKitten.Application.Abstractions.Auth;

public interface IRedisOtpStore
{
    /// <summary>
    /// Stores an OTP code associated with a user, with the given expiration TTL.
    /// </summary>
    Task StoreAsync(Guid userId, string otpCode, TimeSpan expiration, CancellationToken ct = default);

    /// <summary>
    /// Retrieves the stored OTP code associated with the user, or null if expired/not found.
    /// </summary>
    Task<string?> GetAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Removes the OTP code from the store immediately (preventing replay attacks).
    /// </summary>
    Task RemoveAsync(Guid userId, CancellationToken ct = default);
}

