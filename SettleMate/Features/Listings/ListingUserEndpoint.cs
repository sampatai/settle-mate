using Carter;
using Microsoft.AspNetCore.Mvc;
using SettleMate.Abstractions;
using SettleMate.Abstractions.Errors;
using SettleMate.Authorization;
using SettleMate.Constants;
using SettleMate.Extensions;
using SettleMate.Features.Content.Shared;
using SettleMate.Features.Listings.Shared;

namespace SettleMate.Features.Listings;

public sealed class ListingUserEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/listings/{id:guid}/score", GetScore)
            .WithTags(ApiTags.Listings).AllowAnonymous();
        app.MapPut("/listings/{id:guid}/score", UpsertScore)
            .WithTags(ApiTags.Listings).RequireAuthorization(Permissions.ListingScoresUpsert);
        app.MapPost("/me/saved-listings/{id:guid}", Save)
            .WithTags(ApiTags.Listings).RequireAuthorization(Permissions.SavedListingsCreate);
        app.MapDelete("/me/saved-listings/{id:guid}", Remove)
            .WithTags(ApiTags.Listings).RequireAuthorization(Permissions.SavedListingsDelete);
        app.MapGet("/me/saved-listings", GetSaved)
            .WithTags(ApiTags.Listings).RequireAuthorization(Permissions.SavedListingsRead);
        app.MapGet("/me/destinations", GetDestinations)
            .WithTags(ApiTags.Listings).RequireAuthorization(Permissions.DestinationsRead);
        app.MapPost("/me/destinations", AddDestination)
            .WithTags(ApiTags.Listings).RequireAuthorization(Permissions.DestinationsCreate);
        app.MapPut("/me/destinations/{id:guid}", UpdateDestination)
            .WithTags(ApiTags.Listings).RequireAuthorization(Permissions.DestinationsUpdate);
        app.MapDelete("/me/destinations/{id:guid}", DeleteDestination)
            .WithTags(ApiTags.Listings).RequireAuthorization(Permissions.DestinationsDelete);
    }

    private static async Task<IResult> GetScore(Guid id, Guid? destinationId,
        [FromServices] IHandler<GetListingScoreQuery, Result<ListingScoreResponse>> handler, CancellationToken cancellationToken) =>
        (await handler.HandleAsync(new GetListingScoreQuery(id, destinationId), cancellationToken)).ToHttpResult();

    private static async Task<IResult> UpsertScore(Guid id, ListingScoreRequest request,
        [FromServices] IHandler<UpsertListingScoreCommand, Result<ListingScoreResponse>> handler, CancellationToken cancellationToken) =>
        (await handler.HandleAsync(new UpsertListingScoreCommand(id, request), cancellationToken)).ToHttpResult();

    private static async Task<IResult> Save(Guid id,
        [FromServices] IHandler<SaveListingCommand, Result<SavedListingResponse>> handler, CancellationToken cancellationToken) =>
        (await handler.HandleAsync(new SaveListingCommand(id), cancellationToken)).ToHttpResult();

    private static async Task<IResult> Remove(Guid id,
        [FromServices] IHandler<RemoveSavedListingCommand, Result<bool>> handler, CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new RemoveSavedListingCommand(id), cancellationToken);
        return result.IsSuccess ? Results.NoContent() : result.ToHttpResult();
    }

    private static async Task<IResult> GetSaved([AsParameters] GetSavedListingsQuery query,
        [FromServices] IHandler<GetSavedListingsQuery, Result<PagedResponse<SavedListingResponse>>> handler, CancellationToken cancellationToken) =>
        (await handler.HandleAsync(query, cancellationToken)).ToHttpResult();

    private static async Task<IResult> GetDestinations(
        [FromServices] IHandler<GetDestinationsQuery, Result<IReadOnlyList<UserDestinationResponse>>> handler, CancellationToken cancellationToken) =>
        (await handler.HandleAsync(new GetDestinationsQuery(), cancellationToken)).ToHttpResult();

    private static async Task<IResult> AddDestination(UserDestinationRequest request,
        [FromServices] IHandler<AddDestinationCommand, Result<UserDestinationResponse>> handler, CancellationToken cancellationToken) =>
        (await handler.HandleAsync(new AddDestinationCommand(request), cancellationToken)).ToHttpResult();

    private static async Task<IResult> UpdateDestination(Guid id, UserDestinationRequest request,
        [FromServices] IHandler<UpdateDestinationCommand, Result<UserDestinationResponse>> handler, CancellationToken cancellationToken) =>
        (await handler.HandleAsync(new UpdateDestinationCommand(id, request), cancellationToken)).ToHttpResult();

    private static async Task<IResult> DeleteDestination(Guid id,
        [FromServices] IHandler<DeleteDestinationCommand, Result<bool>> handler, CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new DeleteDestinationCommand(id), cancellationToken);
        return result.IsSuccess ? Results.NoContent() : result.ToHttpResult();
    }
}
