using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AulaPedidos.Api.Contracts;
using AulaPedidos.Application.Contracts.Persistence;
using AulaPedidos.Application.Products;
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

        using var initialResponse = await client.GetAsync("/products");
        var initial = await ReadResult<IReadOnlyList<ProductDto>>(initialResponse, HttpStatusCode.OK);
        Assert.True(initial.IsSuccess);
        Assert.Null(initial.Message);
        Assert.Empty(initial.Data!);

        using var created = await client.PostAsJsonAsync("/products", new { price = 12.34m, description = " Lápiz ", alternateDescription = " Pencil " });
        var createdResult = await ReadResult<ProductDto>(created, HttpStatusCode.Created);
        var product = createdResult.Data!;
        Assert.NotEqual(Guid.Empty, product.Id);
        Assert.Equal($"/products/{product.Id}", created.Headers.Location!.OriginalString);
        Assert.Equal("Lápiz", product.Description);
        Assert.Equal("Pencil", product.AlternateDescription);

        using var fetchedCreated = await client.GetAsync(created.Headers.Location);
        Assert.Equal(product, (await ReadResult<ProductDto>(fetchedCreated, HttpStatusCode.OK)).Data);

        using var updated = await client.PutAsJsonAsync(created.Headers.Location, new { price = 0m, description = " Nuevo ", alternateDescription = " " });
        var changed = (await ReadResult<ProductDto>(updated, HttpStatusCode.OK)).Data!;
        Assert.Equal(product.Id, changed.Id);
        Assert.Equal(0m, changed.Price);
        Assert.Equal("Nuevo", changed.Description);
        Assert.Null(changed.AlternateDescription);

        using var fetchedUpdated = await client.GetAsync(created.Headers.Location);
        Assert.Equal(changed, (await ReadResult<ProductDto>(fetchedUpdated, HttpStatusCode.OK)).Data);

        using var listed = await client.GetAsync("/products");
        Assert.Equal(changed, Assert.Single((await ReadResult<IReadOnlyList<ProductDto>>(listed, HttpStatusCode.OK)).Data!));

        using var deleted = await client.DeleteAsync(created.Headers.Location);
        var deleteResult = await ReadResult<object?>(deleted, HttpStatusCode.OK);
        Assert.True(deleteResult.IsSuccess);
        Assert.Null(deleteResult.Message);
        Assert.Null(deleteResult.Data);

        using var missing = await client.GetAsync(created.Headers.Location);
        var missingResult = await ReadResult<object?>(missing, HttpStatusCode.NotFound);
        Assert.False(missingResult.IsSuccess);
        Assert.Equal("Product not found.", missingResult.Message);
        Assert.Null(missingResult.Data);

        using var afterDelete = await client.GetAsync("/products");
        Assert.Empty((await ReadResult<IReadOnlyList<ProductDto>>(afterDelete, HttpStatusCode.OK)).Data!);
    }

    [Theory]
    [InlineData(null, null, 1)]
    [InlineData("", null, 1)]
    [InlineData("   ", null, 1)]
    [InlineData("Valid", null, -1)]
    public async Task Create_InvalidProductReturnsResultWithoutSaving(string? description, string? alternateDescription, decimal price)
    {
        await using var factory = new ProductApiFactory();
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync("/products", new { price, description, alternateDescription });

        await AssertValidationResult(response);
        using var listResponse = await client.GetAsync("/products");
        Assert.Empty((await ReadResult<IReadOnlyList<ProductDto>>(listResponse, HttpStatusCode.OK)).Data!);
    }

    [Fact]
    public async Task DescriptionLimitsAndInvalidUpdateAreEnforcedWithoutPartialWrites()
    {
        await using var factory = new ProductApiFactory();
        using var client = factory.CreateClient();
        using var created = await client.PostAsJsonAsync("/products", new { price = 0m, description = new string('x', 200), alternateDescription = new string('y', 200) });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var product = (await ReadResult<ProductDto>(created, HttpStatusCode.Created)).Data!;

        using var invalidDescription = await client.PostAsJsonAsync("/products", new { price = 1m, description = new string('x', 201) });
        using var invalidAlternate = await client.PostAsJsonAsync("/products", new { price = 1m, description = "Valid", alternateDescription = new string('x', 201) });
        using var invalidUpdate = await client.PutAsJsonAsync(created.Headers.Location, new { price = -0.01m, description = "Changed" });
        await AssertValidationResult(invalidDescription);
        await AssertValidationResult(invalidAlternate);
        await AssertValidationResult(invalidUpdate);

        using var fetchResponse = await client.GetAsync(created.Headers.Location);
        Assert.Equal(product, (await ReadResult<ProductDto>(fetchResponse, HttpStatusCode.OK)).Data);

        using var listResponse = await client.GetAsync("/products");
        Assert.Single((await ReadResult<IReadOnlyList<ProductDto>>(listResponse, HttpStatusCode.OK)).Data!);
    }

    [Fact]
    public async Task Create_AllowsMissingAlternateDescriptionAndExactDecimalRoundTrip()
    {
        await using var factory = new ProductApiFactory();
        using var client = factory.CreateClient();
        using var response = await client.PostAsJsonAsync("/products", new { price = 12.34m, description = "Valid" });
        var created = await ReadResult<ProductDto>(response, HttpStatusCode.Created);

        using var getResponse = await client.GetAsync($"/products/{created.Data!.Id}");
        var product = (await ReadResult<ProductDto>(getResponse, HttpStatusCode.OK)).Data!;
        Assert.Null(product.AlternateDescription);
        Assert.Equal(12.34m, product.Price);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task MissingOrMalformedIdsReturnExpectedStatuses(string id)
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
    public async Task TechnicalFailureReturnsGenericResultWithoutExceptionDetails()
    {
        await using var factory = new ProductApiFactory("Production", services =>
            services.Replace(ServiceDescriptor.Scoped<IUnitOfWork, FailingUnitOfWork>()));
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync("/products", new { price = 1m, description = "Valid" });

        var result = await ReadResult<object?>(response, HttpStatusCode.InternalServerError);
        Assert.False(result.IsSuccess);
        Assert.Equal("An unexpected error occurred.", result.Message);
        Assert.Null(result.Data);
        Assert.DoesNotContain("TechnicalFailureMarker", await response.Content.ReadAsStringAsync());

        using var listResponse = await client.GetAsync("/products");
        Assert.Empty((await ReadResult<IReadOnlyList<ProductDto>>(listResponse, HttpStatusCode.OK)).Data!);
    }

    private sealed class FailingUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("TechnicalFailureMarker");
    }

    private static async Task AssertValidationResult(HttpResponseMessage response)
    {
        var result = await ReadResult<IReadOnlyDictionary<string, string[]>>(response, HttpStatusCode.BadRequest);
        Assert.False(result.IsSuccess);
        Assert.Equal("Validation failed.", result.Message);
        Assert.NotNull(result.Data);
        Assert.NotEmpty(result.Data);
    }

    private static async Task<Result<T>> ReadResult<T>(HttpResponseMessage response, HttpStatusCode expectedStatus)
    {
        Assert.Equal(expectedStatus, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        var json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);
        Assert.True(document.RootElement.TryGetProperty("message", out _));
        Assert.True(document.RootElement.TryGetProperty("data", out _));
        var result = JsonSerializer.Deserialize<Result<T>>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.NotNull(result);
        return result;
    }
}