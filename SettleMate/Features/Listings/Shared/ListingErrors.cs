using SettleMate.Abstractions.Errors;

namespace SettleMate.Features.Listings.Shared;

public static class ListingErrors
{
    public static readonly Error UserRequired =
        Error.Unauthorized("Listings.AuthenticationRequired", "Authentication is required.");
    public static readonly Error NotFound =
        Error.NotFound("Listings.NotFound", "The listing was not found.");
    public static readonly Error DestinationRequired =
        Error.Validation("Listings.DestinationRequired", "A destination is required for travel-time filtering or sorting.");
    public static readonly Error OwnershipDenied =
        Error.Forbidden("Listings.OwnershipDenied", "You cannot modify another provider's listing.");
    public static readonly Error SavedListingConflict =
        Error.Conflict("Listings.SavedListingConflict", "The listing is already saved.");
    public static readonly Error DestinationNotFound =
        Error.NotFound("Listings.DestinationNotFound", "The destination was not found.");
    public static readonly Error ScoreNotFound =
        Error.NotFound("Listings.ScoreNotFound", "The listing score was not found.");
}
