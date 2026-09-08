using SettleMate.Database.Entities.Onboarding;
using SettleMate.Tests.Builders;
using Shouldly;

namespace SettleMate.Tests.Unit.Domain;

public sealed class OnboardingDomainTests
{
    [Fact]
    public void UserProfile_WithUntrimmedRequest_NormalizesStoredValues()
    {
        var request = new OnboardingProfileBuilder()
            .WithState(" qld ")
            .Build();

        var profile = new UserProfile(Guid.NewGuid(), "user-1", 1, request);

        profile.State.ShouldBe("QLD");
        profile.VisaSubclass.ShouldBe("500");
        profile.CareerGoal.ShouldBe("Aged Care");
    }

    [Theory]
    [InlineData("Primary", "MastersCoursework", true, 48)]
    [InlineData("Dependent", "Bachelor", true, 48)]
    [InlineData("Dependent", "MastersCoursework", true, null)]
    [InlineData("Primary", "MastersCoursework", false, 0)]
    public void VisaRule_GetWorkHourLimit_ReturnsApplicableLimit(
        string applicantType,
        string studyLevel,
        bool courseStarted,
        int? expectedLimit)
    {
        var rule = new VisaRule
        {
            VisaSubclass = "500",
            WorkHourLimitPerFortnight = 48,
            DependentBachelorWorkHourLimitPerFortnight = 48,
            DependentPostgraduateWorkHourLimitPerFortnight = null
        };
        var profile = new UserProfileBuilder()
            .WithApplicantType(applicantType)
            .WithStudyLevel(studyLevel)
            .WithCourseStarted(courseStarted)
            .Build();

        rule.GetWorkHourLimit(profile).ShouldBe(expectedLimit);
    }

    [Fact]
    public void ChecklistProgress_WithSixCompletedTasks_ReturnsFiftyPercent()
    {
        var tasks = Enumerable.Range(0, 12)
            .Select(index => ChecklistTask.Create("user-1", $"task-{index}", index < 6))
            .ToArray();

        var progress = ChecklistProgress.Calculate(tasks);

        progress.Total.ShouldBe(12);
        progress.Completed.ShouldBe(6);
        progress.Percentage.ShouldBe(50m);
    }

    [Fact]
    public void ChecklistProgress_WithNoTasks_ReturnsZeroPercent()
    {
        var progress = ChecklistProgress.Calculate([]);

        progress.Total.ShouldBe(0);
        progress.Completed.ShouldBe(0);
        progress.Percentage.ShouldBe(0m);
    }

    [Fact]
    public void RoadmapItem_SetCompleted_SynchronizesChecklistTask()
    {
        var task = ChecklistTask.Create("user-1", "task-1");
        var item = new RoadmapItem(Guid.NewGuid(), 1, "Task", "Description", task);

        item.SetCompleted(true);

        item.Completed.ShouldBeTrue();
        task.Completed.ShouldBeTrue();
        task.CompletedAt.ShouldNotBeNull();
    }

    [Fact]
    public void ChecklistTask_SetCompleted_SynchronizesLinkedRoadmapItems()
    {
        var task = ChecklistTask.Create("user-1", "task-1");
        var item = new RoadmapItem(Guid.NewGuid(), 1, "Task", "Description", task);
        task.RoadmapItems.Add(item);

        task.SetCompleted(true);

        item.Completed.ShouldBeTrue();
        task.CompletedAt.ShouldNotBeNull();
    }
}

