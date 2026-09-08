using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using SettleMate.Database;
using Testcontainers.MsSql;

namespace SettleMate.Tests.Infrastructure;

public sealed class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
   
    private readonly MsSqlContainer database = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-CU14-ubuntu-22.04")
    .WithPassword("Strong_password_123!")
    .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:connection", database.GetConnectionString());
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IConfigureOptions<AuthenticationOptions>>();
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
            }).AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>("Test", _ => { });
        });
    }

    public async Task InitializeAsync()
    {
        await database.StartAsync();
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await database.DisposeAsync();
        await base.DisposeAsync();
    }
}
