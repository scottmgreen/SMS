//-----------------------------------------------------------------------
// <copyright file="ApplicationTestBase.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Base class for Application layer unit tests providing comprehensive testing infrastructure.
//                  Provides DI configuration, mocking utilities, and test data generation aligned with current Application architecture.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics;
using SMS_Application.Interfaces;
using SMS_Domain.Enums;

namespace PDXSMS_UnitTests.Application.Common;

/// <summary>
/// Comprehensive base class for Application layer unit tests
/// Provides modern testing infrastructure with proper DI, mocking, and utilities
/// Aligned with the current SMS Application CQRS architecture
/// </summary>
public abstract class ApplicationTestBase : IDisposable
{
    protected readonly ServiceProvider ServiceProvider;
    protected readonly IMediator Mediator;
    protected readonly ILogger<ApplicationTestBase> Logger;

    protected ApplicationTestBase()
    {
        ServiceProvider = BuildServiceProvider();
        Mediator = ServiceProvider.GetRequiredService<IMediator>();
        Logger = ServiceProvider.GetRequiredService<ILogger<ApplicationTestBase>>();
    }

    /// <summary>
    /// Override this method to register entity-specific services and handlers
    /// </summary>
    protected abstract void RegisterServices(IServiceCollection services);

    /// <summary>
    /// Override this method to register additional mocked services specific to the test
    /// </summary>
    protected virtual void RegisterMockedServices(IServiceCollection services)
    {
        // Default implementation - override in derived classes for specific mocks
    }

    private ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        
        // Load configuration from appsettings.json
        var configuration = LoadTestConfiguration();
        services.AddSingleton<IConfiguration>(configuration);
        
