using Bogus;
using JwtAndRefreshTokens.Database;
using JwtAndRefreshTokens.Database.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SettleMate.Authorization;
using SettleMate.Database.Entities.Identity;
using System.Security.Claims;

namespace SettleMate.Database;

public static class DatabaseSeedService
{
	public static async Task SeedAsync(ApplicationDbContext dbContext, UserManager<User> userManager,
		RoleManager<Role> roleManager)
	{
		await dbContext.Database.MigrateAsync();

		if (await dbContext.Users.AnyAsync())
		{
			return;
		}

        foreach (var roleName in new[] { "User", "Admin" })
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var roleResult = await roleManager.CreateAsync(new Role { Name = roleName });
                if (!roleResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Unable to create required role '{roleName}': " +
                        string.Join(", ", roleResult.Errors.Select(error => error.Description)));
                }
            }
        }

        var userRole = await roleManager.FindByNameAsync("User");
        var adminRole = await roleManager.FindByNameAsync("Admin");
        if (userRole is not null)
        {
            await EnsureRolePermissionsAsync(roleManager, userRole, [Permissions.UsersRead, Permissions.UsersUpdate, Permissions.UsersDelete]);
        }

        if (adminRole is not null)
        {
            await EnsureRolePermissionsAsync(roleManager, adminRole, Permissions.All);
        }

    }

    static async Task EnsureRolePermissionsAsync(
    RoleManager<Role> roleManager,
    Role role,
    IEnumerable<string> permissions)
    {
        var existingClaims = await roleManager.GetClaimsAsync(role);
        foreach (var permission in permissions)
        {
            if (!existingClaims.Any(claim =>
                claim.Type == CustomClaimTypes.Permission && claim.Value == permission))
            {
                var result = await roleManager.AddClaimAsync(
                    role,
                    new System.Security.Claims.Claim(CustomClaimTypes.Permission, permission));
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Unable to grant permission '{permission}' to role '{role.Name}'.");
                }
            }
        }
    }
}
