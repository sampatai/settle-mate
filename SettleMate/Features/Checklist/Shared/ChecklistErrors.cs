using SettleMate.Abstractions.Errors;

namespace SettleMate.Features.Checklist.Shared;

public static class ChecklistErrors
{
    public static readonly Error UserIdRequired =
        Error.Unauthorized("Checklist.AuthenticationRequired", "Authentication is required.");

    public static readonly Error ChecklistNotFound =
        Error.NotFound("Checklist.NotFound", "A checklist has not been created for this user.");

    public static readonly Error TaskNotFound =
        Error.NotFound("Checklist.TaskNotFound", "The checklist task was not found.");

    public static readonly Error UserAccessDenied =
        Error.Forbidden("Checklist.UserAccessDenied", "You cannot access another user's checklist.");
}