using FluentValidation;

namespace SettleMate.Features.Book.DeleteBook;

public class DeleteBookValidator : AbstractValidator<DeleteBookRequest>
{
    public DeleteBookValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("Book Id is required");
    }
}
