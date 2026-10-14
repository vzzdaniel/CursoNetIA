using AulaPedidos.Application.Products;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AulaPedidos.Api.Endpoints;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/products").WithTags("Products");

        group.MapPost("/", async Task<Created<ProductDto>> (ProductRequest request, ISender sender, CancellationToken cancellationToken) =>
        {
            var product = await sender.Send(new CreateProduct(request.Price, request.Description, request.AlternateDescription), cancellationToken);
            return TypedResults.Created($"/products/{product.Id}", product);
        }).ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/", async (ISender sender, CancellationToken cancellationToken) =>
            TypedResults.Ok(await sender.Send(new GetProducts(), cancellationToken)));

        group.MapGet("/{id:guid}", async Task<Results<Ok<ProductDto>, NotFound>> (Guid id, ISender sender, CancellationToken cancellationToken) =>
        {
            var product = await sender.Send(new GetProductById(id), cancellationToken);
            return product is null ? TypedResults.NotFound() : TypedResults.Ok(product);
        });

        group.MapPut("/{id:guid}", async Task<Results<Ok<ProductDto>, NotFound>> (Guid id, ProductRequest request, ISender sender, CancellationToken cancellationToken) =>
        {
            var product = await sender.Send(new UpdateProduct(id, request.Price, request.Description, request.AlternateDescription), cancellationToken);
            return product is null ? TypedResults.NotFound() : TypedResults.Ok(product);
        }).ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapDelete("/{id:guid}", async Task<Results<NoContent, NotFound>> (Guid id, ISender sender, CancellationToken cancellationToken) =>
            await sender.Send(new DeleteProduct(id), cancellationToken) ? TypedResults.NoContent() : TypedResults.NotFound());

        return endpoints;
    }
}

public sealed record ProductRequest(decimal Price, string? Description, string? AlternateDescription);
