using DistributedShortener.Application.Abstractions;
using DistributedShortener.Domain.Aggregates;
using DistributedShortener.Domain.ValueObjects;
using DistributedShortener.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DistributedShortener.Infrastructure.Repositories;

public sealed class LinkRepository(ApplicationDbContext dbContext) : ILinkRepository
{
    public async Task SaveAsync(ShortLink link, CancellationToken ct = default)
    {
        await dbContext.ShortLinks.AddAsync(link, ct);
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task<ShortLink?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        return await  dbContext.ShortLinks
            .AsNoTracking() // IMPORTANT: Reading does not need tracking changes on this object.
            .FirstOrDefaultAsync(x => x.Code == ShortCode.From(code), ct);
    }
}