using DistributedShortener.Domain.Aggregates;

namespace DistributedShortener.Application.Abstractions;

public interface ILinkRepository
{
    Task SaveAsync(ShortLink link, CancellationToken ct = default);
    Task<ShortLink?> GetByCodeAsync(string code, CancellationToken ct = default);
}