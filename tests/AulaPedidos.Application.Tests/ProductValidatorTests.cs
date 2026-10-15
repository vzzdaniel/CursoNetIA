using AulaPedidos.Application.Products;

namespace AulaPedidos.Application.Tests;

public class ProductValidatorTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task MissingDescription_IsRejectedByBothValidators(string? description)
    {
        await AssertBothReject(1m, description, null, "Description");
    }

    [Fact]
    public async Task TooLongDescriptionsAndNegativePrice_AreRejectedByBothValidators()
    {
        await AssertBothReject(1m, new string('x', 201), null, "Description");
        await AssertBothReject(1m, "Valid", new string('y', 201), "AlternateDescription");
        await AssertBothReject(-0.01m, "Valid", null, "Price");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ZeroPriceAndOptionalAlternateDescription_AreValid(string? alternateDescription)
    {
        Assert.True((await new CreateProductValidator().ValidateAsync(new CreateProduct(0m, "Valid", alternateDescription))).IsValid);
        Assert.True((await new UpdateProductValidator().ValidateAsync(new UpdateProduct(Guid.NewGuid(), 0m, "Valid", alternateDescription))).IsValid);
    }

    [Fact]
    public async Task ExactLimitsAfterTrimming_AreValidLikeDomain()
    {
        var description = " " + new string('x', 200) + " ";
        var alternateDescription = " " + new string('y', 200) + " ";
        Assert.True((await new CreateProductValidator().ValidateAsync(new CreateProduct(0m, description, alternateDescription))).IsValid);
        Assert.True((await new UpdateProductValidator().ValidateAsync(new UpdateProduct(Guid.NewGuid(), 0m, description, alternateDescription))).IsValid);
    }

    [Fact]
    public async Task EmptyUpdateId_IsRejected()
    {
        var result = await new UpdateProductValidator().ValidateAsync(new UpdateProduct(Guid.Empty, 1m, "Valid", null));
        Assert.Equal("Id", Assert.Single(result.Errors).PropertyName);
    }

    private static async Task AssertBothReject(decimal price, string? description, string? alternateDescription, string field)
    {
        var create = await new CreateProductValidator().ValidateAsync(new CreateProduct(price, description, alternateDescription));
        var update = await new UpdateProductValidator().ValidateAsync(new UpdateProduct(Guid.NewGuid(), price, description, alternateDescription));
        Assert.False(create.IsValid);
        Assert.False(update.IsValid);
        Assert.All(create.Errors, failure => Assert.Equal(field, failure.PropertyName));
        Assert.All(update.Errors, failure => Assert.Equal(field, failure.PropertyName));
    }
}
