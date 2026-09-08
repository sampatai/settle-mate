using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SettleMate.Abstractions;
using SettleMate.Abstractions.Errors;
using SettleMate.Database;
using SettleMate.Database.Entities.Accommodation;
using SettleMate.Features.Listings.Shared;

namespace SettleMate.Features.Listings;

public sealed record GetListingQuery(Guid Id);

public sealed class GetListingQueryValidator : AbstractValidator<GetListingQuery>
{
    public GetListingQueryValidator() => RuleFor(x => x.Id).NotEmpty();
}

public sealed class GetListingHandler(
    ApplicationDbContext dbContext,
    IValidator<GetListingQuery> validator)
    : IHandler<GetListingQuery, Result<ListingResponse>>
{
    public async Task<Result<ListingResponse>> HandleAsync(
        GetListingQuery query,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(query, cancellationToken);
        var listing = await dbContext.Listings.AsNoTracking()
            .Include(x => x.Amenities)
            .Include(x => x.WeeklyCosts)
            .Include(x => x.Scores)
            .SingleOrDefaultAsync(x => x.Id == query.Id && x.IsActive && x.Status == ListingStatus.Approved, cancellationToken);
        return listing is null
            ? Result<ListingResponse>.Failure([ListingErrors.NotFound])
            : Result<ListingResponse>.Success(ListingMapper.ToResponse(listing, listing.Scores.OrderByDescending(x => x.CalculatedAtUtc).FirstOrDefault()));
    }
}
