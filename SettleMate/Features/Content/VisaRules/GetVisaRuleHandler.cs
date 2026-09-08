using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SettleMate.Abstractions;
using SettleMate.Abstractions.Errors;
using SettleMate.Database;
using SettleMate.Features.Content.Shared;

namespace SettleMate.Features.Content.VisaRules;

public sealed record GetVisaRuleQuery(string VisaSubclass);

public sealed class GetVisaRuleQueryValidator : AbstractValidator<GetVisaRuleQuery>
{
    public GetVisaRuleQueryValidator() => RuleFor(x => x.VisaSubclass).NotEmpty().MaximumLength(20);
}

public sealed class GetVisaRuleHandler(
    ApplicationDbContext dbContext,
    IValidator<GetVisaRuleQuery> validator)
    : IHandler<GetVisaRuleQuery, Result<VisaRuleResponse>>
{
    public async Task<Result<VisaRuleResponse>> HandleAsync(
        GetVisaRuleQuery query,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(query, cancellationToken);
        var rule = await dbContext.VisaRules.AsNoTracking()
            .SingleOrDefaultAsync(x => x.VisaSubclass == query.VisaSubclass.Trim(), cancellationToken);
        return rule is null
            ? Result<VisaRuleResponse>.Failure([ContentErrors.VisaRuleNotFound])
            : Result<VisaRuleResponse>.Success(ContentMapper.ToResponse(rule));
    }
}
