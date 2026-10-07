using BobaShop.Domain.Constants;
using BobaShop.Domain.Entities;
using BobaShop.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BobaShop.Persistence.Configurations;

public class ToppingConfiguration : IEntityTypeConfiguration<Topping>
{
    public void Configure(EntityTypeBuilder<Topping> builder)
    {
        builder.ToTable("Toppings", t =>
            t.HasCheckConstraint("CK_Toppings_Price", "\"Price\" >= 0"));
        builder.HasKey(t => t.ToppingID);

        builder.Property(t => t.ToppingName)
               .IsRequired()
               .HasMaxLength(DbConstraints.NameMaxLength);
        builder.HasIndex(t => t.ToppingName).IsUnique();

        builder.Property(t => t.Price)
               .HasPrecision(DbConstraints.PriceTotalDigits, DbConstraints.PriceScale);
        builder.Property(t => t.IsAvailable).HasDefaultValue(true);
    }
}
