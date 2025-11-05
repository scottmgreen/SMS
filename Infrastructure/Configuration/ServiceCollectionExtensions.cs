using SMS_Infrastructure.Interfaces;
using SMS_Infrastructure.Repositories;
using SMS_Infrastructure.Services;
using SMS_Infrastructure.Persistence;
using SMS_Domain.Interfaces;

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

        // SMS User Repositories
        services.AddScoped<ISMSApplicationUserRepository, SMSApplicationUserRepository>();
        services.AddScoped<ISMSOrganizationalUserRepository, SMSOrganizationalUserRepository>();
        services.AddScoped<ISMSStakeholderUserRepository, SMSStakeholderUserRepository>();

        // SMS Repositories
        services.AddScoped<HazardRepository>();
        services.AddScoped<AirportSharedDatasetRepository>();
        services.AddScoped<ReportRepository>();
        services.AddScoped<InterviewRepository>();
        services.AddScoped<InvestigationRepository>();
        services.AddScoped<RiskAnalysisRepository>();
        services.AddScoped<RiskAssessmentRepository>();
        services.AddScoped<MitigationRepository>();
        services.AddScoped<MitigationAssignmentRepository>();
        services.AddScoped<ReportValidationRepository>();
        services.AddScoped<ScoringPanelRepository>();

        // SMS User Data Services
        services.AddScoped<SMSApplicationUserDataService>();
        services.AddScoped<SMSOrganizationalUserDataService>();
        services.AddScoped<SMSStakeholderUserDataService>();

        // SMS Data Services - All Available Services
        services.AddScoped<HazardDataService>();
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



        return services;
    }
}
