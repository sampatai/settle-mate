using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SettleMate.Abstractions;
using SettleMate.Abstractions.Errors;
using SettleMate.Database;
using SettleMate.Features.Content.Shared;

namespace SettleMate.Features.Content.ChecklistTemplates;

public sealed record ListChecklistTemplatesQuery(int Page = 1, int PageSize = 20, string? Search = null);

public sealed class ListChecklistTemplatesQueryValidator : AbstractValidator<ListChecklistTemplatesQuery>
{
    public ListChecklistTemplatesQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.Search).MaximumLength(100);
    }
}

public sealed class ListChecklistTemplatesHandler(
    ApplicationDbContext dbContext,
    IValidator<ListChecklistTemplatesQuery> validator)
    : IHandler<ListChecklistTemplatesQuery, Result<PagedResponse<ChecklistTemplateResponse>>>
{
    public async Task<Result<PagedResponse<ChecklistTemplateResponse>>> HandleAsync(
        ListChecklistTemplatesQuery query,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(query, cancellationToken);
        var templates = dbContext.ChecklistTemplates.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            templates = templates.Where(x => x.Key.ToLower().Contains(search) || x.Title.ToLower().Contains(search));
        }
        var totalCount = await templates.CountAsync(cancellationToken);
        var items = await templates.OrderBy(x => x.Key)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(x => new ChecklistTemplateResponse(
                x.Id, x.Key, x.WeekNumber, x.Title, x.Description, x.VisaSubclass, x.State,
                x.CareerGoal, x.Provider, x.ApplicationUrl, x.EligibilityNotes,
                x.EstimatedMinutes, x.IsTimeSensitive, x.RequiredDocuments))
            .ToListAsync(cancellationToken);
        return Result<PagedResponse<ChecklistTemplateResponse>>.Success(
            new PagedResponse<ChecklistTemplateResponse>(items, query.Page, query.PageSize, totalCount,
                (int)Math.Ceiling(totalCount / (double)query.PageSize)));
    }
}
