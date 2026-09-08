namespace SettleMate.Database.Entities.Accommodation;

public sealed class UserDestination
{
    private UserDestination() { }

    public UserDestination(Guid id, string userId, string label, decimal latitude, decimal longitude)
    {
        Id = id;
        UserId = userId.Trim();
        Update(label, latitude, longitude);
    }

    public Guid Id { get; private set; }
    public string UserId { get; private set; } = null!;
    public string Label { get; private set; } = null!;
    public decimal Latitude { get; private set; }
    public decimal Longitude { get; private set; }

    public void Update(string label, decimal latitude, decimal longitude)
    {
        Label = label.Trim();
        Latitude = latitude;
        Longitude = longitude;
    }
}
