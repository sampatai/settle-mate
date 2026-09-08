using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SettleMate.Abstractions;
using SettleMate.Abstractions.Errors;
using SettleMate.Database;
using SettleMate.Database.Entities.Accommodation;
using SettleMate.Features.Listings.Shared;

namespace SettleMate.Features.Listings;

public sealed record AddDestinationCommand(UserDestinationRequest Request);
public sealed record GetDestinationsQuery;
public sealed record UpdateDestinationCommand(Guid Id, UserDestinationRequest Request);
public sealed record DeleteDestinationCommand(Guid Id);

public sealed class AddDestinationCommandValidator : AbstractValidator<AddDestinationCommand>
{
    public AddDestinationCommandValidator()
    {
        RuleFor(x => x.Request.Label).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.Latitude).InclusiveBetween(-90, 90);
        RuleFor(x => x.Request.Longitude).InclusiveBetween(-180, 180);
    }
}

public sealed class UpdateDestinationCommandValidator : AbstractValidator<UpdateDestinationCommand>
{
    public UpdateDestinationCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Request.Label).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Request.Latitude).InclusiveBetween(-90, 90);
        RuleFor(x => x.Request.Longitude).InclusiveBetween(-180, 180);
    }
}

public sealed class DeleteDestinationCommandValidator : AbstractValidator<DeleteDestinationCommand>
{
    public DeleteDestinationCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public sealed class AddDestinationHandler(
    ApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IValidator<AddDestinationCommand> validator)
    : IHandler<AddDestinationCommand, Result<UserDestinationResponse>>
{
    public async Task<Result<UserDestinationResponse>> HandleAsync(AddDestinationCommand command, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);
        if (currentUser.UserId is null)
            return Result<UserDestinationResponse>.Failure([ListingErrors.UserRequired]);
        var destination = new UserDestination(Guid.CreateVersion7(), currentUser.UserId, command.Request.Label,
            command.Request.Latitude, command.Request.Longitude);
        dbContext.UserDestinations.Add(destination);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result<UserDestinationResponse>.Success(ToResponse(destination));
    }

    private static UserDestinationResponse ToResponse(UserDestination destination) =>
        new(destination.Id, destination.Label, destination.Latitude, destination.Longitude);
}

public sealed class GetDestinationsHandler(ApplicationDbContext dbContext, ICurrentUser currentUser)
    : IHandler<GetDestinationsQuery, Result<IReadOnlyList<UserDestinationResponse>>>
{
    public async Task<Result<IReadOnlyList<UserDestinationResponse>>> HandleAsync(GetDestinationsQuery query, CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null)
            return Result<IReadOnlyList<UserDestinationResponse>>.Failure([ListingErrors.UserRequired]);
        var destinations = await dbContext.UserDestinations.AsNoTracking()
            .Where(x => x.UserId == currentUser.UserId).OrderBy(x => x.Label)
            .Select(x => new UserDestinationResponse(x.Id, x.Label, x.Latitude, x.Longitude))
            .ToListAsync(cancellationToken);
        return Result<IReadOnlyList<UserDestinationResponse>>.Success(destinations);
    }
}

public sealed class UpdateDestinationHandler(
    ApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IValidator<UpdateDestinationCommand> validator)
    : IHandler<UpdateDestinationCommand, Result<UserDestinationResponse>>
{
    public async Task<Result<UserDestinationResponse>> HandleAsync(UpdateDestinationCommand command, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);
        if (currentUser.UserId is null)
            return Result<UserDestinationResponse>.Failure([ListingErrors.UserRequired]);
        var destination = await dbContext.UserDestinations.SingleOrDefaultAsync(
            x => x.Id == command.Id && x.UserId == currentUser.UserId, cancellationToken);
        if (destination is null)
            return Result<UserDestinationResponse>.Failure([ListingErrors.DestinationNotFound]);
        destination.Update(command.Request.Label, command.Request.Latitude, command.Request.Longitude);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result<UserDestinationResponse>.Success(new UserDestinationResponse(
            destination.Id, destination.Label, destination.Latitude, destination.Longitude));
    }
}

public sealed class DeleteDestinationHandler(
    ApplicationDbContext dbContext,
    ICurrentUser currentUser,
    IValidator<DeleteDestinationCommand> validator)
    : IHandler<DeleteDestinationCommand, Result<bool>>
{
    public async Task<Result<bool>> HandleAsync(DeleteDestinationCommand command, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);
        if (currentUser.UserId is null)
            return Result<bool>.Failure([ListingErrors.UserRequired]);
        var destination = await dbContext.UserDestinations.SingleOrDefaultAsync(
            x => x.Id == command.Id && x.UserId == currentUser.UserId, cancellationToken);
        if (destination is null)
            return Result<bool>.Failure([ListingErrors.DestinationNotFound]);
        dbContext.UserDestinations.Remove(destination);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}
