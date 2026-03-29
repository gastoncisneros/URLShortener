namespace DistributedShortener.Application.Commands.CreateShortLink;

public sealed record CreateShortLinkResult(
    string Code,
    string ShortUrl,
    string OriginalUrl,
    DateTime CreatedAt);