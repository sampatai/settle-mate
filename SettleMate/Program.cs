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


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSQLDatabaseConfiguration(builder.Configuration);
builder.Services
    .AddIdentity<User, Role>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();


builder.Services.AddAuthServices(builder.Configuration);

builder.Services.AddAuthorization();
builder.Services.Replace(
    ServiceDescriptor.Singleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>());


builder.Services.AddHealthChecksConfiguration();
//builder.Services.AddValidatorsFromAssembly(typeof(CreateBookValidator).Assembly);
builder.Services.AddHandlersFromAssembly(typeof(Program).Assembly);
builder.Services.AddExceptionHandler<CustomExceptionHandler>().AddProblemDetails();
builder.Services.AddCarter();
var app = builder.Build();



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
app.UseExceptionHandler();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
    await DatabaseSeedService.SeedAsync(dbContext, userManager, roleManager);
}


await app.RunAsync();
