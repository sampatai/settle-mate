using Carter;
using SettleMate.Abstractions;
using SettleMate.Abstractions.Errors;
using SettleMate.Authorization;
using SettleMate.Constants;
using SettleMate.Extensions;
using SettleMate.Features.Content.ChecklistTemplates;
using SettleMate.Features.Content.Shared;
using SettleMate.Features.Content.VisaRules;

namespace SettleMate.Features.Content;

public sealed class ContentEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/content/checklist-templates", ListChecklistTemplates)
            .WithTags(ApiTags.Content).RequireAuthorization(Permissions.ContentRead);
        app.MapGet("/content/checklist-templates/{id:guid}", GetChecklistTemplate)
            .WithTags(ApiTags.Content).RequireAuthorization(Permissions.ContentRead);
        app.MapPost("/content/checklist-templates", CreateChecklistTemplate)
            .WithTags(ApiTags.Content).RequireAuthorization(Permissions.ContentCreate);
        app.MapPut("/content/checklist-templates/{id:guid}", UpdateChecklistTemplate)
            .WithTags(ApiTags.Content).RequireAuthorization(Permissions.ContentUpdate);
        app.MapDelete("/content/checklist-templates/{id:guid}", DeleteChecklistTemplate)
            .WithTags(ApiTags.Content).RequireAuthorization(Permissions.ContentDelete);

        app.MapGet("/content/visa-rules", ListVisaRules)
            .WithTags(ApiTags.Content).RequireAuthorization(Permissions.ContentRead);
        app.MapGet("/content/visa-rules/{visaSubclass}", GetVisaRule)
            .WithTags(ApiTags.Content).RequireAuthorization(Permissions.ContentRead);
        app.MapPost("/content/visa-rules", CreateVisaRule)
            .WithTags(ApiTags.Content).RequireAuthorization(Permissions.ContentCreate);
        app.MapPut("/content/visa-rules/{visaSubclass}", UpdateVisaRule)
            .WithTags(ApiTags.Content).RequireAuthorization(Permissions.ContentUpdate);
        app.MapDelete("/content/visa-rules/{visaSubclass}", DeleteVisaRule)
            .WithTags(ApiTags.Content).RequireAuthorization(Permissions.ContentDelete);
    }

    private static async Task<IResult> ListChecklistTemplates(
        [AsParameters] ListChecklistTemplatesQuery query,
        IHandler<ListChecklistTemplatesQuery, Result<PagedResponse<ChecklistTemplateResponse>>> handler,
        CancellationToken cancellationToken) =>
        (await handler.HandleAsync(query, cancellationToken)).ToHttpResult();

    private static async Task<IResult> GetChecklistTemplate(
        Guid id,
        IHandler<GetChecklistTemplateQuery, Result<ChecklistTemplateResponse>> handler,
        CancellationToken cancellationToken) =>
        (await handler.HandleAsync(new GetChecklistTemplateQuery(id), cancellationToken)).ToHttpResult();

    private static async Task<IResult> CreateChecklistTemplate(
        ChecklistTemplateRequest request,
        IHandler<CreateChecklistTemplateCommand, Result<ChecklistTemplateResponse>> handler,
        CancellationToken cancellationToken) =>
        (await handler.HandleAsync(new CreateChecklistTemplateCommand(request), cancellationToken)).ToHttpResult();

    private static async Task<IResult> UpdateChecklistTemplate(
        Guid id,
        ChecklistTemplateRequest request,
        IHandler<UpdateChecklistTemplateCommand, Result<ChecklistTemplateResponse>> handler,
        CancellationToken cancellationToken) =>
        (await handler.HandleAsync(new UpdateChecklistTemplateCommand(id, request), cancellationToken)).ToHttpResult();

    private static async Task<IResult> DeleteChecklistTemplate(
        Guid id,
        IHandler<DeleteChecklistTemplateCommand, Result<bool>> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new DeleteChecklistTemplateCommand(id), cancellationToken);
        return result.IsSuccess ? Results.NoContent() : result.ToHttpResult();
    }

    private static async Task<IResult> ListVisaRules(
        [AsParameters] ListVisaRulesQuery query,
        IHandler<ListVisaRulesQuery, Result<PagedResponse<VisaRuleResponse>>> handler,
        CancellationToken cancellationToken) =>
        (await handler.HandleAsync(query, cancellationToken)).ToHttpResult();

    private static async Task<IResult> GetVisaRule(
        string visaSubclass,
        IHandler<GetVisaRuleQuery, Result<VisaRuleResponse>> handler,
        CancellationToken cancellationToken) =>
        (await handler.HandleAsync(new GetVisaRuleQuery(visaSubclass), cancellationToken)).ToHttpResult();

    private static async Task<IResult> CreateVisaRule(
        VisaRuleRequest request,
        IHandler<CreateVisaRuleCommand, Result<VisaRuleResponse>> handler,
        CancellationToken cancellationToken) =>
        (await handler.HandleAsync(new CreateVisaRuleCommand(request), cancellationToken)).ToHttpResult();

    private static async Task<IResult> UpdateVisaRule(
        string visaSubclass,
        VisaRuleRequest request,
        IHandler<UpdateVisaRuleCommand, Result<VisaRuleResponse>> handler,
        CancellationToken cancellationToken) =>
        (await handler.HandleAsync(new UpdateVisaRuleCommand(visaSubclass, request), cancellationToken)).ToHttpResult();

    private static async Task<IResult> DeleteVisaRule(
        string visaSubclass,
        IHandler<DeleteVisaRuleCommand, Result<bool>> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new DeleteVisaRuleCommand(visaSubclass), cancellationToken);
        return result.IsSuccess ? Results.NoContent() : result.ToHttpResult();
    }
}
