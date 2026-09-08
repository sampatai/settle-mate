using SettleMate.Features.Onboarding.Shared;

namespace SettleMate.Features.Onboarding.Commands;

public sealed record SaveOnboardingProfileCommand(
    string UserId,
    OnboardingProfileRequest Request);

public sealed record SetRoadmapItemCompletedCommand(
    string UserId,
    Guid ItemId,
    bool Completed);

