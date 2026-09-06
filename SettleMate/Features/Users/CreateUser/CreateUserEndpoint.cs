using Carter;
using Microsoft.AspNetCore.Mvc;
using SettleMate.Abstractions;
using SettleMate.Abstractions.Errors;
using SettleMate.Constants;
using SettleMate.Features.Users.Shared;

namespace SettleMate.Features.Users.CreateUser;

public sealed class CreateUserEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/users", async (
                [FromBody] CreateUserRequest request,
                IHandler<CreateUserRequest, Result<UserResponse>> handler,
                CancellationToken cancellationToken) =>
            {
                var result = await handler.HandleAsync(request, cancellationToken);
                return result.IsSuccess
                    ? Results.Created($"/users/{result.Data!.Id}", result.Data)
                    : Results.Problem(
                        statusCode: StatusCodes.Status400BadRequest,
                        title: result.Errors![0].Code,
                        detail: result.Errors[0].Description);
            })
            .WithTags(ApiTags.Users)
            .AllowAnonymous()
            .Produces<UserResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);
    }
}
