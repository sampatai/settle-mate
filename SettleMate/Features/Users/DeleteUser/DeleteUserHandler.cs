using Microsoft.AspNetCore.Identity;
using SettleMate.Abstractions;
using SettleMate.Abstractions.Errors;
using SettleMate.Database.Entities.Identity;
using SettleMate.Features.Users.Shared;

namespace SettleMate.Features.Users.DeleteUser;

public sealed class DeleteUserHandler(UserManager<User> userManager)
    : IHandler<string, Result<bool>>
{
    public async Task<Result<bool>> HandleAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return Result<bool>.Failure([UserErrors.NotFound]);
        }

        var deleteResult = await userManager.DeleteAsync(user);
        return deleteResult.Succeeded
            ? Result<bool>.Success(true)
            : Result<bool>.Failure(UserErrors.FromIdentityErrors(deleteResult.Errors));
    }
}
