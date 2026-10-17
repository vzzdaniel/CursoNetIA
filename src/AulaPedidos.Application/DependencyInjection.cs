using AulaPedidos.Application.Products.BulkImport;
using FluentValidation;
using Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace AulaPedidos.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddScoped<ISender>(serviceProvider => serviceProvider.GetRequiredService<IMediator>());
        services.AddScoped<IPublisher>(serviceProvider => serviceProvider.GetRequiredService<IMediator>());
        services.AddScoped<BulkProductImportProcessor>();

        return services;
    }
}
