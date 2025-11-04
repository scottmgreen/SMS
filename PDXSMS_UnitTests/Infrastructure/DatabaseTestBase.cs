using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SMS_Infrastructure.Configuration;
using SMS_Infrastructure.Repositories;
using SMS_Infrastructure.Services;
using SMS_Shared.Configuration;
using SMS_Domain.Entities;

namespace PDXSMS_UnitTests.Infrastructure;

/// <summary>
/// Base class for database integration tests
/// Uses the actual Infrastructure DI configuration for realistic testing
/// </summary>
public abstract class DatabaseTestBase : IDisposable
{
    protected readonly ServiceProvider _serviceProvider;
    protected readonly IConfiguration _configuration;
    protected readonly string _connectionString;
    protected readonly ILogger<DatabaseTestBase> _logger;

    protected DatabaseTestBase()
    {
        // Build configuration from appsettings.json
        var configBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        
        _configuration = configBuilder.Build();
        _connectionString = _configuration.GetConnectionString("DefaultConnectionString")!;

        // Build service collection using Infrastructure DI
        var services = new ServiceCollection();
        
        // Add configuration
        services.AddSingleton<IConfiguration>(_configuration);

        // Add shared services (logging, feature management)
        services.AddSharedServices(_configuration);

        // Add infrastructure services (repositories, data services, etc.)
        services.AddInfrastructureServices(_configuration);

        _serviceProvider = services.BuildServiceProvider();
        _logger = _serviceProvider.GetRequiredService<ILogger<DatabaseTestBase>>();

        _logger.LogInformation("Database integration test initialized with connection: {ConnectionString}", 
            _connectionString.Substring(0, Math.Min(50, _connectionString.Length)) + "...");
    }

    protected T GetService<T>() where T : notnull
    {
        return _serviceProvider.GetRequiredService<T>();
    }

    #region Repository Getters

    protected HazardRepository GetHazardRepository() => GetService<HazardRepository>();
    protected InterviewRepository GetInterviewRepository() => GetService<InterviewRepository>();
    protected InvestigationRepository GetInvestigationRepository() => GetService<InvestigationRepository>();
    protected AirportSharedDatasetRepository GetAirportSharedDatasetRepository() => GetService<AirportSharedDatasetRepository>();
    protected MitigationRepository GetMitigationRepository() => GetService<MitigationRepository>();
    protected MitigationAssignmentRepository GetMitigationAssignmentRepository() => GetService<MitigationAssignmentRepository>();
    protected ReportRepository GetReportRepository() => GetService<ReportRepository>();
    protected ReportValidationRepository GetReportValidationRepository() => GetService<ReportValidationRepository>();
    protected RiskAnalysisRepository GetRiskAnalysisRepository() => GetService<RiskAnalysisRepository>();
    protected RiskAssessmentRepository GetRiskAssessmentRepository() => GetService<RiskAssessmentRepository>();
    protected ScoringPanelRepository GetScoringPanelRepository() => GetService<ScoringPanelRepository>();

    #endregion

    #region DataService Getters

    protected HazardDataService GetHazardDataService() => GetService<HazardDataService>();
    protected InterviewDataService GetInterviewDataService() => GetService<InterviewDataService>();
    protected InvestigationDataService GetInvestigationDataService() => GetService<InvestigationDataService>();
    protected AirportSharedDatasetDataService GetAirportSharedDatasetDataService() => GetService<AirportSharedDatasetDataService>();
    protected MitigationDataService GetMitigationDataService() => GetService<MitigationDataService>();
    protected MitigationAssignmentDataService GetMitigationAssignmentDataService() => GetService<MitigationAssignmentDataService>();
    protected ReportDataService GetReportDataService() => GetService<ReportDataService>();
    protected ReportValidationDataService GetReportValidationDataService() => GetService<ReportValidationDataService>();
    protected RiskAnalysisDataService GetRiskAnalysisDataService() => GetService<RiskAnalysisDataService>();
    protected RiskAssessmentDataService GetRiskAssessmentDataService() => GetService<RiskAssessmentDataService>();
    protected ScoringPanelDataService GetScoringPanelDataService() => GetService<ScoringPanelDataService>();

    #endregion

    #region Test ID Generation Utilities

    /// <summary>
    /// Generates a unique test identifier for use in integration tests.
    /// Format: TEST-####, where #### is a 4-digit random number (0000-9999)
    /// </summary>
    /// <returns>A unique test identifier string</returns>
    protected string GenerateTestId()
    {
        int number = Random.Shared.Next(0, 10000); // 0 to 9999
        return $"TEST-{number:D4}";
    }

