using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SettleMate.Abstractions;
using SettleMate.Abstractions.Errors;
using SettleMate.Database;
using SettleMate.Database.Entities.Accommodation;
using SettleMate.Features.Listings.Shared;

namespace SettleMate.Features.Listings;

public sealed class SearchListingsQueryValidator : AbstractValidator<ListingSearchQuery>
{
    public SearchListingsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
        RuleFor(x => x.MinimumRent).GreaterThanOrEqualTo(0).When(x => x.MinimumRent.HasValue);
        RuleFor(x => x.MaximumRent).GreaterThanOrEqualTo(0).When(x => x.MaximumRent.HasValue);
        RuleFor(x => x).Must(x => !x.MinimumRent.HasValue || !x.MaximumRent.HasValue || x.MinimumRent <= x.MaximumRent)
            .WithMessage("Minimum rent cannot exceed maximum rent.");
        RuleFor(x => x.MaximumTravelMinutes).GreaterThanOrEqualTo(0).When(x => x.MaximumTravelMinutes.HasValue);
        RuleFor(x => x.MinimumRoomCount).GreaterThanOrEqualTo(0).When(x => x.MinimumRoomCount.HasValue);
        RuleFor(x => x.RoomType).MaximumLength(50);
        RuleFor(x => x).Must(x => x.Sort != ListingSort.TravelTime || x.DestinationId.HasValue)
            .WithMessage("A destination is required for travel-time sorting.");
        RuleFor(x => x).Must(x => !x.MaximumTravelMinutes.HasValue || x.DestinationId.HasValue)
            .WithMessage("A destination is required for travel-time filtering.");
    }
}

public sealed class SearchListingsHandler(
    ApplicationDbContext dbContext,
    IValidator<ListingSearchQuery> validator)
    : IHandler<ListingSearchQuery, Result<ListingSearchResponse>>
{
    public async Task<Result<ListingSearchResponse>> HandleAsync(
        ListingSearchQuery query,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(query, cancellationToken);
        var listings = await dbContext.Listings.AsNoTracking()
            .Include(x => x.Amenities)
            .Include(x => x.WeeklyCosts)
            .Where(x => x.IsActive && x.Status == ListingStatus.Approved)
            .ToListAsync(cancellationToken);
        var scores = await dbContext.ListingScores.AsNoTracking()
            .Where(x => query.DestinationId == null || x.DestinationId == query.DestinationId)
            .ToListAsync(cancellationToken);
        var rows = listings.Select(listing =>
        {
            var score = scores.FirstOrDefault(x => x.ListingId == listing.Id);
            return (Listing: listing, Score: score);
        });
        if (query.MinimumRent.HasValue)
            rows = rows.Where(x => x.Listing.WeeklyRent >= query.MinimumRent.Value);
        if (query.MaximumRent.HasValue)
            rows = rows.Where(x => x.Listing.WeeklyRent <= query.MaximumRent.Value);
        if (query.MinimumRoomCount.HasValue)
            rows = rows.Where(x => x.Listing.RoomCount >= query.MinimumRoomCount.Value);
        if (!string.IsNullOrWhiteSpace(query.RoomType))
            rows = rows.Where(x => x.Listing.RoomType.Equals(query.RoomType.Trim(), StringComparison.OrdinalIgnoreCase));
        if (query.AvailableBefore.HasValue)
            rows = rows.Where(x => x.Listing.AvailableFrom <= query.AvailableBefore.Value);
        if (query.MaximumTravelMinutes.HasValue)
            rows = rows.Where(x => x.Score is not null && x.Score.TravelTimeMinutes <= query.MaximumTravelMinutes.Value);

        rows = query.Sort switch
        {
            ListingSort.RentAscending => rows.OrderBy(x => x.Listing.WeeklyRent).ThenBy(x => x.Listing.Id),
            ListingSort.RentDescending => rows.OrderByDescending(x => x.Listing.WeeklyRent).ThenBy(x => x.Listing.Id),
            ListingSort.TravelTime => rows.OrderBy(x => x.Score?.TravelTimeMinutes ?? int.MaxValue).ThenBy(x => x.Listing.Id),
            ListingSort.SafetyScore => rows.OrderByDescending(x => x.Score?.SafetyScore ?? 0).ThenBy(x => x.Listing.Id),
            ListingSort.StudentPopularity => rows.OrderByDescending(x => x.Score?.StudentScore ?? 0).ThenBy(x => x.Listing.Id),
            ListingSort.NewestAvailability => rows.OrderByDescending(x => x.Listing.AvailableFrom).ThenBy(x => x.Listing.Id),
            _ => rows.OrderBy(x => x.Listing.WeeklyRent).ThenBy(x => x.Score?.TravelTimeMinutes ?? int.MaxValue).ThenBy(x => x.Listing.Id)
        };
        var materialized = rows.ToList();
        var totalCount = materialized.Count;
        var items = materialized.Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(x => ListingMapper.ToResponse(x.Listing, x.Score))
            .ToArray();
        return Result<ListingSearchResponse>.Success(new ListingSearchResponse(
            items, query.Page, query.PageSize, totalCount,
            (int)Math.Ceiling(totalCount / (double)query.PageSize)));
    }
}
