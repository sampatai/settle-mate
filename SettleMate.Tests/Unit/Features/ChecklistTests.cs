using SettleMate.Database.Entities.Onboarding;
using SettleMate.Features.Checklist.Commands;
using SettleMate.Features.Checklist.Queries;
using SettleMate.Features.Checklist.Shared;
using SettleMate.Tests.Builders;
using Shouldly;

namespace SettleMate.Tests.Unit.Features;

public sealed class ChecklistTests
{
    [Fact]
    public void ChecklistTemplate_GetGuidanceDescription_IncludesConfiguredGuidance()
    {
        var template = new ChecklistTemplate
        {
            Description = "Apply for screening.",
            Provider = "Queensland Government",
            ApplicationUrl = "https://example.gov.au/screening",
            EligibilityNotes = "Required before regulated work.",
            RequiredDocumentsJson = "[\"Passport\",\"Visa\"]"
        };

        var guidance = template.GetGuidanceDescription();

        guidance.ShouldContain("Queensland Government");
        guidance.ShouldContain("https://example.gov.au/screening");
        guidance.ShouldContain("Passport, Visa");
    }

    [Fact]
    public void ChecklistTemplate_AppliesToMatchingProfile_ReturnsTrue()
    {
        var template = new ChecklistTemplate
        {
            VisaSubclass = "500",
            State = "QLD",
            CareerGoal = "Aged Care"
        };
        var profile = new UserProfileBuilder().Build();

        template.AppliesTo(profile).ShouldBeTrue();
    }

    [Fact]
    public void ChecklistMapper_ToResponse_MapsMetadataAndProgress()
    {
        var task = ChecklistTask.Create("user-1", "screening", completed: true);
        var item = new RoadmapItem(Guid.NewGuid(), 3, "Screening", "Complete screening", task);
        var template = new ChecklistTemplate
        {
            Key = "screening",
            EstimatedMinutes = 45,
            IsTimeSensitive = true,
            ApplicationUrl = "https://example.gov.au",
            RequiredDocumentsJson = "[\"Passport\"]"
        };

        var response = ChecklistMapper.ToResponse(
            [item],
            new Dictionary<string, ChecklistTemplate> { [template.Key] = template });

        response.Tasks.ShouldHaveSingleItem();
        response.Tasks[0].EstimatedMinutes.ShouldBe(45);
        response.Tasks[0].IsTimeSensitive.ShouldBeTrue();
        response.Tasks[0].OfficialUrl.ShouldBe("https://example.gov.au");
        response.Progress.Percentage.ShouldBe(100m);
    }

    [Fact]
    public async Task CompleteChecklistTaskValidator_WithEmptyTaskId_Fails()
    {
        var validator = new CompleteChecklistTaskCommandValidator();

        var result = await validator.ValidateAsync(new CompleteChecklistTaskCommand("user-1", Guid.Empty));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(error => error.PropertyName == "TaskId");
    }

    [Fact]
    public async Task GetChecklistQueryValidator_WithEmptyUserId_Fails()
    {
        var validator = new GetChecklistQueryValidator();

        var result = await validator.ValidateAsync(new GetChecklistQuery(string.Empty));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(error => error.PropertyName == "UserId");
    }
}