    /// <summary>
    /// Generates a unique test identifier with a custom prefix.
    /// Format: {prefix}-####, where #### is a 4-digit random number (0000-9999)
    /// </summary>
    /// <param name="prefix">Custom prefix for the test ID (e.g., "PERF", "LOAD", "STRESS")</param>
    /// <returns>A unique test identifier string with custom prefix</returns>
    protected string GenerateTestId(string prefix)
    {
        int number = Random.Shared.Next(0, 10000); // 0 to 9999
        return $"{prefix}-{number:D4}";
    }

    /// <summary>
    /// Generates a unique test identifier with timestamp for guaranteed uniqueness.
    /// Format: TEST-####-HHMMSS, where #### is random and HHMMSS is current time
    /// </summary>
    /// <returns>A unique test identifier string with timestamp</returns>
    protected string GenerateUniqueTestId()
    {
        int number = Random.Shared.Next(0, 10000); // 0 to 9999
        string timestamp = DateTime.Now.ToString("HHmmss");
        return $"TEST-{number:D4}-{timestamp}";
    }

    /// <summary>
    /// Generates a unique test identifier with custom prefix and timestamp.
    /// Format: {prefix}-####-HHMMSS, where #### is random and HHMMSS is current time
    /// </summary>
    /// <param name="prefix">Custom prefix for the test ID</param>
    /// <returns>A unique test identifier string with custom prefix and timestamp</returns>
    protected string GenerateUniqueTestId(string prefix)
    {
        int number = Random.Shared.Next(0, 10000); // 0 to 9999
        string timestamp = DateTime.Now.ToString("HHmmss");
        return $"{prefix}-{number:D4}-{timestamp}";
    }

    #endregion

    #region Test Entity Creation Helpers

    /// <summary>
    /// Creates a test hazard entity with unique values
    /// </summary>
    protected Hazard CreateTestHazard(string? reportCode = null)
    {
        var testId = GenerateTestId("HZ");
        // Use a temporary ID - the repository will generate the actual code
        var tempId = new HazardID(testId);
        
        var hazard = new Hazard(tempId)
        {
            Code = string.Empty, // Let the database generate this
            Name = $"Test Hazard {testId}",
            Description = $"Test hazard created for integration testing - {testId}",
            ReportCode = reportCode ?? $"RP-TEST-{testId}",
            CreatedBy = "INTEGRATION_TEST",
            CreatedDate = DateTime.UtcNow
        };

        return hazard;
    }

    /// <summary>
    /// Creates a test interview entity with unique values
    /// </summary>
    protected Interview CreateTestInterview(string? investigationCode = null)
    {
        var testId = GenerateTestId("IV");
        var tempId = new InterviewID(testId);
        
        var interview = new Interview(tempId)
        {
            Code = testId,
            InvestigationCode = GenerateTestId("IN"),
            SMSInvestigatorCode = GenerateTestId("SMS"),
            PersonInterviewed = $"Test Person {testId}",
            PersonInterviewedNotes = $"Test person interview notes - {testId}",
            InvestigatorNotes = $"Test investigator notes - {testId}",
            CreatedBy = "INTEGRATION_TEST",
            CreatedDate = DateTime.UtcNow
        };

        return interview;
    }

    /// <summary>
    /// Creates a test airport shared dataset entity with unique values
    /// </summary>
    protected AirportSharedDataset CreateTestAirportSharedDataset()
    {
        var testId = GenerateTestId("AS");
        var tempId = new AirportSharedDatasetID(testId);
        
        var dataset = new AirportSharedDataset(tempId)
        {
            Code = testId,
            ReportID = GenerateTestId("RP"),
            PrivateNarrative = $"Test airport shared dataset - {testId}",
            SharedNarrative = $"Test shared narrative - {testId}",
            CreatedBy = "INTEGRATION_TEST",
            CreatedDate = DateTime.UtcNow
        };

        return dataset;
    }

    /// <summary>
    /// Creates a test report entity with unique values
    /// </summary>
    protected Report CreateTestReport()
    {
        var testId = GenerateTestId("RP");
        var tempId = new ReportID(testId);
        
        var report = new Report(tempId)
        {
            Code = testId,
            Name = $"Test Report {testId}",
            Description = $"Test report created for integration testing - {testId}",
            Status = "Active",
            Stage = "Draft",
            CreatedBy = "INTEGRATION_TEST",
            CreatedDate = DateTime.UtcNow
        };

        return report;
    }

    /// <summary>
    /// Creates a test scoring panel entity with unique values
    /// </summary>
    protected ScoringPanel CreateTestScoringPanel()
    {
        var testId = GenerateTestId("SP");
        var tempId = new ScoringPanelID(testId);
        
        var scoringPanel = new ScoringPanel(tempId)
        {
            Code = testId,
            HazardCode = GenerateTestId("HZ"),
            SMSUserCode = GenerateTestId("SMS"),
            Likelihood = "Medium",
            Severity = "High",
            Score = "75",
            CreatedBy = "INTEGRATION_TEST",
            CreatedDate = DateTime.UtcNow
        };

        return scoringPanel;
    }

