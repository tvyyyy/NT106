using BobaShop.Domain.Constants;
using BobaShop.Domain.Entities;
using BobaShop.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BobaShop.Persistence.Configurations;

public class OrderDetailConfiguration : IEntityTypeConfiguration<OrderDetail>
{
    public void Configure(EntityTypeBuilder<OrderDetail> builder)
    {
        builder.ToTable("OrderDetails", t =>
        {
            t.HasCheckConstraint("CK_OrderDetails_Quantity", "\"Quantity\" > 0");
            t.HasCheckConstraint("CK_OrderDetails_UnitPrice", "\"UnitPrice\" >= 0");
        });
        builder.HasKey(d => d.OrderDetailID);

        builder.Property(d => d.Size).HasConversion<string>().HasMaxLength(DbConstraints.EnumStringMaxLength);
        builder.Property(d => d.SugarLevel).HasConversion<string>().HasMaxLength(DbConstraints.EnumStringMaxLength);
        builder.Property(d => d.IceLevel).HasConversion<string>().HasMaxLength(DbConstraints.EnumStringMaxLength);
        builder.Property(d => d.Note).HasMaxLength(DbConstraints.NoteMaxLength);
        builder.Property(d => d.UnitPrice).HasPrecision(DbConstraints.PriceTotalDigits, DbConstraints.PriceScale);

        builder.HasOne(d => d.Order)
               .WithMany(o => o.OrderDetails)
               .HasForeignKey(d => d.OrderID)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(d => d.Product)
               .WithMany(p => p.OrderDetails)
               .HasForeignKey(d => d.ProductID)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
