namespace SettleMate.Database.Entities.Accommodation;

public sealed class ListingWeeklyCost
{
    private ListingWeeklyCost() { }

    public ListingWeeklyCost(Guid id, Guid listingId, string type, decimal amount, bool included)
    {
        Id = id;
        ListingId = listingId;
        Type = type.Trim();
        Amount = amount;
        Included = included;
    }

    public Guid Id { get; private set; }
    public Guid ListingId { get; private set; }
    public Listing Listing { get; private set; } = null!;
    public string Type { get; private set; } = null!;
    public decimal Amount { get; private set; }
    public bool Included { get; private set; }
}
