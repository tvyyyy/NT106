using BobaShop.Domain.Constants;
using BobaShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BobaShop.Persistence.Configurations;

public class DiningTableConfiguration : IEntityTypeConfiguration<DiningTable>
{
    public void Configure(EntityTypeBuilder<DiningTable> builder)
    {
        builder.ToTable("DiningTables", t => t.HasCheckConstraint("CK_DiningTables_Capacity", "\"Capacity\" > 0"));
        builder.HasKey(t => t.TableID);
        builder.Property(t => t.TableNumber).IsRequired().HasMaxLength(DbConstraints.TableNumberMaxLength);
        builder.Property(t => t.IsActive).HasDefaultValue(true);
        builder.HasIndex(t => t.TableNumber).IsUnique();
    }
}
