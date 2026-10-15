using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace AulaPedidos.Api.Errors;

public sealed class ValidationExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
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
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ValidationProblemDetails(errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation failed",
            },
        });
    }
}
