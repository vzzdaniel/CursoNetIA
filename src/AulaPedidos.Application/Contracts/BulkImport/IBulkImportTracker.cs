namespace AulaPedidos.Application.Contracts.BulkImport;

public interface IBulkImportTracker
{
    Guid Register();

    void MarkProcessing(Guid transactionId);

    void ReportProgress(Guid transactionId, long processed, long failed);

    void MarkCompleted(Guid transactionId);

    void MarkFailed(Guid transactionId, string error);

    void Discard(Guid transactionId);

    /// <summary>
    /// Returns the current state. When the transaction has finished, the state is returned once and the identifier is released.
    /// </summary>
    BulkImportState? GetAndReleaseIfFinished(Guid transactionId);
}
