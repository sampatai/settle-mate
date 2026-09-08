using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SettleMate.Database.Entities.Accommodation;

namespace SettleMate.Database.Mapping.Accommodation;

public sealed class ListingScoreConfiguration : IEntityTypeConfiguration<ListingScore>
{
    public void Configure(EntityTypeBuilder<ListingScore> builder)
    {
        builder.ToTable("ListingScores", "dbo");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Provider).HasMaxLength(100).IsRequired();
        builder.Property(x => x.SafetyScore).HasPrecision(5, 2);
        builder.Property(x => x.StudentScore).HasPrecision(5, 2);
        builder.HasIndex(x => new { x.ListingId, x.DestinationId }).IsUnique();
        builder.HasOne(x => x.Listing).WithMany(x => x.Scores)
            .HasForeignKey(x => x.ListingId).OnDelete(DeleteBehavior.Cascade);
    }
}
