namespace AulaPedidos.Application.Caching;

public interface ICachedQuery
{
    string CacheKey { get; }
}
