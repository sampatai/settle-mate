using Carter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SettleMate.Abstractions;
using SettleMate.Abstractions.Errors;
using SettleMate.Authorization;
using SettleMate.Constants;
using SettleMate.Features.Users.Shared;
using System.Security.Claims;

namespace SettleMate.Features.Users.DeleteUser;

public sealed class DeleteUserEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/users/{userId}", async (
                [FromRoute] string userId,
                ClaimsPrincipal principal,
                IHandler<string, Result<bool>> handler,
                CancellationToken cancellationToken) =>
            {
                if (!IsOwnerOrAdmin(principal, userId))
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status403Forbidden,
                        title: UserErrors.Forbidden.Code,
                        detail: UserErrors.Forbidden.Description);
                }

                var result = await handler.HandleAsync(userId, cancellationToken);
                return result.IsSuccess
                    ? Results.NoContent()
                    : Results.Problem(
                        statusCode: StatusCodes.Status404NotFound,
                        title: result.Errors![0].Code,
                        detail: result.Errors[0].Description);
            })
            .WithTags(ApiTags.Users)
            .RequireAuthorization(Permissions.UsersDelete)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static bool IsOwnerOrAdmin(ClaimsPrincipal principal, string userId) =>
        principal.IsInRole("Admin") ||
        principal.FindFirstValue("userid") == userId ||
        principal.FindFirstValue(ClaimTypes.NameIdentifier) == userId;
}
