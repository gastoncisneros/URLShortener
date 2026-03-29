namespace DistributedShortener.Application.Abstractions;

public interface ICacheService
{
    Task<string?> GetAsync(string key, CancellationToken ct = default);
    Task SetAsync(string key, string value, TimeSpan ttl, CancellationToken ct = default);
    Task DeleteAsync(string key, CancellationToken ct = default);
}