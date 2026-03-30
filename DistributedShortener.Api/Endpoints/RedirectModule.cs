using Carter;
using DistributedShortener.Application.Queries.ResolveShortCode;
using MediatR;

namespace DistributedShortener.Api.Endpoints;

public sealed class RedirectModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/{code}", async (
            string code,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var query = new ResolveShortCodeQuery(code);
            var result = await mediator.Send(query, ct);

            return Results.Redirect(result.OriginalUrl, permanent: false);
        })
        .WithName("ResolveShortCode")
        .WithTags("Redirect")
        .Produces(StatusCodes.Status302Found)
        .Produces(StatusCodes.Status404NotFound);
    }
}