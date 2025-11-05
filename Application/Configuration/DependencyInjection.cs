// -----------------------------------------------------------------------------
// <copyright file="DependencyInjection.cs" company="">
//     Author: Scott Green
//     Date: 2025-07-24
//     Summary: Provides extension methods for registering application services in the DI container.
// </copyright>
// ----------------------------------------------------------------------------->

using Microsoft.Extensions.DependencyInjection;
using SMS_Application.Interfaces;
using SMS_Application.Services;

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
            services.AddScoped<IMediator, Mediator>();
            services.AddScoped<Mediator>();
            
            // SMS User Application Services
            services.AddScoped<SMSApplicationUserService>();
            services.AddScoped<SMSOrganizationalUserService>();
            services.AddScoped<SMSStakeholderUserService>();
            
            // Existing Application Services - only add ones that exist
            services.AddScoped<SystemService>();
            services.AddScoped<MessengerService>();
            services.AddScoped<HazardService>();
            services.AddScoped<AirportSharedDatasetService>(); // This was missing!
            services.AddScoped<ReportService>();
            services.AddScoped<InterviewService>();
            services.AddScoped<InvestigationService>();
            services.AddScoped<RiskAnalysisService>();
            services.AddScoped<RiskAssessmentService>();
            services.AddScoped<MitigationService>();
            services.AddScoped<MitigationAssignmentService>();
            services.AddScoped<ReportValidationService>();
            services.AddScoped<ScoringPanelService>();
            

            // Note: Command and Query handlers are already registered in the existing project
            // They will be extended to include SMS User handlers as needed

            return services;
        }
    }
}

