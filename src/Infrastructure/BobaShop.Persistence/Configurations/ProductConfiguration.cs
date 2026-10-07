using BobaShop.Domain.Constants;
using BobaShop.Domain.Entities;
using BobaShop.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BobaShop.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products", t =>
            t.HasCheckConstraint("CK_Products_Price", "\"Price\" >= 0"));
        builder.HasKey(p => p.ProductID);

        builder.Property(p => p.ProductName).IsRequired().HasMaxLength(DbConstraints.NameMaxLength);
        builder.Property(p => p.Price).HasPrecision(DbConstraints.PriceTotalDigits, DbConstraints.PriceScale);
        builder.Property(p => p.ImageURL).HasMaxLength(DbConstraints.UrlMaxLength);
        builder.Property(p => p.IsAvailable).HasDefaultValue(true);

        builder.HasIndex(p => new { p.CategoryID, p.ProductName }).IsUnique();

        builder.HasOne(p => p.Category)
               .WithMany(c => c.Products)
               .HasForeignKey(p => p.CategoryID)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
