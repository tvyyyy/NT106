using BobaShop.Domain.Constants;
using BobaShop.Domain.Entities;
using BobaShop.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BobaShop.Persistence.Configurations;
public class VoucherConfiguration : IEntityTypeConfiguration<Voucher>
{
    public void Configure(EntityTypeBuilder<Voucher> builder)
    {
        builder.ToTable("Vouchers", t =>
        {
            t.HasCheckConstraint("CK_Vouchers_DiscountValue", "\"DiscountValue\" > 0");
            t.HasCheckConstraint("CK_Vouchers_MinOrderValue", "\"MinOrderValue\" >= 0");
            t.HasCheckConstraint("CK_Vouchers_MaxDiscountAmount", "\"MaxDiscountAmount\" >= 0");
            t.HasCheckConstraint("CK_Vouchers_Quantity", "\"Quantity\" >= 0");
        });
        builder.HasKey(v => v.VoucherID);

        builder.Property(v => v.Code).IsRequired().HasMaxLength(DbConstraints.CodeMaxLength);
        builder.HasIndex(v => v.Code).IsUnique();   // mã giảm giá không được trùng

        builder.Property(v => v.DiscountValue).HasPrecision(DbConstraints.PriceTotalDigits, DbConstraints.PriceScale);
        builder.Property(v => v.MinOrderValue).HasPrecision(DbConstraints.PriceTotalDigits, DbConstraints.PriceScale);
        builder.Property(v => v.MaxDiscountAmount).HasPrecision(DbConstraints.PriceTotalDigits, DbConstraints.PriceScale);

        builder.Property(v => v.IsActive).HasDefaultValue(true);
        builder.Property(v => v.CreatedAt).HasDefaultValueSql("NOW()");
    }
}