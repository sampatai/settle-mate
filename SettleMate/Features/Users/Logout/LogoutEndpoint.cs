using Carter;
using Microsoft.AspNetCore.Mvc;
using SettleMate.Abstractions;
using SettleMate.Abstractions.Errors;
using SettleMate.Authorization;
using SettleMate.Constants;
using SettleMate.Features.Users.Logout;

namespace SettleMate.Features.Users.Logout;

public sealed class LogoutEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/logout", async (
                [FromServices] IHandler<Unit, Result<Unit>> handler,
                CancellationToken cancellationToken) =>
            {
                await handler.HandleAsync(Unit.Value, cancellationToken);
                return Results.Ok();
            })
            .WithName("Logout")
            .WithTags(ApiTags.Users)
            .RequireAuthorization()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
    }
}
