using SettleMate.Features.Onboarding.Shared;

namespace SettleMate.Tests.Builders;

public sealed class OnboardingProfileBuilder
{
    private string country = "Nepal";
    private string visaSubclass = "500";
    private string applicantType = "Primary";
    private string studyLevel = "MastersCoursework";
    private bool courseStarted = true;
    private string state = "QLD";
    private string? universityOrEmployer = "QUT";
    private DateOnly arrivalDate = new(2026, 1, 1);
    private string budgetRange = "100-200";
    private string careerGoal = "Aged Care";

    public OnboardingProfileBuilder WithApplicantType(string value) { applicantType = value; return this; }
    public OnboardingProfileBuilder WithCourseStarted(bool value) { courseStarted = value; return this; }
    public OnboardingProfileBuilder WithStudyLevel(string value) { studyLevel = value; return this; }
    public OnboardingProfileBuilder WithState(string value) { state = value; return this; }
    public OnboardingProfileBuilder WithCareerGoal(string value) { careerGoal = value; return this; }
    public OnboardingProfileBuilder WithArrivalDate(DateOnly value) { arrivalDate = value; return this; }

    public OnboardingProfileRequest Build() => new(
        country,
        visaSubclass,
        applicantType,
        studyLevel,
        courseStarted,
        state,
        universityOrEmployer,
        arrivalDate,
        budgetRange,
        careerGoal);
}
