using AulaPedidos.Application.Contracts.Persistence;

namespace AulaPedidos.Infrastructure.Persistence;

public sealed class EfUnitOfWork(AulaPedidosDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        dbContext.SaveChangesAsync(cancellationToken);
}