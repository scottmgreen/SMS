using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;

namespace SMS_Shared.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedServices(this IServiceCollection services, IConfiguration configuration)
    {
        if (configuration["FeatureManagement:LoggingEnabled"] == "True")
        {
            services.AddLogging(opt => opt.AddConfiguration(configuration)
                    .AddSimpleConsole(x => x.SingleLine = true)
#if WINDOWS
                    .AddEventLog()
#endif
                    .AddDebug())

                ;
        }
        services.AddFeatureManagement(configuration.GetSection("FeatureManagement"));
        return services;
    }
}
