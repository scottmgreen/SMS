//-------------------------------------------------------------------------------
// <copyright file="ServiceCollectionExtensions.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Configuration component for Application layer dependency injection and setup.
//                  Provides dependency injection configuration and service registration
//                  for the Application layer in the Clean Architecture.
// </copyright>
//-------------------------------------------------------------------------------

using SMS_Application.Messaging.Pipelines;
using SMS_Application.Services;
using SMS_Application.Interfaces;

namespace SMS_Application.Configuration;

internal static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the mediator with pipeline support to the service collection
    /// FIXED: Uses ordered pipeline registration to avoid StackOverflowException
    /// </summary>
    public static IServiceCollection AddMediator(this IServiceCollection services, Assembly assembly)
    {
        // Core mediator service
        services.AddScoped<IMediator, Mediator>();

        // 🔧 FIXED: Use a single pipeline that handles the ordering internally
        // Register only ONE pipeline implementation to avoid circular dependencies
        services.AddScoped(typeof(IPipeline<,>), typeof(AuditFieldsPipeline<,>));

        // Register supporting services for other pipelines
        services.AddScoped<IQueryAccessAuditService, QueryAccessAuditService>();
        
        // Register all command/query handlers from the assembly
        RegisterHandlers(services, assembly);

        return services;
    }

    /// <summary>
    /// Register all request handlers from the assembly
    /// </summary>
    private static void RegisterHandlers(IServiceCollection services, Assembly assembly)
    {
        var handlerTypes = assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface && 
                       t.GetInterfaces()
                        .Any(i => i.IsGenericType && 
                                 i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)))
            .ToList();

        foreach (var handlerType in handlerTypes)
        {
            var interfaceType = handlerType.GetInterfaces()
                .First(i => i.IsGenericType && 
                           i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));

            services.AddScoped(interfaceType, handlerType);
        }
    }

    public enum LifeTime
    {
        Singleton,
        Transient,
        Scoped
    }
}
//// Register Application Services
//services.AddScoped<IAirportSharedDatasetService, AirportSharedDatasetService>();
//services.AddScoped<IHazardService, HazardService>();
//services.AddScoped<IInterviewService, InterviewService>();
//services.AddScoped<IInvestigationService, InvestigationService>();
//services.AddScoped<IMitigationAssignmentService, MitigationAssignmentService>();
//services.AddScoped<IMitigationService, MitigationService>();
//services.AddScoped<IReportService, ReportService>();
//services.AddScoped<IReportValidationService, ReportValidationService>();
//services.AddScoped<IRiskAnalysisService, RiskAnalysisService>();
//services.AddScoped<IRiskAssessmentService, RiskAssessmentService>();
//services.AddScoped<IScoringPanelService, ScoringPanelService>();
//services.AddScoped<ISystemService, SystemService>();

//// Register SMS User Management Services
//services.AddScoped<ISMSApplicationUserService, SMSApplicationUserService>();
//services.AddScoped<ISMSOrganizationalUserService, SMSOrganizationalUserService>();
//services.AddScoped<ISMSStakeholderUserService, SMSStakeholderUserService>();
//services.AddScoped<ISMSRoleService, SMSRoleService>();
//services.AddScoped<ISMSWorkflowService, SMSWorkflowService>();
//services.AddScoped<ISMSAuthorizationService, SMSAuthorizationService>();

//// Register Messaging Services
//services.AddScoped<MediatorService>();
//services.AddScoped<MessengerService>();

