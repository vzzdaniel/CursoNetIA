using AulaPedidos.Application.Contracts.BulkImport;
using AulaPedidos.Application.Products.BulkImport;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AulaPedidos.Infrastructure.BulkImport;

public sealed class BulkProductImportWorker(
    IBulkImportQueue queue,
    IServiceScopeFactory scopeFactory,
    ILogger<BulkProductImportWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var job in queue.DequeueAllAsync(stoppingToken))
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var processor = scope.ServiceProvider.GetRequiredService<BulkProductImportProcessor>();
                await processor.ProcessAsync(job, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Bulk product import {TransactionId} failed.", job.TransactionId);
            }
            finally
            {
                DeleteFile(job.FilePath);
            }
        }
    }

    private void DeleteFile(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (IOException exception)
        {
            logger.LogWarning(exception, "Could not delete temporary bulk import file.");
        }
    }
}
