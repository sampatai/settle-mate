using SettleMate.Database.Entities.Onboarding;

namespace SettleMate.Features.Checklist.Shared;

internal static class ChecklistMapper
{
    public static ChecklistResponse ToResponse(
        IEnumerable<RoadmapItem> items,
        IReadOnlyDictionary<string, ChecklistTemplate> templates)
    {
        var orderedItems = items
            .OrderBy(x => x.ChecklistTask.Completed)
            .ThenBy(x => !templates.TryGetValue(x.LinkedChecklistTaskId, out var template) || !template.IsTimeSensitive)
            .ThenBy(x => x.WeekNumber)
            .ThenBy(x => x.Title)
            .ToArray();
        var tasks = orderedItems.Select(item =>
        {
            templates.TryGetValue(item.LinkedChecklistTaskId, out var template);
            return new ChecklistTaskResponse(
                item.ChecklistTaskId,
                item.LinkedChecklistTaskId,
                item.WeekNumber,
                item.Title,
                item.Description,
                template?.RequiredDocuments ?? [],
                template?.EstimatedMinutes ?? 30,
                template?.ApplicationUrl,
                template?.IsTimeSensitive ?? false,
                item.ChecklistTask.Completed,
                item.ChecklistTask.CompletedAt);
        }).ToArray();
        var progress = ChecklistProgress.Calculate(orderedItems.Select(x => x.ChecklistTask));
        return new ChecklistResponse(
            tasks,
            new ChecklistProgressResponse(progress.Total, progress.Completed, progress.Percentage));
    }
}