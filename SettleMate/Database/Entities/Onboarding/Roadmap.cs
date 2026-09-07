using SettleMate.Abstractions;

namespace SettleMate.Database.Entities.Onboarding;

public sealed class Roadmap : AuditableEntity
{
    private Roadmap() { }

    private Roadmap(Guid id, string userId, Guid userProfileId)
    {
        Id = id;
        UserId = userId;
        UserProfileId = userProfileId;
    }

    public Guid Id { get; private set; }
    public string UserId { get; private set; } = null!;
    public Guid UserProfileId { get; private set; }
    public ICollection<RoadmapItem> Items { get; private set; } = [];

    public static Roadmap Create(string userId, Guid userProfileId) =>
        new(Guid.NewGuid(), userId, userProfileId);

    public void AddItem(RoadmapItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        Items.Add(item);
    }
}
