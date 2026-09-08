using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SettleMate.Abstractions;
using SettleMate.Abstractions.Errors;
using SettleMate.Database;
using SettleMate.Database.Entities.Accommodation;
using SettleMate.Features.Listings.Shared;

namespace SettleMate.Features.Listings;

public sealed record UpsertListingScoreCommand(Guid ListingId, ListingScoreRequest Request);

public sealed class UpsertListingScoreCommandValidator : AbstractValidator<UpsertListingScoreCommand>
{
    public UpsertListingScoreCommandValidator()
    {
        RuleFor(x => x.ListingId).NotEmpty();
        RuleFor(x => x.Request.TravelTimeMinutes).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Request.NearbySupermarketCount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Request.NearbyTransportCount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Request.SafetyScore).InclusiveBetween(0, 100);
        RuleFor(x => x.Request.StudentScore).InclusiveBetween(0, 100);
    }
}

public sealed class UpsertListingScoreHandler(
    ApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IHttpContextAccessor httpContextAccessor,
    IValidator<UpsertListingScoreCommand> validator)
    : IHandler<UpsertListingScoreCommand, Result<ListingScoreResponse>>
{
    public async Task<Result<ListingScoreResponse>> HandleAsync(
        UpsertListingScoreCommand command,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);
        if (currentUser.UserId is null)
            return Result<ListingScoreResponse>.Failure([ListingErrors.UserRequired]);
        var listing = await dbContext.Listings.SingleOrDefaultAsync(x => x.Id == command.ListingId, cancellationToken);
        if (listing is null)
            return Result<ListingScoreResponse>.Failure([ListingErrors.NotFound]);
        if (!IsAdmin() && !listing.ProviderId.Equals(currentUser.UserId, StringComparison.Ordinal))
            return Result<ListingScoreResponse>.Failure([ListingErrors.OwnershipDenied]);
        var score = await dbContext.ListingScores
            .SingleOrDefaultAsync(x => x.ListingId == command.ListingId && x.DestinationId == command.Request.DestinationId, cancellationToken);
        if (score is null)
        {
            score = new ListingScore(Guid.CreateVersion7(), command.ListingId, command.Request.DestinationId,
                command.Request.TravelTimeMinutes, command.Request.NearbySupermarketCount,
                command.Request.NearbyTransportCount, command.Request.SafetyScore,
                command.Request.StudentScore, command.Request.Provider);
            dbContext.ListingScores.Add(score);
        }
        else
        {
            score.Update(command.Request.TravelTimeMinutes, command.Request.NearbySupermarketCount,
                command.Request.NearbyTransportCount, command.Request.SafetyScore,
                command.Request.StudentScore, command.Request.Provider);
        }
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result<ListingScoreResponse>.Success(new ListingScoreResponse(
            score.TravelTimeMinutes, score.NearbySupermarketCount, score.NearbyTransportCount,
            score.SafetyScore, score.StudentScore, score.Provider, score.CalculatedAtUtc));
    }

    private bool IsAdmin() => httpContextAccessor.HttpContext?.User.IsInRole("Admin") == true;
}
