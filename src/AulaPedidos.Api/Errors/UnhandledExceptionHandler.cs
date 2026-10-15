using AulaPedidos.Api.Contracts;
using Microsoft.AspNetCore.Diagnostics;

namespace AulaPedidos.Api.Errors;

public sealed class UnhandledExceptionHandler(ILogger<UnhandledExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (httpContext.Response.HasStarted)
        {
            return false;
        }

        if (exception is OperationCanceledException && httpContext.RequestAborted.IsCancellationRequested)
        {
            logger.LogInformation(exception, "Request was canceled by the client.");
            return true;
        }

        logger.LogError(exception, "Unhandled exception while processing request.");
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(
            Result<object?>.Failure("An unexpected error occurred."),
            cancellationToken);
        return true;
    }
}