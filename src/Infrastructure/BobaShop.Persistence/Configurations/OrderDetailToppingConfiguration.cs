using BobaShop.Domain.Constants;
using BobaShop.Domain.Entities;
using BobaShop.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BobaShop.Persistence.Configurations;

public class OrderDetailToppingConfiguration : IEntityTypeConfiguration<OrderDetailTopping>
{
    public void Configure(EntityTypeBuilder<OrderDetailTopping> builder)
    {
        builder.ToTable("OrderDetailToppings", t =>
            t.HasCheckConstraint("CK_OrderDetailToppings_Price", "\"Price\" >= 0"));
        builder.HasKey(x => x.ID);

        builder.Property(x => x.Price).HasPrecision(DbConstraints.PriceTotalDigits, DbConstraints.PriceScale);

        builder.HasIndex(x => new { x.OrderDetailID, x.ToppingID }).IsUnique();

        builder.HasOne(x => x.OrderDetail)
               .WithMany(d => d.OrderDetailToppings)
               .HasForeignKey(x => x.OrderDetailID)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Topping)
               .WithMany(t => t.OrderDetailToppings)
               .HasForeignKey(x => x.ToppingID)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
