using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SettleMate.Abstractions;
using SettleMate.Abstractions.Errors;
using SettleMate.Database;
using SettleMate.Features.Checklist.Shared;

namespace SettleMate.Features.Checklist.Queries;

public sealed class GetChecklistHandler(
    ApplicationDbContext dbContext,
    IValidator<GetChecklistQuery> validator)
    : IHandler<GetChecklistQuery, Result<ChecklistResponse>>
{
    public async Task<Result<ChecklistResponse>> HandleAsync(
        GetChecklistQuery query,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(query, cancellationToken);
        var profile = await dbContext.UserProfiles
            .Where(x => x.UserId == query.UserId)
            .OrderByDescending(x => x.Version)
            .FirstOrDefaultAsync(cancellationToken);
        if (profile is null)
            return Result<ChecklistResponse>.Failure([ChecklistErrors.ChecklistNotFound]);

        var roadmap = await dbContext.Roadmaps
            .Include(x => x.Items)
            .ThenInclude(x => x.ChecklistTask)
            .SingleOrDefaultAsync(x => x.UserId == query.UserId && x.UserProfileId == profile.Id, cancellationToken);
        if (roadmap is null)
            return Result<ChecklistResponse>.Failure([ChecklistErrors.ChecklistNotFound]);

        var keys = roadmap.Items.Select(x => x.LinkedChecklistTaskId).ToArray();
        var templates = await dbContext.ChecklistTemplates.AsNoTracking()
            .Where(x => keys.Contains(x.Key))
            .ToDictionaryAsync(x => x.Key, cancellationToken);
        return Result<ChecklistResponse>.Success(ChecklistMapper.ToResponse(roadmap.Items, templates));
    }
}