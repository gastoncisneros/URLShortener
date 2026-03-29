using MediatR;

namespace DistributedShortener.Application.Commands.CreateShortLink;

public sealed record CreateShortLinkCommand(
    string OriginalUrl,
    DateTime? ExpiresAt = null) : IRequest<CreateShortLinkResult>;