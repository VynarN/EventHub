using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.WebApi.ExceptionHandling;

/// <summary>Catches unhandled exceptions and writes RFC 9457 Problem Details (500).</summary>
internal sealed class Rfc9457ExceptionHandler(
    IProblemDetailsService problemDetailsService,
    IHostEnvironment environment,
    ILogger<Rfc9457ExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception");

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred.",
            Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
            Detail = environment.IsDevelopment()
                ? exception.Message
                : "An unexpected error occurred while processing your request.",
            Instance = httpContext.Request.Path.Value,
        };

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await problemDetailsService
            .WriteAsync(
                new ProblemDetailsContext
                {
                    HttpContext = httpContext,
                    ProblemDetails = problemDetails,
                    Exception = exception,
                })
            .ConfigureAwait(false);

        return true;
    }
}
