using FluentValidation;

namespace SettleMate.Features.Onboarding.Shared;

public class OnboardingProfileValidator : AbstractValidator<OnboardingProfileRequest>
{
    private static readonly string[] AustralianStates =
        ["ACT", "NSW", "NT", "QLD", "SA", "TAS", "VIC", "WA"];

    public OnboardingProfileValidator()
    {
        RuleFor(c => c.Country)
            .NotEmpty().WithMessage("Country of origin is required")
            .MaximumLength(100).WithMessage("Country of origin must not exceed 100 characters");

        RuleFor(c => c.VisaSubclass)
            .NotEmpty().WithMessage("Visa subclass is required")
            .MaximumLength(20).WithMessage("Visa subclass must not exceed 20 characters");

        RuleFor(c => c.ApplicantType)
            .NotEmpty().WithMessage("Applicant type is required")
            .Must(value => value is "Primary" or "Dependent")
            .WithMessage("Applicant type must be Primary or Dependent");

        RuleFor(c => c.StudyLevel)
            .NotEmpty().WithMessage("Study level is required")
            .Must(value => value is "NotStudying" or "Bachelor" or "MastersCoursework" or "MastersResearch" or "Doctoral")
            .WithMessage("Study level must be NotStudying, Bachelor, MastersCoursework, MastersResearch or Doctoral");

        RuleFor(c => c.StudyLevel)
            .NotEqual("NotStudying").When(c => c.ApplicantType == "Dependent")
            .WithMessage("A dependent of a student must provide the primary student's study level");

        RuleFor(c => c.State)
            .NotEmpty().WithMessage("Australian state or territory is required")
            .Must(state => AustralianStates.Contains(state, StringComparer.OrdinalIgnoreCase))
            .WithMessage("State must be a valid Australian state or territory code");

        RuleFor(c => c.ArrivalDate)
            .NotEmpty().WithMessage("Arrival date is required")
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)))
            .WithMessage("Arrival date cannot be in the future");

        RuleFor(c => c.BudgetRange)
            .NotEmpty().WithMessage("Budget range is required")
            .MaximumLength(50).WithMessage("Budget range must not exceed 50 characters");

        RuleFor(c => c.CareerGoal)
            .NotEmpty().WithMessage("Career goal is required")
            .MaximumLength(100).WithMessage("Career goal must not exceed 100 characters");

        RuleFor(c => c.UniversityOrEmployer)
            .MaximumLength(200).WithMessage("University or employer must not exceed 200 characters");
    }
}
