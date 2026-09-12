using System.Collections.Concurrent;
using GymKitten.Application.Abstractions.Auth;
using Microsoft.Extensions.Logging;

namespace GymKitten.Infrastructure.Auth;

/// <summary>
/// In-memory stub for IRedisOtpStore. Replace with a real Redis implementation later.
/// OTP codes are stored in a ConcurrentDictionary and logged for dev/testing purposes.
/// </summary>
public sealed class InMemoryOtpStore : IRedisOtpStore
{
    private static readonly ConcurrentDictionary<string, string> _store = new();
    private readonly ILogger<InMemoryOtpStore> _logger;

    public InMemoryOtpStore(ILogger<InMemoryOtpStore> logger)
    {
        _logger = logger;
    }

    public Task StoreAsync(Guid userId, string otpCode, TimeSpan expiration, CancellationToken ct = default)
    {
        var key = $"otp:{userId}";
        _store[key] = otpCode;

        _logger.LogInformation(
            "[STUB OTP STORE] Stored OTP for user {UserId}: {OtpCode} (TTL: {Expiration})",
            userId, otpCode, expiration);

        return Task.CompletedTask;
    }

    public Task<string?> GetAsync(Guid userId, CancellationToken ct = default)
    {
        var key = $"otp:{userId}";
        _store.TryGetValue(key, out var otpCode);
        return Task.FromResult(otpCode);
    }

    public Task RemoveAsync(Guid userId, CancellationToken ct = default)
    {
        var key = $"otp:{userId}";
        _store.TryRemove(key, out _);
        return Task.CompletedTask;
    }
}
