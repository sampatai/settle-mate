using SettleMate.Abstractions;
using SettleMate.Features.Onboarding.Shared;

namespace SettleMate.Database.Entities.Onboarding;

public sealed class UserProfile : AuditableEntity
{
    private UserProfile() { }

    private UserProfile(Guid id, string userId, int version, OnboardingProfileRequest request)
    {
        Id = id;
        UserId = userId;
        Version = version;
        Country = request.Country.Trim();
        VisaSubclass = request.VisaSubclass.Trim();
        ApplicantType = request.ApplicantType.Trim();
        StudyLevel = request.StudyLevel.Trim();
        CourseStarted = request.CourseStarted;
        State = request.State.Trim().ToUpperInvariant();
        UniversityOrEmployer = string.IsNullOrWhiteSpace(request.UniversityOrEmployer)
            ? null
            : request.UniversityOrEmployer.Trim();
        ArrivalDate = request.ArrivalDate;
        BudgetRange = request.BudgetRange.Trim();
        CareerGoal = request.CareerGoal.Trim();
    }

    public Guid Id { get; private set; }
    public string UserId { get; private set; } = null!;
    public int Version { get; private set; }
    public string Country { get; private set; } = null!;
    public string VisaSubclass { get; private set; } = null!;
    public string ApplicantType { get; private set; } = null!;
    public string StudyLevel { get; private set; } = null!;
    public bool CourseStarted { get; private set; }
    public string State { get; private set; } = null!;
    public string? UniversityOrEmployer { get; private set; }
    public DateOnly ArrivalDate { get; private set; }
    public string BudgetRange { get; private set; } = null!;
    public string CareerGoal { get; private set; } = null!;

    public static UserProfile Create(string userId, int version, OnboardingProfileRequest request) =>
        new(Guid.NewGuid(), userId, version, request);
}
