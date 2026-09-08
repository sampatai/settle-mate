using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SettleMate.Database.Entities.Identity;
using SettleMate.Database.Entities.Onboarding;
using SettleMate.Database.Entities.Accommodation;
using SettleMate.Features.Users.Login;
using System.Data;

namespace SettleMate.Database;

public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<User, Role, string,
            UserClaim, UserRole, UserLogin,
            RoleClaim, UserToken>
        (options)
{
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    public DbSet<UserProfile> UserProfiles { get; set; } = null!;
    public DbSet<VisaRule> VisaRules { get; set; } = null!;
    public DbSet<ChecklistTemplate> ChecklistTemplates { get; set; } = null!;
    public DbSet<Roadmap> Roadmaps { get; set; } = null!;
    public DbSet<RoadmapItem> RoadmapItems { get; set; } = null!;
    public DbSet<ChecklistTask> ChecklistTasks { get; set; } = null!;
    public DbSet<Listing> Listings { get; set; } = null!;
    public DbSet<ListingScore> ListingScores { get; set; } = null!;
    public DbSet<SavedListing> SavedListings { get; set; } = null!;
    public DbSet<UserDestination> UserDestinations { get; set; } = null!;
    public DbSet<ListingAmenity> ListingAmenities { get; set; } = null!;
    public DbSet<ListingWeeklyCost> ListingWeeklyCosts { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        modelBuilder.HasDefaultSchema(DatabaseConsts.Schema);



        modelBuilder.Entity<User>(b =>
        {
            // Each User can have many UserClaims
            b.HasMany(e => e.Claims)
                .WithOne(e => e.User)
                .HasForeignKey(uc => uc.UserId)
                .IsRequired();

            // Each User can have many UserLogins
            b.HasMany(e => e.UserLogins)
                .WithOne(e => e.User)
                .HasForeignKey(ul => ul.UserId)
                .IsRequired();

            // Each User can have many UserTokens
            b.HasMany(e => e.UserTokens)
                .WithOne(e => e.User)
                .HasForeignKey(ut => ut.UserId)
                .IsRequired();

            // Each User can have many entries in the UserRole join table
            b.HasMany(e => e.UserRoles)
                .WithOne(e => e.User)
                .HasForeignKey(ur => ur.UserId)
                .IsRequired();
        });

        modelBuilder.Entity<Role>(b =>
        {
            b.ToTable("Roles", DatabaseConsts.Schema);

            // Each Role can have many entries in the UserRole join table
            b.HasMany(e => e.UserRoles)
                .WithOne(e => e.Role)
                .HasForeignKey(ur => ur.RoleId)
                .IsRequired();

            // Each Role can have many associated RoleClaims
            b.HasMany(e => e.RoleClaims)
                .WithOne(e => e.Role)
                .HasForeignKey(rc => rc.RoleId)
                .IsRequired();
        });

        modelBuilder.Entity<UserRole>(b =>
        {
            b.HasKey(x => new { x.UserId, x.RoleId });
        });

        modelBuilder.Entity<UserProfile>(b =>
        {
            b.ToTable("UserProfiles", "dbo");
            b.HasKey(x => x.Id);
            b.HasIndex(x => new { x.UserId, x.Version }).IsUnique();
            b.Property(x => x.Country).HasMaxLength(100).IsRequired();
            b.Property(x => x.VisaSubclass).HasMaxLength(20).IsRequired();
            b.Property(x => x.ApplicantType).HasMaxLength(20).IsRequired();
            b.Property(x => x.StudyLevel).HasMaxLength(30).IsRequired();
            b.Property(x => x.State).HasMaxLength(3).IsRequired();
            b.Property(x => x.BudgetRange).HasMaxLength(50).IsRequired();
            b.Property(x => x.CareerGoal).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<VisaRule>(b =>
        {
            b.ToTable("VisaRules", "dbo");
            b.HasKey(x => x.VisaSubclass);
            b.Property(x => x.VisaSubclass).HasMaxLength(20);
            b.Property(x => x.RequiredDocumentsJson).IsRequired();
        });

        modelBuilder.Entity<ChecklistTemplate>(b =>
        {
            b.ToTable("ChecklistTemplates", "dbo");
            b.HasKey(x => x.Id);
            b.HasIndex(x => x.Key).IsUnique();
            b.Property(x => x.Key).HasMaxLength(100).IsRequired();
            b.Property(x => x.Title).HasMaxLength(200).IsRequired();
            b.Property(x => x.Description).HasMaxLength(1000).IsRequired();
            b.Property(x => x.Provider).HasMaxLength(200);
            b.Property(x => x.ApplicationUrl).HasMaxLength(500);
            b.Property(x => x.EligibilityNotes).HasMaxLength(1000);
            b.Property(x => x.EstimatedMinutes).IsRequired();
            b.Property(x => x.IsTimeSensitive).IsRequired();
            b.Property(x => x.RequiredDocumentsJson).IsRequired();
        });

        modelBuilder.Entity<Roadmap>(b =>
        {
            b.ToTable("Roadmaps", "dbo");
            b.HasKey(x => x.Id);
            b.HasIndex(x => new { x.UserId, x.UserProfileId }).IsUnique();
            b.HasMany(x => x.Items).WithOne(x => x.Roadmap)
                .HasForeignKey(x => x.RoadmapId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RoadmapItem>(b =>
        {
            b.ToTable("RoadmapItems", "dbo");
            b.HasKey(x => x.Id);
            b.Property(x => x.Title).HasMaxLength(200).IsRequired();
            b.Property(x => x.Description).HasMaxLength(2000).IsRequired();
            b.Property(x => x.LinkedChecklistTaskId).HasMaxLength(100).IsRequired();
            b.HasOne(x => x.ChecklistTask)
                .WithMany(x => x.RoadmapItems)
                .HasForeignKey(x => x.ChecklistTaskId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ChecklistTask>(b =>
        {
            b.ToTable("ChecklistTasks", "dbo");
            b.HasKey(x => x.Id);
            b.HasIndex(x => new { x.UserId, x.Key }).IsUnique();
            b.Property(x => x.UserId).HasMaxLength(450).IsRequired();
            b.Property(x => x.Key).HasMaxLength(100).IsRequired();
            b.Property(x => x.CompletedAt);
        });
    }
    // public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
    // {
    //     return await base.SaveChangesAsync(cancellationToken);
    // }
}
