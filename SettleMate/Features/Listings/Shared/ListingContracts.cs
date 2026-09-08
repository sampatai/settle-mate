using SettleMate.Database.Entities.Accommodation;
using SettleMate.Features.Content.Shared;

namespace SettleMate.Features.Listings.Shared;

public enum ListingSort
{
    Relevance,
    RentAscending,
    RentDescending,
    TravelTime,
    SafetyScore,
    StudentPopularity,
    NewestAvailability
}

public sealed record ListingResponse(
    Guid Id,
    string ProviderId,
    string Title,
    string Address,
    string Suburb,
    decimal Latitude,
    decimal Longitude,
    decimal WeeklyRent,
    int RoomCount,
    string RoomType,
    DateOnly AvailableFrom,
    string? SourceUrl,
    bool IsActive,
    ListingStatus Status,
    IReadOnlyList<string> Amenities,
    decimal EstimatedWeeklyCost,
    ListingScoreResponse? Score);

public sealed record ListingScoreResponse(
    int TravelTimeMinutes,
    int NearbySupermarketCount,
    int NearbyTransportCount,
    decimal SafetyScore,
    decimal StudentScore,
    string Provider,
    DateTime CalculatedAtUtc);

public sealed record ListingRequest(
    string Title,
    string Address,
    string Suburb,
    decimal Latitude,
    decimal Longitude,
    decimal WeeklyRent,
    int RoomCount,
    string RoomType,
    DateOnly AvailableFrom,
    string? SourceUrl,
    ListingStatus Status = ListingStatus.Draft,
    bool IsActive = true);

public sealed record ListingScoreRequest(
    Guid? DestinationId,
    int TravelTimeMinutes,
    int NearbySupermarketCount,
    int NearbyTransportCount,
    decimal SafetyScore,
    decimal StudentScore,
    string Provider = "provider");

public sealed record SavedListingResponse(
    Guid SavedListingId,
    DateTime SavedAtUtc,
    ListingResponse Listing);

public sealed record UserDestinationRequest(
    string Label,
    decimal Latitude,
    decimal Longitude);

public sealed record UserDestinationResponse(
    Guid Id,
    string Label,
    decimal Latitude,
    decimal Longitude);

public sealed record ListingSearchQuery(
    int Page = 1,
    int PageSize = 20,
    decimal? MinimumRent = null,
    decimal? MaximumRent = null,
    int? MaximumTravelMinutes = null,
    int? MinimumRoomCount = null,
    string? RoomType = null,
    DateOnly? AvailableBefore = null,
    Guid? DestinationId = null,
    ListingSort Sort = ListingSort.Relevance);

public sealed record ListingSearchResponse(
    IReadOnlyList<ListingResponse> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);

public static class ListingMapper
{
    public static ListingResponse ToResponse(Listing listing, ListingScore? score = null) => new(
        listing.Id,
        listing.ProviderId,
        listing.Title,
        listing.Address,
        listing.Suburb,
        listing.Latitude,
        listing.Longitude,
        listing.WeeklyRent,
        listing.RoomCount,
        listing.RoomType,
        listing.AvailableFrom,
        listing.SourceUrl,
        listing.IsActive,
        listing.Status,
        listing.Amenities.Select(x => x.Name).OrderBy(x => x).ToArray(),
        listing.WeeklyRent + listing.WeeklyCosts.Where(x => !x.Included).Sum(x => x.Amount),
        score is null ? null : new ListingScoreResponse(
            score.TravelTimeMinutes,
            score.NearbySupermarketCount,
            score.NearbyTransportCount,
            score.SafetyScore,
            score.StudentScore,
            score.Provider,
            score.CalculatedAtUtc));
}
