using DistributedShortener.Application.Abstractions;
using StackExchange.Redis;

namespace DistributedShortener.Infrastructure.Services;

public sealed class CacheService(IConnectionMultiplexer multiplexer) : ICacheService
{
    private readonly IDatabase _db = multiplexer.GetDatabase();
    
    public async Task<string?> GetAsync(string key, CancellationToken ct = default)
    {
        var value = await _db.StringGetAsync(key);
        return value.HasValue ? value.ToString() : null;
    }

    public async Task SetAsync(string key, string value, TimeSpan ttl, CancellationToken ct = default)
    {
        await _db.StringSetAsync(key, value, ttl);
    }

    public async Task DeleteAsync(string key, CancellationToken ct = default)
    {
        await _db.KeyDeleteAsync(key);
    }
}