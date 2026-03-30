using DistributedShortener.Application.Commands.CreateShortLink;
using MediatR;

namespace DistributedShortener.Application.Queries.ResolveShortCode;

public sealed record ResolveShortCodeQuery(string Code) 
    : IRequest<ResolveShortCodeResult>;