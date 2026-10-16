using Mediator;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace AulaPedidos.Application.Caching;

public sealed class CachingBehavior<TMessage, TResponse>(IMemoryCache cache, IOptions<ProductCacheOptions> options)
    : IPipelineBehavior<TMessage, TResponse> where TMessage : IMessage
{
    public async ValueTask<TResponse> Handle(TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken cancellationToken)
    {
        if (message is not ICachedQuery cachedQuery)
        {
            return await next(message, cancellationToken);
        }

        if (cache.TryGetValue(cachedQuery.CacheKey, out TResponse? cached))
        {
            return cached!;
        }

        var response = await next(message, cancellationToken);
        _ = cache.Set(cachedQuery.CacheKey, response, options.Value.AbsoluteExpiration);
        return response;
    }
}
