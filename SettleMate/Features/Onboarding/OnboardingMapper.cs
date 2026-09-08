using System.Text.Json;
using SettleMate.Database.Entities.Onboarding;
using SettleMate.Features.Onboarding.Shared;

namespace SettleMate.Features.Onboarding;

internal static class OnboardingMapper
{
    public static OnboardingResponse ToResponse(
        UserProfile profile,
        VisaRule rule,
        IEnumerable<RoadmapItem> items,
        IReadOnlyDictionary<string, ChecklistTemplate>? templates = null)
    {
        var orderedItems = items.OrderBy(item => item.WeekNumber)
            .ThenBy(item => item.Title)
            .ToArray();
        var completed = orderedItems.Count(item => item.Completed);
        return new(
            profile.Id,
            profile.Version,
            profile.Country,
            profile.VisaSubclass,
            profile.State,
            profile.UniversityOrEmployer,
            profile.ArrivalDate,
            profile.BudgetRange,
            profile.CareerGoal,
            new VisaRuleResponse(
                rule.VisaSubclass,
                profile.ApplicantType,
                profile.StudyLevel,
                profile.CourseStarted,
                rule.GetWorkHourLimit(profile),
                rule.TfnEligible,
                rule.NdisEligible,
                rule.BlueCardRequiredForChildRelatedWork,
                JsonSerializer.Deserialize<string[]>(rule.RequiredDocumentsJson) ?? []),
            orderedItems.Select(item => MapItem(item, templates))
                .ToArray(),
            new OnboardingProgressResponse(
                orderedItems.Length,
                completed,
                orderedItems.Length == 0 ? 0m : Math.Round(completed * 100m / orderedItems.Length, 2)));
    }

    private static RoadmapItemResponse MapItem(
        RoadmapItem item,
        IReadOnlyDictionary<string, ChecklistTemplate>? templates)
    {
        ChecklistTemplate? template = null;
        templates?.TryGetValue(item.LinkedChecklistTaskId, out template);
        return new RoadmapItemResponse(
            item.Id,
            item.WeekNumber,
            ((item.WeekNumber - 1) / 4) + 1,
            item.Title,
            item.Description,
            item.ChecklistTaskId,
            item.LinkedChecklistTaskId,
            item.Completed,
            template?.RequiredDocuments ?? [],
            template?.EstimatedMinutes ?? 30,
            template?.ApplicationUrl,
            template?.IsTimeSensitive ?? false,
            item.ChecklistTask?.CompletedAt);
    }

}
