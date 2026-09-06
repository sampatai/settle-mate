using Microsoft.AspNetCore.Identity;
using SettleMate.Abstractions;
using SettleMate.Abstractions.Errors;
using SettleMate.Database.Entities.Identity;
using SettleMate.Features.Users.Shared;

namespace SettleMate.Features.Users.UpdateUserRole;

public sealed class UpdateUserRoleHandler(
    UserManager<User> userManager,
    RoleManager<Role> roleManager)
    : IHandler<UpdateUserRoleCommand, Result<UserResponse>>
{
    public async Task<Result<UserResponse>> HandleAsync(
        UpdateUserRoleCommand command,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(command.UserId);
        if (user is null)
        {
            return Result<UserResponse>.Failure([UserErrors.NotFound]);
        }

        var role = command.Request.Role.Trim();
        if (!await roleManager.RoleExistsAsync(role))
        {
            return Result<UserResponse>.Failure([UserErrors.RoleNotFound]);
        }

        var currentRoles = await userManager.GetRolesAsync(user);
        if (currentRoles.Count == 1 && currentRoles[0].Equals(role, StringComparison.OrdinalIgnoreCase))
        {
            user.UpdatedAtUtc = DateTime.UtcNow;
            await userManager.UpdateAsync(user);
            return Result<UserResponse>.Success(UserResponse.FromUser(user, [role]));
        }

        var addResult = currentRoles.Contains(role, StringComparer.OrdinalIgnoreCase)
            ? IdentityResult.Success
            : await userManager.AddToRoleAsync(user, role);
        if (!addResult.Succeeded)
        {
            return Result<UserResponse>.Failure(UserErrors.FromIdentityErrors(addResult.Errors));
        }

        var rolesToRemove = currentRoles
            .Where(currentRole => !currentRole.Equals(role, StringComparison.OrdinalIgnoreCase))
            .ToList();
        var removeResult = rolesToRemove.Count == 0
            ? IdentityResult.Success
            : await userManager.RemoveFromRolesAsync(user, rolesToRemove);
        if (!removeResult.Succeeded)
        {
            return Result<UserResponse>.Failure(UserErrors.FromIdentityErrors(removeResult.Errors));
        }

        user.UpdatedAtUtc = DateTime.UtcNow;
        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            return Result<UserResponse>.Failure(UserErrors.FromIdentityErrors(updateResult.Errors));
        }

        return Result<UserResponse>.Success(
            UserResponse.FromUser(user, [role]));
    }
}

public sealed record UpdateUserRoleCommand(string UserId, UpdateUserRoleRequest Request);
