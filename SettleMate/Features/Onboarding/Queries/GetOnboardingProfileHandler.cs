using Microsoft.EntityFrameworkCore;
using SettleMate.Abstractions;
using SettleMate.Abstractions.Errors;
using SettleMate.Database;
using SettleMate.Features.Onboarding.Shared;

namespace SettleMate.Features.Onboarding.Queries;

public sealed class GetOnboardingProfileHandler(ApplicationDbContext dbContext)
    : IHandler<GetOnboardingProfileQuery, Result<OnboardingResponse>>
{
    public async Task<Result<OnboardingResponse>> HandleAsync(
        GetOnboardingProfileQuery query,
        CancellationToken cancellationToken)
    {
        var profile = await dbContext.UserProfiles
            .Where(x => x.UserId == query.UserId)
            .OrderByDescending(x => x.Version)
            .FirstOrDefaultAsync(cancellationToken);
        if (profile is null)
            return Result<OnboardingResponse>.Failure([OnboardingErrors.ProfileNotFound]);

        var rule = await dbContext.VisaRules.AsNoTracking()
            .SingleAsync(x => x.VisaSubclass == profile.VisaSubclass, cancellationToken);
        var roadmap = await dbContext.Roadmaps.Include(x => x.Items)
            .ThenInclude(x => x.ChecklistTask)
            .SingleAsync(x => x.UserProfileId == profile.Id, cancellationToken);
        var keys = roadmap.Items.Select(x => x.LinkedChecklistTaskId).ToArray();
        var templates = await dbContext.ChecklistTemplates.AsNoTracking()
            .Where(x => keys.Contains(x.Key))
            .ToDictionaryAsync(x => x.Key, cancellationToken);
        return Result<OnboardingResponse>.Success(OnboardingMapper.ToResponse(profile, rule, roadmap.Items, templates));
    }
}