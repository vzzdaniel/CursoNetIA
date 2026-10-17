namespace AulaPedidos.Application.Products.BulkImport;

public sealed class BulkImportOptions
{
    public const string SectionName = "BulkImport";

    public int BatchSize { get; set; } = 5000;

    public int QueueCapacity { get; set; } = 10;

    public TimeSpan ResultTtl { get; set; } = TimeSpan.FromMinutes(5);
}
