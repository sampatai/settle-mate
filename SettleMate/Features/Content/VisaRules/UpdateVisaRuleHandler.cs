using System.Text.Json;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SettleMate.Abstractions;
using SettleMate.Abstractions.Errors;
using SettleMate.Database;
using SettleMate.Features.Content.Shared;

namespace SettleMate.Features.Content.VisaRules;

public sealed record UpdateVisaRuleCommand(string VisaSubclass, VisaRuleRequest Request);

public sealed class UpdateVisaRuleCommandValidator : AbstractValidator<UpdateVisaRuleCommand>
{
    public UpdateVisaRuleCommandValidator()
    {
        RuleFor(x => x.VisaSubclass).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Request.RequiredDocuments).NotNull();
    }
}

public sealed class UpdateVisaRuleHandler(
    ApplicationDbContext dbContext,
    IValidator<UpdateVisaRuleCommand> validator)
    : IHandler<UpdateVisaRuleCommand, Result<VisaRuleResponse>>
{
    public async Task<Result<VisaRuleResponse>> HandleAsync(
        UpdateVisaRuleCommand command,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);
        var rule = await dbContext.VisaRules.SingleOrDefaultAsync(x => x.VisaSubclass == command.VisaSubclass.Trim(), cancellationToken);
        if (rule is null)
            return Result<VisaRuleResponse>.Failure([ContentErrors.VisaRuleNotFound]);
        rule.Update(command.Request.WorkHourLimitPerFortnight,
            command.Request.DependentBachelorWorkHourLimitPerFortnight,
            command.Request.DependentPostgraduateWorkHourLimitPerFortnight,
            command.Request.TfnEligible, command.Request.NdisEligible,
            command.Request.BlueCardRequiredForChildRelatedWork,
            JsonSerializer.Serialize(command.Request.RequiredDocuments));
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result<VisaRuleResponse>.Success(ContentMapper.ToResponse(rule));
    }
}
