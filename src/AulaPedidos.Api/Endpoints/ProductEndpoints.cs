using AulaPedidos.Api.Contracts;
using AulaPedidos.Application.Products;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AulaPedidos.Api.Endpoints;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/products").WithTags("Products");

        group.MapPost("/", async Task<Created<Result<ProductDto>>> (ProductRequest request, ISender sender, CancellationToken cancellationToken) =>
        {
            var product = await sender.Send(new CreateProduct(request.Price, request.Description, request.AlternateDescription), cancellationToken);
            return TypedResults.Created($"/products/{product.Id}", Result<ProductDto>.Success(product));
        });

        group.MapGet("/", async Task<Ok<Result<IReadOnlyList<ProductDto>>>> (ISender sender, CancellationToken cancellationToken) =>
            TypedResults.Ok(Result<IReadOnlyList<ProductDto>>.Success(await sender.Send(new GetProducts(), cancellationToken))));

        group.MapGet("/{id:guid}", async Task<Results<Ok<Result<ProductDto>>, NotFound<Result<object?>>>> (Guid id, ISender sender, CancellationToken cancellationToken) =>
        {
            var product = await sender.Send(new GetProductById(id), cancellationToken);
            return product is null
                ? TypedResults.NotFound(Result<object?>.Failure("Product not found."))
                : TypedResults.Ok(Result<ProductDto>.Success(product));
        });

        group.MapPut("/{id:guid}", async Task<Results<Ok<Result<ProductDto>>, NotFound<Result<object?>>>> (Guid id, ProductRequest request, ISender sender, CancellationToken cancellationToken) =>
        {
            var product = await sender.Send(new UpdateProduct(id, request.Price, request.Description, request.AlternateDescription), cancellationToken);
            return product is null
                ? TypedResults.NotFound(Result<object?>.Failure("Product not found."))
                : TypedResults.Ok(Result<ProductDto>.Success(product));
        });

        group.MapDelete("/{id:guid}", async Task<Results<Ok<Result<object?>>, NotFound<Result<object?>>>> (Guid id, ISender sender, CancellationToken cancellationToken) =>
            await sender.Send(new DeleteProduct(id), cancellationToken)
                ? TypedResults.Ok(Result<object?>.Success(data: null))
                : TypedResults.NotFound(Result<object?>.Failure("Product not found.")));

        return endpoints;
    }
}

public sealed record ProductRequest(decimal Price, string? Description, string? AlternateDescription);
