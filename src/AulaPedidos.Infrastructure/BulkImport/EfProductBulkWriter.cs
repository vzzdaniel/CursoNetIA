using AulaPedidos.Application.Contracts.BulkImport;
using AulaPedidos.Domain.Products;
using AulaPedidos.Infrastructure.Persistence;

namespace AulaPedidos.Infrastructure.BulkImport;

public sealed class EfProductBulkWriter(AulaPedidosDbContext dbContext) : IProductBulkWriter
{
    public async Task AddRangeAsync(IReadOnlyCollection<Product> products, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(products);

        dbContext.Products.AddRange(products);
        try
        {
            _ = await dbContext.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            dbContext.ChangeTracker.Clear();
        }
    }
}
