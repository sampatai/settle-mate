using Carter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SettleMate.Abstractions;
using SettleMate.Abstractions.Errors;
using SettleMate.Constants;
using SettleMate.Features.Users.Shared;
using System.Security.Claims;

namespace SettleMate.Features.Users.UpdateUser;

public sealed class UpdateUserEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/users/{userId}", async (
                [FromRoute] string userId,
                [FromBody] UpdateUserRequest request,
                ClaimsPrincipal principal,
                IHandler<UpdateUserCommand, Result<UserResponse>> handler,
                CancellationToken cancellationToken) =>
            {
                if (!IsOwnerOrAdmin(principal, userId))
                {
                    return Results.Problem(
                        statusCode: StatusCodes.Status403Forbidden,
                        title: UserErrors.Forbidden.Code,
                        detail: UserErrors.Forbidden.Description);
                }

                var result = await handler.HandleAsync(
                    new UpdateUserCommand(userId, request),
                    cancellationToken);
                return result.IsSuccess
                    ? Results.Ok(result.Data)
                    : Results.Problem(
                        statusCode: StatusCodes.Status400BadRequest,
                        title: result.Errors![0].Code,
                        detail: result.Errors[0].Description);
            })
            .WithTags(ApiTags.Users)
            .RequireAuthorization()
            .Produces<UserResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);
    }

    private static bool IsOwnerOrAdmin(ClaimsPrincipal principal, string userId) =>
        principal.IsInRole("Admin") ||
        principal.FindFirstValue("userid") == userId ||
        principal.FindFirstValue(ClaimTypes.NameIdentifier) == userId;
}
