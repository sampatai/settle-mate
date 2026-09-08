namespace SettleMate.Features.Checklist.Commands;

public sealed record CompleteChecklistTaskCommand(string UserId, Guid TaskId);

public sealed record ReopenChecklistTaskCommand(string UserId, Guid TaskId);