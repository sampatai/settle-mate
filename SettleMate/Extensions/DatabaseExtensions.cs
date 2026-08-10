using Microsoft.EntityFrameworkCore;
using SettleMate.Database;

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

            return services;
        }
    }
}
