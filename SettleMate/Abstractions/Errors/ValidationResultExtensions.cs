using FluentValidation.Results;

namespace SettleMate.Abstractions.Errors;

public static class ValidationResultExtensions
{
    public static Result<T> ToFailureResult<T>(
        this ValidationResult validationResult,
        string errorCodePrefix) =>
        Result<T>.Failure(
            [
                new ValidationError(
                    validationResult.Errors
                        .Select(error => Error.Validation(
                            $"{errorCodePrefix}.{error.PropertyName}",
                            error.ErrorMessage))
                        .ToArray())
            ]);
}
