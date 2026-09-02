using Carter;
using Microsoft.AspNetCore.Mvc;
using SettleMate.Features.Users.Shared;

namespace JwtAndRefreshTokens.Features.Users;


public class RefreshTokenEndpoint : ICarterModule
{
	public void AddRoutes(IEndpointRouteBuilder app)
	{
		app.MapPost("/refresh", Handle);
	}

	private static async Task<IResult> Handle(
		[FromBody] RefreshTokenRequest request,
		IClientAuthorizationService authorizationService,
		CancellationToken cancellationToken)
	{
		var result = await authorizationService.RefreshTokenAsync(
			request.Token,
			request.RefreshToken,
			cancellationToken);

		if (!result.IsSuccess)
		{
			return Results.Problem(
				statusCode: 400,
				detail: result.Errors?[0].Message,
				title: result.Errors?[0].Code);
		}

		var response = new RefreshTokenResponse(result.Data!.Token, result.Data.RefreshToken);
		return Results.Ok(response);
	}
}
