namespace SettleMate.Authorization;

public static class Permissions
{
    public const string UsersRead = "users:read";
    public const string UsersUpdate = "users:update";
    public const string UsersDelete = "users:delete";
    public const string UsersManageRoles = "users:manage_roles";
    public const string ContentRead = "content:read";
    public const string ContentCreate = "content:create";
    public const string ContentUpdate = "content:update";
    public const string ContentDelete = "content:delete";
    public const string ListingsRead = "listings:read";
    public const string ListingsCreate = "listings:create";
    public const string ListingsUpdate = "listings:update";
    public const string ListingsDelete = "listings:delete";
    public const string ListingScoresRead = "listing_scores:read";
    public const string ListingScoresUpsert = "listing_scores:upsert";
    public const string SavedListingsRead = "saved_listings:read";
    public const string SavedListingsCreate = "saved_listings:create";
    public const string SavedListingsDelete = "saved_listings:delete";
    public const string DestinationsRead = "destinations:read";
    public const string DestinationsCreate = "destinations:create";
    public const string DestinationsUpdate = "destinations:update";
    public const string DestinationsDelete = "destinations:delete";

    public static readonly string[] All =
    [
        UsersRead,
        UsersUpdate,
        UsersDelete,
        UsersManageRoles,
        ContentRead,
        ContentCreate,
        ContentUpdate,
        ContentDelete,
        ListingsRead,
        ListingsCreate,
        ListingsUpdate,
        ListingsDelete,
        ListingScoresRead,
        ListingScoresUpsert,
        SavedListingsRead,
        SavedListingsCreate,
        SavedListingsDelete,
        DestinationsRead,
        DestinationsCreate,
        DestinationsUpdate,
        DestinationsDelete
    ];
}

public static class CustomClaimTypes
{
    public const string Permission = "permission";
}
