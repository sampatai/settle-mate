using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SettleMate.Abstractions;
using SettleMate.Abstractions.Errors;
using SettleMate.Database;
using SettleMate.Features.Listings.Shared;

namespace SettleMate.Features.Listings;

public sealed record GetListingScoreQuery(Guid ListingId, Guid? DestinationId);

public sealed class GetListingScoreQueryValidator : AbstractValidator<GetListingScoreQuery>
{
    public GetListingScoreQueryValidator() => RuleFor(x => x.ListingId).NotEmpty();
}

public sealed class GetListingScoreHandler(
    ApplicationDbContext dbContext,
    IValidator<GetListingScoreQuery> validator)
    : IHandler<GetListingScoreQuery, Result<ListingScoreResponse>>
{
    public async Task<Result<ListingScoreResponse>> HandleAsync(
        GetListingScoreQuery query,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(query, cancellationToken);
        var score = await dbContext.ListingScores.AsNoTracking()
            .Where(x => x.ListingId == query.ListingId && (query.DestinationId == null || x.DestinationId == query.DestinationId))
            .OrderByDescending(x => x.CalculatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
        return score is null
            ? Result<ListingScoreResponse>.Failure([ListingErrors.ScoreNotFound])
            : Result<ListingScoreResponse>.Success(new ListingScoreResponse(
                score.TravelTimeMinutes, score.NearbySupermarketCount, score.NearbyTransportCount,
                score.SafetyScore, score.StudentScore, score.Provider, score.CalculatedAtUtc));
    }
}
