using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SettleMate.Database.Entities.Accommodation;

namespace SettleMate.Database.Mapping.Accommodation;

public sealed class ListingConfiguration : IEntityTypeConfiguration<Listing>
{
    public void Configure(EntityTypeBuilder<Listing> builder)
    {
        builder.ToTable("Listings", "dbo");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ProviderId).HasMaxLength(450).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Address).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Suburb).HasMaxLength(100).IsRequired();
        builder.Property(x => x.WeeklyRent).HasPrecision(10, 2).IsRequired();
        builder.Property(x => x.RoomType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.SourceUrl).HasMaxLength(500);
        builder.HasIndex(x => new { x.IsActive, x.Status, x.Suburb, x.AvailableFrom });
        builder.HasIndex(x => x.ProviderId);
    }
}
