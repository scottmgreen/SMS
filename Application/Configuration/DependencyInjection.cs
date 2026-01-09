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
            // 🔥 CLEANER: Use the Assembly class itself instead of a random handler
            var applicationAssembly = Assembly.GetExecutingAssembly(); // Gets current assembly (Application)
            
            // Alternative: Use the marker interface approach
            // var applicationAssembly = typeof(IApplicationAssemblyMarker).Assembly;
            
            services.AddApplicationMediator(applicationAssembly);
            
            // SMS User Application Services - INTERFACE BINDINGS ONLY
            services.AddScoped<ISMSApplicationUserService, SMSApplicationUserService>();
            services.AddScoped<ISMSOrganizationalUserService, SMSOrganizationalUserService>();
            services.AddScoped<ISMSStakeholderUserService, SMSStakeholderUserService>();
            
            // SMS Group Application Services - INTERFACE BINDINGS ONLY
            services.AddScoped<ISMSOrganizationalGroupService, SMSOrganizationalGroupService>();
            services.AddScoped<ISMSApplicationGroupService, SMSApplicationGroupService>();
            
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
            services.AddScoped<SafetyPerformanceIndicatorService>();

            // SMS Audit Management Services (NEW) - TEMPORARILY DISABLED UNTIL INFRASTRUCTURE IS READY
            services.AddScoped<SMSAuditPlanService>();
            services.AddScoped<SMSAuditService>();
            
            // NOTE: SMS Audit Data Services are registered in Infrastructure layer (ServiceCollectionExtensions.cs)
            // These services are already available through Infrastructure registration:

            return services;
        }

        /// <summary>
        /// Adds mediator and automatically registers all command and query handlers from the specified assembly.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <param name="assembly">The assembly to scan for handlers.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection AddApplicationMediator(this IServiceCollection services, Assembly assembly)
        {
            // Register the mediator service
            services.AddScoped<IMediator, SMS_Application.Services.Mediator>();

            // Register all command handlers
            var handlerTypes = assembly.GetTypes()
                .Where(t => t.GetInterfaces()
                    .Any(i => i.IsGenericType && 
                             (i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))))
                .ToList();

            foreach (var handlerType in handlerTypes)
            {
                var interfaceType = handlerType.GetInterfaces()
                    .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));
                
                services.AddScoped(interfaceType, handlerType);
            }

            return services;
        }
    }
}

