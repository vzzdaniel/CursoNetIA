using AulaPedidos.Application.Caching;
using AulaPedidos.Application.Contracts.Persistence;
using AulaPedidos.Domain.Products;
using Mediator;

namespace AulaPedidos.Application.Products;

public sealed record GetProducts : IQuery<IReadOnlyList<ProductDto>>, ICachedQuery
{
    public string CacheKey => ProductCacheKeys.All;
}

public sealed class GetProductsHandler(IRepository<Product> repository) : IQueryHandler<GetProducts, IReadOnlyList<ProductDto>>
{
    public async ValueTask<IReadOnlyList<ProductDto>> Handle(GetProducts query, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var products = await repository.ListAsync(_ => true, cancellationToken);
        return products.Select(ProductDto.FromProduct).ToArray();
    }
}