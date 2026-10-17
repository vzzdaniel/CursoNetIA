using AulaPedidos.Application.Contracts.BulkImport;
using AulaPedidos.Application.Contracts.Discounts;
using AulaPedidos.Application.Contracts.Persistence;
using AulaPedidos.Infrastructure.BulkImport;
using AulaPedidos.Infrastructure.Discounts;
using AulaPedidos.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace AulaPedidos.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString("AulaPedidos");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = "Data Source=App_Data/aulapedidos.db";
        }

        EnsureDatabaseDirectoryExists(connectionString);

        _ = services.AddDbContext<AulaPedidosDbContext>(options => options.UseSqlite(connectionString));
        _ = services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        _ = services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        _ = services.AddScoped<IProductBulkWriter, EfProductBulkWriter>();
        _ = services.AddSingleton(TimeProvider.System);
        _ = services.AddSingleton<IBulkImportTracker, InMemoryBulkImportTracker>();
        _ = services.AddSingleton<IBulkImportQueue, ChannelBulkImportQueue>();
        _ = services.AddHostedService<BulkProductImportWorker>();

        var discountBaseUrl = configuration["ProductDiscount:BaseUrl"];
        if (string.IsNullOrWhiteSpace(discountBaseUrl))
        {
            discountBaseUrl = "http://localhost:5193";
        }

        _ = services.AddHttpClient<IProductDiscountClient, ProductDiscountClient>(client => client.BaseAddress = new Uri(discountBaseUrl))
            .AddResilienceHandler("product-discount", pipeline =>
            {
                _ = pipeline.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
                {
                    SamplingDuration = TimeSpan.FromSeconds(10),
                    MinimumThroughput = 5,
                    FailureRatio = 1.0,
                    BreakDuration = TimeSpan.FromSeconds(15),
                });
                _ = pipeline.AddTimeout(TimeSpan.FromSeconds(2));
            });

        return services;
    }

    private static void EnsureDatabaseDirectoryExists(string connectionString)
    {
        var dataSource = new SqliteConnectionStringBuilder(connectionString).DataSource;
        if (string.IsNullOrWhiteSpace(dataSource)
            || dataSource.Equals(":memory:", StringComparison.OrdinalIgnoreCase)
            || dataSource.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var directory = Path.GetDirectoryName(Path.GetFullPath(dataSource));
        if (!string.IsNullOrEmpty(directory))
        {
            _ = Directory.CreateDirectory(directory);
        }
    }
}