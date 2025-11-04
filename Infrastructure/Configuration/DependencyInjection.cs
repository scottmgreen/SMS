using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using SMS_Infrastructure.Interfaces;

namespace SMS_Infrastructure.Configuration;

public static class DependencyInjection
{
       
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {

        services.AddHttpContextAccessor();
        services.AddScoped<IConnectionService, ConnectionService>();
        services.AddScoped<IUserDetailsFactory, UserDetailsFactory>();
        services.AddScoped<ILogSupport, LogSupport>();
        services.AddScoped<LogSupport>();
        services.AddSingleton<FileService>();
        services.AddDataServices(configuration);

        return services;
    }
    
}
