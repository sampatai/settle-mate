namespace SettleMate.Database.Entities.Accommodation;

public sealed class SavedListing
{
    private SavedListing() { }

    public SavedListing(Guid id, string userId, Guid listingId)
    {
        Id = id;
        UserId = userId.Trim();
        ListingId = listingId;
        SavedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public string UserId { get; private set; } = null!;
    public Guid ListingId { get; private set; }
    public Listing Listing { get; private set; } = null!;
    public DateTime SavedAtUtc { get; private set; }
}
