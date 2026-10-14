using AulaPedidos.Application.Contracts.Persistence;
using AulaPedidos.Domain.Products;
using Mediator;

namespace AulaPedidos.Application.Products;

public sealed record GetProductById(Guid Id) : IQuery<ProductDto?>;

public sealed class GetProductByIdHandler(IRepository<Product> repository) : IQueryHandler<GetProductById, ProductDto?>
{
    public async ValueTask<ProductDto?> Handle(GetProductById query, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var product = await repository.FirstOrDefaultAsync(product => product.Id == query.Id, cancellationToken);
        return product is null ? null : ProductDto.FromProduct(product);
    }
}