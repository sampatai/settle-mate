using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SettleMate.Abstractions;
using SettleMate.Abstractions.Errors;
using SettleMate.Database;
using SettleMate.Database.Entities.Onboarding;
using SettleMate.Features.Onboarding.Commands;
using SettleMate.Features.Onboarding.Shared;

namespace SettleMate.Features.Onboarding.Queries;

public sealed class PreviewOnboardingHandler(
    ApplicationDbContext dbContext,
    IValidator<OnboardingProfileRequest> validator)
    : IHandler<PreviewOnboardingQuery, Result<OnboardingResponse>>
{
    public async Task<Result<OnboardingResponse>> HandleAsync(
        PreviewOnboardingQuery query,
        CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(query.Request, cancellationToken);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        var rule = await dbContext.VisaRules.AsNoTracking()
            .SingleOrDefaultAsync(x => x.VisaSubclass == query.Request.VisaSubclass.Trim(), cancellationToken);
        if (rule is null)
            return Result<OnboardingResponse>.Failure([OnboardingErrors.VisaRuleNotFound(query.Request.VisaSubclass)]);

        var profile = new UserProfile(Guid.CreateVersion7(), string.Empty, 0, query.Request);
        var templates = await dbContext.ChecklistTemplates.AsNoTracking()
            .Where(x => (x.VisaSubclass == null || x.VisaSubclass == profile.VisaSubclass) &&
                        (x.State == null || x.State == profile.State) &&
                        (x.CareerGoal == null || x.CareerGoal.ToLower() == profile.CareerGoal.ToLower()))
            .OrderBy(x => x.WeekNumber)
            .ToListAsync(cancellationToken);
        var roadmap = new Roadmap(Guid.CreateVersion7(), string.Empty, profile.Id);
        foreach (var template in templates)
        {
            var task = ChecklistTask.Create(string.Empty, template.Key);
            roadmap.AddItem(new RoadmapItem(Guid.CreateVersion7(), template.WeekNumber, template.Title, template.GetGuidanceDescription(), task));
        }
        var tfnKey = $"visa:{rule.VisaSubclass}:tax-file-number";
        roadmap.AddItem(new RoadmapItem(Guid.CreateVersion7(),
            1,
            rule.TfnEligible ? "Apply for a Tax File Number (TFN)" : "Confirm your visa work conditions",
            rule.TfnEligible
                ? $"Your {rule.VisaSubclass} visa is eligible for a TFN. Keep your identity and visa documents ready."
                : $"Your {rule.VisaSubclass} visa is not marked TFN-eligible in the curated rules. Check Home Affairs guidance.",
            ChecklistTask.Create(string.Empty, tfnKey)));
        var workKey = $"visa:{rule.VisaSubclass}:{profile.ApplicantType.ToLowerInvariant()}-work-conditions";
        var workTask = ChecklistTask.Create(string.Empty, workKey);
        var limit = rule.GetWorkHourLimit(profile);
        roadmap.AddItem(new RoadmapItem(Guid.CreateVersion7(),
            2,
            "Plan work within your visa limit",
            limit == 0
                ? "Do not work before the primary student's course has started. Check your visa grant conditions in VEVO."
                : limit is null
                    ? "Your curated rule records unlimited work after the course starts. Confirm conditions in VEVO."
                    : $"Keep work within {limit} hours per fortnight and confirm conditions in VEVO.",
            workTask));
        if (rule.BlueCardRequiredForChildRelatedWork)
            roadmap.AddItem(new RoadmapItem(Guid.CreateVersion7(), 3, "Check Blue Card requirements",
                "If your career involves children, confirm state screening requirements.",
                ChecklistTask.Create(string.Empty, $"visa:{rule.VisaSubclass}:blue-card")));
        if (rule.NdisEligible)
            roadmap.AddItem(new RoadmapItem(Guid.CreateVersion7(), 3, "Check NDIS access",
                "Review residency and disability eligibility before relying on NDIS support.",
                ChecklistTask.Create(string.Empty, $"visa:{rule.VisaSubclass}:ndis")));
        return Result<OnboardingResponse>.Success(
            OnboardingMapper.ToResponse(profile, rule, roadmap.Items, templates.ToDictionary(x => x.Key)));
    }
}