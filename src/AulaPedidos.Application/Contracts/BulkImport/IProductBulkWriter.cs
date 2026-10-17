using AulaPedidos.Domain.Products;

namespace AulaPedidos.Application.Contracts.BulkImport;

public interface IProductBulkWriter
{
    Task AddRangeAsync(IReadOnlyCollection<Product> products, CancellationToken cancellationToken);
}
