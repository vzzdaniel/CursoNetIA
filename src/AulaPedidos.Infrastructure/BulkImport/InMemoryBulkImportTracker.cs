using System.Collections.Concurrent;
using AulaPedidos.Application.Contracts.BulkImport;
using AulaPedidos.Application.Products.BulkImport;
using Microsoft.Extensions.Options;

namespace AulaPedidos.Infrastructure.BulkImport;

public sealed class InMemoryBulkImportTracker(IOptions<BulkImportOptions> options, TimeProvider timeProvider) : IBulkImportTracker
{
    private sealed class Entry(Guid id, DateTimeOffset createdAt)
    {
        public Guid Id { get; } = id;
        public DateTimeOffset CreatedAt { get; } = createdAt;
        public BulkImportStatus Status { get; set; } = BulkImportStatus.Pending;
        public long Processed { get; set; }
        public long Failed { get; set; }
        public string? Error { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }

        public bool IsFinished => Status is BulkImportStatus.Completed or BulkImportStatus.Failed;

        public BulkImportState ToState() => new(Id, Status, Processed, Failed, Error, CreatedAt, CompletedAt);
    }

    private readonly ConcurrentDictionary<Guid, Entry> entries = new();

    public Guid Register()
    {
        PurgeExpired();
        var id = Guid.NewGuid();
        entries[id] = new Entry(id, timeProvider.GetUtcNow());
        return id;
    }

    public void MarkProcessing(Guid transactionId) => Update(transactionId, entry => entry.Status = BulkImportStatus.Processing);

    public void ReportProgress(Guid transactionId, long processed, long failed) => Update(transactionId, entry =>
    {
        entry.Processed = processed;
        entry.Failed = failed;
    });

    public void MarkCompleted(Guid transactionId) => Update(transactionId, entry =>
    {
        entry.Status = BulkImportStatus.Completed;
        entry.CompletedAt = timeProvider.GetUtcNow();
    });

    public void MarkFailed(Guid transactionId, string error) => Update(transactionId, entry =>
    {
        entry.Status = BulkImportStatus.Failed;
        entry.Error = error;
        entry.CompletedAt = timeProvider.GetUtcNow();
    });

    public void Discard(Guid transactionId) => _ = entries.TryRemove(transactionId, out _);

    public BulkImportState? GetAndReleaseIfFinished(Guid transactionId)
    {
        PurgeExpired();
        if (!entries.TryGetValue(transactionId, out var entry))
        {
            return null;
        }

        BulkImportState state;
        lock (entry)
        {
            state = entry.ToState();
            if (entry.IsFinished)
            {
                _ = entries.TryRemove(transactionId, out _);
            }
        }

        return state;
    }

    private void Update(Guid transactionId, Action<Entry> update)
    {
        if (entries.TryGetValue(transactionId, out var entry))
        {
            lock (entry)
            {
                update(entry);
            }
        }
    }

    private void PurgeExpired()
    {
        var threshold = timeProvider.GetUtcNow() - options.Value.ResultTtl;
        foreach (var (id, entry) in entries)
        {
            if (entry.CompletedAt is { } completedAt && completedAt < threshold)
            {
                _ = entries.TryRemove(id, out _);
            }
        }
    }
}
