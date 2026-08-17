using Application.Common.Interfaces;
using StackExchange.Redis;

namespace Infrastructure.Services;

/// <summary>
/// Requires StackExchange.Redis. Kept separate from <see cref="RedisCacheService"/>
/// so attempt limits and single-use semantics stay isolated from general cache keys.
/// </summary>
public class RedisVerificationCodeStore(IConnectionMultiplexer redis) : IVerificationCodeStore
{
    private const int MaxAttempts = 5;
    private static readonly TimeSpan AttemptsWindow = TimeSpan.FromMinutes(10);

    private IDatabase Db => redis.GetDatabase();

    private static string CodeKey(string email) =>
        $"verify:code:{email.ToLowerInvariant()}";

    private static string AttemptsKey(string email) =>
        $"verify:attempts:{email.ToLowerInvariant()}";

    public async Task StoreCodeAsync(
        string email,
        string code,
        TimeSpan ttl,
        CancellationToken cancellationToken = default)
    {
        await Db.StringSetAsync(CodeKey(email), code, ttl);
        // Freshly issued code resets the guess counter
        await Db.KeyDeleteAsync(AttemptsKey(email));
    }

    public async Task<bool> ValidateAndConsumeAsync(
        string email,
        string code,
        CancellationToken cancellationToken = default)
    {
        var attemptsKey = AttemptsKey(email);
        var attempts = await Db.StringIncrementAsync(attemptsKey);
        if (attempts == 1)
            await Db.KeyExpireAsync(attemptsKey, AttemptsWindow);

        // Cap brute-forcing a 6-digit code within its lifetime
        if (attempts > MaxAttempts)
            return false;

        var stored = await Db.StringGetAsync(CodeKey(email));
        if (stored.IsNullOrEmpty || stored != code)
            return false;

        // Single-use — delete on successful validation
        await Db.KeyDeleteAsync(CodeKey(email));
        await Db.KeyDeleteAsync(attemptsKey);
        return true;
    }
}
