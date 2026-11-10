using BobaShop.Domain.Constants;
using BobaShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BobaShop.Persistence.Configurations;

public class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.ToTable("PasswordResetTokens");
        builder.HasKey(t => t.TokenID);

        builder.Property(t => t.TokenHash).IsRequired().HasMaxLength(DbConstraints.PasswordHashMaxLength);
        builder.Property(t => t.ExpiresAt).IsRequired();
        builder.Property(t => t.CreatedAt).HasDefaultValueSql("NOW()");

        // Tra token mới nhất của user (cooldown 60 giây, kiểm tra OTP)
        builder.HasIndex(t => new { t.UserID, t.CreatedAt });

        builder.HasOne(t => t.User)
               .WithMany(u => u.PasswordResetTokens)
               .HasForeignKey(t => t.UserID)
               .OnDelete(DeleteBehavior.Cascade);
    }
}