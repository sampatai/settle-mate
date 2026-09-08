using Microsoft.EntityFrameworkCore;
using SettleMate.Abstractions;
using SettleMate.Abstractions.Errors;
using SettleMate.Database;
using SettleMate.Features.Onboarding.Shared;

namespace SettleMate.Features.Onboarding.Commands;

public sealed class SetRoadmapItemCompletedHandler(ApplicationDbContext dbContext)
    : IHandler<SetRoadmapItemCompletedCommand, Result<bool>>
{
    public async Task<Result<bool>> HandleAsync(
        SetRoadmapItemCompletedCommand command,
        CancellationToken cancellationToken)
    {
        var item = await dbContext.RoadmapItems
            .Include(x => x.ChecklistTask)
            .SingleOrDefaultAsync(x => x.Id == command.ItemId && x.Roadmap.UserId == command.UserId, cancellationToken);
        if (item is null)
            return Result<bool>.Failure([OnboardingErrors.RoadmapItemNotFound]);
        item.SetCompleted(command.Completed);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}