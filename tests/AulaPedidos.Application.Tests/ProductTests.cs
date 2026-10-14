using AulaPedidos.Domain.Products;

namespace AulaPedidos.Application.Tests;

public class ProductTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(" Pencil ")]
    public void Create_NormalizesDescriptionsAndAllowsZeroPrice(string? alternateDescription)
    {
        var product = Product.Create(0, " Lápiz ", alternateDescription);

        Assert.NotEqual(Guid.Empty, product.Id);
        Assert.Equal(0, product.Price);
        Assert.Equal("Lápiz", product.Description);
        Assert.Equal(string.IsNullOrWhiteSpace(alternateDescription) ? null : "Pencil", product.AlternateDescription);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_RejectsMissingDescription(string? description)
    {
        Assert.Throws<ProductValidationException>(() => Product.Create(1, description, null));
    }

    [Fact]
    public void Update_ValidatesBeforeChangingStateAndPreservesId()
    {
        var product = Product.Create(12.34m, "Original", "Alternative");
        var id = product.Id;

        Assert.Throws<ProductValidationException>(() => product.Update(-0.01m, "New", null));
        Assert.Throws<ProductValidationException>(() => product.Update(1, new string('x', 201), null));
        Assert.Throws<ProductValidationException>(() => product.Update(1, "New", new string('x', 201)));
        Assert.Equal(12.34m, product.Price);
        Assert.Equal("Original", product.Description);
        Assert.Equal("Alternative", product.AlternateDescription);

        product.Update(0, new string('x', 200), new string('y', 200));

        Assert.Equal(id, product.Id);
        Assert.Equal(0, product.Price);
        Assert.Equal(200, product.Description.Length);
        Assert.Equal(200, product.AlternateDescription!.Length);
    }
}