        // Add logging with test-appropriate levels
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.AddDebug();
            builder.SetMinimumLevel(LogLevel.Information);
        });
        
        // Core Application services
        RegisterCoreApplicationServices(services);
        
        // Allow derived classes to register their specific services
        RegisterServices(services);
        
        // Allow derived classes to register mocked services
        RegisterMockedServices(services);
        
        return services.BuildServiceProvider();
    }

    private static IConfiguration LoadTestConfiguration()
    {
        var configData = new Dictionary<string, string>
        {
            ["ConnectionStrings:DefaultConnection"] = "Data Source=:memory:",
            ["Logging:LogLevel:Default"] = "Information",
            ["Testing:Environment"] = "UnitTest"
        };

        return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddInMemoryCollection(configData)
            .Build();
    }

    private static void RegisterCoreApplicationServices(IServiceCollection services)
    {
        // Register the core mediator service
        services.AddTransient<IMediator, SMS_Application.Services.Mediator>();
    }

    #region Mock Service Factories

    /// <summary>
    /// Creates a mock logger for the specified type
    /// </summary>
    protected static ILogger<T> CreateMockLogger<T>()
    {
        return new Mock<ILogger<T>>().Object;
    }

    #endregion

    #region Test Entity Creation Helpers

    /// <summary>
    /// Creates a test Hazard entity with realistic values based on actual entity structure
    /// </summary>
    protected static Hazard CreateTestHazard(string? code = null)
    {
        var hazardCode = code ?? GenerateUniqueCode("HZ");
        var hazardId = new HazardID(hazardCode);
        return new Hazard(hazardId)
        {
            Code = hazardCode,
            Name = $"Test Hazard {hazardCode}",
            Description = $"Unit test hazard created for testing - {hazardCode}",
            ReportCode = GenerateUniqueCode("RP"),
            Status = HazardStatus.InitialRiskAssessment, // Use actual enum value
            HazardCategory = "Safety",
            HazardType = "Operational",
            CreatedBy = "TEST_USER",
            CreatedDate = DateTime.UtcNow,
            UpdatedBy = "TEST_USER", 
            UpdatedDate = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a test Report entity with realistic values based on actual entity structure
    /// </summary>
    protected static Report CreateTestReport(string? code = null)
    {
        var reportCode = code ?? GenerateUniqueCode("RP");
        var reportId = new ReportID(reportCode);
        return new Report(reportId)
        {
            Code = reportCode,
            Name = $"Test Report {reportCode}",
            Description = $"Unit test report created for testing - {reportCode}",
            Status = "Active", // Report.Status is string, not enum
            Stage = "Investigation",
            SubmittedBy = "TEST_USER",
            SubmittedDate = DateTime.UtcNow,
            CreatedBy = "TEST_USER",
            CreatedDate = DateTime.UtcNow,
            UpdatedBy = "TEST_USER",
            UpdatedDate = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a test Investigation entity
    /// </summary>
    protected static Investigation CreateTestInvestigation(string? code = null)
    {
        var investigationCode = code ?? GenerateUniqueCode("INV");
        var investigationId = new InvestigationID(investigationCode);
        return new Investigation(investigationId)
        {
            Code = investigationCode,
            ReportCode = GenerateUniqueCode("RP"),
            Status = InvestigationStatus.InvestigatorAssigned, // Use actual enum value
            CreatedBy = "TEST_USER",
            CreatedDate = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a test Interview entity
    /// </summary>
    protected static Interview CreateTestInterview(string? code = null)
    {
        var interviewCode = code ?? GenerateUniqueCode("IV");
        var interviewId = new InterviewID(interviewCode);
        return new Interview(interviewId)
        {
            Code = interviewCode,
            InvestigationCode = GenerateUniqueCode("INV"),
            Status = InterviewStatus.InterviewComplete, // Use actual enum value
            CreatedBy = "TEST_USER",
            CreatedDate = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a test Mitigation entity
    /// </summary>
    protected static Mitigation CreateTestMitigation(string? code = null)
    {
        var mitigationCode = code ?? GenerateUniqueCode("MIT");
        var mitigationId = new MitigationID(mitigationCode);
        return new Mitigation(mitigationId)
        {
            Code = mitigationCode,
            HazardCode = GenerateUniqueCode("HZ"),
            Description = $"Unit test mitigation - {mitigationCode}",
            Status = MitigationStatus.PendingApproval, // Use actual enum value
            CreatedBy = "TEST_USER",
            CreatedDate = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a test MitigationAssignment entity
    /// </summary>
    protected static MitigationAssignment CreateTestMitigationAssignment(string? code = null)
    {
        var assignmentCode = code ?? GenerateUniqueCode("MA");
        var assignmentId = new MitigationAssignmentID(assignmentCode);
        return new MitigationAssignment(assignmentId)
        {
            Code = assignmentCode,
            MitigationCode = GenerateUniqueCode("MIT"),
            CreatedBy = "TEST_USER",
            CreatedDate = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a test RiskAnalysis entity with actual properties from the domain
    /// </summary>
    protected static RiskAnalysis CreateTestRiskAnalysis(string? code = null)
    {
        var riskCode = code ?? GenerateUniqueCode("RA");
        var riskId = new RiskAnalysisID(riskCode);
        return new RiskAnalysis(riskId)
        {
            Code = riskCode,
            AssessmentType = RiskAnalysisType.Initial,
            HazardCode = GenerateUniqueCode("HZ"),
            RiskAssessmentCode = GenerateUniqueCode("RASS"),
            InitialWorstCredibleOutcome = "Equipment damage",
            InitialRootCause = "Human error",
            InitialAdditionalComments = "Test risk analysis for unit testing",
            ResidualWorstCredibleOutcome = "Minor damage",
            ResidualRootCause = "Process improvement",
            ResidualAdditionalComments = "Post-mitigation analysis",
            CreatedBy = "TEST_USER",
            CreatedDate = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a test RiskAssessment entity
    /// </summary>
    protected static RiskAssessment CreateTestRiskAssessment(string? code = null)
    {
        var assessmentCode = code ?? GenerateUniqueCode("RA");
        var assessmentId = new RiskAssessmentID(assessmentCode);
        return new RiskAssessment(assessmentId)
        {
            Code = assessmentCode,
            HazardCode = GenerateUniqueCode("HZ"),
            Status = RiskAssessmentStatus.AssessmentUnderway, // Use actual enum value
            CreatedBy = "TEST_USER",
            CreatedDate = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a test ScoringPanel entity
    /// </summary>
    protected static ScoringPanel CreateTestScoringPanel(string? code = null)
    {
        var panelCode = code ?? GenerateUniqueCode("SP");
        var panelId = new ScoringPanelID(panelCode);
        return new ScoringPanel(panelId)
        {
            Code = panelCode,
            CreatedBy = "TEST_USER",
            CreatedDate = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a test AirportSharedDataset entity
    /// </summary>
    protected static AirportSharedDataset CreateTestAirportSharedDataset(string? code = null)
    {
        var datasetCode = code ?? GenerateUniqueCode("ASD");
        var datasetId = new AirportSharedDatasetID(datasetCode);
        return new AirportSharedDataset(datasetId)
        {
            Code = datasetCode,
            CreatedBy = "TEST_USER",
            CreatedDate = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a test ReportValidation entity
    /// </summary>
    protected static ReportValidation CreateTestReportValidation(string? code = null)
    {
        var validationCode = code ?? GenerateUniqueCode("RV");
        var validationId = new ReportValidationID(validationCode);
        return new ReportValidation(validationId)
        {
            Code = validationCode,
            ReportCode = GenerateUniqueCode("RP"),
            Status = ReportValidationStatus.ValidationNeeded, // Use actual enum value
            CreatedBy = "TEST_USER",
            CreatedDate = DateTime.UtcNow
        };
    }

    #endregion

    #region Test Utilities

    /// <summary>
    /// Generates a unique test code with timestamp and random component
    /// </summary>
    protected static string GenerateUniqueCode(string prefix)
    {
        var timestamp = DateTime.Now.ToString("HHmmss");
        var random = Random.Shared.Next(100, 999);
        return $"{prefix}-T{timestamp}{random}";
    }

    /// <summary>
    /// Creates a collection of test entities using a factory method
    /// </summary>
    protected static List<T> CreateTestEntities<T>(int count, Func<string, T> factory, string prefix)
    {
        return Enumerable.Range(1, count)
            .Select(i => factory($"{prefix}-{i:D3}"))
            .ToList();
    }

    /// <summary>
    /// Creates a cancellation token with the specified timeout
    /// </summary>
    protected static CancellationToken CreateTimeoutToken(TimeSpan timeout)
    {
        return new CancellationTokenSource(timeout).Token;
    }

    /// <summary>
    /// Measures execution time of an async operation
    /// </summary>
    protected static async Task<(T Result, TimeSpan Duration)> MeasureAsync<T>(Func<Task<T>> operation)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = await operation();
        stopwatch.Stop();
        return (result, stopwatch.Elapsed);
    }

    /// <summary>
    /// Validates that a Result object contains expected success/failure state
    /// </summary>
    protected static void ValidateResult<T>(Result<T> result, bool shouldSucceed, string? expectedError = null)
    {
        result.Should().NotBeNull();
        
        if (shouldSucceed)
        {
            result.IsSuccess.Should().BeTrue($"Expected successful result but got error: {result.Error}");
            result.Value.Should().NotBeNull();
        }
        else
        {
            result.IsFailure.Should().BeTrue("Expected failed result but got success");
            if (!string.IsNullOrEmpty(expectedError))
            {
                result.Error.ToString().Should().Contain(expectedError);
            }
        }
    }

    /// <summary>
    /// Creates a realistic test command cancellation scenario
    /// </summary>
    protected static async Task<bool> TestCancellationScenario<T>(Func<CancellationToken, Task<T>> operation)
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        try
        {
            await operation(cts.Token);
            return false; // Operation completed despite cancellation
        }
        catch (OperationCanceledException)
        {
            return true; // Cancellation was properly honored
        }
        catch
        {
            return false; // Some other exception occurred
        }
    }

    #endregion

    #region Performance Testing

    /// <summary>
    /// Performance testing metrics container
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
        
        public override string ToString()
        {
            return $"Performance: {SuccessfulIterations}/{TotalIterations} successful ({SuccessRate:F1}%), " +
                   $"Avg: {AverageDuration.TotalMilliseconds:F1}ms, " +
                   $"Range: {MinDuration.TotalMilliseconds:F1}ms - {MaxDuration.TotalMilliseconds:F1}ms";
        }
    }

    /// <summary>
    /// Measures performance of an operation across multiple iterations
    /// </summary>
    protected static async Task<PerformanceMetrics> MeasurePerformanceAsync<T>(
        Func<Task<T>> operation, 
        int iterations = 10)
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