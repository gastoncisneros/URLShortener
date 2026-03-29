namespace DistributedShortener.Domain.Events;

public sealed record LinkCreatedEvent(
    string Code,
    string OriginalUrl,
    DateTime CreatedAt
    ) : IDomainEvent;