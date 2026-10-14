using AulaPedidos.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AulaPedidos.Api.IntegrationTests;

internal sealed class ProductApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    private readonly string environment;
    private readonly Action<IServiceCollection>? configureServices;

    public ProductApiFactory(string environment = "Development", Action<IServiceCollection>? configureServices = null)
    {
        this.environment = environment;
        this.configureServices = configureServices;
        connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(environment);
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AulaPedidosDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AulaPedidosDbContext>>();
            services.AddDbContext<AulaPedidosDbContext>(options => options.UseSqlite(connection));
            configureServices?.Invoke(services);
        });
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await connection.DisposeAsync();
    }
}
