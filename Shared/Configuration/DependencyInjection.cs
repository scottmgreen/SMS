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
        services.Configure<NotificationSettings>(configuration.GetSection(NotificationSettings.SectionName));




        return services;
    }
}
public class NotificationSettings
{
    public const string SectionName = "NotificationSettings";

    /// <summary>
    /// Allow error notifications to be displayed
    /// </summary>
    public bool AllowErrorNotifications { get; set; } = true;

    /// <summary>
    /// Allow success notifications to be displayed
    /// </summary>
    public bool AllowSuccessNotifications { get; set; } = true;

    /// <summary>
    /// Allow warning notifications to be displayed
    /// </summary>
    public bool AllowWarningNotifications { get; set; } = true;

    /// <summary>
    /// Allow info notifications to be displayed
    /// </summary>
    public bool AllowInfoNotifications { get; set; } = true;
}