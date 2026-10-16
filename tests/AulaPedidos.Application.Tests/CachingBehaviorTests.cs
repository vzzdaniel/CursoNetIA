using AulaPedidos.Application.Caching;
using Mediator;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace AulaPedidos.Application.Tests;

public sealed class CachingBehaviorTests
{
    private sealed record TestQuery(string Key) : IQuery<string?>, ICachedQuery
    {
        public string CacheKey => Key;
    }

    private sealed record TestCommand : ICommand<int>, ICacheInvalidatingCommand
    {
        public IReadOnlyCollection<string> CacheKeysToInvalidate => ["k"];
    }

    [Fact]
    public async Task NullResult_IsCachedAndInvalidatedByCommand()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var caching = new CachingBehavior<TestQuery, string?>(cache, Options.Create(new ProductCacheOptions()));
        var invalidation = new CacheInvalidationBehavior<TestCommand, int>(cache);
        var calls = 0;
        ValueTask<string?> Next(TestQuery message, CancellationToken token)
        {
            calls++;
            return ValueTask.FromResult<string?>(null);
        }

        _ = await caching.Handle(new TestQuery("k"), Next, CancellationToken.None);
        _ = await caching.Handle(new TestQuery("k"), Next, CancellationToken.None);
        Assert.Equal(1, calls);

        _ = await invalidation.Handle(new TestCommand(), (_, _) => ValueTask.FromResult(1), CancellationToken.None);
        _ = await caching.Handle(new TestQuery("k"), Next, CancellationToken.None);
        Assert.Equal(2, calls);
    }
}
