using AulaPedidos.Api.Contracts;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;

namespace AulaPedidos.Api.Errors;

public sealed class ValidationExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not ValidationException validationException)
        {
            return false;
        }

        var errors = validationException.Errors.GroupBy(failure => failure.PropertyName)
            .ToDictionary(group => group.Key, group => group.Select(failure => failure.ErrorMessage).ToArray());
        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        await httpContext.Response.WriteAsJsonAsync(
            Result<IReadOnlyDictionary<string, string[]>>.Failure("Validation failed.", errors),
            cancellationToken);
        return true;
    }
}
