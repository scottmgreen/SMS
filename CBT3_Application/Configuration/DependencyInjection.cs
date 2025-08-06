// -----------------------------------------------------------------------------
// <copyright file="DependencyInjection.cs" company="">
//     Author: Scott Green
//     Date: 2025-07-24
//     Summary: Provides extension methods for registering application services in the DI container.
// </copyright>
// ----------------------------------------------------------------------------->

using Microsoft.FeatureManagement;

namespace CBT3_Application.Configuration
{
    /// <summary>
    /// Provides extension methods for registering application services in the DI container.
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Adds application services to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IMessenger, MessengerService>();
            services.AddMediator(Assembly.GetExecutingAssembly());
            services.AddScoped<SystemService>();
            services.AddScoped<DashboardService>();
            services.AddScoped<TrainingService>();
            services.AddScoped<RegistrationService>();
            services.AddScoped<CourseMachine>();
            services.AddScoped<LessonQuizService>();
            return services;
        }
    }
}

