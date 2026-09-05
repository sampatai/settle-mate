using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SettleMate.Abstractions;
using SettleMate.Abstractions.Errors;
using SettleMate.Database.Entities.Identity;
using SettleMate.Features.Users.Shared;

namespace SettleMate.Features.Users.GetUsers;

public sealed class GetUsersHandler(UserManager<User> userManager)
    : IHandler<Unit, Result<IReadOnlyList<UserResponse>>>
{
    public async Task<Result<IReadOnlyList<UserResponse>>> HandleAsync(
        Unit request,
        CancellationToken cancellationToken)
    {
        var users = await userManager.Users
            .AsNoTracking()
            .OrderBy(user => user.Email)
            .ToListAsync(cancellationToken);

        var responses = new List<UserResponse>(users.Count);
        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            responses.Add(UserResponse.FromUser(user, roles));
        }

        return Result<IReadOnlyList<UserResponse>>.Success(responses);
    }
}
