using BobaShop.Domain.Constants;
using BobaShop.Domain.Entities;
using BobaShop.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BobaShop.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.UserID);

        builder.Property(u => u.FullName).IsRequired().HasMaxLength(DbConstraints.NameMaxLength);
        builder.Property(u => u.Phone).IsRequired().HasMaxLength(DbConstraints.PhoneMaxLength);
        builder.Property(u => u.Email).IsRequired().HasMaxLength(DbConstraints.EmailMaxLength);
        builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(DbConstraints.PasswordHashMaxLength);

        builder.Property(u => u.Status)
               .HasConversion<string>()
               .HasMaxLength(DbConstraints.EnumStringMaxLength)
               .HasDefaultValue(UserStatus.Active);
        builder.Property(u => u.CreatedAt).HasDefaultValueSql("NOW()");

        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.Phone).IsUnique();

        builder.HasOne(u => u.Role)
               .WithMany(r => r.Users)
               .HasForeignKey(u => u.RoleID)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
