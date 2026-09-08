using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SettleMate.Abstractions;
using SettleMate.Abstractions.Errors;
using SettleMate.Database;
using SettleMate.Database.Entities.Accommodation;
using SettleMate.Features.Listings.Shared;

namespace SettleMate.Features.Listings;

public sealed record SaveListingCommand(Guid ListingId);

public sealed class SaveListingCommandValidator : AbstractValidator<SaveListingCommand>
{
    public SaveListingCommandValidator() => RuleFor(x => x.ListingId).NotEmpty();
}

public sealed class SaveListingHandler(
    ApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IValidator<SaveListingCommand> validator)
    : IHandler<SaveListingCommand, Result<SavedListingResponse>>
{
    public async Task<Result<SavedListingResponse>> HandleAsync(
        SaveListingCommand command,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);
        if (currentUser.UserId is null)
            return Result<SavedListingResponse>.Failure([ListingErrors.UserRequired]);
        var listing = await dbContext.Listings.Include(x => x.Amenities).Include(x => x.WeeklyCosts)
            .SingleOrDefaultAsync(x => x.Id == command.ListingId && x.IsActive && x.Status == ListingStatus.Approved, cancellationToken);
        if (listing is null)
            return Result<SavedListingResponse>.Failure([ListingErrors.NotFound]);
        if (await dbContext.SavedListings.AnyAsync(x => x.UserId == currentUser.UserId && x.ListingId == command.ListingId, cancellationToken))
            return Result<SavedListingResponse>.Failure([ListingErrors.SavedListingConflict]);
        var saved = new SavedListing(Guid.CreateVersion7(), currentUser.UserId, command.ListingId);
        dbContext.SavedListings.Add(saved);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result<SavedListingResponse>.Success(new SavedListingResponse(saved.Id, saved.SavedAtUtc, ListingMapper.ToResponse(listing)));
    }
}
