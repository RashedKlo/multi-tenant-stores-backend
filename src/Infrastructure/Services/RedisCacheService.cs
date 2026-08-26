using System.Text.Json;
using Application.Common.Interfaces;
using StackExchange.Redis;

namespace Infrastructure.Services;

public class RedisCacheService(IConnectionMultiplexer redis) : ICacheService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private IDatabase Db => redis.GetDatabase();

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var value = await Db.StringGetAsync(key);
        return value.IsNullOrEmpty
            ? default
            : JsonSerializer.Deserialize<T>((string)value!, JsonOptions);
    }

  public async Task SetAsync<T>(
    string key,
    T value,
    TimeSpan? expiry = null,
    CancellationToken cancellationToken = default)
{
    var json = JsonSerializer.Serialize(value, JsonOptions);
    await Db.StringSetAsync(key, json, expiry.HasValue?expiry.Value:TimeSpan.FromHours(1));
}
    public Task RemoveAsync(string key, CancellationToken cancellationToken = default) =>
        Db.KeyDeleteAsync(key);
}
