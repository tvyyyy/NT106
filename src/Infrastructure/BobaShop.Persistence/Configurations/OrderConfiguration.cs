using BobaShop.Domain.Constants;
using BobaShop.Domain.Entities;
using BobaShop.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BobaShop.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders", t =>
        {
            t.HasCheckConstraint("CK_Orders_Amounts",
                "\"TotalAmount\" >= 0 AND \"DiscountAmount\" >= 0 AND \"FinalAmount\" = \"TotalAmount\" - \"DiscountAmount\" AND \"FinalAmount\" >= 0");
        });
        builder.HasKey(o => o.OrderID);

        builder.Property(o => o.OrderType).HasConversion<string>().HasMaxLength(DbConstraints.EnumStringMaxLength);
        builder.Property(o => o.PaymentMethod).HasConversion<string>().HasMaxLength(DbConstraints.EnumStringMaxLength);
        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(DbConstraints.EnumStringMaxLength)
               .HasDefaultValue(OrderStatus.Pending);

        builder.Property(o => o.TotalAmount).HasPrecision(DbConstraints.PriceTotalDigits, DbConstraints.PriceScale);
        builder.Property(o => o.DiscountAmount).HasPrecision(DbConstraints.PriceTotalDigits, DbConstraints.PriceScale);
        builder.Property(o => o.FinalAmount).HasPrecision(DbConstraints.PriceTotalDigits, DbConstraints.PriceScale);
        builder.Property(o => o.CreatedAt).HasDefaultValueSql("NOW()");

        builder.HasIndex(o => new { o.UserID, o.CreatedAt });
        builder.HasIndex(o => o.Status);

        builder.HasOne(o => o.User)
               .WithMany(u => u.Orders)
               .HasForeignKey(o => o.UserID)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Voucher)
               .WithMany(v => v.Orders)
               .HasForeignKey(o => o.VoucherID)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
