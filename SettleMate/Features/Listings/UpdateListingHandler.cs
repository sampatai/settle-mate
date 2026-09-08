using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SettleMate.Abstractions;
using SettleMate.Abstractions.Errors;
using SettleMate.Database;
using SettleMate.Database.Entities.Accommodation;
using SettleMate.Features.Listings.Shared;

namespace SettleMate.Features.Listings;

public sealed record UpdateListingCommand(Guid Id, ListingRequest Request);

public sealed class UpdateListingCommandValidator : AbstractValidator<UpdateListingCommand>
{
    public UpdateListingCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Request.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.Address).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Request.Suburb).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Request.Latitude).InclusiveBetween(-90, 90);
        RuleFor(x => x.Request.Longitude).InclusiveBetween(-180, 180);
        RuleFor(x => x.Request.WeeklyRent).GreaterThan(0);
        RuleFor(x => x.Request.RoomCount).GreaterThan(0);
        RuleFor(x => x.Request.RoomType).NotEmpty().MaximumLength(50);
    }
}

public sealed class UpdateListingHandler(
    ApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IHttpContextAccessor httpContextAccessor,
    IValidator<UpdateListingCommand> validator)
    : IHandler<UpdateListingCommand, Result<ListingResponse>>
{
    public async Task<Result<ListingResponse>> HandleAsync(
        UpdateListingCommand command,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);
        if (currentUser.UserId is null)
            return Result<ListingResponse>.Failure([ListingErrors.UserRequired]);
        var listing = await dbContext.Listings
            .Include(x => x.Amenities)
            .Include(x => x.WeeklyCosts)
            .Include(x => x.Scores)
            .SingleOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
        if (listing is null)
            return Result<ListingResponse>.Failure([ListingErrors.NotFound]);
        if (!IsAdmin() && !listing.ProviderId.Equals(currentUser.UserId, StringComparison.Ordinal))
            return Result<ListingResponse>.Failure([ListingErrors.OwnershipDenied]);
        listing.Update(command.Request.Title, command.Request.Address, command.Request.Suburb,
            command.Request.Latitude, command.Request.Longitude, command.Request.WeeklyRent,
            command.Request.RoomCount, command.Request.RoomType, command.Request.AvailableFrom,
            command.Request.SourceUrl);
        if (IsAdmin())
        {
            listing.SetStatus(command.Request.Status);
            listing.SetActive(command.Request.IsActive);
        }
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result<ListingResponse>.Success(ListingMapper.ToResponse(listing, listing.Scores.OrderByDescending(x => x.CalculatedAtUtc).FirstOrDefault()));
    }

    private bool IsAdmin() => httpContextAccessor.HttpContext?.User.IsInRole("Admin") == true;
}
