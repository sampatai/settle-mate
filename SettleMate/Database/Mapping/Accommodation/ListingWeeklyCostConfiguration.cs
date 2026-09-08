using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SettleMate.Database.Entities.Accommodation;

namespace SettleMate.Database.Mapping.Accommodation;

public sealed class ListingWeeklyCostConfiguration : IEntityTypeConfiguration<ListingWeeklyCost>
{
    public void Configure(EntityTypeBuilder<ListingWeeklyCost> builder)
    {
        builder.ToTable("ListingWeeklyCosts", "dbo");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Type).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Amount).HasPrecision(10, 2).IsRequired();
        builder.HasIndex(x => new { x.ListingId, x.Type }).IsUnique();
        builder.HasOne(x => x.Listing).WithMany(x => x.WeeklyCosts)
            .HasForeignKey(x => x.ListingId).OnDelete(DeleteBehavior.Cascade);
    }
}
