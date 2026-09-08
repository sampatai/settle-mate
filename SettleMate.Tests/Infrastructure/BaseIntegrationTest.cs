using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using SettleMate.Database;

namespace SettleMate.Tests.Infrastructure;

public abstract class BaseIntegrationTest : IClassFixture<IntegrationTestWebAppFactory>
{
    private readonly IntegrationTestWebAppFactory factory;

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        this.factory = factory;
    }

    protected HttpClient CreateClient(
        string? userId = null,
        params string[] permissions)
    {
        var client = factory.CreateClient();
        if (userId is not null)
            client.DefaultRequestHeaders.Add("X-Test-User", userId);
        if (permissions.Length > 0)
            client.DefaultRequestHeaders.Add("X-Test-Permissions", string.Join(',', permissions));
        return client;
    }

    protected IServiceScope CreateScope() => factory.Services.CreateScope();

    protected async Task ResetUsersAsync()
    {
        using var scope = CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.ExecuteSqlRawAsync("DELETE FROM [auth].[AspNetUserTokens]");
        await dbContext.Database.ExecuteSqlRawAsync("DELETE FROM [auth].[AspNetUserRoles]");
        await dbContext.Database.ExecuteSqlRawAsync("DELETE FROM [auth].[AspNetUsers]");
    }
}
