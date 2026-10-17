using System.Threading.Channels;
using AulaPedidos.Application.Contracts.BulkImport;
using AulaPedidos.Application.Products.BulkImport;
using Microsoft.Extensions.Options;

namespace AulaPedidos.Infrastructure.BulkImport;

public sealed class ChannelBulkImportQueue : IBulkImportQueue
{
    private readonly Channel<BulkImportJob> channel;

    public ChannelBulkImportQueue(IOptions<BulkImportOptions> options)
    {
        channel = Channel.CreateBounded<BulkImportJob>(new BoundedChannelOptions(Math.Max(1, options.Value.QueueCapacity))
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
        });
    }

    public bool TryEnqueue(BulkImportJob job)
    {
        ArgumentNullException.ThrowIfNull(job);
        return channel.Writer.TryWrite(job);
    }

    public IAsyncEnumerable<BulkImportJob> DequeueAllAsync(CancellationToken cancellationToken) =>
        channel.Reader.ReadAllAsync(cancellationToken);
}
