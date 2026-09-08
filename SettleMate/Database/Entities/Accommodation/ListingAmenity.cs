namespace SettleMate.Database.Entities.Accommodation;

public sealed class ListingAmenity
{
    private ListingAmenity() { }

    public ListingAmenity(Guid id, Guid listingId, string name)
    {
        Id = id;
        ListingId = listingId;
        Name = name.Trim();
    }

    public Guid Id { get; private set; }
    public Guid ListingId { get; private set; }
    public Listing Listing { get; private set; } = null!;
    public string Name { get; private set; } = null!;
}
