using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using reportCore.Interfaces;
using reportShared.Services;

namespace reportShared
{
    public static class Startup
    {
        public static IServiceCollection AddDependenciesShared(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<ITerminalControllerService, TerminalControllerService>();
            services.AddTransient<IReportDownloadService, ReportDownloadService>();


            return services;
        }
    }
}
