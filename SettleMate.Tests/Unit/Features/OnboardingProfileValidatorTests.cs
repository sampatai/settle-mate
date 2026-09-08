using SettleMate.Features.Onboarding.Commands;
using SettleMate.Tests.Builders;
using Shouldly;

namespace SettleMate.Tests.Unit.Features;

public sealed class OnboardingProfileValidatorTests
{
    private readonly OnboardingProfileValidator validator = new();

    [Fact]
    public async Task Validate_WithValidProfile_Succeeds()
    {
        var result = await validator.ValidateAsync(new OnboardingProfileBuilder().Build());

        result.IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("Unknown Career")]
    public async Task Validate_WithUnsupportedCareerGoal_Fails(string careerGoal)
    {
        var result = await validator.ValidateAsync(
            new OnboardingProfileBuilder().WithCareerGoal(careerGoal).Build());

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(error => error.PropertyName == "CareerGoal");
    }

    [Fact]
    public async Task Validate_WithFutureArrivalDate_Fails()
    {
        var result = await validator.ValidateAsync(
            new OnboardingProfileBuilder()
                .WithArrivalDate(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)))
                .Build());

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(error => error.PropertyName == "ArrivalDate");
    }

    [Fact]
    public async Task Validate_WithDependentNotStudying_Fails()
    {
        var result = await validator.ValidateAsync(
            new OnboardingProfileBuilder()
                .WithApplicantType("Dependent")
                .WithStudyLevel("NotStudying")
                .Build());

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(error => error.PropertyName == "StudyLevel");
    }
}
