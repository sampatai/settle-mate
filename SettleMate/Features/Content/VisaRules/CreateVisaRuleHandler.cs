using System.Text.Json;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SettleMate.Abstractions;
using SettleMate.Abstractions.Errors;
using SettleMate.Database;
using SettleMate.Database.Entities.Onboarding;
using SettleMate.Features.Content.Shared;

namespace SettleMate.Features.Content.VisaRules;

public sealed record CreateVisaRuleCommand(VisaRuleRequest Request);

public sealed class CreateVisaRuleCommandValidator : AbstractValidator<CreateVisaRuleCommand>
{
    public CreateVisaRuleCommandValidator()
    {
        RuleFor(x => x.Request.VisaSubclass).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Request.WorkHourLimitPerFortnight).GreaterThanOrEqualTo(0).When(x => x.Request.WorkHourLimitPerFortnight.HasValue);
        RuleFor(x => x.Request.DependentBachelorWorkHourLimitPerFortnight).GreaterThanOrEqualTo(0).When(x => x.Request.DependentBachelorWorkHourLimitPerFortnight.HasValue);
        RuleFor(x => x.Request.DependentPostgraduateWorkHourLimitPerFortnight).GreaterThanOrEqualTo(0).When(x => x.Request.DependentPostgraduateWorkHourLimitPerFortnight.HasValue);
        RuleFor(x => x.Request.RequiredDocuments).NotNull();
    }
}

public sealed class CreateVisaRuleHandler(
    ApplicationDbContext dbContext,
    IValidator<CreateVisaRuleCommand> validator)
    : IHandler<CreateVisaRuleCommand, Result<VisaRuleResponse>>
{
    public async Task<Result<VisaRuleResponse>> HandleAsync(
        CreateVisaRuleCommand command,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);
        var subclass = command.Request.VisaSubclass.Trim();
        if (await dbContext.VisaRules.AnyAsync(x => x.VisaSubclass == subclass, cancellationToken))
            return Result<VisaRuleResponse>.Failure([ContentErrors.VisaRuleConflict]);
        var rule = new VisaRule { VisaSubclass = subclass };
        rule.Update(command.Request.WorkHourLimitPerFortnight,
            command.Request.DependentBachelorWorkHourLimitPerFortnight,
            command.Request.DependentPostgraduateWorkHourLimitPerFortnight,
            command.Request.TfnEligible, command.Request.NdisEligible,
            command.Request.BlueCardRequiredForChildRelatedWork,
            JsonSerializer.Serialize(command.Request.RequiredDocuments));
        dbContext.VisaRules.Add(rule);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result<VisaRuleResponse>.Success(ContentMapper.ToResponse(rule));
    }
}
