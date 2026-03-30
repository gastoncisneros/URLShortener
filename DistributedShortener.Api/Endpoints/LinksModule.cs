using Carter;
using DistributedShortener.Application.Commands.CreateShortLink;
using DistributedShortener.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DistributedShortener.Api.Endpoints;

public sealed class LinksModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/links", async (
            CreateLinkRequest request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new CreateShortLinkCommand(request.OriginalUrl, request.ExpiresAt);
            var result = await mediator.Send(command, ct);
            
            return Results.Created($"/links/{result.Code}", result);
        })
        .WithName("CreateShortLink")
        .WithTags("Links")
        .Produces<CreateShortLinkResult>(StatusCodes.Status201Created)
        .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest);;
    }
}