using Carter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SettleMate.Abstractions;
using SettleMate.Abstractions.Errors;
using SettleMate.Features.Onboarding.Shared;
using SettleMate.Features.Onboarding.Commands;
using SettleMate.Features.Onboarding.Queries;

namespace SettleMate.Features.Onboarding;

public sealed class OnboardingEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/onboarding/preview", Preview)
            .WithTags("Onboarding")
            .AllowAnonymous()
            .Produces<OnboardingResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem();

        app.MapPost("/onboarding/profile", SaveProfile)
            .WithTags("Onboarding")
            .RequireAuthorization()
            .Produces<OnboardingResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem();

        app.MapGet("/onboarding/profile", GetProfile)
            .WithTags("Onboarding")
            .RequireAuthorization()
            .Produces<OnboardingResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        app.MapPatch("/onboarding/roadmap/items/{itemId:guid}", SetItemCompleted)
            .WithTags("Onboarding")
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        app.MapPatch("/checklist/tasks/{taskId:guid}", SetChecklistTaskCompleted)
            .WithTags("Checklist")
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
    }

    private async Task<IResult> Preview(
        OnboardingProfileRequest request,
        IHandler<PreviewOnboardingQuery, Result<OnboardingResponse>> handler,
        CancellationToken cancellationToken)
    {
        return ToResult(await handler.HandleAsync(
            new PreviewOnboardingQuery(request),
            cancellationToken));
    }

    private async Task<IResult> SaveProfile(
        OnboardingProfileRequest request,
        ICurrentUser currentUser,
        IHandler<SaveOnboardingProfileCommand, Result<OnboardingResponse>> handler,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId;
        return userId is null
            ? Results.Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Authentication required.")
            : ToResult(await handler.HandleAsync(
                new SaveOnboardingProfileCommand(userId, request),
                cancellationToken));
    }

    private async Task<IResult> GetProfile(
        ICurrentUser currentUser,
        IHandler<GetOnboardingProfileQuery, Result<OnboardingResponse>> handler,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId;
        return userId is null
            ? Results.Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Authentication required.")
            : ToResult(await handler.HandleAsync(
                new GetOnboardingProfileQuery(userId),
                cancellationToken));
    }

    private async Task<IResult> SetItemCompleted(
        Guid itemId,
        CompleteRoadmapItemRequest request,
        ICurrentUser currentUser,
        IHandler<SetRoadmapItemCompletedCommand, Result<bool>> handler,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId;
        if (userId is null)
            return Results.Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Authentication required.");
        var result = await handler.HandleAsync(
            new SetRoadmapItemCompletedCommand(userId, itemId, request.Completed),
            cancellationToken);
        return result.IsSuccess ? Results.NoContent() : ToResult(result);
    }

    private async Task<IResult> SetChecklistTaskCompleted(
        Guid taskId,
        CompleteRoadmapItemRequest request,
        ICurrentUser currentUser,
        IHandler<SetChecklistTaskCompletedCommand, Result<bool>> handler,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId;
        if (userId is null)
            return Results.Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Authentication required.");
        var result = await handler.HandleAsync(
            new SetChecklistTaskCompletedCommand(userId, taskId, request.Completed),
            cancellationToken);
        return result.IsSuccess ? Results.NoContent() : ToResult(result);
    }

    private static IResult ToResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return Results.Ok(result.Data);
        if (result.Errors?.FirstOrDefault() is ValidationError validationError)
        {
            var errors = validationError.Errors
                .GroupBy(error => error.Code.Replace("Onboarding.", string.Empty))
                .ToDictionary(group => group.Key, group => group.Select(error => error.Description ?? string.Empty).ToArray());
            return Results.ValidationProblem(errors);
        }
        var error = result.Errors?.FirstOrDefault() ?? Error.Unexpected("Onboarding.Unknown", "The onboarding request failed.");
        var status = error.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status400BadRequest
        };
        return Results.Problem(statusCode: status, title: error.Code, detail: error.Description);
    }
}
