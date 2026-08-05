using Microsoft.EntityFrameworkCore;
using SettleMate.Database;

namespace SettleMate.Extensions
{
    public static class DatabaseExtensions
    {
        public static IServiceCollection AddSQLDatabaseConfiguration(this IServiceCollection services,
            IConfiguration configuration)
        {

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString("connection"));
            });

            return services;
        }
    }
}
