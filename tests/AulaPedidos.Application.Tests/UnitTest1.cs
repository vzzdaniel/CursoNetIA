using Mediator;
using Microsoft.Extensions.DependencyInjection;

namespace AulaPedidos.Application.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddApplication_WithNullServices_ThrowsArgumentNullException()
    {
        var action = () => DependencyInjection.AddApplication(null!);

        _ = Assert.Throws<ArgumentNullException>(action);
    }

    [Fact]
    public void AddApplication_RegistersSenderAndPublisher()
    {
        var services = new ServiceCollection();

        services.AddApplication();

        var senderRegistration = services.SingleOrDefault(descriptor => descriptor.ServiceType == typeof(ISender));
        var publisherRegistration = services.SingleOrDefault(descriptor => descriptor.ServiceType == typeof(IPublisher));

        Assert.NotNull(senderRegistration);
        Assert.NotNull(publisherRegistration);
    }
}
