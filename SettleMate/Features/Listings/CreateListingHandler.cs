using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SettleMate.Abstractions;
using SettleMate.Abstractions.Errors;
using SettleMate.Database;
using SettleMate.Database.Entities.Accommodation;
using SettleMate.Features.Listings.Shared;

namespace SettleMate.Features.Listings;

public sealed record CreateListingCommand(ListingRequest Request);

public sealed class CreateListingCommandValidator : AbstractValidator<CreateListingCommand>
{
    public CreateListingCommandValidator()
    {
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

public sealed class CreateListingHandler(
    ApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IHttpContextAccessor httpContextAccessor,
    IValidator<CreateListingCommand> validator)
    : IHandler<CreateListingCommand, Result<ListingResponse>>
{
    public async Task<Result<ListingResponse>> HandleAsync(
        CreateListingCommand command,
        CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);
        if (currentUser.UserId is null)
            return Result<ListingResponse>.Failure([ListingErrors.UserRequired]);
        var listing = new Listing(Guid.CreateVersion7(), currentUser.UserId, command.Request.Title,
            command.Request.Address, command.Request.Suburb, command.Request.Latitude, command.Request.Longitude,
            command.Request.WeeklyRent, command.Request.RoomCount, command.Request.RoomType,
            command.Request.AvailableFrom, command.Request.SourceUrl);
        listing.SetStatus(IsAdmin() ? command.Request.Status : ListingStatus.Pending);
        listing.SetActive(command.Request.IsActive);
        dbContext.Listings.Add(listing);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result<ListingResponse>.Success(ListingMapper.ToResponse(listing));
    }

    private bool IsAdmin() => httpContextAccessor.HttpContext?.User.IsInRole("Admin") == true;
}
