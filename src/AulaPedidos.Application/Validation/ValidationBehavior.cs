using FluentValidation;
using FluentValidation.Results;
using Mediator;

namespace AulaPedidos.Application.Validation;

public sealed class ValidationBehavior<TMessage, TResponse>(IEnumerable<IValidator<TMessage>> validators)
    : IPipelineBehavior<TMessage, TResponse> where TMessage : IMessage
{
    public async ValueTask<TResponse> Handle(TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var failures = new List<ValidationFailure>();
        foreach (var validator in validators)
        {
            var result = await validator.ValidateAsync(new ValidationContext<TMessage>(message), cancellationToken);
            failures.AddRange(result.Errors);
        }

        cancellationToken.ThrowIfCancellationRequested();
        if (failures.Count > 0)
        {
            throw new ValidationException(failures);
        }

        return await next(message, cancellationToken);
    }
}
