using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SettleMate.Abstractions;
using SettleMate.Abstractions.Errors;
using SettleMate.Database;
using SettleMate.Features.Content.Shared;

namespace SettleMate.Features.Content.VisaRules;

public sealed record ListVisaRulesQuery(int Page = 1, int PageSize = 20, string? Search = null);

public sealed class ListVisaRulesQueryValidator : AbstractValidator<ListVisaRulesQuery>
{
    public ListVisaRulesQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Search).MaximumLength(20);
    }
}

public sealed class ListVisaRulesHandler(
    ApplicationDbContext dbContext,
    IValidator<ListVisaRulesQuery> validator)
    : IHandler<ListVisaRulesQuery, Result<PagedResponse<VisaRuleResponse>>>
{
    public async Task<Result<PagedResponse<VisaRuleResponse>>> HandleAsync(
        ListVisaRulesQuery query,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(query, cancellationToken);
        var rules = dbContext.VisaRules.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            rules = rules.Where(x => x.VisaSubclass.ToLower().Contains(search));
        }
        var totalCount = await rules.CountAsync(cancellationToken);
        var items = await rules.OrderBy(x => x.VisaSubclass)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);
        var responses = items.Select(ContentMapper.ToResponse).ToArray();
        return Result<PagedResponse<VisaRuleResponse>>.Success(
            new PagedResponse<VisaRuleResponse>(responses, query.Page, query.PageSize, totalCount,
                (int)Math.Ceiling(totalCount / (double)query.PageSize)));
    }
}
