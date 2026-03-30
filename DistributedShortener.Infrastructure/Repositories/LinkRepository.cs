using DistributedShortener.Application.Abstractions;
using DistributedShortener.Domain.Aggregates;

namespace DistributedShortener.Infrastructure.Repositories;

public class LinkRepository : ILinkRepository
{
    public Task SaveAsync(ShortLink link, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<ShortLink?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}