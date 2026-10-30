using DigitalWardrobe.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalWardrobe.Data.Configurations;

public class ClothingItemConfiguration : IEntityTypeConfiguration<ClothingItem>
{
    public void Configure(EntityTypeBuilder<ClothingItem> builder)
    {
        builder.Property(i => i.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(i => i.Brand)
            .HasMaxLength(100);

        builder.Property(i => i.ImageKey)
            .HasMaxLength(100);

        builder.HasOne<IdentityUser>()
            .WithMany()
            .HasForeignKey(i => i.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}