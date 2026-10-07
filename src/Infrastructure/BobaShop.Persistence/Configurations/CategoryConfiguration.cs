using BobaShop.Domain.Constants;
using BobaShop.Domain.Entities;
using BobaShop.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BobaShop.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        builder.HasKey(c => c.CategoryID);

        builder.Property(c => c.CategoryName)
               .IsRequired()
               .HasMaxLength(DbConstraints.NameMaxLength);
        builder.HasIndex(c => c.CategoryName).IsUnique();
    }
}
