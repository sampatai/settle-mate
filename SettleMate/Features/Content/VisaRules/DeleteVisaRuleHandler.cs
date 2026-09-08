using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SettleMate.Abstractions;
using SettleMate.Abstractions.Errors;
using SettleMate.Database;
using SettleMate.Features.Content.Shared;

namespace SettleMate.Features.Content.VisaRules;

public sealed record DeleteVisaRuleCommand(string VisaSubclass);

public sealed class DeleteVisaRuleCommandValidator : AbstractValidator<DeleteVisaRuleCommand>
{
    public DeleteVisaRuleCommandValidator() => RuleFor(x => x.VisaSubclass).NotEmpty().MaximumLength(20);
}

public sealed class DeleteVisaRuleHandler(
    ApplicationDbContext dbContext,
    IValidator<DeleteVisaRuleCommand> validator)
    : IHandler<DeleteVisaRuleCommand, Result<bool>>
{
    public async Task<Result<bool>> HandleAsync(
        DeleteVisaRuleCommand command,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);
        var rule = await dbContext.VisaRules.SingleOrDefaultAsync(x => x.VisaSubclass == command.VisaSubclass.Trim(), cancellationToken);
        if (rule is null)
            return Result<bool>.Failure([ContentErrors.VisaRuleNotFound]);
        dbContext.VisaRules.Remove(rule);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}
