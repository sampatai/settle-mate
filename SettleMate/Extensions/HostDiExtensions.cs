using SettleMate.Features.Users.Login;
using SettleMate.HostedServices;

namespace SettleMate.Extensions
{
    public static class HostDiExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddMemoryCache();
            services.AddHostedService<InvalidatedTokensHostedService>();
            services.AddScoped<ITokenHelper, TokenHelper>();
            return services;

        }
    }
}
