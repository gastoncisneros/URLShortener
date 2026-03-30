namespace DistributedShortener.Application.DTOs;

public sealed record CreateLinkRequest(
    string OriginalUrl,
    DateTime? ExpiresAt = null);