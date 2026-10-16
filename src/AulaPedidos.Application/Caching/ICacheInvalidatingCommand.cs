namespace AulaPedidos.Application.Caching;

public interface ICacheInvalidatingCommand
{
    IReadOnlyCollection<string> CacheKeysToInvalidate { get; }
}
