using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SettleMate.Database.Entities.Accommodation;

namespace SettleMate.Database.Mapping.Accommodation;

public sealed class ListingAmenityConfiguration : IEntityTypeConfiguration<ListingAmenity>
{
    public void Configure(EntityTypeBuilder<ListingAmenity> builder)
    {
        builder.ToTable("ListingAmenities", "dbo");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.HasIndex(x => new { x.ListingId, x.Name }).IsUnique();
        builder.HasOne(x => x.Listing).WithMany(x => x.Amenities)
            .HasForeignKey(x => x.ListingId).OnDelete(DeleteBehavior.Cascade);
    }
}
