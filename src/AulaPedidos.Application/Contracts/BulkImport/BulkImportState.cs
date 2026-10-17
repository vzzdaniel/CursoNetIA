namespace AulaPedidos.Application.Contracts.BulkImport;

public sealed record BulkImportState(
    Guid TransactionId,
    BulkImportStatus Status,
    long Processed,
    long Failed,
    string? Error,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt);
