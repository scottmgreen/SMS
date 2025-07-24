

using CBT3_Infrastructure.Interfaces;

namespace CBT3_Infrastructure.Configuration;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataServices(this IServiceCollection services, IConfiguration configuration)
    {
        
        services.AddScoped<FileService>();

        services.AddScoped<SystemRepository>();
        services.AddScoped<SystemDataService>();

        services.AddScoped<DashboardRepository>();
        services.AddScoped<DashboardDataService>();

        services.AddScoped<CourseRepository>();
        services.AddScoped<CourseDataService>();

        services.AddScoped<TrainingRepository>();
        services.AddScoped<TrainingDataService>();


        

        return services;
    }
}
