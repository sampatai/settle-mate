using Microsoft.AspNetCore.Authorization;

namespace SettleMate.Authorization;

public sealed class PermissionAuthorizationRequirement(params string[] allowedPermissions)
    : AuthorizationHandler<PermissionAuthorizationRequirement>, IAuthorizationRequirement
{
    public IReadOnlyList<string> AllowedPermissions { get; } = allowedPermissions;

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionAuthorizationRequirement requirement)
    {
        if (requirement.AllowedPermissions.Any(permission =>
            context.User.HasClaim(CustomClaimTypes.Permission, permission)))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

public static class PermissionPolicyExtensions
{
    public static AuthorizationPolicyBuilder RequirePermission(
        this AuthorizationPolicyBuilder builder,
        params string[] permissions)
    {
        builder.AddRequirements(new PermissionAuthorizationRequirement(permissions));
        return builder;
    }
}
