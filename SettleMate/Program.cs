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
app.MapCarter();

app.UseHttpsRedirection();
app.UseExceptionHandler();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();

    foreach (var roleName in new[] { "User", "Admin" })
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            var roleResult = await roleManager.CreateAsync(new Role { Name = roleName });
            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Unable to create required role '{roleName}': " +
                    string.Join(", ", roleResult.Errors.Select(error => error.Description)));
            }
        }
    }

    var userRole = await roleManager.FindByNameAsync("User");
    var adminRole = await roleManager.FindByNameAsync("Admin");
    if (userRole is not null)
    {
        await EnsureRolePermissionsAsync(roleManager, userRole, [Permissions.UsersRead, Permissions.UsersUpdate, Permissions.UsersDelete]);
    }

    if (adminRole is not null)
    {
        await EnsureRolePermissionsAsync(roleManager, adminRole, Permissions.All);
    }

    //await DatabaseSeedService.SeedAsync(dbContext, userManager, roleManager);
}

static async Task EnsureRolePermissionsAsync(
    RoleManager<Role> roleManager,
    Role role,
    IEnumerable<string> permissions)
{
    var existingClaims = await roleManager.GetClaimsAsync(role);
    foreach (var permission in permissions)
    {
        if (!existingClaims.Any(claim =>
            claim.Type == CustomClaimTypes.Permission && claim.Value == permission))
        {
            var result = await roleManager.AddClaimAsync(
                role,
                new System.Security.Claims.Claim(CustomClaimTypes.Permission, permission));
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Unable to grant permission '{permission}' to role '{role.Name}'.");
            }
        }
    }
}
await app.RunAsync();
