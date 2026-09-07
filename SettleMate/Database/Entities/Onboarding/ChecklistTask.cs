namespace SettleMate.Database.Entities.Onboarding;

public sealed class ChecklistTask
{
    private ChecklistTask() { }

    private ChecklistTask(Guid id, string userId, string key, bool completed)
    {
        Id = id;
        UserId = userId;
        Key = key;
        Completed = completed;
    }

    public Guid Id { get; private set; }
    public string UserId { get; private set; } = null!;
    public string Key { get; private set; } = null!;
    public bool Completed { get; private set; }
    public ICollection<RoadmapItem> RoadmapItems { get; private set; } = [];

    public static ChecklistTask Create(string userId, string key, bool completed = false) =>
        new(Guid.NewGuid(), userId, key, completed);

    public void SetCompleted(bool completed)
    {
        Completed = completed;
        foreach (var item in RoadmapItems)
            item.SetCompletedWithoutSynchronizingTask(completed);
    }
}
