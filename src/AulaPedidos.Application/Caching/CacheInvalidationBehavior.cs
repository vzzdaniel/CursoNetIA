using Mediator;
using Microsoft.Extensions.Caching.Memory;

namespace AulaPedidos.Application.Caching;

public sealed class CacheInvalidationBehavior<TMessage, TResponse>(IMemoryCache cache)
    : IPipelineBehavior<TMessage, TResponse> where TMessage : IMessage
{
    public async ValueTask<TResponse> Handle(TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
    {
        var response = await next(message, cancellationToken);

        if (message is ICacheInvalidatingCommand command)
        {
            foreach (var key in command.CacheKeysToInvalidate)
            {
                cache.Remove(key);
            }
        }

        return response;
    }
}
