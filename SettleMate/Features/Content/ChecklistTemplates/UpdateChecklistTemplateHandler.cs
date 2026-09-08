using System.Text.Json;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SettleMate.Abstractions;
using SettleMate.Abstractions.Errors;
using SettleMate.Database;
using SettleMate.Features.Content.Shared;

namespace SettleMate.Features.Content.ChecklistTemplates;

public sealed record UpdateChecklistTemplateCommand(Guid Id, ChecklistTemplateRequest Request);

public sealed class UpdateChecklistTemplateCommandValidator : AbstractValidator<UpdateChecklistTemplateCommand>
{
    public UpdateChecklistTemplateCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Request.Key).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.WeekNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.Request.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.Description).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.Request.EstimatedMinutes).InclusiveBetween(1, 1440);
    }
}

public sealed class UpdateChecklistTemplateHandler(
    ApplicationDbContext dbContext,
    IValidator<UpdateChecklistTemplateCommand> validator)
    : IHandler<UpdateChecklistTemplateCommand, Result<ChecklistTemplateResponse>>
{
    public async Task<Result<ChecklistTemplateResponse>> HandleAsync(
        UpdateChecklistTemplateCommand command,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);
        var template = await dbContext.ChecklistTemplates
            .SingleOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
        if (template is null)
            return Result<ChecklistTemplateResponse>.Failure([ContentErrors.ChecklistTemplateNotFound]);
        var key = command.Request.Key.Trim();
        if (await dbContext.ChecklistTemplates.AnyAsync(x => x.Id != command.Id && x.Key == key, cancellationToken))
            return Result<ChecklistTemplateResponse>.Failure([ContentErrors.ChecklistTemplateConflict]);
        template.Update(key, command.Request.WeekNumber, command.Request.Title, command.Request.Description,
            command.Request.VisaSubclass, command.Request.State, command.Request.CareerGoal,
            command.Request.Provider, command.Request.ApplicationUrl, command.Request.EligibilityNotes,
            command.Request.EstimatedMinutes, command.Request.IsTimeSensitive,
            JsonSerializer.Serialize(command.Request.RequiredDocuments));
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result<ChecklistTemplateResponse>.Success(ContentMapper.ToResponse(template));
    }
}
