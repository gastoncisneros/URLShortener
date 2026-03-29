using DistributedShortener.Domain.Events;
using DistributedShortener.Domain.ValueObjects;

namespace DistributedShortener.Domain.Aggregates;

public sealed class ShortLink
{
    private readonly List<IDomainEvent> _domainEvents = new();
    
    public ShortCode Code { get; private set; }
    public OriginalUrl OriginalUrl { get; private set; }
    public string CreatedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public bool IsActive { get; private set; }

    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents;

    private ShortLink() { }

    public static ShortLink Create(string originalUrl, string createdBy, DateTime? expiresAt = null)
    {
        var link = new ShortLink
        {
            Code = ShortCode.Generate(),
            OriginalUrl = OriginalUrl.From(originalUrl),
            CreatedBy =  createdBy,
            CreatedAt =  DateTime.UtcNow,
            ExpiresAt = expiresAt,
            IsActive = true
        };
        
        // The Aggregate registers its own event
        link._domainEvents.Add(new LinkCreatedEvent(
            link.Code.Value,
            link.OriginalUrl.Value,
            link.CreatedAt));
        
        return link;
    }
}