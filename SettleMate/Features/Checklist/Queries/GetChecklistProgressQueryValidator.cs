using FluentValidation;

namespace SettleMate.Features.Checklist.Queries;

public sealed class GetChecklistProgressQueryValidator : AbstractValidator<GetChecklistProgressQuery>
{
    public GetChecklistProgressQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}