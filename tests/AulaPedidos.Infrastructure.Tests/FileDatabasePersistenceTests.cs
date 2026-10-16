using AulaPedidos.Application.Products;
using AulaPedidos.Domain.Products;
using AulaPedidos.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AulaPedidos.Infrastructure.Tests;

public class FileDatabasePersistenceTests
{
    [Fact]
    public async Task ProductsSurviveAcrossContextsUsingSqliteFile()
    {
        var directory = Path.Combine(Path.GetTempPath(), "AulaPedidosTests", Guid.NewGuid().ToString("N"));
        _ = Directory.CreateDirectory(directory);
        var connectionString = $"Data Source={Path.Combine(directory, "aulapedidos.db")}";

        try
        {
            ProductDto created;
            await using (var context = CreateContext(connectionString))
            {
                await context.Database.MigrateAsync();
                var handler = new CreateProductHandler(new EfRepository<Product>(context), new EfUnitOfWork(context));
                created = await handler.Handle(new CreateProduct(12.34m, "Persisted", null), default);
            }

            await using (var context = CreateContext(connectionString))
            {
                await context.Database.MigrateAsync();
                var handler = new GetProductByIdHandler(new EfRepository<Product>(context));
                Assert.Equal(created, await handler.Handle(new GetProductById(created.Id), default));
            }
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            Directory.Delete(directory, recursive: true);
        }
    }

    private static AulaPedidosDbContext CreateContext(string connectionString) =>
        new(new DbContextOptionsBuilder<AulaPedidosDbContext>().UseSqlite(connectionString).Options);
}
