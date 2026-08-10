using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using SettleMate.Configuration;
using SettleMate.Database;
using SettleMate.Database.Entities;
using SettleMate.Exceptions;
using SettleMate.Extensions;
using SettleMate.Features.Book.CreateBook;
using System.Data;
using System.Reflection;
using System.Text;

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

builder.Services.AddOptions<AuthConfiguration>()
    .Bind(builder.Configuration.GetSection(nameof(AuthConfiguration)));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["AuthConfiguration:Issuer"],
            ValidAudience = builder.Configuration["AuthConfiguration:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["AuthConfiguration:Key"]!))
        };
    });

builder.Services.AddAuthorization();

builder.Services.RegisterApiEndpointsFromAssembly(Assembly.GetExecutingAssembly());
builder.Services.AddHealthChecksConfiguration();
builder.Services.AddValidatorsFromAssembly(typeof(CreateBookValidator).Assembly);
builder.Services.AddHandlersFromAssembly(typeof(Program).Assembly);
builder.Services.AddExceptionHandler<CustomExceptionHandler>().AddProblemDetails();

var app = builder.Build();

app.MapApiEndpoints();

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
app.UseHttpsRedirection();
app.UseExceptionHandler();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();

    //await DatabaseSeedService.SeedAsync(dbContext, userManager, roleManager);
}
await app.RunAsync();
