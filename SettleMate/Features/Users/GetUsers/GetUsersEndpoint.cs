using Carter;
using Microsoft.AspNetCore.Mvc;
using SettleMate.Authorization;
using SettleMate.Abstractions;
using SettleMate.Abstractions.Errors;
using SettleMate.Constants;
using SettleMate.Features.Users.Shared;

namespace SettleMate.Features.Users.GetUsers;

public sealed class GetUsersEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/users", async (
                [FromServices] IHandler<Unit, Result<IReadOnlyList<UserResponse>>> handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(Unit.Value, cancellationToken);
                return Results.Ok(result.Data);
            })
            .WithTags(ApiTags.Users)
            .RequireAuthorization(Permissions.UsersRead)
            .Produces<IReadOnlyList<UserResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);
    }
}
