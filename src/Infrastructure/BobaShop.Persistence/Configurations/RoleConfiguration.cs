using BobaShop.Domain.Constants;
using BobaShop.Domain.Entities;
using BobaShop.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BobaShop.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");
        builder.HasKey(r => r.RoleID);

        builder.Property(r => r.RoleName)
               .IsRequired()
               .HasMaxLength(DbConstraints.EnumStringMaxLength);
        builder.HasIndex(r => r.RoleName).IsUnique();

        builder.HasData(
            new Role { RoleID = (int)UserRole.Customer, RoleName = nameof(UserRole.Customer) },
            new Role { RoleID = (int)UserRole.Admin,    RoleName = nameof(UserRole.Admin) });
    }
}
