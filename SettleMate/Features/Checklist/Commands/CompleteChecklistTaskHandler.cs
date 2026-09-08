using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SettleMate.Abstractions;
using SettleMate.Abstractions.Errors;
using SettleMate.Database;
using SettleMate.Features.Checklist.Shared;

namespace SettleMate.Features.Checklist.Commands;

public sealed class CompleteChecklistTaskHandler(
    ApplicationDbContext dbContext,
    IValidator<CompleteChecklistTaskCommand> validator)
    : IHandler<CompleteChecklistTaskCommand, Result<bool>>
{
    public async Task<Result<bool>> HandleAsync(
        CompleteChecklistTaskCommand command,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);
        var task = await dbContext.ChecklistTasks
            .Include(x => x.RoadmapItems)
            .SingleOrDefaultAsync(x => x.Id == command.TaskId && x.UserId == command.UserId, cancellationToken);
        if (task is null)
            return Result<bool>.Failure([ChecklistErrors.TaskNotFound]);

        task.SetCompleted(true);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}