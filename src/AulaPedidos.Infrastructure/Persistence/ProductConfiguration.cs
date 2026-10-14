using AulaPedidos.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AulaPedidos.Infrastructure.Persistence;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(product => product.Id);
        builder.Property(product => product.Id).ValueGeneratedNever();
        builder.Property(product => product.Price).HasPrecision(18, 2);
        builder.Property(product => product.Description).IsRequired().HasMaxLength(Product.MaximumDescriptionLength);
        builder.Property(product => product.AlternateDescription).HasMaxLength(Product.MaximumDescriptionLength);
    }
}
