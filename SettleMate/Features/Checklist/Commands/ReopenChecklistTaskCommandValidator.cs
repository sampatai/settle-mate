using FluentValidation;

namespace SettleMate.Features.Checklist.Commands;

public sealed class ReopenChecklistTaskCommandValidator : AbstractValidator<ReopenChecklistTaskCommand>
{
    public ReopenChecklistTaskCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.TaskId).NotEmpty();
    }
}