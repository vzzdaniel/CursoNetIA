using System.Text.Json;
using AulaPedidos.Application.Caching;
using AulaPedidos.Application.Contracts.BulkImport;
using AulaPedidos.Domain.Products;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace AulaPedidos.Application.Products.BulkImport;

public sealed class BulkProductImportProcessor(
    IProductBulkWriter writer,
    IBulkImportTracker tracker,
    IMemoryCache cache,
    IOptions<BulkImportOptions> options)
{
    public async Task ProcessAsync(BulkImportJob job, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(job);

        var batchSize = Math.Max(1, options.Value.BatchSize);
        var batch = new List<Product>(batchSize);
        long processed = 0;
        long failed = 0;

        tracker.MarkProcessing(job.TransactionId);
        try
        {
            await using var stream = new FileStream(
                job.FilePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 81920,
                FileOptions.Asynchronous | FileOptions.SequentialScan);

            await foreach (var item in JsonSerializer.DeserializeAsyncEnumerable<BulkProductItem>(stream, JsonSerializerOptions.Web, cancellationToken))
            {
                try
                {
                    batch.Add(Product.Create(
                        item?.Price ?? -1,
                        item?.Description,
                        item?.AlternateDescription));
                }
                catch (ProductValidationException)
                {
                    failed++;
                }

                if (batch.Count >= batchSize)
                {
                    processed += await FlushAsync(job.TransactionId, batch, processed, failed, cancellationToken);
                }
            }

            processed += await FlushAsync(job.TransactionId, batch, processed, failed, cancellationToken);
            tracker.ReportProgress(job.TransactionId, processed, failed);
            tracker.MarkCompleted(job.TransactionId);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            tracker.ReportProgress(job.TransactionId, processed, failed);
            tracker.MarkFailed(job.TransactionId, exception is JsonException
                ? "The payload is not a valid JSON array of products."
                : "The bulk import failed unexpectedly.");
            throw;
        }
        finally
        {
            cache.Remove(ProductCacheKeys.All);
        }
    }

    private async Task<int> FlushAsync(Guid transactionId, List<Product> batch, long processed, long failed, CancellationToken cancellationToken)
    {
        if (batch.Count == 0)
        {
            return 0;
        }

        var count = batch.Count;
        await writer.AddRangeAsync(batch, cancellationToken);
        batch.Clear();
        tracker.ReportProgress(transactionId, processed + count, failed);
        return count;
    }
}
