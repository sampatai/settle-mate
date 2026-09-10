using Microsoft.AspNetCore.Identity;
using SettleMate.Abstractions;
using SettleMate.Abstractions.Errors;
using SettleMate.Database.Entities.Identity;
using SettleMate.Features.Users.Shared;

namespace SettleMate.Features.Users.GetCurrentUser;

public sealed class GetCurrentUserHandler(
    ICurrentUser currentUser,
    UserManager<User> userManager)
    : IHandler<Unit, Result<UserResponse>>
{
    public async Task<Result<UserResponse>> HandleAsync(
        Unit request,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null)
        {
            return Result<UserResponse>.Failure([UserErrors.NotFound]);
        }

        var user = await userManager.FindByIdAsync(currentUser.UserId);
        if (user is null)
        {
            return Result<UserResponse>.Failure([UserErrors.NotFound]);
        }

        var roles = await userManager.GetRolesAsync(user);
        return Result<UserResponse>.Success(UserResponse.FromUser(user, roles));
    }
}
