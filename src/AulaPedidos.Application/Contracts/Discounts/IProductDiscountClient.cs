namespace AulaPedidos.Application.Contracts.Discounts;

public interface IProductDiscountClient
{
    /// <summary>Returns the discount percentage, or null when the discount service is unavailable.</summary>
    ValueTask<int?> GetDiscountPercentageAsync(CancellationToken cancellationToken);
}