    /// <summary>
    /// Creates a test mitigation entity with unique values
    /// </summary>
    protected Mitigation CreateTestMitigation()
    {
        var testId = GenerateTestId("MT");
        var tempId = new MitigationID(testId);
        
        var mitigation = new Mitigation(tempId)
        {
            Code = testId,
            HazardCode = GenerateTestId("HZ"),
            CreatedBy = "INTEGRATION_TEST",
            CreatedDate = DateTime.UtcNow
        };

        return mitigation;
    }

    /// <summary>
    /// Creates a test risk analysis entity with unique values
    /// </summary>
    protected RiskAnalysis CreateTestRiskAnalysis()
    {
        var testId = GenerateTestId();
        var tempId = new RiskAnalysisID($"RA-{testId}");
        
        var riskAnalysis = new RiskAnalysis(tempId)
        {
            Code = $"RA-{testId}",
            Name = $"Test Risk Analysis {testId}",
            Description = $"Test risk analysis created for integration testing - {testId}",
            HazardCode = $"HZ-{testId}",
            Status = "Active",
            Stage = "Analysis",
            WorstCredibleOutcome = "Equipment damage",
            RootCause = "Human error",
            CreatedBy = "INTEGRATION_TEST",
            CreatedDate = DateTime.UtcNow
        };

        return riskAnalysis;
    }

    /// <summary>
    /// Creates a test risk assessment entity with unique values
    /// </summary>
    protected RiskAssessment CreateTestRiskAssessment()
    {
        var testId = GenerateTestId();
        var tempId = new RiskAssessmentID($"RAS-{testId}");
        
        var riskAssessment = new RiskAssessment(tempId)
        {
            Code = $"RAS-{testId}",
            Name = $"Test Risk Assessment {testId}",
            Description = $"Test risk assessment created for integration testing - {testId}",
            HazardCode = $"HZ-{testId}",
            AssessmentType = "Initial",
            Status = "Active",
            Stage = "Assessment",
            CreatedBy = "INTEGRATION_TEST",
            CreatedDate = DateTime.UtcNow
        };

        return riskAssessment;
    }

    /// <summary>
    /// Creates a test report validation entity with unique values
    /// </summary>
    protected ReportValidation CreateTestReportValidation()
    {
        var testId = GenerateTestId();
        var tempId = new ReportValidationID($"RV-{testId}");
        
        var reportValidation = new ReportValidation(tempId)
        {
            Code = $"RV-{testId}",
            ReportCode = $"RPT-{testId}",
            ValidationDecision = "Approved",
            Status = "Active",
            Stage = "Validation",
            CreatedBy = "INTEGRATION_TEST",
            CreatedDate = DateTime.UtcNow
        };

        return reportValidation;
    }

    /// <summary>
    /// Creates a test mitigation assignment entity with unique values
    /// </summary>
    protected MitigationAssignment CreateTestMitigationAssignment()
    {
        var testId = GenerateTestId();
        var tempId = new MitigationAssignmentID($"MA-{testId}");
        
        var mitigationAssignment = new MitigationAssignment(tempId)
        {
            Code = $"MA-{testId}",
            MitigationCode = $"MIT-{testId}",
            DepartmentCode = $"DEPT-{testId}",
            CreatedBy = "INTEGRATION_TEST",
            CreatedDate = DateTime.UtcNow
        };

        return mitigationAssignment;
    }

    /// <summary>
    /// Creates a test investigation entity with unique values
    /// </summary>
    protected Investigation CreateTestInvestigation()
    {
        var testId = GenerateTestId();
        var tempId = new InvestigationID($"INV-{testId}");
        
        var investigation = new Investigation(tempId)
        {
            Code = $"INV-{testId}",
            ReportCode = $"RPT-{testId}",
            InvestigationNotes = $"Test investigation notes - {testId}",
            CreatedBy = "INTEGRATION_TEST",
            CreatedDate = DateTime.UtcNow
        };

        return investigation;
    }

    #endregion

    /// <summary>
    /// Validates that the database connection is working
    /// </summary>
    protected async Task<bool> ValidateDatabaseConnectionAsync()
    {
        try
        {
            var hazardRepo = GetHazardRepository();
            var result = await hazardRepo.GetAllHazardsAsync();
            return result.IsSuccess;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database connection validation failed");
            return false;
        }
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
        GC.SuppressFinalize(this);
    }
}