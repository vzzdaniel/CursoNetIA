using AulaPedidos.Api.Contracts;
using AulaPedidos.Domain.Products;
using Microsoft.AspNetCore.Diagnostics;

namespace AulaPedidos.Api.Errors;

public sealed class ProductValidationExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not ProductValidationException)
        {
            return false;
        }

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        await httpContext.Response.WriteAsJsonAsync(Result<object?>.Failure(exception.Message), cancellationToken);
        return true;
    }
}