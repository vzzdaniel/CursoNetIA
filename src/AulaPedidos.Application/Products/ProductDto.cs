using AulaPedidos.Domain.Products;

namespace AulaPedidos.Application.Products;

public sealed record ProductDto(Guid Id, decimal Price, string Description, string? AlternateDescription, int? DiscountPercentage = null, decimal? DiscountedPrice = null)
{
    public ProductDto WithDiscount(int? discountPercentage) =>
        discountPercentage is null
            ? this
            : this with
            {
                DiscountPercentage = discountPercentage,
                DiscountedPrice = Math.Round(Price * (100 - discountPercentage.Value) / 100m, 2),
            };

    public static ProductDto FromProduct(Product product) =>
        new(product.Id, product.Price, product.Description, product.AlternateDescription);
}