using DistributedShortener.Application.Abstractions;
using DistributedShortener.Application.Commands.CreateShortLink;
using DistributedShortener.Domain.Exceptions;
using MediatR;

namespace DistributedShortener.Application.Queries.ResolveShortCode;

public sealed class ResolveShortCodeHandler(ILinkRepository repository, ICacheService cache)
    : IRequestHandler<ResolveShortCodeQuery, ResolveShortCodeResult>
{
    public async Task<ResolveShortCodeResult> Handle(ResolveShortCodeQuery request, CancellationToken cancellationToken)
    {
        // 1. Search in Redis
        var cached = await cache.GetAsync(request.Code, cancellationToken);
        
        if (cached != null) return new ResolveShortCodeResult(cached);
        
        // 2. MISS. Search in PostgresSQL
        var link = await repository.GetByCodeAsync(request.Code, cancellationToken);

        if (link is null) throw new ShortCodeNotFoundException(request.Code);
        
        //3. Verify is Active
        if (!link.IsActive || (link.ExpiresAt.HasValue && link.ExpiresAt < DateTime.UtcNow))
        {
            throw new ShortCodeNotFoundException(request.Code);
        }
        
        // 4. Fill cache for next request
        var ttl = link.ExpiresAt.HasValue
            ? link.ExpiresAt.Value - DateTime.UtcNow
            : TimeSpan.FromDays(7);

        await cache.SetAsync(request.Code, link.OriginalUrl.Value, ttl, cancellationToken);
        
        return new ResolveShortCodeResult(link.OriginalUrl.Value);
    }
}