// -----------------------------------------------------------------------------
// <copyright file="DependencyInjection.cs" company="">
//     Author: Scott Green
//     Date: 2025-07-24
//     Summary: Provides extension methods for registering application services in the DI container.
// </copyright>
// ----------------------------------------------------------------------------->

using Microsoft.FeatureManagement;

namespace SMS_Application.Configuration
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
            // Core Application Services
            services.AddScoped<IMessenger, MessengerService>();
            services.AddMediator(Assembly.GetExecutingAssembly());

            // System and Training Services  
            services.AddScoped<SystemService>();
            
            // SMS Application Services - Safety Management System
            services.AddScoped<HazardService>();
            services.AddScoped<ReportService>();
            services.AddScoped<InvestigationService>();
            services.AddScoped<InterviewService>();
            services.AddScoped<RiskAnalysisService>();
            services.AddScoped<RiskAssessmentService>();
            services.AddScoped<MitigationService>();
            services.AddScoped<MitigationAssignmentService>();
            services.AddScoped<ReportValidationService>();
            services.AddScoped<ScoringPanelService>();

            return services;
        }
    }
}

