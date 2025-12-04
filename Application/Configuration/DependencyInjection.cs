// -----------------------------------------------------------------------------
// <copyright file="DependencyInjection.cs" company="">
//     Author: Scott Green
//     Date: 2025-07-24
//     Summary: Provides extension methods for registering application services in the DI container.
// </copyright>
// ----------------------------------------------------------------------------->

using Microsoft.Extensions.DependencyInjection;
using SMS_Application.Interfaces;
using SMS_Application.Messaging.QueryHandlers;
using SMS_Application.Messaging.CommandHandlers;
using System.Reflection;

using SMS_Domain.Interfaces;
using SMS_Infrastructure.Interfaces;
using SMS_Infrastructure.Persistence;
using Application.Interfaces;

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
            // 🔥 CLEANER: Use the Assembly class itself instead of a random handler
            var applicationAssembly = Assembly.GetExecutingAssembly(); // Gets current assembly (Application)
            
            // Alternative: Use the marker interface approach
            // var applicationAssembly = typeof(IApplicationAssemblyMarker).Assembly;
            
            services.AddMediator(applicationAssembly);
            
            // SMS User Application Services - INTERFACE BINDINGS ONLY
            services.AddScoped<ISMSApplicationUserService, SMSApplicationUserService>();
            services.AddScoped<ISMSOrganizationalUserService, SMSOrganizationalUserService>();
            services.AddScoped<ISMSStakeholderUserService, SMSStakeholderUserService>();
            
            // SMS Workflow Services - SINGLE REGISTRATION ONLY
            services.AddScoped<ISMSRiskAssessmentWorkflowService, SMSRiskAssessmentWorkflowService>();
            services.AddScoped<ISMSInvestigationWorkflowService, SMSInvestigationWorkflowService>();
            //services.AddScoped<ISMSWorkflowService, SMSWorkflowService>();
            //services.AddScoped<ISMSAuthorizationService, SMSAuthorizationService>();
            //services.AddScoped<ISMSRoleService, SMSRoleService>();
            
            // Application Services - INTERFACE BINDINGS ONLY
            services.AddScoped<IHazardFileService, HazardFileService>();
            services.AddScoped<IReportValidationService, ReportValidationService>();

            // **NEW**: SMS Session Service - Direct Session Management
            services.AddScoped<ISMSSessionService, SMSSessionService>();
            
            // Concrete Application Services (where no interface exists)
            services.AddScoped<SystemService>();
            services.AddScoped<MessengerService>();
            services.AddScoped<HazardService>();
            services.AddScoped<HazardLocationService>();
            services.AddScoped<AirportSharedDatasetService>();
            services.AddScoped<ReportService>();
            services.AddScoped<InterviewService>();
            services.AddScoped<InvestigationService>();
            services.AddScoped<RiskAnalysisService>();
            services.AddScoped<RiskAssessmentService>();
            services.AddScoped<MitigationService>();
            services.AddScoped<MitigationAssignmentService>();
            services.AddScoped<ScoringPanelService>();

            return services;
        }
    }
}

