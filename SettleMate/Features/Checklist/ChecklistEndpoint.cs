using Carter;
using SettleMate.Abstractions;
using SettleMate.Abstractions.Errors;
using SettleMate.Constants;
using SettleMate.Extensions;
using SettleMate.Features.Checklist.Commands;
using SettleMate.Features.Checklist.Queries;
using SettleMate.Features.Checklist.Shared;

namespace SettleMate.Features.Checklist;

public sealed class ChecklistEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/checklist/{userId}", GetChecklist)
            .WithTags(ApiTags.Checklist)
            .WithName("GetUserChecklist")
            .RequireAuthorization()
            .Produces<ChecklistResponse>()
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        app.MapPost("/checklist/{userId}/tasks/{taskId:guid}/complete", CompleteTask)
            .WithTags(ApiTags.Checklist)
            .WithName("CompleteChecklistTask")
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        app.MapPost("/checklist/{userId}/tasks/{taskId:guid}/reopen", ReopenTask)
            .WithTags(ApiTags.Checklist)
            .WithName("ReopenChecklistTask")
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);

        app.MapGet("/checklist/{userId}/progress", GetProgress)
            .WithTags(ApiTags.Checklist)
            .WithName("GetChecklistProgress")
            .RequireAuthorization()
            .Produces<ChecklistProgressResponse>()
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetChecklist(
        string userId,
        ICurrentUser currentUser,
        IHandler<GetChecklistQuery, Result<ChecklistResponse>> handler,
        CancellationToken cancellationToken)
    {
        var accessResult = EnsureUserAccess(userId, currentUser);
        if (accessResult is not null)
            return accessResult;

        return (await handler.HandleAsync(new GetChecklistQuery(userId), cancellationToken)).ToHttpResult();
    }

    private static async Task<IResult> CompleteTask(
        string userId,
        Guid taskId,
        ICurrentUser currentUser,
        IHandler<CompleteChecklistTaskCommand, Result<bool>> handler,
        CancellationToken cancellationToken)
    {
        var accessResult = EnsureUserAccess(userId, currentUser);
        if (accessResult is not null)
            return accessResult;

        var result = await handler.HandleAsync(
            new CompleteChecklistTaskCommand(userId, taskId), cancellationToken);
        return result.IsSuccess ? Results.NoContent() : result.ToHttpResult();
    }

    private static async Task<IResult> ReopenTask(
        string userId,
        Guid taskId,
        ICurrentUser currentUser,
        IHandler<ReopenChecklistTaskCommand, Result<bool>> handler,
        CancellationToken cancellationToken)
    {
        var accessResult = EnsureUserAccess(userId, currentUser);
        if (accessResult is not null)
            return accessResult;

        var result = await handler.HandleAsync(
            new ReopenChecklistTaskCommand(userId, taskId), cancellationToken);
        return result.IsSuccess ? Results.NoContent() : result.ToHttpResult();
    }

    private static async Task<IResult> GetProgress(
        string userId,
        ICurrentUser currentUser,
        IHandler<GetChecklistProgressQuery, Result<ChecklistProgressResponse>> handler,
        CancellationToken cancellationToken)
    {
        var accessResult = EnsureUserAccess(userId, currentUser);
        if (accessResult is not null)
            return accessResult;

        return (await handler.HandleAsync(new GetChecklistProgressQuery(userId), cancellationToken)).ToHttpResult();
    }

    private static IResult? EnsureUserAccess(string userId, ICurrentUser currentUser)
    {
        if (currentUser.UserId is null)
            return Results.Problem(statusCode: StatusCodes.Status401Unauthorized, title: ChecklistErrors.UserIdRequired.Code);
        if (!string.Equals(currentUser.UserId, userId, StringComparison.Ordinal))
            return Results.Problem(statusCode: StatusCodes.Status403Forbidden, title: ChecklistErrors.UserAccessDenied.Code);
        return null;
    }
}