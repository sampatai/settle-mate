using Carter;
using Microsoft.AspNetCore.Identity;
using Scalar.AspNetCore;
using SettleMate.Authorization;
using SettleMate.Database;
using SettleMate.Database.Entities.Identity;
using SettleMate.Exceptions;
using SettleMate.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SettleMate.Middlewares;
using FluentValidation;
using SettleMate.Features.Onboarding.Shared;
using SettleMate.Features.Onboarding.Commands;
using SettleMate.Abstractions;
using SettleMate.Security;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSQLDatabaseConfiguration(builder.Configuration);

builder.Services.AddAuthServices(builder.Configuration);

builder.Services.AddAuthorization();
builder.Services.Replace(
    ServiceDescriptor.Singleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>());


builder.Services.AddHealthChecksConfiguration();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddValidatorsFromAssemblyContaining<OnboardingProfileValidator>();
builder.Services.AddHandlersFromAssembly(typeof(Program).Assembly);
builder.Services.AddExceptionHandler<CustomExceptionHandler>().AddProblemDetails();
builder.Services.AddCarter();
var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseHealthChecks();

app.MapScalarApiReference(options =>
{
    options.WithTheme(ScalarTheme.DeepSpace);
    options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
});
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<CheckRevocatedTokensMiddleware>();

app.MapCarter();

app.UseHttpsRedirection();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
    await DatabaseSeedService.SeedAsync(dbContext, userManager, roleManager);
}


await app.RunAsync();

public partial class Program;
