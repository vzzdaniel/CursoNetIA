using AulaPedidos.Api.Contracts;
using AulaPedidos.Application.Contracts.BulkImport;
using AulaPedidos.Application.Products;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace AulaPedidos.Api.Endpoints;

public static class ProductEndpoints
{
    private const string BulkImportStatusRoute = "GetBulkImportStatus";
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/products").WithTags("Products");

        group.MapPost("/", async Task<Created<Result<ProductDto>>> (ProductRequest request, ISender sender, CancellationToken cancellationToken) =>
        {
            var product = await sender.Send(new CreateProduct(request.Price, request.Description, request.AlternateDescription), cancellationToken);
            return TypedResults.Created($"/products/{product.Id}", Result<ProductDto>.Success(product));
        });

        group.MapPost("/bulk", async Task<Results<AcceptedAtRoute<Result<BulkImportAccepted>>, JsonHttpResult<Result<object?>>>> (
            HttpRequest request,
            IBulkImportQueue queue,
            IBulkImportTracker tracker,
            CancellationToken cancellationToken) =>
        {
            var filePath = Path.Combine(Path.GetTempPath(), $"aulapedidos-bulk-{Guid.NewGuid():N}.json");
            var transactionId = Guid.Empty;
            var enqueued = false;
            try
            {
                await using (var file = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, FileOptions.Asynchronous))
                {
                    await request.Body.CopyToAsync(file, cancellationToken);
                }

                transactionId = tracker.Register();
                enqueued = queue.TryEnqueue(new BulkImportJob(transactionId, filePath));
            }
            finally
            {
                if (!enqueued)
                {
                    File.Delete(filePath);
                    if (transactionId != Guid.Empty)
                    {
                        tracker.Discard(transactionId);
                    }
                }
            }

            return enqueued
                ? TypedResults.AcceptedAtRoute(Result<BulkImportAccepted>.Success(new BulkImportAccepted(transactionId)), BulkImportStatusRoute, new { transactionId })
                : TypedResults.Json(Result<object?>.Failure("The bulk import queue is full. Try again later."), statusCode: StatusCodes.Status503ServiceUnavailable);
        })
        .WithMetadata(new DisableRequestSizeLimitAttribute());

        group.MapGet("/bulk/{transactionId:guid}", Results<Ok<Result<BulkImportState>>, NotFound<Result<object?>>> (Guid transactionId, IBulkImportTracker tracker) =>
        {
            var state = tracker.GetAndReleaseIfFinished(transactionId);
            return state is null
                ? TypedResults.NotFound(Result<object?>.Failure("Bulk import transaction not found."))
                : TypedResults.Ok(Result<BulkImportState>.Success(state));
        }).WithName(BulkImportStatusRoute);

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

public sealed record BulkImportAccepted(Guid TransactionId);
