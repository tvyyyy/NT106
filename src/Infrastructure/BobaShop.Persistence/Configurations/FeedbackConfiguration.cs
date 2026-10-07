using BobaShop.Domain.Constants;
using BobaShop.Domain.Entities;
using BobaShop.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BobaShop.Persistence.Configurations;

public class FeedbackConfiguration : IEntityTypeConfiguration<Feedback>
{
    public void Configure(EntityTypeBuilder<Feedback> builder)
    {
        builder.ToTable("Feedbacks", t =>
            t.HasCheckConstraint("CK_Feedbacks_Rating",
                $"\"Rating\" BETWEEN {DbConstraints.RatingMin} AND {DbConstraints.RatingMax}"));
        builder.HasKey(f => f.FeedbackID);

        builder.Property(f => f.Comment).HasMaxLength(DbConstraints.NoteMaxLength);
        builder.Property(f => f.CreatedAt).HasDefaultValueSql("NOW()");

        builder.HasIndex(f => new { f.OrderID, f.ProductID }).IsUnique();
        builder.HasIndex(f => f.ProductID);

        builder.HasOne(f => f.User)
               .WithMany(u => u.Feedbacks)
               .HasForeignKey(f => f.UserID)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(f => f.Order)
               .WithMany(o => o.Feedbacks)
               .HasForeignKey(f => f.OrderID)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(f => f.Product)
               .WithMany(p => p.Feedbacks)
               .HasForeignKey(f => f.ProductID)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
