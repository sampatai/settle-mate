namespace SettleMate.Authorization;

public static class Permissions
{
    public const string UsersRead = "users:read";
    public const string UsersUpdate = "users:update";
    public const string UsersDelete = "users:delete";
    public const string UsersManageRoles = "users:manage_roles";

    public static readonly string[] All =
    [
        UsersRead,
        UsersUpdate,
        UsersDelete,
        UsersManageRoles
    ];
}

public static class CustomClaimTypes
{
    public const string Permission = "permission";
}
