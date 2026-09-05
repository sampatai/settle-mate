using Carter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SettleMate.Abstractions;
using SettleMate.Abstractions.Errors;
using SettleMate.Constants;
using SettleMate.Features.Users.Shared;

namespace SettleMate.Features.Users.UpdateUserRole;

public sealed class UpdateUserRoleEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/users/{userId}/role", async (
                [FromRoute] string userId,
                [FromBody] UpdateUserRoleRequest request,
                IHandler<UpdateUserRoleCommand, Result<UserResponse>> handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(
                    new UpdateUserRoleCommand(userId, request),
                    cancellationToken);
                return result.IsSuccess
                    ? Results.Ok(result.Data)
                    : Results.Problem(
                        statusCode: result.Errors![0].Type == ErrorType.NotFound
                            ? StatusCodes.Status404NotFound
                            : StatusCodes.Status400BadRequest,
                        title: result.Errors[0].Code,
                        detail: result.Errors[0].Description);
            })
            .WithTags(ApiTags.Users)
            .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" })
            .Produces<UserResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);
    }
}
