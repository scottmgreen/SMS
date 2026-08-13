//-----------------------------------------------------------------------
// <copyright file="DependencyInjection.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Infrastructure layer dependency injection configuration providing service registration, repository setup, and data service coordination.
//                  Infrastructure configuration providing dependency injection,
//                  service registration, and system setup.
// </copyright>
//-----------------------------------------------------------------------

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
    public static IServiceCollection AddDataServices(this IServiceCollection services, IConfiguration configuration)
    {
        // File Services
        services.AddScoped<FileService>();

        // System Services
        services.AddScoped<SMSSystemRepository>();
        services.AddScoped<SystemDataService>();

        // SMS User Repositories - INTERFACE BINDINGS
        services.AddScoped<ISMSApplicationUserRepository, SMSApplicationUserRepository>();
        services.AddScoped<ISMSOrganizationalUserRepository, SMSOrganizationalUserRepository>();
        services.AddScoped<ISMSStakeholderUserRepository, SMSStakeholderUserRepository>();
        services.AddScoped<ISMSStakeholderUserTitleRepository, SMSStakeholderUserTitleRepository>();
        services.AddScoped<ISMSDepartmentRepository, SMSDepartmentRepository>();
        services.AddScoped<ISMSCompanyRepository, SMSCompanyRepository>();
        services.AddScoped<ISMSUserRoleRepository, SMSUserRoleRepository>();

        // SMS Repositories - BOTH CONCRETE AND INTERFACE BINDINGS
        services.AddScoped<HazardRepository>();
        services.AddScoped<IHazardRepository, HazardRepository>();

        services.AddScoped<HazardLocationRepository>();
        services.AddScoped<IHazardLocationRepository, HazardLocationRepository>();

        services.AddScoped<HazardFileRepository>();
        services.AddScoped<IHazardFileRepository, HazardFileRepository>();

        services.AddScoped<AirportSharedDatasetRepository>();
        services.AddScoped<ReportRepository>();
        services.AddScoped<HazardReportTrackingRepository>();
        services.AddScoped<InterviewRepository>();
        services.AddScoped<InvestigationRepository>();
        services.AddScoped<RiskAnalysisRepository>();
        services.AddScoped<RiskAssessmentRepository>();
        services.AddScoped<IRiskAssessmentRepository, RiskAssessmentRepository>();
        services.AddScoped<MitigationRepository>();
        services.AddScoped<MitigationAssignmentRepository>();
        services.AddScoped<ReportValidationRepository>();
        services.AddScoped<ScoringPanelRepository>();
        services.AddScoped<SafetyPerformanceIndicatorRepository>();
        services.AddScoped<EventQueueRepository>();
        services.AddScoped<IEventQueueRepository, EventQueueRepository>();
        services.AddScoped<SMSApplicationUserRepository>();
        services.AddScoped<SMSApplicationGroupRepository>();
        services.AddScoped<SMSOrganizationalUserRepository>();
        services.AddScoped<SMSOrganizationalGroupRepository>();
        services.AddScoped<SMSStakeholderGroupRepository>();
        services.AddScoped<SMSStakeholderUserRepository>();
        services.AddScoped<SMSStakeholderUserTitleRepository>();
        services.AddScoped<SMSDepartmentRepository>();
        services.AddScoped<SMSCompanyRepository>();
        services.AddScoped<SMSUserRoleRepository>();

        // SMS Audit Management Repositories (NEW) - NOW AVAILABLE
        services.AddScoped<SMSAuditPlanRepository>();
        services.AddScoped<SMSAuditRepository>();
        services.AddScoped<SMSAuditFindingRepository>();
        services.AddScoped<SMSAuditEvidenceRepository>();

        // SMS User Data Services
        services.AddScoped<SMSApplicationUserDataService>();
        services.AddScoped<SMSOrganizationalUserDataService>();
        services.AddScoped<SMSStakeholderUserDataService>();
        services.AddScoped<SMSApplicationGroupDataService>();
        services.AddScoped<SMSOrganizationalGroupDataService>();
        services.AddScoped<SMSStakeholderGroupDataService>();
        services.AddScoped<SMSUserRoleDataService>();

        // SMS Data Services - ALL AVAILABLE SERVICES
        services.AddScoped<HazardDataService>();
        services.AddScoped<HazardLocationDataService>();
        services.AddScoped<HazardFileExternalStorageService>();
        services.AddScoped<HazardFileDataService>();
        services.AddScoped<AirportSharedDatasetDataService>();
        services.AddScoped<ReportDataService>();
        services.AddScoped<HazardReportTrackingDataService>();
        services.AddScoped<InterviewDataService>();
        services.AddScoped<InvestigationDataService>();
        services.AddScoped<RiskAnalysisDataService>();
        services.AddScoped<RiskAssessmentDataService>();
        services.AddScoped<MitigationDataService>();
        services.AddScoped<MitigationAssignmentDataService>();
        services.AddScoped<ReportValidationDataService>();
        services.AddScoped<ScoringPanelDataService>();
        services.AddScoped<SafetyPerformanceIndicatorDataService>();
        services.AddScoped<EventQueueDataService>();
        services.AddScoped<IEventQueueDataService, EventQueueDataService>();

        // SMS Audit Management Data Services (NEW) - NOW ENABLED WITH PERSISTENCE LAYER
        services.AddScoped<SMSAuditPlanDataService>();
        services.AddScoped<SMSAuditDataService>();
        services.AddScoped<SMSAuditFindingDataService>();
        services.AddScoped<SMSAuditEvidenceDataService>();

        return services;
    }
}

