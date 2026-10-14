using AulaPedidos.Application.Products;
using AulaPedidos.Domain.Products;
using AulaPedidos.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace AulaPedidos.Infrastructure.Tests;

public class ProductPersistenceTests
{
    [Fact]
    public async Task HandlersPersistCrudThroughExistingRepositoryAndUnitOfWork()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AulaPedidosDbContext>().UseSqlite(connection).Options;
        await using var context = new AulaPedidosDbContext(options);
        await context.Database.MigrateAsync();
        var repository = new EfRepository<Product>(context);
        var unitOfWork = new EfUnitOfWork(context);
        var create = new CreateProductHandler(repository, unitOfWork);
        var update = new UpdateProductHandler(repository, unitOfWork);
        var delete = new DeleteProductHandler(repository, unitOfWork);
        var get = new GetProductByIdHandler(repository);
        var list = new GetProductsHandler(repository);

        Assert.Empty(await list.Handle(new GetProducts(), default));
        var product = await create.Handle(new CreateProduct(12.34m, " Original ", null), default);
        context.ChangeTracker.Clear();
        Assert.Equal(product, await get.Handle(new GetProductById(product.Id), default));

        var changed = await update.Handle(new UpdateProduct(product.Id, 0, "Changed", " Other "), default);
        Assert.NotNull(changed);
        Assert.Equal(product.Id, changed.Id);
        context.ChangeTracker.Clear();
        Assert.Equal(changed, await get.Handle(new GetProductById(product.Id), default));
        Assert.Equal(changed, Assert.Single(await list.Handle(new GetProducts(), default)));
        Assert.True(await delete.Handle(new DeleteProduct(product.Id), default));
        context.ChangeTracker.Clear();
        Assert.Null(await get.Handle(new GetProductById(product.Id), default));
        Assert.Null(await update.Handle(new UpdateProduct(product.Id, 1, "Missing", null), default));
        Assert.False(await delete.Handle(new DeleteProduct(product.Id), default));
        Assert.Empty(await list.Handle(new GetProducts(), default));

        await Assert.ThrowsAsync<ProductValidationException>(() => create.Handle(new CreateProduct(-1, "Invalid", null), default).AsTask());
        Assert.False(context.ChangeTracker.HasChanges());

        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        await Assert.ThrowsAsync<OperationCanceledException>(() => create.Handle(new CreateProduct(1, "Canceled", null), cancellation.Token).AsTask());
        await Assert.ThrowsAsync<OperationCanceledException>(() => update.Handle(new UpdateProduct(Guid.NewGuid(), 1, "Canceled", null), cancellation.Token).AsTask());
        await Assert.ThrowsAsync<OperationCanceledException>(() => delete.Handle(new DeleteProduct(Guid.NewGuid()), cancellation.Token).AsTask());
        await Assert.ThrowsAsync<OperationCanceledException>(() => get.Handle(new GetProductById(Guid.NewGuid()), cancellation.Token).AsTask());
        await Assert.ThrowsAsync<OperationCanceledException>(() => list.Handle(new GetProducts(), cancellation.Token).AsTask());
        Assert.False(context.ChangeTracker.HasChanges());
    }

    [Fact]
    public async Task MigrationIsAdditiveAndIdempotentForExistingDatabase()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using (var command = connection.CreateCommand())
        {
            command.CommandText = "CREATE TABLE Legacy (Id INTEGER PRIMARY KEY, Value TEXT NOT NULL); INSERT INTO Legacy VALUES (1, 'preserved');";
            await command.ExecuteNonQueryAsync();
        }

        await using var context = new AulaPedidosDbContext(new DbContextOptionsBuilder<AulaPedidosDbContext>().UseSqlite(connection).Options);
        await context.Database.MigrateAsync();
        await context.Database.MigrateAsync();
        Assert.False(context.Database.HasPendingModelChanges());
        Assert.Empty(await context.Products.ToListAsync());
        await using var verify = connection.CreateCommand();
        verify.CommandText = "SELECT Value FROM Legacy WHERE Id = 1";
        Assert.Equal("preserved", await verify.ExecuteScalarAsync());

        var entityType = context.Model.FindEntityType(typeof(Product))!;
        Assert.Equal(200, entityType.FindProperty(nameof(Product.Description))!.GetMaxLength());
        Assert.Equal(200, entityType.FindProperty(nameof(Product.AlternateDescription))!.GetMaxLength());
        Assert.Equal(18, entityType.FindProperty(nameof(Product.Price))!.GetPrecision());
        Assert.Equal(2, entityType.FindProperty(nameof(Product.Price))!.GetScale());
    }
}
