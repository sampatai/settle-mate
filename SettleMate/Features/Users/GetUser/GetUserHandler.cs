using Microsoft.AspNetCore.Identity;
using SettleMate.Abstractions;
using SettleMate.Abstractions.Errors;
using SettleMate.Database.Entities.Identity;
using SettleMate.Features.Users.Shared;

namespace SettleMate.Features.Users.GetUser;

public sealed class GetUserHandler(UserManager<User> userManager)
    : IHandler<string, Result<UserResponse>>
{
    public async Task<Result<UserResponse>> HandleAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return Result<UserResponse>.Failure([UserErrors.NotFound]);
        }

        var roles = await userManager.GetRolesAsync(user);
        return Result<UserResponse>.Success(UserResponse.FromUser(user, roles));
    }
}
