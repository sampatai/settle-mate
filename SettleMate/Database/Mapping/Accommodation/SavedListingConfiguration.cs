using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SettleMate.Database.Entities.Accommodation;

namespace SettleMate.Database.Mapping.Accommodation;

public sealed class SavedListingConfiguration : IEntityTypeConfiguration<SavedListing>
{
    public void Configure(EntityTypeBuilder<SavedListing> builder)
    {
        builder.ToTable("SavedListings", "dbo");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.UserId).HasMaxLength(450).IsRequired();
        builder.HasIndex(x => new { x.UserId, x.ListingId }).IsUnique();
        builder.HasOne(x => x.Listing).WithMany()
            .HasForeignKey(x => x.ListingId).OnDelete(DeleteBehavior.Cascade);
    }
}
