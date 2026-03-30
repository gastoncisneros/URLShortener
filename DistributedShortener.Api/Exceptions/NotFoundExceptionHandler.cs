using DistributedShortener.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace DistributedShortener.Api.Exceptions;

public sealed class NotFoundExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not ShortCodeNotFoundException notFoundEx) return false;
        
        httpContext.Response.StatusCode = StatusCodes.Status404NotFound;

        await httpContext.Response.WriteAsJsonAsync(new
        {
            title  = "Short code not found",
            status = 404,
            detail = notFoundEx.Message
        }, cancellationToken);

        return true;
        
    }
}