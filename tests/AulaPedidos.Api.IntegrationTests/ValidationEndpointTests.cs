using System.Net;
using System.Net.Http.Json;
using AulaPedidos.Api.Errors;
using AulaPedidos.Application.Contracts.Persistence;
using AulaPedidos.Application.Products;
using AulaPedidos.Domain.Products;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        var problem = await ReadValidationProblem(response);
        Assert.Equal(new[] { "First error", "Second error" }, problem.Errors["Description"]);
        Assert.Single(problem.Errors["Price"]);
        Assert.Equal(0, saveCalls);
        Assert.DoesNotContain("SensitiveInputMarker", await response.Content.ReadAsStringAsync());
        Assert.Empty((await client.GetFromJsonAsync<ProductDto[]>("/products"))!);
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

        var problem = await ReadValidationProblem(response);
        Assert.Equal("Query rejected", Assert.Single(Assert.Single(problem.Errors).Value));
    }

    [Fact]
    public async Task InvalidUpdate_ReturnsAllFieldsWithoutChangingStoredProduct()
    {
        await using var factory = new ProductApiFactory("Production");
        using var client = factory.CreateClient();
        using var created = await client.PostAsJsonAsync("/products", new { price = 1m, description = "Original" });
        var original = (await created.Content.ReadFromJsonAsync<ProductDto>())!;

        using var response = await client.PutAsJsonAsync(created.Headers.Location, new
        {
            price = -1m, description = " ", alternateDescription = new string('x', 201),
        });

        var problem = await ReadValidationProblem(response);
        Assert.Equal(new[] { "AlternateDescription", "Description", "Price" }, problem.Errors.Keys.Order());
        Assert.Equal(original, await client.GetFromJsonAsync<ProductDto>(created.Headers.Location));
    }

    [Fact]
    public async Task DomainValidationException_StillUsesExistingBadRequestContract()
    {
        await using var factory = new ProductApiFactory("Production", services =>
            services.Replace(ServiceDescriptor.Scoped<IUnitOfWork>(_ => new CallbackUnitOfWork(() =>
                throw new ProductValidationException("Domain rule failed")))));
        using var client = factory.CreateClient();

        using var response = await client.PostAsJsonAsync("/products", new { price = 1m, description = "Valid" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = (await response.Content.ReadFromJsonAsync<ProblemDetails>())!;
        Assert.Equal("Invalid product", problem.Title);
        Assert.Equal("Domain rule failed", problem.Detail);
    }

    [Fact]
    public async Task ValidationExceptionHandler_DoesNotHandleCancellationOrUnexpectedFailures()
    {
        await using var factory = new ProductApiFactory();
        using var client = factory.CreateClient();
        var handler = ActivatorUtilities.CreateInstance<ValidationExceptionHandler>(factory.Services);
        var context = new DefaultHttpContext();

        Assert.False(await handler.TryHandleAsync(context, new OperationCanceledException(), default));
        Assert.False(await handler.TryHandleAsync(context, new InvalidOperationException(), default));
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    private static async Task<ValidationProblemDetails> ReadValidationProblem(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType!.MediaType);
        var problem = (await response.Content.ReadFromJsonAsync<ValidationProblemDetails>())!;
        Assert.Equal(400, problem.Status);
        Assert.Equal("Validation failed", problem.Title);
        Assert.Null(problem.Detail);
        Assert.NotEmpty(problem.Errors);
        return problem;
    }

    private sealed class CallbackUnitOfWork(Func<int> save) : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(save());
    }
}
