using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AulaPedidos.Api.IntegrationTests;

public class ApiTests
{
    [Theory]
    [InlineData("Development", HttpStatusCode.OK)]
    [InlineData("Staging", HttpStatusCode.NotFound)]
    [InlineData("Production", HttpStatusCode.NotFound)]
    public async Task OpenApi_ShouldBeEnvironmentDependent(string environment, HttpStatusCode expectedStatusCode)
    {
        await using var factory = CreateFactory(environment);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/openapi/v1.json");

        Assert.Equal(expectedStatusCode, response.StatusCode);
    }

    [Theory]
    [InlineData("Development")]
    [InlineData("Staging")]
    [InlineData("Production")]
    public async Task Health_ShouldBeAvailableInAllEnvironments(string environment)
    {
        await using var factory = CreateFactory(environment);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static WebApplicationFactory<Program> CreateFactory(string environment) =>
        new ProductApiFactory(environment);
}
