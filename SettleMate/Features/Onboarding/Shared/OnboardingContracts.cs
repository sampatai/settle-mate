namespace SettleMate.Features.Onboarding.Shared;

public sealed record OnboardingProfileRequest(
    string Country,
    string VisaSubclass,
    string ApplicantType,
    string StudyLevel,
    bool CourseStarted,
    string State,
    string? UniversityOrEmployer,
    DateOnly ArrivalDate,
    string BudgetRange,
    string CareerGoal);

public sealed record CompleteRoadmapItemRequest(bool Completed);

public sealed record VisaRuleResponse(
    string VisaSubclass,
    string ApplicantType,
    string StudyLevel,
    bool CourseStarted,
    int? WorkHourLimitPerFortnight,
    bool TfnEligible,
    bool NdisEligible,
    bool BlueCardRequiredForChildRelatedWork,
    IReadOnlyList<string> RequiredDocuments);

public sealed record RoadmapItemResponse(
    Guid ItemId,
    int WeekNumber,
    int MonthNumber,
    string Title,
    string Description,
    Guid ChecklistTaskId,
    string LinkedChecklistTaskId,
    bool Completed,
    IReadOnlyList<string> RequiredDocuments,
    int EstimatedMinutes,
    string? OfficialUrl,
    bool IsTimeSensitive,
    DateTimeOffset? CompletedAt);

public sealed record OnboardingProgressResponse(
    int Total,
    int Completed,
    decimal Percentage);

public sealed record OnboardingResponse(
    Guid ProfileId,
    int ProfileVersion,
    string Country,
    string VisaSubclass,
    string State,
    string? UniversityOrEmployer,
    DateOnly ArrivalDate,
    string BudgetRange,
    string CareerGoal,
    VisaRuleResponse VisaRule,
    IReadOnlyList<RoadmapItemResponse> Items,
    OnboardingProgressResponse Progress);
