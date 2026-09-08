using Carter;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SettleMate.Abstractions;
using SettleMate.Abstractions.Errors;
using SettleMate.Features.Onboarding.Shared;
using SettleMate.Features.Onboarding.Commands;
using SettleMate.Features.Onboarding.Queries;
using SettleMate.Extensions;
using SettleMate.Constants;

namespace SettleMate.Features.Onboarding;

public sealed class OnboardingEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/onboarding/preview", Preview)
            .WithTags(ApiTags.Onboarding)
            .AllowAnonymous()
            .Produces<OnboardingResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem();

        app.MapPost("/onboarding/start", SaveProfile)
            .WithTags(ApiTags.Onboarding)
            .WithName("StartOnboarding")
            .RequireAuthorization()
            .Produces<OnboardingResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem();

        app.MapPut("/onboarding/profile", SaveProfile)
            .WithTags(ApiTags.Onboarding)
            .WithName("UpdateProfile")
            .RequireAuthorization()
            .Produces<OnboardingResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem();

        app.MapGet("/onboarding/roadmap/{userId}", GetRoadmap)
            .WithTags(ApiTags.Roadmap)
            .WithName("GetRoadmap")
            .RequireAuthorization()
            .Produces<OnboardingResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        app.MapPost("/onboarding/roadmap/{userId}/regenerate", RegenerateRoadmap)
            .WithTags(ApiTags.Roadmap)
            .WithName("RegenerateRoadmap")
            .RequireAuthorization()
            .Produces<OnboardingResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden)
            .ProducesValidationProblem();

        app.MapPatch("/onboarding/roadmap/items/{itemId:guid}", SetItemCompleted)
            .WithTags(ApiTags.Onboarding)
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        app.MapPatch("/checklist/tasks/{taskId:guid}", SetChecklistTaskCompleted)
            .WithTags(ApiTags.Checklist)
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
    }

    private async Task<IResult> Preview(
        OnboardingProfileRequest request,
        IHandler<PreviewOnboardingQuery, Result<OnboardingResponse>> handler,
        CancellationToken cancellationToken)
    {
        return (await handler.HandleAsync(
            new PreviewOnboardingQuery(request),
            cancellationToken)).ToHttpResult();
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
            : (await handler.HandleAsync(
                new SaveOnboardingProfileCommand(userId, request),
                cancellationToken)).ToHttpResult();
    }

    private async Task<IResult> GetRoadmap(
        string userId,
        ICurrentUser currentUser,
        IHandler<GetOnboardingProfileQuery, Result<OnboardingResponse>> handler,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null)
            return Results.Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Authentication required.");
        if (!string.Equals(currentUser.UserId, userId, StringComparison.Ordinal))
            return Results.Problem(statusCode: StatusCodes.Status403Forbidden, title: "You cannot access another user's roadmap.");

        return (await handler.HandleAsync(
            new GetOnboardingProfileQuery(currentUser.UserId),
            cancellationToken)).ToHttpResult();
    }

    private async Task<IResult> RegenerateRoadmap(
        string userId,
        OnboardingProfileRequest request,
        ICurrentUser currentUser,
        IHandler<SaveOnboardingProfileCommand, Result<OnboardingResponse>> handler,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null)
            return Results.Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Authentication required.");
        if (!string.Equals(currentUser.UserId, userId, StringComparison.Ordinal))
            return Results.Problem(statusCode: StatusCodes.Status403Forbidden, title: "You cannot regenerate another user's roadmap.");

        return (await handler.HandleAsync(
            new SaveOnboardingProfileCommand(currentUser.UserId, request),
            cancellationToken)).ToHttpResult();
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
        return result.IsSuccess ? Results.NoContent() : result.ToHttpResult();
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
        return result.IsSuccess ? Results.NoContent() : result.ToHttpResult();
    }

}
