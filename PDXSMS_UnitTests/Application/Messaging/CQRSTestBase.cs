using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using SMS_Application.Interfaces;
using SMS_Application.Services;
using SMS_Domain.Entities;

namespace PDXSMS_UnitTests.Application.CQRS;

/// <summary>
/// Base class for CQRS unit tests providing common setup and utilities
/// Provides consistent DI configuration and helper methods for all CQRS tests
/// </summary>
public abstract class CQRSTestBase : IDisposable
{
    protected readonly ServiceProvider ServiceProvider;
    protected readonly IMediator Mediator;

    protected CQRSTestBase()
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
        
        // Register core mediator services
        services.AddTransient<IMediator, Mediator>();
        
        // Allow derived classes to register their specific services
        RegisterServices(services);
        
        return services.BuildServiceProvider();
    }

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
    /// Creates a test ScoringPanel entity with unique values
    /// </summary>
    protected static ScoringPanel CreateTestScoringPanel(string? code = null)
    {
        var scoringPanelCode = code ?? GenerateTestCode("SP");
        var scoringPanelId = new ScoringPanelID(scoringPanelCode);
        return new ScoringPanel(scoringPanelId)
        {
            Code = scoringPanelCode,
            HazardCode = GenerateTestCode("HZ"),
            SMSUserCode = GenerateTestCode("USER"),
            Likelihood = "Medium",
            Severity = "High",
            Score = "75",
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

    #region Mock Creation Helpers

    /// <summary>
    /// Creates a mock logger for the specified type
    /// </summary>
    protected static Mock<ILogger<T>> CreateMockLogger<T>()
    {
        return new Mock<ILogger<T>>();
    }

    /// <summary>
    /// Creates a mock data service with basic setup
    /// </summary>
    protected static Mock<T> CreateMockDataService<T>() where T : class
    {
        // This would need to be adjusted based on the specific constructor of each DataService
        return new Mock<T>();
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