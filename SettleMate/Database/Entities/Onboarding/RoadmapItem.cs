namespace SettleMate.Database.Entities.Onboarding;

public sealed class RoadmapItem
{
    private RoadmapItem() { }

    private RoadmapItem(Guid id, int weekNumber, string title, string description, ChecklistTask checklistTask)
    {
        Id = id;
        WeekNumber = weekNumber;
        Title = title;
        Description = description;
        LinkedChecklistTaskId = checklistTask.Key;
        ChecklistTaskId = checklistTask.Id;
        ChecklistTask = checklistTask;
        Completed = checklistTask.Completed;
    }

    public Guid Id { get; private set; }
    public Guid RoadmapId { get; private set; }
    public Roadmap Roadmap { get; private set; } = null!;
    public int WeekNumber { get; private set; }
    public string Title { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public string LinkedChecklistTaskId { get; private set; } = null!;
    public Guid ChecklistTaskId { get; private set; }
    public ChecklistTask ChecklistTask { get; private set; } = null!;
    public bool Completed { get; private set; }

    public static RoadmapItem Create(int weekNumber, string title, string description, ChecklistTask checklistTask) =>
        new(Guid.NewGuid(), weekNumber, title, description, checklistTask);

    public void SetCompleted(bool completed)
    {
        Completed = completed;
        ChecklistTask.SetCompleted(completed);
    }

    internal void SetCompletedWithoutSynchronizingTask(bool completed) => Completed = completed;
}
