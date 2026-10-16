using AulaPedidos.Application.Contracts.Persistence;
using AulaPedidos.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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