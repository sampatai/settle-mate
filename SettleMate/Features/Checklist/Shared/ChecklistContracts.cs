namespace SettleMate.Features.Checklist.Shared;

public sealed record ChecklistTaskResponse(
    Guid TaskId,
    string Key,
    int WeekNumber,
    string Title,
    string Explanation,
    IReadOnlyList<string> RequiredDocuments,
    int EstimatedMinutes,
    string? OfficialUrl,
    bool IsTimeSensitive,
    bool Completed,
    DateTimeOffset? CompletedAt);

public sealed record ChecklistProgressResponse(
    int Total,
    int Completed,
    decimal Percentage);

public sealed record ChecklistResponse(
    IReadOnlyList<ChecklistTaskResponse> Tasks,
    ChecklistProgressResponse Progress);