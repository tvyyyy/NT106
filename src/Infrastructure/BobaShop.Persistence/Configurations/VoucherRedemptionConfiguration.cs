using BobaShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BobaShop.Persistence.Configurations;

public class VoucherRedemptionConfiguration : IEntityTypeConfiguration<VoucherRedemption>
{
    public void Configure(EntityTypeBuilder<VoucherRedemption> builder)
    {
        builder.ToTable("VoucherRedemptions");
        builder.HasKey(r => r.RedemptionID);
        builder.Property(r => r.RedeemedAt).HasDefaultValueSql("NOW()");
        builder.HasIndex(r => new { r.VoucherID, r.OrderID }).IsUnique();
        builder.HasOne(r => r.Voucher).WithMany(v => v.Redemptions).HasForeignKey(r => r.VoucherID).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(r => r.User).WithMany(u => u.VoucherRedemptions).HasForeignKey(r => r.UserID).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(r => r.Order).WithMany(o => o.VoucherRedemptions).HasForeignKey(r => r.OrderID).OnDelete(DeleteBehavior.Restrict);
    }
}
