using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SettleMate.Database.Entities.Accommodation;

namespace SettleMate.Database.Mapping.Accommodation;

public sealed class UserDestinationConfiguration : IEntityTypeConfiguration<UserDestination>
{
    public void Configure(EntityTypeBuilder<UserDestination> builder)
    {
        builder.ToTable("UserDestinations", "dbo");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.UserId).HasMaxLength(450).IsRequired();
        builder.Property(x => x.Label).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => x.UserId);
    }
}
