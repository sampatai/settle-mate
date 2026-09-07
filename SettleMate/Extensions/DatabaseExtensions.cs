using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SettleMate.Database;
using SettleMate.Database.Entities.Identity;

namespace SettleMate.Extensions
{
    public static class DatabaseExtensions
    {
        public static IServiceCollection AddSQLDatabaseConfiguration(this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("connection");

            services.AddSingleton<AuditableInterceptor>();

            services.AddDbContext<ApplicationDbContext>((provider, options) =>
            {
                var interceptor = provider.GetRequiredService<AuditableInterceptor>();

                options.EnableSensitiveDataLogging()
                    .UseSqlServer(connectionString, sqlServerOptions =>
                    {
                        sqlServerOptions.MigrationsHistoryTable(DatabaseConsts.MigrationTableName, DatabaseConsts.Schema);
                    })
                    .AddInterceptors(interceptor);
            });
            services
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
            return services;
        }
    }
}
