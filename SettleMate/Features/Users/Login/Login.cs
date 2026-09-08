using Carter;
using Microsoft.AspNetCore.Mvc;
using SettleMate.Abstractions;
using SettleMate.Abstractions.Errors;
using SettleMate.Constants;
using SettleMate.Extensions;
using SettleMate.Features.Users.Shared;

namespace SettleMate.Features.Users.Login
{
    public class Login : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("login", async (
            [FromServices] IHandler<LoginUserRequest, Result<LoginResponse>> handler,
            [FromBody] LoginUserRequest request,
            CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(request, cancellationToken);
                return Results.Ok(result);
            })
        .WithTags(ApiTags.Users)
        .Produces<LoginResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status422UnprocessableEntity)
        .Produces(StatusCodes.Status500InternalServerError)
        .Produces(StatusCodes.Status401Unauthorized);
        }
    }
}
