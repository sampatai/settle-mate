using SettleMate.Database.Entities.Onboarding;

namespace SettleMate.Features.Content.Shared;

public sealed record PageRequest(int Page = 1, int PageSize = 20);

public sealed record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);

public sealed record ChecklistTemplateResponse(
    Guid Id,
    string Key,
    int WeekNumber,
    string Title,
    string Description,
    string? VisaSubclass,
    string? State,
    string? CareerGoal,
    string? Provider,
    string? ApplicationUrl,
    string? EligibilityNotes,
    int EstimatedMinutes,
    bool IsTimeSensitive,
    IReadOnlyList<string> RequiredDocuments);

public sealed record ChecklistTemplateRequest(
    string Key,
    int WeekNumber,
    string Title,
    string Description,
    string? VisaSubclass,
    string? State,
    string? CareerGoal,
    string? Provider,
    string? ApplicationUrl,
    string? EligibilityNotes,
    int EstimatedMinutes,
    bool IsTimeSensitive,
    IReadOnlyList<string> RequiredDocuments);

public sealed record VisaRuleResponse(
    string VisaSubclass,
    int? WorkHourLimitPerFortnight,
    int? DependentBachelorWorkHourLimitPerFortnight,
    int? DependentPostgraduateWorkHourLimitPerFortnight,
    bool TfnEligible,
    bool NdisEligible,
    bool BlueCardRequiredForChildRelatedWork,
    IReadOnlyList<string> RequiredDocuments);

public sealed record VisaRuleRequest(
    string VisaSubclass,
    int? WorkHourLimitPerFortnight,
    int? DependentBachelorWorkHourLimitPerFortnight,
    int? DependentPostgraduateWorkHourLimitPerFortnight,
    bool TfnEligible,
    bool NdisEligible,
    bool BlueCardRequiredForChildRelatedWork,
    IReadOnlyList<string> RequiredDocuments);

public static class ContentMapper
{
    public static ChecklistTemplateResponse ToResponse(ChecklistTemplate template) => new(
        template.Id,
        template.Key,
        template.WeekNumber,
        template.Title,
        template.Description,
        template.VisaSubclass,
        template.State,
        template.CareerGoal,
        template.Provider,
        template.ApplicationUrl,
        template.EligibilityNotes,
        template.EstimatedMinutes,
        template.IsTimeSensitive,
        template.RequiredDocuments);

    public static VisaRuleResponse ToResponse(VisaRule rule) => new(
        rule.VisaSubclass,
        rule.WorkHourLimitPerFortnight,
        rule.DependentBachelorWorkHourLimitPerFortnight,
        rule.DependentPostgraduateWorkHourLimitPerFortnight,
        rule.TfnEligible,
        rule.NdisEligible,
        rule.BlueCardRequiredForChildRelatedWork,
        System.Text.Json.JsonSerializer.Deserialize<string[]>(rule.RequiredDocumentsJson) ?? []);
}
