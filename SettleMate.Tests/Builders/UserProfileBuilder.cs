using SettleMate.Database.Entities.Onboarding;

namespace SettleMate.Tests.Builders;

public sealed class UserProfileBuilder
{
    private string applicantType = "Primary";
    private string studyLevel = "MastersCoursework";
    private bool courseStarted = true;

    public UserProfileBuilder WithApplicantType(string value) { applicantType = value; return this; }
    public UserProfileBuilder WithStudyLevel(string value) { studyLevel = value; return this; }
    public UserProfileBuilder WithCourseStarted(bool value) { courseStarted = value; return this; }

    public UserProfile Build() => new(
        Guid.NewGuid(),
        "user-1",
        1,
        new OnboardingProfileBuilder()
            .WithApplicantType(applicantType)
            .WithStudyLevel(studyLevel)
            .WithCourseStarted(courseStarted)
            .Build());
}
