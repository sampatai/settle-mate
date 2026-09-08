namespace SettleMate.Features.Checklist.Queries;

public sealed record GetChecklistQuery(string UserId);

public sealed record GetChecklistProgressQuery(string UserId);