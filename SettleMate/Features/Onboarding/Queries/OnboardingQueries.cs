using SettleMate.Features.Onboarding.Shared;

namespace SettleMate.Features.Onboarding.Queries;

public sealed record PreviewOnboardingQuery(OnboardingProfileRequest Request);

public sealed record GetOnboardingProfileQuery(string UserId);
