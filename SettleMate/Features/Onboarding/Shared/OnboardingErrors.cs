using SettleMate.Abstractions.Errors;

namespace SettleMate.Features.Onboarding.Shared;

public static class OnboardingErrors
{
    public static Error VisaRuleNotFound(string visaSubclass) =>
        Error.NotFound("Onboarding.VisaRuleNotFound", $"Visa subclass '{visaSubclass}' is not supported.");

    public static readonly Error ProfileNotFound =
        Error.NotFound("Onboarding.ProfileNotFound", "An onboarding profile has not been created.");

    public static readonly Error RoadmapItemNotFound =
        Error.NotFound("Onboarding.RoadmapItemNotFound", "The roadmap item was not found.");
}
