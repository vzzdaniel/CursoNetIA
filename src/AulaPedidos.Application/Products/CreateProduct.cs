using AulaPedidos.Application.Caching;
using AulaPedidos.Application.Contracts.Persistence;
using AulaPedidos.Domain.Products;
using Mediator;

namespace AulaPedidos.Application.Products;

public sealed record CreateProduct(decimal Price, string? Description, string? AlternateDescription)
    : ICommand<ProductDto>, ICacheInvalidatingCommand
{
    public IReadOnlyCollection<string> CacheKeysToInvalidate => [ProductCacheKeys.All];
}

public sealed class CreateProductHandler(IRepository<Product> repository, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateProduct, ProductDto>
{
    public async ValueTask<ProductDto> Handle(CreateProduct command, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var product = Product.Create(command.Price, command.Description, command.AlternateDescription);
        await repository.AddAsync(product, cancellationToken);
        _ = await unitOfWork.SaveChangesAsync(cancellationToken);
        return ProductDto.FromProduct(product);
    }
}