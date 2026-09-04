using System.Linq;

namespace SettleMate.Abstractions.Errors;

public sealed record ValidationError : Error
{
    public ValidationError(Error[] errors)
        : base(
            "Validation.General",
            "One or more validation errors occurred",
            ErrorType.Validation)
    {
        Errors = errors;
    }

    public Error[] Errors { get; }

    public static ValidationError FromResults<T>(IEnumerable<Result<T>> results) =>
        new(
            results
                .Where(r => !r.IsSuccess)
                .SelectMany(r => r.Errors ?? new List<Error>())
                .ToArray()
        );
}