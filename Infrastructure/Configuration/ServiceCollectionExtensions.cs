using SMS_Infrastructure.Interfaces;
using SMS_Infrastructure.Services;
using SMS_Domain.Interfaces;
using Infrastructure.Interfaces;

namespace SMS_Infrastructure.Configuration;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataServices(this IServiceCollection services, IConfiguration configuration)
    {
        // File Services
        services.AddScoped<FileService>();

        // System Services
        services.AddScoped<SystemRepository>();
        services.AddScoped<SystemDataService>();

        // SMS User Repositories - INTERFACE BINDINGS
        services.AddScoped<ISMSApplicationUserRepository, SMSApplicationUserRepository>();
        services.AddScoped<ISMSOrganizationalUserRepository, SMSOrganizationalUserRepository>();
        services.AddScoped<ISMSStakeholderUserRepository, SMSStakeholderUserRepository>();
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
        services.AddScoped<SMSApplicationUserRepository>();
        services.AddScoped<SMSApplicationGroupRepository>();
        services.AddScoped<SMSOrganizationalUserRepository>();
        services.AddScoped<SMSOrganizationalGroupRepository>();
        services.AddScoped<SMSStakeholderGroupRepository>();
        services.AddScoped<SMSStakeholderUserRepository>();
        services.AddScoped<SMSUserRoleRepository>();

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
        services.AddScoped<HazardFileDataService>();
        services.AddScoped<AirportSharedDatasetDataService>();
        services.AddScoped<ReportDataService>();
        services.AddScoped<InterviewDataService>();
        services.AddScoped<InvestigationDataService>();
        services.AddScoped<RiskAnalysisDataService>();
        services.AddScoped<RiskAssessmentDataService>();
        services.AddScoped<MitigationDataService>();
        services.AddScoped<MitigationAssignmentDataService>();
        services.AddScoped<ReportValidationDataService>();
        services.AddScoped<ScoringPanelDataService>();
        services.AddScoped<SafetyPerformanceIndicatorDataService>();

        return services;
    }
}
