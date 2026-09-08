namespace SettleMate.Database.Entities.Accommodation;

public enum ListingStatus
{
    Draft,
    Pending,
    Approved,
    Rejected
}

public sealed class Listing
{
    private Listing() { }

    public Listing(
        Guid id,
        string providerId,
        string title,
        string address,
        string suburb,
        decimal latitude,
        decimal longitude,
        decimal weeklyRent,
        int roomCount,
        string roomType,
        DateOnly availableFrom,
        string? sourceUrl)
    {
        Id = id;
        ProviderId = providerId.Trim();
        Update(title, address, suburb, latitude, longitude, weeklyRent, roomCount, roomType, availableFrom, sourceUrl);
        Status = ListingStatus.Draft;
        IsActive = true;
    }

    public Guid Id { get; private set; }
    public string ProviderId { get; private set; } = null!;
    public string Title { get; private set; } = null!;
    public string Address { get; private set; } = null!;
    public string Suburb { get; private set; } = null!;
    public decimal Latitude { get; private set; }
    public decimal Longitude { get; private set; }
    public decimal WeeklyRent { get; private set; }
    public int RoomCount { get; private set; }
    public string RoomType { get; private set; } = null!;
    public DateOnly AvailableFrom { get; private set; }
    public string? SourceUrl { get; private set; }
    public bool IsActive { get; private set; }
    public ListingStatus Status { get; private set; }
    public ICollection<ListingAmenity> Amenities { get; private set; } = [];
    public ICollection<ListingWeeklyCost> WeeklyCosts { get; private set; } = [];
    public ICollection<ListingScore> Scores { get; private set; } = [];

    public void Update(
        string title,
        string address,
        string suburb,
        decimal latitude,
        decimal longitude,
        decimal weeklyRent,
        int roomCount,
        string roomType,
        DateOnly availableFrom,
        string? sourceUrl)
    {
        Title = title.Trim();
        Address = address.Trim();
        Suburb = suburb.Trim();
        Latitude = latitude;
        Longitude = longitude;
        WeeklyRent = weeklyRent;
        RoomCount = roomCount;
        RoomType = roomType.Trim();
        AvailableFrom = availableFrom;
        SourceUrl = string.IsNullOrWhiteSpace(sourceUrl) ? null : sourceUrl.Trim();
    }

    public void SetStatus(ListingStatus status) => Status = status;
    public void SetActive(bool isActive) => IsActive = isActive;
    public void AddAmenity(ListingAmenity amenity) => Amenities.Add(amenity);
    public void AddWeeklyCost(ListingWeeklyCost cost) => WeeklyCosts.Add(cost);
}
