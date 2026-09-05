using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace SettleMate.Authorization;

public sealed class PermissionPolicyProvider(
    IOptions<AuthorizationOptions> options)
    : DefaultAuthorizationPolicyProvider(options)
{
    public override Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (!policyName.Contains(':', StringComparison.Ordinal))
        {
            return base.GetPolicyAsync(policyName);
        }

        var policy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .RequirePermission(policyName)
            .Build();

        return Task.FromResult<AuthorizationPolicy?>(policy);
    }
}
