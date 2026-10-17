namespace AulaPedidos.Application.Contracts.BulkImport;

public interface IBulkImportQueue
{
    bool TryEnqueue(BulkImportJob job);

    IAsyncEnumerable<BulkImportJob> DequeueAllAsync(CancellationToken cancellationToken);
}
