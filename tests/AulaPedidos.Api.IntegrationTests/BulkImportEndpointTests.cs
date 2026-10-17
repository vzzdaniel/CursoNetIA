using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using AulaPedidos.Api.Contracts;
using AulaPedidos.Api.Endpoints;
using AulaPedidos.Application.Contracts.BulkImport;
using AulaPedidos.Application.Products;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AulaPedidos.Api.IntegrationTests;

public class BulkImportEndpointTests
{
    [Fact]
    public async Task PostBulk_ReturnsAcceptedAndReleasesTransactionAfterFinalStatus()
    {
        await using var factory = new ProductApiFactory();
        using var client = factory.CreateClient();
        const string payload = """
            [
              { "price": 1.5, "description": "Uno", "alternateDescription": null },
              { "price": 2, "description": "Dos" },
              { "price": -1, "description": "Invalido" }
            ]
            """;

        using var accepted = await PostBulkAsync(client, payload);

        Assert.Equal(HttpStatusCode.Accepted, accepted.StatusCode);
        var transactionId = (await accepted.Content.ReadFromJsonAsync<Result<BulkImportAccepted>>(JsonSerializerOptions.Web))!.Data!.TransactionId;
        Assert.NotEqual(Guid.Empty, transactionId);
        Assert.EndsWith($"/products/bulk/{transactionId}", accepted.Headers.Location!.OriginalString, StringComparison.Ordinal);

        var state = await WaitForFinalStateAsync(client, accepted.Headers.Location!);
        Assert.Equal(BulkImportStatus.Completed, state.Status);
        Assert.Equal(2, state.Processed);
        Assert.Equal(1, state.Failed);

        using var released = await client.GetAsync(accepted.Headers.Location);
        Assert.Equal(HttpStatusCode.NotFound, released.StatusCode);

        using var listed = await client.GetAsync("/products");
        var products = (await listed.Content.ReadFromJsonAsync<Result<IReadOnlyList<ProductDto>>>(JsonSerializerOptions.Web))!.Data!;
        Assert.Equal(2, products.Count);
    }

    [Fact]
    public async Task PostBulk_WithMalformedPayload_ReportsFailedStatus()
    {
        await using var factory = new ProductApiFactory();
        using var client = factory.CreateClient();

        using var accepted = await PostBulkAsync(client, "{ not json");

        Assert.Equal(HttpStatusCode.Accepted, accepted.StatusCode);
        var state = await WaitForFinalStateAsync(client, accepted.Headers.Location!);
        Assert.Equal(BulkImportStatus.Failed, state.Status);
        Assert.NotNull(state.Error);
    }

    [Fact]
    public async Task GetBulk_WithUnknownTransaction_ReturnsNotFound()
    {
        await using var factory = new ProductApiFactory();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync($"/products/bulk/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PostBulk_WhenQueueIsFull_ReturnsServiceUnavailableAndLeavesNoTransaction()
    {
        await using var factory = new ProductApiFactory(configureServices: services =>
        {
            services.RemoveAll<IBulkImportQueue>();
            services.AddSingleton<IBulkImportQueue, FullQueue>();
        });
        using var client = factory.CreateClient();

        using var response = await PostBulkAsync(client, "[]");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }

    private static Task<HttpResponseMessage> PostBulkAsync(HttpClient client, string payload) =>
        client.PostAsync("/products/bulk", new StringContent(payload, Encoding.UTF8, "application/json"));

    private static async Task<BulkImportState> WaitForFinalStateAsync(HttpClient client, Uri location)
    {
        var deadline = DateTimeOffset.UtcNow.AddSeconds(30);
        while (DateTimeOffset.UtcNow < deadline)
        {
            using var response = await client.GetAsync(location);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var state = (await response.Content.ReadFromJsonAsync<Result<BulkImportState>>(JsonSerializerOptions.Web))!.Data!;
            if (state.Status is BulkImportStatus.Completed or BulkImportStatus.Failed)
            {
                return state;
            }

            await Task.Delay(50);
        }

        throw new TimeoutException("The bulk import did not finish in time.");
    }

    private sealed class FullQueue : IBulkImportQueue
    {
        public bool TryEnqueue(BulkImportJob job) => false;

        public async IAsyncEnumerable<BulkImportJob> DequeueAllAsync([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await Task.Delay(Timeout.Infinite, cancellationToken);
            yield break;
        }
    }
}
