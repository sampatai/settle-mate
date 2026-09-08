using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SettleMate.Abstractions;
using SettleMate.Abstractions.Errors;
using SettleMate.Database;
using SettleMate.Features.Listings.Shared;

namespace SettleMate.Features.Listings;

public sealed record RemoveSavedListingCommand(Guid ListingId);

public sealed class RemoveSavedListingCommandValidator : AbstractValidator<RemoveSavedListingCommand>
{
    public RemoveSavedListingCommandValidator() => RuleFor(x => x.ListingId).NotEmpty();
}

public sealed class RemoveSavedListingHandler(
    ApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IValidator<RemoveSavedListingCommand> validator)
    : IHandler<RemoveSavedListingCommand, Result<bool>>
{
    public async Task<Result<bool>> HandleAsync(
        RemoveSavedListingCommand command,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);
        if (currentUser.UserId is null)
            return Result<bool>.Failure([ListingErrors.UserRequired]);
        var saved = await dbContext.SavedListings
            .SingleOrDefaultAsync(x => x.UserId == currentUser.UserId && x.ListingId == command.ListingId, cancellationToken);
        if (saved is null)
            return Result<bool>.Failure([ListingErrors.NotFound]);
        dbContext.SavedListings.Remove(saved);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}
