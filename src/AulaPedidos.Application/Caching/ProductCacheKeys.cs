namespace AulaPedidos.Application.Caching;

public static class ProductCacheKeys
{
    public const string All = "products:all";

    public static string ById(Guid id) => $"product:{id}";
}
