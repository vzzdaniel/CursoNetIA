using AulaPedidos.Application.Caching;
using AulaPedidos.Application.Contracts.Persistence;
using AulaPedidos.Domain.Products;
using Mediator;

namespace AulaPedidos.Application.Products;

public sealed record DeleteProduct(Guid Id) : ICommand<bool>, ICacheInvalidatingCommand
{
    public IReadOnlyCollection<string> CacheKeysToInvalidate => [ProductCacheKeys.ById(Id), ProductCacheKeys.All];
}

public sealed class DeleteProductHandler(IRepository<Product> repository, IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteProduct, bool>
{
    public async ValueTask<bool> Handle(DeleteProduct command, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var product = await repository.FirstOrDefaultAsync(product => product.Id == command.Id, cancellationToken);
        if (product is null)
        {
            return false;
        }

        await repository.RemoveAsync(product, cancellationToken);
        _ = await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}