//-----------------------------------------------------------------------
// <copyright file="DependencyInjection.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Configuration component for Application layer dependency injection and setup.
//                  Provides dependency injection configuration and service registration
//                  for the Application layer in the Clean Architecture.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Application.Interfaces;
using SMS_Application.Messaging.Pipelines;

namespace SMS_Application.Configuration
{
    /// <summary>
    /// Application Layer Dependency Injection Configuration
    /// Provides extension methods for registering application services in the DI container.
    /// Handles automatic handler discovery, mediator configuration, and service lifetime management.
    /// 
    /// Methods:
    /// - AddApplicationServices: Registers all application layer services with appropriate lifetimes
    /// - AddApplicationMediator: Configures CQRS mediator and automatically discovers handlers
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

            services.AddApplicationMediator(applicationAssembly);

            #region SMS User Management Services

            // SMS User Application Services - INTERFACE BINDINGS (Only for existing interfaces)
            services.AddScoped<ISMSApplicationUserService, SMSApplicationUserService>();
            services.AddScoped<ISMSOrganizationalUserService, SMSOrganizationalUserService>();
            services.AddScoped<ISMSStakeholderUserService, SMSStakeholderUserService>();

            // SMS User Application Services - CONCRETE REGISTRATIONS 
            services.AddScoped<SMSApplicationUserService>();
            services.AddScoped<SMSOrganizationalUserService>();
            services.AddScoped<SMSStakeholderUserService>();

            // SMS Group Application Services - INTERFACE BINDINGS (Only for existing interfaces)
            services.AddScoped<ISMSOrganizationalGroupService, SMSOrganizationalGroupService>();
            services.AddScoped<ISMSApplicationGroupService, SMSApplicationGroupService>();

            // SMS Group Application Services - CONCRETE REGISTRATIONS (For Query Handlers)
            services.AddScoped<SMSApplicationGroupService>();
            services.AddScoped<SMSOrganizationalGroupService>();
            services.AddScoped<SMSStakeholderGroupService>();

            // SMS Role Management
            services.AddScoped<SMSUserRoleService>();

            #endregion

            #region Core Safety Management Services

            // Application Services - Clean Architecture Pattern
            services.AddScoped<HazardService>();
            services.AddScoped<ReportService>(); 
            services.AddScoped<ReportValidationService>();
            services.AddScoped<InvestigationService>();
            services.AddScoped<InterviewService>();
            services.AddScoped<MitigationService>();
            services.AddScoped<MitigationAssignmentService>();
            services.AddScoped<RiskAnalysisService>();
            services.AddScoped<RiskAssessmentService>();
            services.AddScoped<HazardReportTrackingService>();
            services.AddScoped<ScoringPanelService>();
            services.AddScoped<SafetyPerformanceIndicatorService>();
            services.AddScoped<HazardFileService>();

            // Application Service Interfaces - Clean Architecture Pattern (Only existing interfaces)
            services.AddScoped<IHazardService, HazardService>();
            services.AddScoped<IHazardFileService, HazardFileService>();
            services.AddScoped<IRiskAssessmentService, RiskAssessmentService>();
            services.AddScoped<IRiskAnalysisService, RiskAnalysisService>();
            services.AddScoped<IInterviewService, InterviewService>();
            services.AddScoped<IMitigationService, MitigationService>();
            services.AddScoped<IScoringPanelService, ScoringPanelService>();
            services.AddScoped<ISafetyPerformanceIndicatorService, SafetyPerformanceIndicatorService>();

            #endregion

            #region Supporting Services

            // Supporting Application Services
            services.AddScoped<HazardLocationService>();
            services.AddScoped<AirportSharedDatasetService>();
            services.AddScoped<SystemService>();
            services.AddScoped<MessengerService>();

            #endregion

            #region SMS Workflow Services

            // SMS Workflow Services - SINGLE REGISTRATION ONLY
            services.AddScoped<ISMSInvestigationWorkflowService, SMSInvestigationWorkflowService>();

            #endregion

            #region SMS Audit Management Services

            // SMS Audit Management Services - READY FOR USE
            services.AddScoped<SMSAuditPlanService>();
            services.AddScoped<SMSAuditService>();

            // NOTE: SMS Audit Data Services are registered in Infrastructure layer (ServiceCollectionExtensions.cs)
            // These services are already available through Infrastructure registration

            #endregion

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
            // Use the enhanced mediator with pipeline support
            services.AddMediator(assembly);

            return services;
        }
    }
}

























