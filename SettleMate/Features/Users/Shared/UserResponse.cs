using SettleMate.Database.Entities.Identity;

namespace SettleMate.Features.Users.Shared;

public sealed record LoginResponse(string Token, string RefreshToken);

public sealed record UserResponse(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    IReadOnlyList<string> Roles,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc)
{
    public static UserResponse FromUser(User user, IEnumerable<string> roles) =>
        new(
            user.Id,
            user.Email ?? string.Empty,
            user.FirstName,
            user.LastName,
            user.PhoneNumber,
            roles.ToList(),
            user.CreatedAtUtc,
            user.UpdatedAtUtc);
}
