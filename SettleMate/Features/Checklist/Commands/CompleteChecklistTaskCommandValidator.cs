using FluentValidation;

namespace SettleMate.Features.Checklist.Commands;

public sealed class CompleteChecklistTaskCommandValidator : AbstractValidator<CompleteChecklistTaskCommand>
{
    public CompleteChecklistTaskCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.TaskId).NotEmpty();
    }
}