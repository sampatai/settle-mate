namespace SettleMate.Database.Entities.Accommodation;

public sealed class ListingScore
{
    private ListingScore() { }

    public ListingScore(
        Guid id,
        Guid listingId,
        Guid? destinationId,
        int travelTimeMinutes,
        int nearbySupermarketCount,
        int nearbyTransportCount,
        decimal safetyScore,
        decimal studentScore,
        string provider)
    {
        Id = id;
        ListingId = listingId;
        DestinationId = destinationId;
        Update(travelTimeMinutes, nearbySupermarketCount, nearbyTransportCount, safetyScore, studentScore, provider);
    }

    public Guid Id { get; private set; }
    public Guid ListingId { get; private set; }
    public Listing Listing { get; private set; } = null!;
    public Guid? DestinationId { get; private set; }
    public int TravelTimeMinutes { get; private set; }
    public int NearbySupermarketCount { get; private set; }
    public int NearbyTransportCount { get; private set; }
    public decimal SafetyScore { get; private set; }
    public decimal StudentScore { get; private set; }
    public string Provider { get; private set; } = null!;
    public DateTime CalculatedAtUtc { get; private set; }

    public void Update(
        int travelTimeMinutes,
        int nearbySupermarketCount,
        int nearbyTransportCount,
        decimal safetyScore,
        decimal studentScore,
        string provider)
    {
        TravelTimeMinutes = travelTimeMinutes;
        NearbySupermarketCount = nearbySupermarketCount;
        NearbyTransportCount = nearbyTransportCount;
        SafetyScore = safetyScore;
        StudentScore = studentScore;
        Provider = provider.Trim();
        CalculatedAtUtc = DateTime.UtcNow;
    }
}
