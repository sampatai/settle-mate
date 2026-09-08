using SettleMate.Abstractions.Errors;

namespace SettleMate.Features.Onboarding.Shared;

public static class OnboardingErrors
{
    public const string CodePrefix = "Onboarding";

    public static Error VisaRuleNotFound(string visaSubclass) =>
        Error.NotFound($"{CodePrefix}.VisaRuleNotFound", $"Visa subclass '{visaSubclass}' is not supported.");

    public static readonly Error ProfileNotFound =
        Error.NotFound($"{CodePrefix}.ProfileNotFound", "An onboarding profile has not been created.");

    public static readonly Error RoadmapItemNotFound =
        Error.NotFound($"{CodePrefix}.RoadmapItemNotFound", "The roadmap item was not found.");
}
