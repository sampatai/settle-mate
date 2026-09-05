using Microsoft.AspNetCore.Identity;
using SettleMate.Abstractions;
using SettleMate.Abstractions.Errors;
using SettleMate.Database.Entities.Identity;
using SettleMate.Features.Users.Shared;

namespace SettleMate.Features.Users.UpdateUser;

public sealed class UpdateUserHandler(UserManager<User> userManager)
    : IHandler<UpdateUserCommand, Result<UserResponse>>
{
    public async Task<Result<UserResponse>> HandleAsync(
        UpdateUserCommand command,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(command.UserId);
        if (user is null)
        {
            return Result<UserResponse>.Failure([UserErrors.NotFound]);
        }

        user.Email = command.Request.Email;
        user.UserName = command.Request.Email;
        user.FirstName = command.Request.FirstName;
        user.LastName = command.Request.LastName;
        user.PhoneNumber = command.Request.PhoneNumber;
        user.UpdatedAtUtc = DateTime.UtcNow;

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return Result<UserResponse>.Failure(UserErrors.FromIdentityErrors(updateResult.Errors));
        }

        if (!string.IsNullOrWhiteSpace(command.Request.Password))
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var passwordResult = await userManager.ResetPasswordAsync(
                user,
                token,
                command.Request.Password);
            if (!passwordResult.Succeeded)
            {
                return Result<UserResponse>.Failure(UserErrors.FromIdentityErrors(passwordResult.Errors));
            }
        }

        var roles = await userManager.GetRolesAsync(user);
        return Result<UserResponse>.Success(UserResponse.FromUser(user, roles));
    }
}

public sealed record UpdateUserCommand(string UserId, UpdateUserRequest Request);
