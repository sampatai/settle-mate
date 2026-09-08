using FluentValidation;

namespace SettleMate.Features.Checklist.Queries;

public sealed class GetChecklistQueryValidator : AbstractValidator<GetChecklistQuery>
{
    public GetChecklistQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}