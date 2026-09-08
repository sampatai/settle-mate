using Carter;
using SettleMate.Abstractions;
using SettleMate.Abstractions.Errors;
using SettleMate.Authorization;
using SettleMate.Constants;
using SettleMate.Extensions;
using SettleMate.Features.Listings.Shared;

namespace SettleMate.Features.Listings;

public sealed class ListingEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/listings", Search)
            .WithTags(ApiTags.Listings).AllowAnonymous();
        app.MapGet("/listings/{id:guid}", Get)
            .WithTags(ApiTags.Listings).AllowAnonymous();
        app.MapPost("/listings", Create)
            .WithTags(ApiTags.Listings).RequireAuthorization(Permissions.ListingsCreate);
        app.MapPut("/listings/{id:guid}", Update)
            .WithTags(ApiTags.Listings).RequireAuthorization(Permissions.ListingsUpdate);
        app.MapDelete("/listings/{id:guid}", Delete)
            .WithTags(ApiTags.Listings).RequireAuthorization(Permissions.ListingsDelete);
    }

    private static async Task<IResult> Search(
        [AsParameters] ListingSearchQuery query,
        IHandler<ListingSearchQuery, Result<ListingSearchResponse>> handler,
        CancellationToken cancellationToken) =>
        (await handler.HandleAsync(query, cancellationToken)).ToHttpResult();

    private static async Task<IResult> Get(
        Guid id,
        IHandler<GetListingQuery, Result<ListingResponse>> handler,
        CancellationToken cancellationToken) =>
        (await handler.HandleAsync(new GetListingQuery(id), cancellationToken)).ToHttpResult();

    private static async Task<IResult> Create(
        ListingRequest request,
        IHandler<CreateListingCommand, Result<ListingResponse>> handler,
        CancellationToken cancellationToken) =>
        (await handler.HandleAsync(new CreateListingCommand(request), cancellationToken)).ToHttpResult();

    private static async Task<IResult> Update(
        Guid id,
        ListingRequest request,
        IHandler<UpdateListingCommand, Result<ListingResponse>> handler,
        CancellationToken cancellationToken) =>
        (await handler.HandleAsync(new UpdateListingCommand(id, request), cancellationToken)).ToHttpResult();

    private static async Task<IResult> Delete(
        Guid id,
        IHandler<DeleteListingCommand, Result<bool>> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new DeleteListingCommand(id), cancellationToken);
        return result.IsSuccess ? Results.NoContent() : result.ToHttpResult();
    }
}
