using DistributedShortener.Application.Abstractions;
using DistributedShortener.Domain.Aggregates;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace DistributedShortener.Application.Commands.CreateShortLink;

public sealed class CreateShortLinkCommandHandler : IRequestHandler<CreateShortLinkCommand, CreateShortLinkResult>
{
    private readonly ILinkRepository _linkRepository;
    private readonly ICacheService _cacheService;
    private readonly IConfiguration _configuration;

    public CreateShortLinkCommandHandler(
        ILinkRepository linkRepository,
        ICacheService cacheService,
        IConfiguration configuration)
    {
        _linkRepository = linkRepository;
        _cacheService = cacheService;
        _configuration = configuration;
    }
    
    public async Task<CreateShortLinkResult> Handle(CreateShortLinkCommand command, CancellationToken cancellationToken)
    {
        // 1. Create the aggregate. All the business logic lives in the Domain
        // Creates the Short Link, fires the event, The model objects validates the url...
        ShortLink link = ShortLink.Create(command.OriginalUrl, "User", expiresAt: command.ExpiresAt);
        
        // 2. Persist in PostgreSQL
        await _linkRepository.SaveAsync(link, cancellationToken);
        
        // 3. Seed cache to avoid cold-start miss on first redirect
        // ttl: Time to Live
        var ttl  = link.ExpiresAt.HasValue
                ? link.ExpiresAt.Value - DateTime.UtcNow
                : TimeSpan.FromDays(7);

        await _cacheService.SetAsync(link.Code.Value, link.OriginalUrl.Value, ttl, cancellationToken);
        
        // 4. Build Short URL
        var baseUrl = _configuration["ShortUrl:BaseUrl"] ?? "https://short.ly";
        var shortUrl = $"{baseUrl}/{link.Code.Value}";
        
        // 5. Return
        return new CreateShortLinkResult(
            Code: link.Code.Value,
            ShortUrl: shortUrl,
            OriginalUrl: link.OriginalUrl.Value,
            CreatedAt: link.CreatedAt);
    }
}