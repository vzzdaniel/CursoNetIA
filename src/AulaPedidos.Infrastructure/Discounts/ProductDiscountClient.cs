using AulaPedidos.Application.Contracts.Discounts;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace AulaPedidos.Infrastructure.Discounts;

public sealed class ProductDiscountClient(HttpClient httpClient, ILogger<ProductDiscountClient> logger) : IProductDiscountClient
{
    private sealed record DiscountResponse(int Percentage);

    public async ValueTask<int?> GetDiscountPercentageAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            var response = await httpClient.GetFromJsonAsync<DiscountResponse>("mock/discount", cancellationToken);
            return response?.Percentage;
        }
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(exception, "Discount service unavailable; continuing without discount.");
            return null;
        }
    }
}
