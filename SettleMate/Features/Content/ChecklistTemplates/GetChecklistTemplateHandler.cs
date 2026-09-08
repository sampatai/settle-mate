using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SettleMate.Abstractions;
using SettleMate.Abstractions.Errors;
using SettleMate.Database;
using SettleMate.Features.Content.Shared;

namespace SettleMate.Features.Content.ChecklistTemplates;

public sealed record GetChecklistTemplateQuery(Guid Id);

public sealed class GetChecklistTemplateQueryValidator : AbstractValidator<GetChecklistTemplateQuery>
{
    public GetChecklistTemplateQueryValidator() => RuleFor(x => x.Id).NotEmpty();
}

public sealed class GetChecklistTemplateHandler(
    ApplicationDbContext dbContext,
    IValidator<GetChecklistTemplateQuery> validator)
    : IHandler<GetChecklistTemplateQuery, Result<ChecklistTemplateResponse>>
{
    public async Task<Result<ChecklistTemplateResponse>> HandleAsync(
        GetChecklistTemplateQuery query,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(query, cancellationToken);
        var template = await dbContext.ChecklistTemplates.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == query.Id, cancellationToken);
        return template is null
            ? Result<ChecklistTemplateResponse>.Failure([ContentErrors.ChecklistTemplateNotFound])
            : Result<ChecklistTemplateResponse>.Success(ContentMapper.ToResponse(template));
    }
}
