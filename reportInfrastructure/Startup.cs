using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace reportInfrastructure
{
    public static class Startup
    {
        public static IServiceCollection AddDependencies(this IServiceCollection services, IConfiguration configuration)
        {

            return services;
        }
    }
}
