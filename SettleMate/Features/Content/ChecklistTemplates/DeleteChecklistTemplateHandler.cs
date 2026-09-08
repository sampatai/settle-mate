using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SettleMate.Abstractions;
using SettleMate.Abstractions.Errors;
using SettleMate.Database;
using SettleMate.Features.Content.Shared;

namespace SettleMate.Features.Content.ChecklistTemplates;

public sealed record DeleteChecklistTemplateCommand(Guid Id);

public sealed class DeleteChecklistTemplateCommandValidator : AbstractValidator<DeleteChecklistTemplateCommand>
{
    public DeleteChecklistTemplateCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public sealed class DeleteChecklistTemplateHandler(
    ApplicationDbContext dbContext,
    IValidator<DeleteChecklistTemplateCommand> validator)
    : IHandler<DeleteChecklistTemplateCommand, Result<bool>>
{
    public async Task<Result<bool>> HandleAsync(
        DeleteChecklistTemplateCommand command,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);
        var template = await dbContext.ChecklistTemplates
            .SingleOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
        if (template is null)
            return Result<bool>.Failure([ContentErrors.ChecklistTemplateNotFound]);
        dbContext.ChecklistTemplates.Remove(template);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}
