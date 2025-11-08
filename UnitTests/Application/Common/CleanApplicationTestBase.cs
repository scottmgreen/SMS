using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SMS_Application.Interfaces;
using SMS_Application.Services;
using SMS_Domain.Entities;
using SMS_Infrastructure.Services;
using SMS_Infrastructure.Repositories;
using SMS_Infrastructure.Interfaces;
using SMS_Infrastructure.Configuration;
using PDXSMS_UnitTests.Utilities;
//using PDXSMS_UnitTests.Utilities;

namespace PDXSMS_UnitTests.Application.Common;

/// <summary>
/// Enhanced base class for Application layer integration tests with database cleanup capabilities
/// Provides clean database state for each test run
/// </summary>
public abstract class CleanApplicationTestBase : IDisposable
{
    protected readonly ServiceProvider ServiceProvider;
    protected readonly IMediator Mediator;
    protected readonly DatabaseCleanupUtility DatabaseCleanup;
    private readonly ILogger<CleanApplicationTestBase> _logger;

    protected CleanApplicationTestBase()
    {
        ServiceProvider = BuildServiceProvider();
        Mediator = ServiceProvider.GetRequiredService<IMediator>();
        DatabaseCleanup = ServiceProvider.GetRequiredService<DatabaseCleanupUtility>();
        _logger = ServiceProvider.GetRequiredService<ILogger<CleanApplicationTestBase>>();
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
            builder.SetMinimumLevel(LogLevel.Warning);
        });

        // Use Infrastructure DI configuration
        services.AddInfrastructureServices(configuration);

        // Register core mediator 
        services.AddTransient<IMediator, Mediator>();

        // Register database cleanup utility
        services.AddScoped<DatabaseCleanupUtility>();

        // Allow derived classes to register their specific services
        RegisterServices(services);

        return services.BuildServiceProvider();
    }

    /// <summary>
    /// Cleans up all SMS tables before running tests
    /// Call this in your test setup methods for a clean slate
    /// </summary>
    protected async Task CleanupDatabaseAsync()
    {
        _logger.LogInformation("Cleaning up database for test...");
        var success = await DatabaseCleanup.TruncateAllTablesAsync();

        if (!success)
        {
            throw new InvalidOperationException("Failed to cleanup database before test execution");
        }

        // Verify cleanup was successful
        var isEmpty = await DatabaseCleanup.VerifyTablesAreEmptyAsync();
        if (!isEmpty)
        {
            throw new InvalidOperationException("Database cleanup verification failed - tables are not empty");
        }

        _logger.LogInformation("Database cleanup completed successfully");
    }

    /// <summary>
    /// Cleans up specific tables only
    /// Useful for targeted test cleanup
    /// </summary>
    protected async Task CleanupSpecificTablesAsync(params string[] tableNames)
    {
        _logger.LogInformation("Cleaning up specific tables: {Tables}", string.Join(", ", tableNames));
        var success = await DatabaseCleanup.TruncateSpecificTablesAsync(tableNames);

        if (!success)
        {
            throw new InvalidOperationException($"Failed to cleanup tables: {string.Join(", ", tableNames)}");
        }
    }

    /// <summary>
    /// Gets record counts for all SMS tables
    /// Useful for test verification
    /// </summary>
    protected async Task<Dictionary<string, int>> GetTableRecordCountsAsync()
    {
        return await DatabaseCleanup.GetTableRecordCountsAsync();
    }

    /// <summary>
    /// Verifies that specified tables have the expected record counts
    /// </summary>
    protected async Task<bool> VerifyRecordCountsAsync(Dictionary<string, int> expectedCounts)
    {
        var actualCounts = await GetTableRecordCountsAsync();

        foreach (var expected in expectedCounts)
        {
            if (!actualCounts.ContainsKey(expected.Key))
            {
                _logger.LogError("Table {TableName} not found in database", expected.Key);
                return false;
            }

            if (actualCounts[expected.Key] != expected.Value)
            {
                _logger.LogError("Table {TableName} has {ActualCount} records, expected {ExpectedCount}",
                    expected.Key, actualCounts[expected.Key], expected.Value);
                return false;
            }
        }

        return true;
    }

    #region Entity Creation Helpers - Same as ApplicationTestBase

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

    protected static string GenerateTestCode(string prefix)
    {
        var timestamp = DateTime.Now.ToString("HHmmss");
        var random = Random.Shared.Next(100, 999);
        return $"{prefix}-TEST-{timestamp}-{random}";
    }

    #endregion

    public virtual void Dispose()
    {
        ServiceProvider?.Dispose();
        GC.SuppressFinalize(this);
    }
}