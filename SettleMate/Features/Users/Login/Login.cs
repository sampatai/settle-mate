using Carter;
using Microsoft.AspNetCore.Mvc;
using SettleMate.Abstractions;
using SettleMate.Constants;
using SettleMate.Extensions;
using SettleMate.Features.Book.CreateBook;
using SettleMate.Features.Users.Shared;

namespace SettleMate.Features.Users.Login
{
    public class Login : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("login", async (
            IHandler<LoginUserRequest, Result<UserResponse>> handler,
            [FromBody] LoginUserRequest request,
            CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(request, cancellationToken);
                return result.ToHttpResult(user => Results.Ok(user));
            })
        .WithTags(ApiTags.Users)
        .Produces<UserResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status422UnprocessableEntity)
        .Produces(StatusCodes.Status500InternalServerError)
        .Produces(StatusCodes.Status401Unauthorized);
        }
    }
}
