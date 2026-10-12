using AulaPedidos.Application.Contracts.Persistence;
using AulaPedidos.Infrastructure.Persistence;
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
        //test
        var connectionString = configuration.GetConnectionString("AulaPedidos");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = "Data Source=aulapedidos.db";
        }

        _ = services.AddDbContext<AulaPedidosDbContext>(options => options.UseSqlite(connectionString));
        _ = services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        _ = services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        return services;
    }
}