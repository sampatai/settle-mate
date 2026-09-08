using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SettleMate.Abstractions;
using SettleMate.Abstractions.Errors;
using SettleMate.Database;
using SettleMate.Database.Entities.Onboarding;
using SettleMate.Features.Checklist.Shared;

namespace SettleMate.Features.Checklist.Queries;

public sealed class GetChecklistProgressHandler(
    ApplicationDbContext dbContext,
    IValidator<GetChecklistProgressQuery> validator)
    : IHandler<GetChecklistProgressQuery, Result<ChecklistProgressResponse>>
{
    public async Task<Result<ChecklistProgressResponse>> HandleAsync(
        GetChecklistProgressQuery query,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(query, cancellationToken);
        var profile = await dbContext.UserProfiles
            .Where(x => x.UserId == query.UserId)
            .OrderByDescending(x => x.Version)
            .FirstOrDefaultAsync(cancellationToken);
        if (profile is null)
            return Result<ChecklistProgressResponse>.Failure([ChecklistErrors.ChecklistNotFound]);

        var tasks = await dbContext.RoadmapItems
            .Where(x => x.Roadmap.UserId == query.UserId && x.Roadmap.UserProfileId == profile.Id)
            .Select(x => x.ChecklistTask)
            .Distinct()
            .ToListAsync(cancellationToken);
        var progress = ChecklistProgress.Calculate(tasks);
        return Result<ChecklistProgressResponse>.Success(
            new ChecklistProgressResponse(progress.Total, progress.Completed, progress.Percentage));
    }
}