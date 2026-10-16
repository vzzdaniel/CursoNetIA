using AulaPedidos.Application.Caching;
using AulaPedidos.Application.Products;
using Mediator;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace AulaPedidos.Application.Tests;

public sealed class CachingBehaviorExtraTests
{
    private sealed record PlainQuery : IQuery<int>;

    private sealed record KeyedQuery(string Key) : IQuery<int>, ICachedQuery
    {
        public string CacheKey => Key;
    }

    private sealed record KeyedCommand(params string[] Keys) : ICommand<int>, ICacheInvalidatingCommand
    {
        public IReadOnlyCollection<string> CacheKeysToInvalidate => Keys;
    }

    private static CachingBehavior<KeyedQuery, int> CreateCaching(IMemoryCache cache) =>
        new(cache, Options.Create(new ProductCacheOptions()));

    [Fact]
    public async Task MessageWithoutCacheKey_AlwaysCallsHandler()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var behavior = new CachingBehavior<PlainQuery, int>(cache, Options.Create(new ProductCacheOptions()));
        var calls = 0;
        ValueTask<int> Next(PlainQuery message, CancellationToken token) => ValueTask.FromResult(++calls);

        _ = await behavior.Handle(new PlainQuery(), Next, CancellationToken.None);
        _ = await behavior.Handle(new PlainQuery(), Next, CancellationToken.None);

        Assert.Equal(2, calls);
    }

    [Fact]
    public async Task DifferentKeys_AreCachedIndependently()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var behavior = CreateCaching(cache);
        var calls = 0;
        ValueTask<int> Next(KeyedQuery message, CancellationToken token) => ValueTask.FromResult(++calls);

        var first = await behavior.Handle(new KeyedQuery("a"), Next, CancellationToken.None);
        var second = await behavior.Handle(new KeyedQuery("b"), Next, CancellationToken.None);
        var firstAgain = await behavior.Handle(new KeyedQuery("a"), Next, CancellationToken.None);

        Assert.Equal(1, first);
        Assert.Equal(2, second);
        Assert.Equal(1, firstAgain);
    }

    [Fact]
    public async Task HandlerException_IsNotCached()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var behavior = CreateCaching(cache);

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await behavior.Handle(new KeyedQuery("a"), (_, _) => throw new InvalidOperationException(), CancellationToken.None));

        var result = await behavior.Handle(new KeyedQuery("a"), (_, _) => ValueTask.FromResult(7), CancellationToken.None);
        Assert.Equal(7, result);
    }

    [Fact]
    public async Task Invalidation_RemovesOnlyListedKeys()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        _ = cache.Set("a", 1);
        _ = cache.Set("b", 2);
        var behavior = new CacheInvalidationBehavior<KeyedCommand, int>(cache);

        _ = await behavior.Handle(new KeyedCommand("a"), (_, _) => ValueTask.FromResult(0), CancellationToken.None);

        Assert.False(cache.TryGetValue("a", out _));
        Assert.True(cache.TryGetValue("b", out _));
    }

    [Fact]
    public async Task Invalidation_DoesNotRemoveKeysWhenHandlerFails()
    {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        _ = cache.Set("a", 1);
        var behavior = new CacheInvalidationBehavior<KeyedCommand, int>(cache);

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await behavior.Handle(new KeyedCommand("a"), (_, _) => throw new InvalidOperationException(), CancellationToken.None));

        Assert.True(cache.TryGetValue("a", out _));
    }

    [Fact]
    public void ProductMessages_DeclareExpectedCacheKeys()
    {
        var id = Guid.NewGuid();

        Assert.Equal(ProductCacheKeys.ById(id), new GetProductById(id).CacheKey);
        Assert.Equal(ProductCacheKeys.All, new GetProducts().CacheKey);
        Assert.Equal([ProductCacheKeys.All], new CreateProduct(1m, "d", null).CacheKeysToInvalidate);
        Assert.Equal([ProductCacheKeys.ById(id), ProductCacheKeys.All], new UpdateProduct(id, 1m, "d", null).CacheKeysToInvalidate);
        Assert.Equal([ProductCacheKeys.ById(id), ProductCacheKeys.All], new DeleteProduct(id).CacheKeysToInvalidate);
    }
}
