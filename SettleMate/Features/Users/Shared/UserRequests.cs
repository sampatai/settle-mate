namespace SettleMate.Features.Users.Shared;

public sealed record CreateUserRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber);

public sealed record UpdateUserRequest(
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string? Password);

public sealed record UpdateUserRoleRequest(string Role);
