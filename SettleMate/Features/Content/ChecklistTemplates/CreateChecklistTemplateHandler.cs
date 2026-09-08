using System.Text.Json;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SettleMate.Abstractions;
using SettleMate.Abstractions.Errors;
using SettleMate.Database;
using SettleMate.Database.Entities.Onboarding;
using SettleMate.Features.Content.Shared;

namespace SettleMate.Features.Content.ChecklistTemplates;

public sealed record CreateChecklistTemplateCommand(ChecklistTemplateRequest Request);

public sealed class CreateChecklistTemplateCommandValidator : AbstractValidator<CreateChecklistTemplateCommand>
{
    public CreateChecklistTemplateCommandValidator()
    {
        RuleFor(x => x.Request.Key).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.WeekNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.Request.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.Description).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.Request.EstimatedMinutes).InclusiveBetween(1, 1440);
        RuleFor(x => x.Request.RequiredDocuments).NotNull();
    }
}

public sealed class CreateChecklistTemplateHandler(
    ApplicationDbContext dbContext,
    IValidator<CreateChecklistTemplateCommand> validator)
    : IHandler<CreateChecklistTemplateCommand, Result<ChecklistTemplateResponse>>
{
    public async Task<Result<ChecklistTemplateResponse>> HandleAsync(
        CreateChecklistTemplateCommand command,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);
        var key = command.Request.Key.Trim();
        if (await dbContext.ChecklistTemplates.AnyAsync(x => x.Key == key, cancellationToken))
            return Result<ChecklistTemplateResponse>.Failure([ContentErrors.ChecklistTemplateConflict]);

        var template = new ChecklistTemplate { Id = Guid.CreateVersion7() };
        template.Update(key, command.Request.WeekNumber, command.Request.Title, command.Request.Description,
            command.Request.VisaSubclass, command.Request.State, command.Request.CareerGoal,
            command.Request.Provider, command.Request.ApplicationUrl, command.Request.EligibilityNotes,
            command.Request.EstimatedMinutes, command.Request.IsTimeSensitive,
            JsonSerializer.Serialize(command.Request.RequiredDocuments));
        dbContext.ChecklistTemplates.Add(template);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result<ChecklistTemplateResponse>.Success(ContentMapper.ToResponse(template));
    }
}
