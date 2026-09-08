using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SettleMate.Features.Users.Login;

namespace SettleMate.Database.Mapping;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens", "auth");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.JwtId).IsRequired();
        builder.Property(e => e.ExpiryDate).IsRequired();
        builder.Property(e => e.Invalidated).IsRequired();
        builder.Property(e => e.UserId).IsRequired();
       

        builder.HasOne(e => e.User)
	        .WithMany()
	        .HasForeignKey(e => e.UserId);
    }
}
