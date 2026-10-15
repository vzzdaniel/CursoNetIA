using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AulaPedidos.Api.Contracts;
using AulaPedidos.Api.Errors;
using AulaPedidos.Application.Contracts.Persistence;
using AulaPedidos.Application.Products;
using AulaPedidos.Domain.Products;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AulaPedidos.Api.IntegrationTests;

public class ValidationEndpointTests
{
    [Fact]
    public async Task RealPipeline_AggregatesRegisteredValidatorsAndSkipsSaving()
    {
        var saveCalls = 0;
        var first = new InlineValidator<CreateProduct>();
        first.RuleFor(command => command.Description).Must(_ => false).WithMessage("First error");
        var second = new InlineValidator<CreateProduct>();
        second.RuleFor(command => command.Description).Must(_ => false).WithMessage("Second error");
        await using var factory = new ProductApiFactory("Production", services =>
        {
            services.AddScoped<IValidator<CreateProduct>>(_ => first);
            services.AddScoped<IValidator<CreateProduct>>(_ => second);
            services.Replace(ServiceDescriptor.Scoped<IUnitOfWork>(_ => new CallbackUnitOfWork(() =>
            {
                saveCalls++;
                return 0;
            })));
        });
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync("/products", new { price = -1m, description = "SensitiveInputMarker" });

        var result = await ReadValidationResult(response);
        Assert.Equal(new[] { "First error", "Second error" }, result.Data!["Description"]);
        Assert.Single(result.Data["Price"]);
        Assert.Equal(0, saveCalls);
        Assert.DoesNotContain("SensitiveInputMarker", await response.Content.ReadAsStringAsync());
        using var productsResponse = await client.GetAsync("/products");
        Assert.Empty((await ReadProductsResult(productsResponse)).Data!);
    }

    [Fact]
    public async Task RealPipeline_ValidatesQueriesWithRegisteredValidators()
    {
        var validator = new InlineValidator<GetProducts>();
        validator.RuleFor(query => query).Must(_ => false).WithMessage("Query rejected");
        await using var factory = new ProductApiFactory("Production", services =>
            services.AddScoped<IValidator<GetProducts>>(_ => validator));
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/products");

        var result = await ReadValidationResult(response);
        Assert.Equal("Query rejected", Assert.Single(Assert.Single(result.Data!).Value));
    }

    [Fact]
    public async Task InvalidUpdate_ReturnsAllFieldsWithoutChangingStoredProduct()
    {
        await using var factory = new ProductApiFactory("Production");
        using var client = factory.CreateClient();
        using var created = await client.PostAsJsonAsync("/products", new { price = 1m, description = "Original" });
        var original = (await created.Content.ReadFromJsonAsync<Result<ProductDto>>())!.Data!;

        using var response = await client.PutAsJsonAsync(created.Headers.Location, new
        {
            price = -1m, description = " ", alternateDescription = new string('x', 201),
        });

        var result = await ReadValidationResult(response);
        Assert.Equal(new[] { "AlternateDescription", "Description", "Price" }, result.Data!.Keys.Order());
        using var currentResponse = await client.GetAsync(created.Headers.Location);
        Assert.Equal(original, (await currentResponse.Content.ReadFromJsonAsync<Result<ProductDto>>())!.Data);
    }

    [Fact]
    public async Task DomainValidationException_ReturnsResultContract()
    {
        await using var factory = new ProductApiFactory("Production", services =>
            services.Replace(ServiceDescriptor.Scoped<IUnitOfWork>(_ => new CallbackUnitOfWork(() =>
                throw new ProductValidationException("Domain rule failed")))));
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync("/products", new { price = 1m, description = "Valid" });

        var result = await ReadResult<object?>(response, HttpStatusCode.BadRequest);
        Assert.False(result.IsSuccess);
        Assert.Equal("Domain rule failed", result.Message);
        Assert.Null(result.Data);
    }

    [Fact]
    public async Task ValidationExceptionHandler_DoesNotHandleCancellationOrUnexpectedFailures()
    {
        await using var factory = new ProductApiFactory();
        var handler = new ValidationExceptionHandler();
        var context = new DefaultHttpContext();

        Assert.False(await handler.TryHandleAsync(context, new OperationCanceledException(), default));
        Assert.False(await handler.TryHandleAsync(context, new InvalidOperationException(), default));
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    private static async Task<Result<IReadOnlyDictionary<string, string[]>>> ReadValidationResult(HttpResponseMessage response)
    {
        var result = await ReadResult<IReadOnlyDictionary<string, string[]>>(response, HttpStatusCode.BadRequest);
        Assert.False(result.IsSuccess);
        Assert.Equal("Validation failed.", result.Message);
        Assert.NotNull(result.Data);
        Assert.NotEmpty(result.Data);
        return result;
    }

    private static async Task<Result<IReadOnlyList<ProductDto>>> ReadProductsResult(HttpResponseMessage response)
    {
        var result = await ReadResult<IReadOnlyList<ProductDto>>(response, HttpStatusCode.OK);
        Assert.True(result.IsSuccess);
        Assert.Null(result.Message);
        return result;
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

    private sealed class CallbackUnitOfWork(Func<int> save) : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(save());
    }
}
