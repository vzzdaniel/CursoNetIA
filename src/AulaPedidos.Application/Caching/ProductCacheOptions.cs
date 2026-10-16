namespace AulaPedidos.Application.Caching;

public sealed class ProductCacheOptions
{
    public const string SectionName = "Caching:Products";

    public TimeSpan AbsoluteExpiration { get; set; } = TimeSpan.FromSeconds(10);
}
