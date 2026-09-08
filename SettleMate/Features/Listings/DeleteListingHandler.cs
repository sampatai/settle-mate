using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SettleMate.Abstractions;
using SettleMate.Abstractions.Errors;
using SettleMate.Database;
using SettleMate.Features.Listings.Shared;

namespace SettleMate.Features.Listings;

public sealed record DeleteListingCommand(Guid Id);

public sealed class DeleteListingCommandValidator : AbstractValidator<DeleteListingCommand>
{
    public DeleteListingCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public sealed class DeleteListingHandler(
    ApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IHttpContextAccessor httpContextAccessor,
    IValidator<DeleteListingCommand> validator)
    : IHandler<DeleteListingCommand, Result<bool>>
{
    public async Task<Result<bool>> HandleAsync(
        DeleteListingCommand command,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);
        if (currentUser.UserId is null)
            return Result<bool>.Failure([ListingErrors.UserRequired]);
        var listing = await dbContext.Listings.SingleOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
        if (listing is null)
            return Result<bool>.Failure([ListingErrors.NotFound]);
        if (!httpContextAccessor.HttpContext?.User.IsInRole("Admin") == true &&
            !listing.ProviderId.Equals(currentUser.UserId, StringComparison.Ordinal))
            return Result<bool>.Failure([ListingErrors.OwnershipDenied]);
        listing.SetActive(false);
        listing.SetStatus(Database.Entities.Accommodation.ListingStatus.Rejected);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}
