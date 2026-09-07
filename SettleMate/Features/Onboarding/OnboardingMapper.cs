using System.Text.Json;
using SettleMate.Database.Entities.Onboarding;
using SettleMate.Features.Onboarding.Shared;

namespace SettleMate.Features.Onboarding;

internal static class OnboardingMapper
{
    public static OnboardingResponse ToResponse(
        UserProfile profile,
        VisaRule rule,
        IEnumerable<RoadmapItem> items) =>
        new(
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
            items.OrderBy(item => item.WeekNumber)
                .ThenBy(item => item.Title)
                .Select(item => new RoadmapItemResponse(
                    item.Id,
                    item.WeekNumber,
                    ((item.WeekNumber - 1) / 4) + 1,
                    item.Title,
                    item.Description,
                    item.ChecklistTaskId,
                    item.LinkedChecklistTaskId,
                    item.Completed))
                .ToArray());

}
