using GymKitten.Application.Abstractions.Auth;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace GymKitten.Infrastructure.Auth;

public sealed class RedisOtpStore : IRedisOtpStore
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisOtpStore> _logger;
    private const string Prefix = "otp:";

    public RedisOtpStore(IConnectionMultiplexer redis, ILogger<RedisOtpStore> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task StoreAsync(Guid userId, string otpCode, TimeSpan expiration, CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        var key = $"{Prefix}{userId}";
        await db.StringSetAsync(key, otpCode, expiration);
        _logger.LogInformation("[REDIS OTP] Stored OTP for user {UserId} with key {Key} (TTL: {Expiration})", userId, key, expiration);
    }

    public async Task<string?> GetAsync(Guid userId, CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        var key = $"{Prefix}{userId}";
        var value = await db.StringGetAsync(key);
        return value.IsNullOrEmpty ? null : value.ToString();
    }

    public async Task RemoveAsync(Guid userId, CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        var key = $"{Prefix}{userId}";
        await db.KeyDeleteAsync(key);
        _logger.LogInformation("[REDIS OTP] Removed OTP key {Key} for user {UserId}", key, userId);
    }
}
