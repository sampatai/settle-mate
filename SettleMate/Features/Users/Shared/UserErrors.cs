using Microsoft.AspNetCore.Identity;
using SettleMate.Abstractions.Errors;

namespace SettleMate.Features.Users.Shared;

public static class UserErrors
{
    public static readonly Error NotFound =
        Error.NotFound("user.not_found", "User was not found.");

    public static readonly Error Forbidden =
        Error.Forbidden("user.forbidden", "You are not allowed to manage this user.");

    public static readonly Error RoleNotFound =
        Error.NotFound("role.not_found", "The specified role was not found.");

    public static List<Error> FromIdentityErrors(IEnumerable<IdentityError> errors) =>
        errors.Select(error => Error.Validation($"identity.{error.Code}", error.Description)).ToList();
}
