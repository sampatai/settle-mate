using System.Net;
using SettleMate.Tests.Infrastructure;
using Shouldly;

namespace SettleMate.Tests.Integration;

[Trait("Category", "Integration")]
public sealed class ChecklistEndpointTests(IntegrationTestWebAppFactory factory)
    : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task GetChecklist_WithoutAuthentication_ReturnsUnauthorized()
    {
        using var client = CreateClient();

        var response = await client.GetAsync("/checklist/user-1");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetChecklist_ForAnotherUser_ReturnsForbidden()
    {
        using var client = CreateClient("user-1");

        var response = await client.GetAsync("/checklist/user-2");

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }
}
