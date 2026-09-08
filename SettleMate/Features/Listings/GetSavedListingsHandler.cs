using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SettleMate.Abstractions;
using SettleMate.Abstractions.Errors;
using SettleMate.Database;
using SettleMate.Features.Content.Shared;
using SettleMate.Features.Listings.Shared;

namespace SettleMate.Features.Listings;

public sealed record GetSavedListingsQuery(int Page = 1, int PageSize = 20);

public sealed class GetSavedListingsQueryValidator : AbstractValidator<GetSavedListingsQuery>
{
    public GetSavedListingsQueryValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
    }
}

public sealed class GetSavedListingsHandler(
    ApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IValidator<GetSavedListingsQuery> validator)
    : IHandler<GetSavedListingsQuery, Result<PagedResponse<SavedListingResponse>>>
{
    public async Task<Result<PagedResponse<SavedListingResponse>>> HandleAsync(
        GetSavedListingsQuery query,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(query, cancellationToken);
        if (currentUser.UserId is null)
            return Result<PagedResponse<SavedListingResponse>>.Failure([ListingErrors.UserRequired]);
        var saved = dbContext.SavedListings.AsNoTracking()
            .Include(x => x.Listing).ThenInclude(x => x.Amenities)
            .Include(x => x.Listing).ThenInclude(x => x.WeeklyCosts)
            .Where(x => x.UserId == currentUser.UserId && x.Listing.IsActive && x.Listing.Status == Database.Entities.Accommodation.ListingStatus.Approved);
        var total = await saved.CountAsync(cancellationToken);
        var items = await saved.OrderByDescending(x => x.SavedAtUtc)
            .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize)
            .Select(x => new SavedListingResponse(x.Id, x.SavedAtUtc, ListingMapper.ToResponse(x.Listing)))
            .ToListAsync(cancellationToken);
        return Result<PagedResponse<SavedListingResponse>>.Success(new PagedResponse<SavedListingResponse>(
            items, query.Page, query.PageSize, total, (int)Math.Ceiling(total / (double)query.PageSize)));
    }
}
