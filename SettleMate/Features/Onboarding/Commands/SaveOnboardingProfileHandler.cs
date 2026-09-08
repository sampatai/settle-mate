using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SettleMate.Abstractions;
using SettleMate.Abstractions.Errors;
using SettleMate.Database;
using SettleMate.Database.Entities.Onboarding;
using SettleMate.Features.Onboarding.Shared;

namespace SettleMate.Features.Onboarding.Commands;

public sealed class SaveOnboardingProfileHandler(
    ApplicationDbContext dbContext,
    IValidator<OnboardingProfileRequest> validator)
    : IHandler<SaveOnboardingProfileCommand, Result<OnboardingResponse>>
{
    public async Task<Result<OnboardingResponse>> HandleAsync(
        SaveOnboardingProfileCommand command,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(command.Request, cancellationToken);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        var rule = await dbContext.VisaRules.AsNoTracking()
            .SingleOrDefaultAsync(x => x.VisaSubclass == command.Request.VisaSubclass.Trim(), cancellationToken);
        if (rule is null)
            return Result<OnboardingResponse>.Failure([OnboardingErrors.VisaRuleNotFound(command.Request.VisaSubclass)]);

        var previousProfile = await dbContext.UserProfiles
            .Where(x => x.UserId == command.UserId)
            .OrderByDescending(x => x.Version)
            .FirstOrDefaultAsync(cancellationToken);
        var completedKeys = await dbContext.ChecklistTasks
            .Where(x => x.UserId == command.UserId && x.Completed)
            .Select(x => x.Key)
            .ToListAsync(cancellationToken);

        var profile = new UserProfile(Guid.CreateVersion7(), command.UserId, (previousProfile?.Version ?? 0) + 1, command.Request);
        var roadmap = await BuildRoadmapAsync(command.UserId, profile, rule, completedKeys, cancellationToken);
        dbContext.UserProfiles.Add(profile);
        dbContext.Roadmaps.Add(roadmap);
        await dbContext.SaveChangesAsync(cancellationToken);
        var keys = roadmap.Items.Select(x => x.LinkedChecklistTaskId).ToArray();
        var templates = await dbContext.ChecklistTemplates.AsNoTracking()
            .Where(x => keys.Contains(x.Key))
            .ToDictionaryAsync(x => x.Key, cancellationToken);
        return Result<OnboardingResponse>.Success(OnboardingMapper.ToResponse(profile, rule, roadmap.Items, templates));
    }

    private async Task<Roadmap> BuildRoadmapAsync(
        string userId,
        UserProfile profile,
        VisaRule rule,
        IReadOnlyCollection<string> completedKeys,
        CancellationToken cancellationToken)
    {
        var templates = await dbContext.ChecklistTemplates.AsNoTracking()
            .Where(x => (x.VisaSubclass == null || x.VisaSubclass == profile.VisaSubclass) &&
                        (x.State == null || x.State == profile.State) &&
                        (x.CareerGoal == null || x.CareerGoal.ToLower() == profile.CareerGoal.ToLower()))
            .OrderBy(x => x.WeekNumber)
            .ToListAsync(cancellationToken);
        var definitions = templates.Select(x => (WeekNumber: x.WeekNumber, Title: x.Title, Description: x.GetGuidanceDescription(), Key: x.Key)).ToList();
        definitions.Add(profile.BudgetRange.Contains("low", StringComparison.OrdinalIgnoreCase)
            ? (6, "Find a low-cost community support option", $"Compare free or low-cost services near {profile.State}.", "profile:budget:low-cost-support")
            : (6, "Review your settlement budget", $"Review spending against your {profile.BudgetRange} budget range.", "profile:budget:review"));
        definitions.Add((7, $"Build a first step toward {profile.CareerGoal}",
            $"Choose one practical action toward your {profile.CareerGoal} goal.", "profile:career:next-step"));

        var workKey = $"visa:{rule.VisaSubclass}:{profile.ApplicantType.ToLowerInvariant()}-work-conditions";
        var keys = definitions.Select(x => x.Key)
            .Concat([$"visa:{rule.VisaSubclass}:tax-file-number", workKey])
            .Concat(rule.NdisEligible ? [$"visa:{rule.VisaSubclass}:ndis"] : [])
            .Concat(rule.BlueCardRequiredForChildRelatedWork ? [$"visa:{rule.VisaSubclass}:blue-card"] : [])
            .Distinct().ToArray();
        var tasks = await EnsureChecklistTasksAsync(userId, keys, completedKeys, cancellationToken);
        var roadmap = new Roadmap(Guid.CreateVersion7(), profile.UserId, profile.Id);
        foreach (var definition in definitions)
            roadmap.AddItem(CreateItem(definition.WeekNumber, definition.Title, definition.Description, tasks[definition.Key]));

        var tfnKey = $"visa:{rule.VisaSubclass}:tax-file-number";
        roadmap.AddItem(CreateItem(1, rule.TfnEligible ? "Apply for a Tax File Number (TFN)" : "Confirm your visa work conditions",
            rule.TfnEligible
                ? $"Your {rule.VisaSubclass} visa is eligible for a TFN. Keep your identity and visa documents ready."
                : $"Your {rule.VisaSubclass} visa is not marked TFN-eligible in the curated rules. Check Home Affairs guidance.",
            tasks[tfnKey]));
        roadmap.AddItem(CreateItem(2, "Plan work within your visa limit", WorkDescription(profile, rule), tasks[workKey]));
        if (rule.NdisEligible)
            roadmap.AddItem(CreateItem(3, "Check NDIS access", "Review residency and disability eligibility before relying on NDIS support.",
                tasks[$"visa:{rule.VisaSubclass}:ndis"]));
        if (rule.BlueCardRequiredForChildRelatedWork)
            roadmap.AddItem(CreateItem(3, "Check Blue Card requirements", "If your career involves children, confirm state screening requirements.",
                tasks[$"visa:{rule.VisaSubclass}:blue-card"]));
        return roadmap;
    }

    private async Task<Dictionary<string, ChecklistTask>> EnsureChecklistTasksAsync(
        string userId,
        IEnumerable<string> keys,
        IReadOnlyCollection<string> completedKeys,
        CancellationToken cancellationToken)
    {
        var keyArray = keys.ToArray();
        var tasks = await dbContext.ChecklistTasks
            .Where(x => x.UserId == userId && keyArray.Contains(x.Key))
            .ToDictionaryAsync(x => x.Key, cancellationToken);
        foreach (var key in keyArray)
        {
            if (tasks.ContainsKey(key))
                continue;
            var task = ChecklistTask.Create(userId, key, completedKeys.Contains(key));
            dbContext.ChecklistTasks.Add(task);
            tasks[key] = task;
        }
        return tasks;
    }

    private static RoadmapItem CreateItem(int week, string title, string description, ChecklistTask task) =>
       new RoadmapItem(Guid.CreateVersion7(), week, title, description, task);

    private static string WorkDescription(UserProfile profile, VisaRule rule)
    {
        var limit = rule.GetWorkHourLimit(profile);
        return limit == 0
            ? "Do not work before the primary student's course has started. Check your visa grant conditions in VEVO."
            : limit is null
                ? "Your curated rule records unlimited work after the course starts. Confirm conditions in VEVO."
                : $"Keep work within {limit} hours per fortnight and confirm conditions in VEVO.";
    }
}

public sealed class OnboardingProfileValidator : AbstractValidator<OnboardingProfileRequest>
{
    private static readonly string[] AustralianStates =
        ["ACT", "NSW", "NT", "QLD", "SA", "TAS", "VIC", "WA"];
    private static readonly string[] CareerCategories =
        ["Aged Care", "Hospitality", "IT", "Child Care", "Construction"];

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
            .MaximumLength(100).WithMessage("Career goal must not exceed 100 characters")
            .Must(goal => CareerCategories.Contains(goal, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Career goal must be Aged Care, Hospitality, IT, Child Care or Construction");
        RuleFor(c => c.UniversityOrEmployer)
            .MaximumLength(200).WithMessage("University or employer must not exceed 200 characters");
    }
}