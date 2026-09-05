using Microsoft.AspNetCore.Identity;
using SettleMate.Abstractions;
using SettleMate.Abstractions.Errors;
using SettleMate.Database.Entities.Identity;
using SettleMate.Features.Users.Shared;

namespace SettleMate.Features.Users.CreateUser;

public sealed class CreateUserHandler(
    UserManager<User> userManager,
    RoleManager<Role> roleManager)
    : IHandler<CreateUserRequest, Result<UserResponse>>
{
    private const string DefaultRole = "User";

    public async Task<Result<UserResponse>> HandleAsync(
        CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        var user = new User
        {
            UserName = request.Email,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            FirstName = request.FirstName,
            LastName = request.LastName,
            CreatedAtUtc = DateTime.UtcNow
        };

        var createResult = await userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            return Result<UserResponse>.Failure(UserErrors.FromIdentityErrors(createResult.Errors));
        }

        if (!await roleManager.RoleExistsAsync(DefaultRole))
        {
            var roleResult = await roleManager.CreateAsync(new Role { Name = DefaultRole });
            if (!roleResult.Succeeded)
            {
                await userManager.DeleteAsync(user);
                return Result<UserResponse>.Failure(UserErrors.FromIdentityErrors(roleResult.Errors));
            }
        }

        var roleAssignmentResult = await userManager.AddToRoleAsync(user, DefaultRole);
        if (!roleAssignmentResult.Succeeded)
        {
            await userManager.DeleteAsync(user);
            return Result<UserResponse>.Failure(UserErrors.FromIdentityErrors(roleAssignmentResult.Errors));
        }

        return Result<UserResponse>.Success(UserResponse.FromUser(user, [DefaultRole]));
    }
}
