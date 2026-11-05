using Microsoft.AspNetCore.Diagnostics;
using CreditApplication.Api.Responses;

namespace CreditApplication.Api.Middleware;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var requestId = httpContext.TraceIdentifier;

        logger.LogError(
            exception,
            "Unhandled exception for {Method} {Path}. RequestId: {RequestId}",
            httpContext.Request.Method,
            httpContext.Request.Path,
            requestId);

        httpContext.Response.StatusCode =
            StatusCodes.Status500InternalServerError;

        var response = ApiErrorResponseMapper.Map(
            httpContext,
            StatusCodes.Status500InternalServerError,
            "Terjadi kesalahan tak terduga pada server.");

        await httpContext.Response.WriteAsJsonAsync(
            response,
            cancellationToken);

        return true;
    }
}