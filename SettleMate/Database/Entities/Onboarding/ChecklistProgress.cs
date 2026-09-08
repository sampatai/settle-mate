namespace SettleMate.Database.Entities.Onboarding;

public static class ChecklistProgress
{
    public static (int Total, int Completed, decimal Percentage) Calculate(
        IEnumerable<ChecklistTask> tasks)
    {
        var taskList = tasks.ToArray();
        var completed = taskList.Count(x => x.Completed);
        var percentage = taskList.Length == 0
            ? 0m
            : Math.Round(completed * 100m / taskList.Length, 2, MidpointRounding.AwayFromZero);
        return (taskList.Length, completed, percentage);
    }
}