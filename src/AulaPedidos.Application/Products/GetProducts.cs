using AulaPedidos.Application.Caching;
using AulaPedidos.Application.Contracts.Discounts;
using AulaPedidos.Application.Contracts.Persistence;
using AulaPedidos.Domain.Products;
using Mediator;

namespace AulaPedidos.Application.Products;

public sealed record GetProducts : IQuery<IReadOnlyList<ProductDto>>, ICachedQuery
{
    public string CacheKey => ProductCacheKeys.All;
}

public sealed class GetProductsHandler(IRepository<Product> repository, IProductDiscountClient discountClient)
    : IQueryHandler<GetProducts, IReadOnlyList<ProductDto>>
{
    public async ValueTask<IReadOnlyList<ProductDto>> Handle(GetProducts query, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var products = await repository.ListAsync(_ => true, cancellationToken);
        var result = new List<ProductDto>(products.Count);
        foreach (var product in products)
        {
            var discount = await discountClient.GetDiscountPercentageAsync(cancellationToken);
            result.Add(ProductDto.FromProduct(product).WithDiscount(discount));
        }

        return result;
    }
}