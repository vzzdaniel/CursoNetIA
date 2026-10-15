using System.Net;
using System.Net.Http.Json;
using AulaPedidos.Application.Contracts.Persistence;
using AulaPedidos.Application.Products;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AulaPedidos.Api.IntegrationTests;

public class ProductEndpointTests
{
    [Fact]
    public async Task Crud_PersistsAcrossRequestsAndReturnsExpectedStatuses()
    {
        await using var factory = new ProductApiFactory();
        using var client = factory.CreateClient();

        Assert.Empty((await client.GetFromJsonAsync<ProductDto[]>("/products"))!);
        using var created = await client.PostAsJsonAsync("/products", new { price = 12.34m, description = " Lápiz ", alternateDescription = " Pencil " });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var product = (await created.Content.ReadFromJsonAsync<ProductDto>())!;
        Assert.NotEqual(Guid.Empty, product.Id);
        Assert.Equal($"/products/{product.Id}", created.Headers.Location!.OriginalString);
        Assert.Equal("Lápiz", product.Description);
        Assert.Equal("Pencil", product.AlternateDescription);
        Assert.Equal(product, await client.GetFromJsonAsync<ProductDto>(created.Headers.Location));

        using var updated = await client.PutAsJsonAsync(created.Headers.Location, new { price = 0m, description = " Nuevo ", alternateDescription = " " });
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);
        var changed = (await updated.Content.ReadFromJsonAsync<ProductDto>())!;
        Assert.Equal(product.Id, changed.Id);
        Assert.Equal(0m, changed.Price);
        Assert.Equal("Nuevo", changed.Description);
        Assert.Null(changed.AlternateDescription);
        Assert.Equal(changed, await client.GetFromJsonAsync<ProductDto>(created.Headers.Location));
        Assert.Equal(changed, Assert.Single((await client.GetFromJsonAsync<ProductDto[]>("/products"))!));

        using var deleted = await client.DeleteAsync(created.Headers.Location);
        Assert.Equal(HttpStatusCode.NoContent, deleted.StatusCode);
        using var missing = await client.GetAsync(created.Headers.Location);
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        Assert.Empty((await client.GetFromJsonAsync<ProductDto[]>("/products"))!);
    }

    [Theory]
    [InlineData(null, null, 1)]
    [InlineData("", null, 1)]
    [InlineData("   ", null, 1)]
    [InlineData("Valid", null, -1)]
    public async Task Create_InvalidProductReturnsProblemDetailsWithoutSaving(string? description, string? alternateDescription, decimal price)
    {
        await using var factory = new ProductApiFactory();
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync("/products", new { price, description, alternateDescription });

        await AssertValidationProblem(response);
        Assert.Empty((await client.GetFromJsonAsync<ProductDto[]>("/products"))!);
    }

    [Fact]
    public async Task DescriptionLimitsAndInvalidUpdateAreEnforcedWithoutPartialWrites()
    {
        await using var factory = new ProductApiFactory();
        using var client = factory.CreateClient();
        using var created = await client.PostAsJsonAsync("/products", new { price = 0m, description = new string('x', 200), alternateDescription = new string('y', 200) });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var product = (await created.Content.ReadFromJsonAsync<ProductDto>())!;

        using var invalidDescription = await client.PostAsJsonAsync("/products", new { price = 1m, description = new string('x', 201) });
        using var invalidAlternate = await client.PostAsJsonAsync("/products", new { price = 1m, description = "Valid", alternateDescription = new string('x', 201) });
        using var invalidUpdate = await client.PutAsJsonAsync(created.Headers.Location, new { price = -0.01m, description = "Changed" });
        await AssertValidationProblem(invalidDescription);
        await AssertValidationProblem(invalidAlternate);
        await AssertValidationProblem(invalidUpdate);
        Assert.Equal(product, await client.GetFromJsonAsync<ProductDto>(created.Headers.Location));
        Assert.Single((await client.GetFromJsonAsync<ProductDto[]>("/products"))!);
    }

    [Fact]
    public async Task Create_AllowsMissingAlternateDescriptionAndExactDecimalRoundTrip()
    {
        await using var factory = new ProductApiFactory();
        using var client = factory.CreateClient();
        using var response = await client.PostAsJsonAsync("/products", new { price = 12.34m, description = "Valid" });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var product = (await client.GetFromJsonAsync<ProductDto>(response.Headers.Location))!;
        Assert.Null(product.AlternateDescription);
        Assert.Equal(12.34m, product.Price);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task MissingOrMalformedIdsReturnNotFound(string id)
    {
        await using var factory = new ProductApiFactory();
        using var client = factory.CreateClient();
        using var get = await client.GetAsync($"/products/{id}");
        using var put = await client.PutAsJsonAsync($"/products/{id}", new { price = 1m, description = "Valid" });
        using var delete = await client.DeleteAsync($"/products/{id}");
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
        Assert.Equal(id == Guid.Empty.ToString() ? HttpStatusCode.BadRequest : HttpStatusCode.NotFound, put.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, delete.StatusCode);
    }

    [Fact]
    public async Task MalformedJsonIsBadRequest()
    {
        await using var factory = new ProductApiFactory("Production");
        using var client = factory.CreateClient();
        using var content = new StringContent("{\"price\":\"invalid\"}", System.Text.Encoding.UTF8, "application/json");
        using var response = await client.PostAsync("/products", content);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task TechnicalFailureReturnsGenericProblemWithoutExceptionDetails()
    {
        await using var factory = new ProductApiFactory("Production", services =>
            services.Replace(ServiceDescriptor.Scoped<IUnitOfWork, FailingUnitOfWork>()));
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync("/products", new { price = 1m, description = "Valid" });

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType!.MediaType);
        var problem = (await response.Content.ReadFromJsonAsync<ProblemDetails>())!;
        Assert.Equal(500, problem.Status);
        Assert.Null(problem.Detail);
        Assert.DoesNotContain("TechnicalFailureMarker", await response.Content.ReadAsStringAsync());
        Assert.Empty((await client.GetFromJsonAsync<ProductDto[]>("/products"))!);
    }

    private sealed class FailingUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("TechnicalFailureMarker");
    }

    private static async Task AssertValidationProblem(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType!.MediaType);
        var problem = (await response.Content.ReadFromJsonAsync<ValidationProblemDetails>())!;
        Assert.Equal(400, problem.Status);
        Assert.Equal("Validation failed", problem.Title);
        Assert.NotEmpty(problem.Errors);
        Assert.Null(problem.Detail);
    }
}
