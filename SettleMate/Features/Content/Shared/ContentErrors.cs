using SettleMate.Abstractions.Errors;

namespace SettleMate.Features.Content.Shared;

public static class ContentErrors
{
    public static readonly Error ChecklistTemplateNotFound =
        Error.NotFound("Content.ChecklistTemplateNotFound", "The checklist template was not found.");
    public static readonly Error ChecklistTemplateConflict =
        Error.Conflict("Content.ChecklistTemplateConflict", "A checklist template with that key already exists.");
    public static readonly Error VisaRuleNotFound =
        Error.NotFound("Content.VisaRuleNotFound", "The visa rule was not found.");
    public static readonly Error VisaRuleConflict =
        Error.Conflict("Content.VisaRuleConflict", "A visa rule with that subclass already exists.");
}
