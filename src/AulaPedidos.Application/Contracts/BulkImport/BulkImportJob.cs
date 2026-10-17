namespace AulaPedidos.Application.Contracts.BulkImport;

public sealed record BulkImportJob(Guid TransactionId, string FilePath);
