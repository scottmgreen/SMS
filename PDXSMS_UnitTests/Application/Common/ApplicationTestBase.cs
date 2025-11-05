using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Moq;
using SMS_Application.Interfaces;
using SMS_Application.Services;
using SMS_Domain.Entities;
using SMS_Infrastructure.Services;
using SMS_Infrastructure.Repositories;
using SMS_Infrastructure.Interfaces;
using SMS_Infrastructure.Configuration;

namespace PDXSMS_UnitTests.Application.Common;

/// <summary>
/// Base class for Application layer unit tests providing common setup and utilities
/// Provides consistent DI configuration and helper methods for all Application tests
/// </summary>
public abstract class ApplicationTestBase : IDisposable
{
    protected readonly ServiceProvider ServiceProvider;
    protected readonly IMediator Mediator;

    protected ApplicationTestBase()
    {
        ServiceProvider = BuildServiceProvider();
        Mediator = ServiceProvider.GetRequiredService<IMediator>();
    }

    /// <summary>
    /// Override this method to register entity-specific services and handlers
    /// </summary>
    protected abstract void RegisterServices(IServiceCollection services);

    private ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        
        // Load configuration from appsettings.json like the real application
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
            
        services.AddSingleton<IConfiguration>(configuration);
        
        // Add logging services
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Warning); // Keep logs quiet for tests
        });
        
        // Use Infrastructure DI configuration like DatabaseTestBase
        services.AddInfrastructureServices(configuration);
        
        // Register core mediator 
        services.AddTransient<IMediator, Mediator>();
        
        // Allow derived classes to register their specific services
        RegisterServices(services);
        
        return services.BuildServiceProvider();
    }

    #region Mock Creation Helpers

    /// <summary>
    /// Creates a mock HazardDataService with proper constructor mocking
    /// </summary>
    protected static Mock<HazardDataService> CreateMockHazardDataService()
    {
        var mockLogger = new Mock<ILogger<HazardDataService>>();
        var mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
        var mockConfiguration = new Mock<IConfiguration>();
        var mockLogSupport = new Mock<ILogSupport>();
        
        // Create a concrete HazardRepository instance for the mock
        var hazardRepo = new HazardRepository(
            Mock.Of<ILogger<HazardRepository>>(),
            mockLogSupport.Object,
            mockConfiguration.Object);
        
        // Mock the HazardDataService with CallBase = false to avoid concrete class issues
        var mockDataService = new Mock<HazardDataService>(
            mockLogger.Object,
            mockServiceScopeFactory.Object,
            mockConfiguration.Object,
            hazardRepo)
        {
            CallBase = false // This prevents calling actual methods on the concrete class
        };

        return mockDataService;
    }

    /// <summary>
    /// Creates a mock ReportDataService with proper constructor mocking
    /// </summary>
    protected static Mock<ReportDataService> CreateMockReportDataService()
    {
        var mockLogger = new Mock<ILogger<ReportDataService>>();
        var mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
        var mockConfiguration = new Mock<IConfiguration>();
        var mockLogSupport = new Mock<ILogSupport>();
        
        var reportRepo = new ReportRepository(
            Mock.Of<ILogger<ReportRepository>>(),
            mockLogSupport.Object,
            mockConfiguration.Object);
        
        var mockDataService = new Mock<ReportDataService>(
            mockLogger.Object,
            mockServiceScopeFactory.Object,
            mockConfiguration.Object,
            reportRepo)
        {
            CallBase = false
        };

        return mockDataService;
    }

    /// <summary>
    /// Creates a mock logger for the specified type
    /// </summary>
    protected static Mock<ILogger<T>> CreateMockLogger<T>()
    {
        return new Mock<ILogger<T>>();
    }

    #endregion

    #region Test Entity Creation Helpers

    /// <summary>
    /// Creates a test Hazard entity with unique values
    /// </summary>
    protected static Hazard CreateTestHazard(string? code = null)
    {
        var hazardCode = code ?? GenerateTestCode("HZ");
        var hazardId = new HazardID(hazardCode);
        return new Hazard(hazardId)
        {
            Code = hazardCode,
            Name = $"Test Hazard {hazardCode}",
            Description = $"Test hazard for unit testing - {hazardCode}",
            ReportCode = GenerateTestCode("RP"),
            CreatedBy = "UNIT_TEST",
            CreatedDate = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a test Report entity with unique values
    /// </summary>
    protected static Report CreateTestReport(string? code = null)
    {
        var reportCode = code ?? GenerateTestCode("RP");
        var reportId = new ReportID(reportCode);
        return new Report(reportId)
        {
            Code = reportCode,
            Name = $"Test Report {reportCode}",
            Description = $"Test report for unit testing - {reportCode}",
            Status = "Active",
            Stage = "Draft",
            CreatedBy = "UNIT_TEST",
            CreatedDate = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a test Investigation entity with unique values
    /// </summary>
    protected static Investigation CreateTestInvestigation(string? code = null)
    {
        var investigationCode = code ?? GenerateTestCode("INV");
        var investigationId = new InvestigationID(investigationCode);
        return new Investigation(investigationId)
        {
            Code = investigationCode,
            ReportCode = GenerateTestCode("RP"),
            InvestigationNotes = $"Test investigation notes - {investigationCode}",
            CreatedBy = "UNIT_TEST",
            CreatedDate = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a test Mitigation entity with unique values
    /// </summary>
    protected static Mitigation CreateTestMitigation(string? code = null)
    {
        var mitigationCode = code ?? GenerateTestCode("MIT");
        var mitigationId = new MitigationID(mitigationCode);
        return new Mitigation(mitigationId)
        {
            Code = mitigationCode,
            HazardCode = GenerateTestCode("HZ"),
            CreatedBy = "UNIT_TEST",
            CreatedDate = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a test Interview entity with unique values
    /// </summary>
    protected static Interview CreateTestInterview(string? code = null)
    {
        var interviewCode = code ?? GenerateTestCode("IV");
        var interviewId = new InterviewID(interviewCode);
        return new Interview(interviewId)
        {
            Code = interviewCode,
            CreatedBy = "UNIT_TEST",
            CreatedDate = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a test RiskAnalysis entity with unique values
    /// </summary>
    protected static RiskAnalysis CreateTestRiskAnalysis(string? code = null)
    {
        var riskAnalysisCode = code ?? GenerateTestCode("RA");
        var riskAnalysisId = new RiskAnalysisID(riskAnalysisCode);
        return new RiskAnalysis(riskAnalysisId)
        {
            Code = riskAnalysisCode,
            HazardCode = GenerateTestCode("HZ"),
            CreatedBy = "UNIT_TEST",
            CreatedDate = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a test RiskAssessment entity with unique values
    /// </summary>
    protected static RiskAssessment CreateTestRiskAssessment(string? code = null)
    {
        var riskAssessmentCode = code ?? GenerateTestCode("RA");
        var riskAssessmentId = new RiskAssessmentID(riskAssessmentCode);
        return new RiskAssessment(riskAssessmentId)
        {
            Code = riskAssessmentCode,
            HazardCode = GenerateTestCode("HZ"),
            CreatedBy = "UNIT_TEST",
            CreatedDate = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a test ScoringPanel entity with unique values
    /// </summary>
    protected static ScoringPanel CreateTestScoringPanel(string? code = null)
    {
        var scoringPanelCode = code ?? GenerateTestCode("SP");
        var scoringPanelId = new ScoringPanelID(scoringPanelCode);
        return new ScoringPanel(scoringPanelId)
        {
            Code = scoringPanelCode,
            CreatedBy = "UNIT_TEST",
            CreatedDate = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a test AirportSharedDataset entity with unique values
    /// </summary>
    protected static AirportSharedDataset CreateTestAirportSharedDataset(string? code = null)
    {
        var datasetCode = code ?? GenerateTestCode("ASD");
        var datasetId = new AirportSharedDatasetID(datasetCode);
        return new AirportSharedDataset(datasetId)
        {
            Code = datasetCode,
            ReportID = GenerateTestCode("RP"),
            CreatedBy = "UNIT_TEST",
            CreatedDate = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a test MitigationAssignment entity with unique values
    /// </summary>
    protected static MitigationAssignment CreateTestMitigationAssignment(string? code = null)
    {
        var assignmentCode = code ?? GenerateTestCode("MA");
        var assignmentId = new MitigationAssignmentID(assignmentCode);
        return new MitigationAssignment(assignmentId)
        {
            Code = assignmentCode,
            MitigationCode = GenerateTestCode("MIT"),
            CreatedBy = "UNIT_TEST",
            CreatedDate = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a test ReportValidation entity with unique values
    /// </summary>
    protected static ReportValidation CreateTestReportValidation(string? code = null)
    {
        var validationCode = code ?? GenerateTestCode("RV");
        var validationId = new ReportValidationID(validationCode);
        return new ReportValidation(validationId)
        {
            Code = validationCode,
            ReportCode = GenerateTestCode("RP"),
            CreatedBy = "UNIT_TEST",
            CreatedDate = DateTime.UtcNow
        };
    }

    #endregion

    #region Test Utilities

    /// <summary>
    /// Generates a unique test code with the given prefix
    /// </summary>
    protected static string GenerateTestCode(string prefix)
    {
        var timestamp = DateTime.Now.ToString("HHmmss");
        var random = Random.Shared.Next(100, 999);
        return $"{prefix}-TEST-{timestamp}-{random}";
    }

    /// <summary>
    /// Creates a list of test entities of the specified type
    /// </summary>
    protected static List<T> CreateTestEntities<T>(int count, Func<string, T> entityFactory, string prefix)
    {
        return Enumerable.Range(1, count)
            .Select(i => entityFactory($"{prefix}-{i:D3}"))
            .ToList();
    }

    /// <summary>
    /// Creates a CancellationToken that will be cancelled after the specified delay
    /// </summary>
    protected static CancellationToken CreateTimeoutToken(TimeSpan timeout)
    {
        var cts = new CancellationTokenSource(timeout);
        return cts.Token;
    }

    /// <summary>
    /// Measures the execution time of an async operation
    /// </summary>
    protected static async Task<(T Result, TimeSpan Duration)> MeasureAsync<T>(Func<Task<T>> operation)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await operation();
        stopwatch.Stop();
        return (result, stopwatch.Elapsed);
    }

    #endregion

    #region Performance Testing Helpers

    /// <summary>
    /// Executes a test operation multiple times and measures performance metrics
    /// </summary>
    protected static async Task<PerformanceMetrics> MeasurePerformanceAsync<T>(
        Func<Task<T>> operation, 
        int iterations = 100)
    {
        var durations = new List<TimeSpan>();
        var exceptions = new List<Exception>();

        for (int i = 0; i < iterations; i++)
        {
            try
            {
                var (_, duration) = await MeasureAsync(operation);
                durations.Add(duration);
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        }

        return new PerformanceMetrics
        {
            TotalIterations = iterations,
            SuccessfulIterations = durations.Count,
            FailedIterations = exceptions.Count,
            AverageDuration = durations.Any() ? TimeSpan.FromTicks((long)durations.Average(d => d.Ticks)) : TimeSpan.Zero,
            MinDuration = durations.Any() ? durations.Min() : TimeSpan.Zero,
            MaxDuration = durations.Any() ? durations.Max() : TimeSpan.Zero,
            Exceptions = exceptions
        };
    }

    #endregion

    public virtual void Dispose()
    {
        ServiceProvider?.Dispose();
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Performance metrics for test operations
/// </summary>
public class PerformanceMetrics
{
    public int TotalIterations { get; set; }
    public int SuccessfulIterations { get; set; }
    public int FailedIterations { get; set; }
    public TimeSpan AverageDuration { get; set; }
    public TimeSpan MinDuration { get; set; }
    public TimeSpan MaxDuration { get; set; }
    public List<Exception> Exceptions { get; set; } = new();

    public double SuccessRate => TotalIterations > 0 ? (double)SuccessfulIterations / TotalIterations * 100 : 0;
}