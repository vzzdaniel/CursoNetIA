using AulaPedidos.Application.Contracts.Persistence;
using AulaPedidos.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AulaPedidos.Infrastructure.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddInfrastructure_WithNullServices_ThrowsArgumentNullException()
    {
        var configuration = new ConfigurationBuilder().Build();
        var action = () => DependencyInjection.AddInfrastructure(null!, configuration);

        _ = Assert.Throws<ArgumentNullException>(action);
    }

    [Fact]
    public void AddInfrastructure_WithNullConfiguration_ThrowsArgumentNullException()
    {
        var services = new ServiceCollection();
        var action = () => services.AddInfrastructure(null!);

        _ = Assert.Throws<ArgumentNullException>(action);
    }

    [Fact]
    public void AddInfrastructure_ReturnsSameServiceCollection()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        var result = services.AddInfrastructure(configuration);

        Assert.Same(services, result);
    }

    [Fact]
    public async Task AddInfrastructure_ResolvesRepositoryAndUnitOfWork()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
            [
                new KeyValuePair<string, string?>("ConnectionStrings:AulaPedidos", connection.ConnectionString),
            ])
            .Build();

        var services = new ServiceCollection();
        _ = services.AddInfrastructure(configuration);

        using var provider = services.BuildServiceProvider(validateScopes: true);
        await using var scope = provider.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<AulaPedidosDbContext>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var hasRepositoryRegistration = services.Any(descriptor =>
            descriptor.ServiceType.IsGenericType
            && descriptor.ServiceType.GetGenericTypeDefinition() == typeof(IRepository<>));

        Assert.NotNull(dbContext);
        Assert.NotNull(unitOfWork);
        Assert.True(hasRepositoryRegistration);
    }

    [Fact]
    public async Task EfRepository_PredicateQueriesAndCommit_WorkAsExpected()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        using var provider = BuildRepositoryServiceProvider(connection);
        await EnsureCreatedAsync(provider);

        await using (var scope = provider.CreateAsyncScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IRepository<TestEntity>>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            await repository.AddAsync(new TestEntity { Name = "alpha" });
            await repository.AddAsync(new TestEntity { Name = "beta" });
            _ = await unitOfWork.SaveChangesAsync();
        }

        await using (var scope = provider.CreateAsyncScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IRepository<TestEntity>>();

            var found = await repository.FirstOrDefaultAsync(entity => entity.Name == "alpha");
            var notFound = await repository.FirstOrDefaultAsync(entity => entity.Name == "gamma");
            var filtered = await repository.ListAsync(entity => entity.Name.Contains('a'));
            var any = await repository.AnyAsync(entity => entity.Name == "beta");
            var count = await repository.CountAsync(entity => entity.Name.Contains('a'));

            Assert.NotNull(found);
            Assert.Null(notFound);
            Assert.Equal(2, filtered.Count);
            Assert.True(any);
            Assert.Equal(2, count);
        }
    }

    [Fact]
    public async Task EfRepository_RemoveAndSaveChangesAsync_PersistsDeletion()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        using var provider = BuildRepositoryServiceProvider(connection);
        await EnsureCreatedAsync(provider);

        Guid entityId;

        await using (var scope = provider.CreateAsyncScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IRepository<TestEntity>>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var entity = new TestEntity { Name = "to-delete" };
            await repository.AddAsync(entity);
            _ = await unitOfWork.SaveChangesAsync();
            entityId = entity.Id;
        }

        await using (var scope = provider.CreateAsyncScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IRepository<TestEntity>>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var existing = await repository.FirstOrDefaultAsync(entity => entity.Id == entityId);
            Assert.NotNull(existing);

            await repository.RemoveAsync(existing);
            _ = await unitOfWork.SaveChangesAsync();
        }

        await using (var scope = provider.CreateAsyncScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IRepository<TestEntity>>();
            var deleted = await repository.FirstOrDefaultAsync(entity => entity.Id == entityId);

            Assert.Null(deleted);
        }
    }

    [Fact]
    public async Task R005_FirstOrDefaultAsync_WithNullPredicate_ThrowsArgumentNullException()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        using var provider = BuildRepositoryServiceProvider(connection);
        await EnsureCreatedAsync(provider);

        await using var scope = provider.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository<TestEntity>>();

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => repository.FirstOrDefaultAsync(null!, CancellationToken.None));
    }

    [Fact]
    public async Task R005_ListAsync_WithNullPredicate_ThrowsArgumentNullException()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        using var provider = BuildRepositoryServiceProvider(connection);
        await EnsureCreatedAsync(provider);

        await using var scope = provider.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository<TestEntity>>();

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => repository.ListAsync(null!, CancellationToken.None));
    }

    [Fact]
    public async Task R005_AnyAsync_WithNullPredicate_ThrowsArgumentNullException()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        using var provider = BuildRepositoryServiceProvider(connection);
        await EnsureCreatedAsync(provider);

        await using var scope = provider.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository<TestEntity>>();

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => repository.AnyAsync(null!, CancellationToken.None));
    }

    [Fact]
    public async Task R005_CountAsync_WithNullPredicate_ThrowsArgumentNullException()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        using var provider = BuildRepositoryServiceProvider(connection);
        await EnsureCreatedAsync(provider);

        await using var scope = provider.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository<TestEntity>>();

        await Assert.ThrowsAsync<ArgumentNullException>(
            () => repository.CountAsync(null!, CancellationToken.None));
    }

    [Fact]
    public async Task Q001_ListAsync_WithCombinedAndPredicate_ReturnsExpectedRows()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        using var provider = BuildRepositoryServiceProvider(connection);
        await EnsureCreatedAsync(provider);

        await using (var scope = provider.CreateAsyncScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IRepository<TestEntity>>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            await repository.AddAsync(new TestEntity { Name = "A-Enabled", Category = "A", IsEnabled = true });
            await repository.AddAsync(new TestEntity { Name = "B-Enabled", Category = "B", IsEnabled = true });
            await repository.AddAsync(new TestEntity { Name = "A-Disabled", Category = "A", IsEnabled = false });
            await repository.AddAsync(new TestEntity { Name = "C-Disabled", Category = "C", IsEnabled = false });
            _ = await unitOfWork.SaveChangesAsync();
        }

        await using (var scope = provider.CreateAsyncScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IRepository<TestEntity>>();

            var results = await repository.ListAsync(entity => entity.IsEnabled && entity.Category == "A");

            var result = Assert.Single(results);
            Assert.Equal("A-Enabled", result.Name);
        }
    }

    [Fact]
    public async Task Q002_ListAsync_WithCombinedOrPredicate_ReturnsExpectedRows()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        using var provider = BuildRepositoryServiceProvider(connection);
        await EnsureCreatedAsync(provider);

        await using (var scope = provider.CreateAsyncScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IRepository<TestEntity>>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            await repository.AddAsync(new TestEntity { Name = "A-Enabled", Category = "A", IsEnabled = true });
            await repository.AddAsync(new TestEntity { Name = "B-Enabled", Category = "B", IsEnabled = true });
            await repository.AddAsync(new TestEntity { Name = "B-Disabled", Category = "B", IsEnabled = false });
            await repository.AddAsync(new TestEntity { Name = "C-Disabled", Category = "C", IsEnabled = false });
            _ = await unitOfWork.SaveChangesAsync();
        }

        await using (var scope = provider.CreateAsyncScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IRepository<TestEntity>>();

            var results = await repository.ListAsync(entity => entity.IsEnabled || entity.Category == "B");

            Assert.Equal(3, results.Count);
            Assert.Contains(results, entity => entity.Name == "A-Enabled");
            Assert.Contains(results, entity => entity.Name == "B-Enabled");
            Assert.Contains(results, entity => entity.Name == "B-Disabled");
        }
    }

    [Fact]
    public async Task U002_AddAsync_WithoutSaveChanges_DoesNotPersist()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        using var provider = BuildRepositoryServiceProvider(connection);
        await EnsureCreatedAsync(provider);

        await using (var scope = provider.CreateAsyncScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IRepository<TestEntity>>();
            await repository.AddAsync(new TestEntity { Name = "without-save" });
        }

        await using (var scope = provider.CreateAsyncScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IRepository<TestEntity>>();
            var result = await repository.FirstOrDefaultAsync(entity => entity.Name == "without-save");

            Assert.Null(result);
        }
    }

    [Fact]
    public async Task U003_SaveChangesAsync_WhenConstraintIsViolated_ThrowsDbUpdateException()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        using var provider = BuildRepositoryServiceProvider(connection);
        await EnsureCreatedAsync(provider);

        await using (var scope = provider.CreateAsyncScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IRepository<TestEntity>>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            await repository.AddAsync(new TestEntity { Name = "duplicate-name", Category = "A", IsEnabled = true });
            _ = await unitOfWork.SaveChangesAsync();
        }

        await using (var scope = provider.CreateAsyncScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IRepository<TestEntity>>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            await repository.AddAsync(new TestEntity { Name = "duplicate-name", Category = "B", IsEnabled = false });

            _ = await Assert.ThrowsAsync<DbUpdateException>(() => unitOfWork.SaveChangesAsync());
        }
    }

    [Fact]
    public async Task U006_SaveChangesAsync_AfterScopeDisposed_ThrowsObjectDisposedException()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        using var provider = BuildRepositoryServiceProvider(connection);
        await EnsureCreatedAsync(provider);

        IUnitOfWork unitOfWork;

        await using (var scope = provider.CreateAsyncScope())
        {
            unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        }

        _ = await Assert.ThrowsAsync<ObjectDisposedException>(() => unitOfWork.SaveChangesAsync());
    }

    [Fact]
    public async Task R006_ListAsyncAndSaveChangesAsync_WithCanceledToken_ThrowsOperationCanceledException()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        using var provider = BuildRepositoryServiceProvider(connection);
        await EnsureCreatedAsync(provider);

        await using var scope = provider.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository<TestEntity>>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        await repository.AddAsync(new TestEntity { Name = "cancel-test" });

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => repository.ListAsync(entity => entity.Name == "cancel-test", cts.Token));

        await Assert.ThrowsAsync<OperationCanceledException>(() => unitOfWork.SaveChangesAsync(cts.Token));
    }

    private static ServiceProvider BuildServiceProvider(SqliteConnection connection)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
            [
                new KeyValuePair<string, string?>("ConnectionStrings:AulaPedidos", connection.ConnectionString),
            ])
            .Build();

        var services = new ServiceCollection();
        _ = services.AddInfrastructure(configuration);

        return services.BuildServiceProvider(validateScopes: true);
    }

    private static ServiceProvider BuildRepositoryServiceProvider(SqliteConnection connection)
    {
        var services = new ServiceCollection();
        _ = services.AddDbContext<TestAulaPedidosDbContext>(options => options.UseSqlite(connection));
        _ = services.AddScoped<AulaPedidosDbContext>(
            serviceProvider => serviceProvider.GetRequiredService<TestAulaPedidosDbContext>());
        _ = services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        _ = services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        return services.BuildServiceProvider(validateScopes: true);
    }

    private static async Task EnsureCreatedAsync(ServiceProvider provider)
    {
        await using var scope = provider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AulaPedidosDbContext>();
        _ = await dbContext.Database.EnsureCreatedAsync();
    }

    private sealed class TestEntity
    {
        public Guid Id { get; init; } = Guid.NewGuid();

        public string Name { get; init; } = string.Empty;

        public string Category { get; init; } = string.Empty;

        public bool IsEnabled { get; init; }
    }

    private sealed class TestAulaPedidosDbContext(DbContextOptions<TestAulaPedidosDbContext> options)
        : AulaPedidosDbContext(options)
    {
        public DbSet<TestEntity> TestEntities => Set<TestEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ArgumentNullException.ThrowIfNull(modelBuilder);

            _ = modelBuilder.Entity<TestEntity>()
                .HasIndex(entity => entity.Name)
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}
