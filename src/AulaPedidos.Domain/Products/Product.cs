namespace AulaPedidos.Domain.Products;

public sealed class Product
{
    public const int MaximumDescriptionLength = 200;

    private Product() { }

    public Guid Id { get; private set; }
    public decimal Price { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string? AlternateDescription { get; private set; }

    public static Product Create(decimal price, string? description, string? alternateDescription)
    {
        var product = new Product();
        product.Update(price, description, alternateDescription);
        product.Id = Guid.NewGuid();
        return product;
    }

    public void Update(decimal price, string? description, string? alternateDescription)
    {
        var normalizedDescription = description?.Trim();
        var normalizedAlternateDescription = string.IsNullOrWhiteSpace(alternateDescription)
            ? null : alternateDescription.Trim();

        if (string.IsNullOrEmpty(normalizedDescription) || normalizedDescription.Length > MaximumDescriptionLength)
        {
            throw new ProductValidationException("Description is required and must contain at most 200 characters.");
        }

        if (normalizedAlternateDescription?.Length > MaximumDescriptionLength)
        {
            throw new ProductValidationException("AlternateDescription must contain at most 200 characters.");
        }

        if (price < 0)
        {
            throw new ProductValidationException("Price must be non-negative.");
        }

        Price = price;
        Description = normalizedDescription;
        AlternateDescription = normalizedAlternateDescription;
    }
}