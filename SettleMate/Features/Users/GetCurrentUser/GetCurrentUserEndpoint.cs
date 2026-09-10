using Carter;
using Microsoft.AspNetCore.Mvc;
using SettleMate.Abstractions;
using SettleMate.Abstractions.Errors;
using SettleMate.Authorization;
using SettleMate.Constants;
using SettleMate.Features.Users.Shared;

namespace SettleMate.Features.Users.GetCurrentUser;

public sealed class GetCurrentUserEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/me", async (
                [FromServices] IHandler<Unit, Result<UserResponse>> handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(Unit.Value, cancellationToken);
                return result.IsSuccess
                    ? Results.Ok(result.Data)
                    : Results.Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: result.Errors![0].Code,
                        detail: result.Errors[0].Description);
            })
            .WithName("GetCurrentUser")
            .WithTags(ApiTags.Users)
            .RequireAuthorization(Permissions.UsersRead)
            .Produces<UserResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);
    }
}
