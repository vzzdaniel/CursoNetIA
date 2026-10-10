using Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace AulaPedidos.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<ISender>(serviceProvider => serviceProvider.GetRequiredService<IMediator>());
        services.AddScoped<IPublisher>(serviceProvider => serviceProvider.GetRequiredService<IMediator>());

        return services;
    }
}