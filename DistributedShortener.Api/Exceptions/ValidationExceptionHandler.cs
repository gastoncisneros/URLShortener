using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;

namespace DistributedShortener.Api.Exceptions;

// Global error handling for FluentValidation validators
public sealed class ValidationExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken ct)
    {
        if (exception is not ValidationException validationEx)
            return false; // no es mía, que lo maneje otro handler

        context.Response.StatusCode = StatusCodes.Status400BadRequest;

        var errors = validationEx.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());

        await context.Response.WriteAsJsonAsync(new
        {
            title  = "Validation failed",
            status = 400,
            errors
        }, ct);

        return true; // manejado, no sigas
    }
}