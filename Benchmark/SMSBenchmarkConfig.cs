using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Validators;
using SMS_Domain.Entities;
using SMS_Domain.Common;

namespace BenchmarkProject;

// ==============================================
// SMS BENCHMARK CONFIGURATION
// ==============================================

/// <summary>
/// Custom benchmark configuration for SMS performance testing
/// Provides standardized settings for consistent benchmarking across all test classes
/// </summary>
public class SMSBenchmarkConfig : ManualConfig
{
    public SMSBenchmarkConfig()
    {
        // Add jobs for different scenarios
        AddJob(Job.Default
            .WithToolchain(InProcessEmitToolchain.Instance)
            .WithId("SMS-InProcess")
            .AsBaseline());

        AddJob(Job.Default
            .WithId("SMS-Standard"));

        // Configure exporters
        AddExporter(MarkdownExporter.GitHub);
        AddExporter(HtmlExporter.Default);

        // Configure loggers
        AddLogger(ConsoleLogger.Default);

        // Add custom columns
        AddColumn(StatisticColumn.Mean);
        AddColumn(StatisticColumn.Error);
        AddColumn(StatisticColumn.StdDev);
        AddColumn(StatisticColumn.Median);
        AddColumn(RankColumn.Arabic);
        AddColumn(BaselineRatioColumn.RatioMean);

        // Add validators but disable optimization validator for development
        AddValidator(ExecutionValidator.FailOnError);
        AddValidator(JitOptimizationsValidator.FailOnError);
        
        // Disable optimization validator to allow Debug builds during development
        WithOptions(ConfigOptions.DisableOptimizationsValidator);

        // Set summary style
        WithSummaryStyle(SummaryStyle.Default.WithRatioStyle(RatioStyle.Trend));
    }
}

// ==============================================
// SMS BENCHMARK CATEGORIES
// ==============================================

/// <summary>
/// Benchmark categories for organized testing
/// </summary>
public static class SMSBenchmarkCategories
{
    public const string MEDIATOR = "Mediator";
    public const string DATABASE = "Database"; 
    public const string ENTITY = "Entity";
    public const string MEMORY = "Memory";
    public const string CONCURRENCY = "Concurrency";
    public const string SERIALIZATION = "Serialization";
    public const string CACHING = "Caching";
    public const string LOGGING = "Logging";
    public const string VALUEOBJECT = "ValueObject";
    public const string CRUD = "CRUD";
}

// ==============================================
// SMS BENCHMARK RUNNERS
// ==============================================

/// <summary>
/// Convenience class for running specific benchmark categories
/// </summary>
public static class SMSBenchmarkRunner
{
    /// <summary>
    /// Run all SMS benchmarks with default configuration
    /// </summary>
    public static Summary[] RunAll()
    {
        var config = new SMSBenchmarkConfig();
        return BenchmarkDotNet.Running.BenchmarkRunner.Run(typeof(Program).Assembly, config);
    }

    /// <summary>
    /// Run only mediator-related benchmarks
    /// </summary>
    public static Summary RunMediatorBenchmarks()
    {
        var config = new SMSBenchmarkConfig();
        return BenchmarkDotNet.Running.BenchmarkRunner.Run<SMSMediatorBenchmarks>(config);
    }

    /// <summary>
    /// Run only database-related benchmarks
    /// </summary>
    public static Summary RunDatabaseBenchmarks()
    {
        var config = new SMSBenchmarkConfig();
        return BenchmarkDotNet.Running.BenchmarkRunner.Run<SMSDatabaseBenchmarks>(config);
    }

    /// <summary>
    /// Run only entity-related benchmarks
    /// </summary>
    public static Summary RunEntityBenchmarks()
    {
        var config = new SMSBenchmarkConfig();
        return BenchmarkDotNet.Running.BenchmarkRunner.Run<SMSEntityBenchmarks>(config);
    }

    /// <summary>
    /// Run memory and allocation benchmarks
    /// </summary>
    public static Summary RunMemoryBenchmarks()
    {
        var config = new SMSBenchmarkConfig();
        return BenchmarkDotNet.Running.BenchmarkRunner.Run<SMSMemoryBenchmarks>(config);
    }

    /// <summary>
    /// Run concurrency benchmarks
    /// </summary>
    public static Summary RunConcurrencyBenchmarks()
    {
        var config = new SMSBenchmarkConfig();
        return BenchmarkDotNet.Running.BenchmarkRunner.Run<SMSConcurrencyBenchmarks>(config);
    }

    /// <summary>
    /// Run lightweight benchmarks (good for CI/CD)
    /// </summary>
    public static Summary RunLightweightBenchmarks()
    {
        var config = new SMSBenchmarkConfig();
        
        // Add a fast job for CI scenarios
        config.AddJob(Job.Dry.WithId("SMS-Fast"));
        
        return BenchmarkDotNet.Running.BenchmarkRunner.Run<SMSEntityBenchmarks>(config);
    }
}

// ==============================================
// SMS BENCHMARK UTILITIES
// ==============================================

/// <summary>
/// Utility methods for SMS benchmarking
/// </summary>
public static class SMSBenchmarkUtilities
{
    /// <summary>
    /// Generate test data for benchmarks
    /// </summary>
    public static List<T> GenerateTestData<T>(int count, Func<int, T> factory)
    {
        var list = new List<T>(count);
        for (int i = 0; i < count; i++)
        {
            list.Add(factory(i));
        }
        return list;
    }

    /// <summary>
    /// Create a unique identifier for benchmark runs
    /// </summary>
    public static string CreateBenchmarkId()
    {
        return $"SMS-BENCH-{DateTime.UtcNow:yyyyMMdd-HHmmss}-{Guid.NewGuid().ToString("N")[..8]}";
    }

    /// <summary>
    /// Format benchmark results for logging
    /// </summary>
    public static string FormatResults(string benchmarkName, TimeSpan duration, int operations)
    {
        var opsPerSecond = operations > 0 && duration.TotalSeconds > 0 
            ? operations / duration.TotalSeconds 
            : 0;
            
        return $"[{benchmarkName}] Duration: {duration.TotalMilliseconds:F2}ms, " +
               $"Operations: {operations}, Ops/sec: {opsPerSecond:F2}";
    }

    /// <summary>
    /// Validate benchmark environment
    /// </summary>
    public static bool ValidateEnvironment()
    {
        try
        {
            // Check if required services are available
            var serviceProvider = ServiceProviderContainer.ServiceProvider;
            var mediator = serviceProvider?.GetService(typeof(SMS_Application.Interfaces.IMediator));
            
            return mediator != null;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Create benchmark summary
    /// </summary>
    public static BenchmarkSummary CreateSummary(Dictionary<string, object> results)
    {
        return new BenchmarkSummary
        {
            Timestamp = DateTime.UtcNow,
            TotalBenchmarks = results.Count,
            Results = results,
            Environment = GetEnvironmentInfo()
        };
    }

    private static Dictionary<string, string> GetEnvironmentInfo()
    {
        return new Dictionary<string, string>
        {
            ["MachineName"] = Environment.MachineName,
            ["OSVersion"] = Environment.OSVersion.ToString(),
            ["ProcessorCount"] = Environment.ProcessorCount.ToString(),
            ["Is64BitOS"] = Environment.Is64BitOperatingSystem.ToString(),
            ["WorkingSet"] = Environment.WorkingSet.ToString(),
            [".NETVersion"] = Environment.Version.ToString()
        };
    }
}

// ==============================================
// BENCHMARK SUMMARY CLASSES
// ==============================================

public class BenchmarkSummary
{
    public DateTime Timestamp { get; set; }
    public int TotalBenchmarks { get; set; }
    public Dictionary<string, object> Results { get; set; } = new();
    public Dictionary<string, string> Environment { get; set; } = new();
}

// ==============================================
// SMS BENCHMARK COMPARISON TESTS
// ==============================================

/// <summary>
/// Comparative benchmarks to test different implementation approaches
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
[RankColumn]
public class SMSComparisonBenchmarks
{
    private List<Hazard> _hazards;
    
    [GlobalSetup]
    public void Setup()
    {
        _hazards = SMSBenchmarkUtilities.GenerateTestData(1000, i => 
            new Hazard(new HazardID($"COMP-HZ-{i:D6}"))
            {
                Code = $"COMP-HZ-{i:D6}",
                Name = $"Comparison Test Hazard {i}",
                Description = $"Testing different implementation approaches - iteration {i}",
                ReportCode = $"CRPT-{i:D6}",
                ScoringPanelCode = $"SP-{i % 10:D2}",
                AverageScore = (1.0 + (i % 5)).ToString("F1")
            });
    }

    [Benchmark(Baseline = true)]
    public int LinearSearch_FindHazard()
    {
        var targetCode = "COMP-HZ-000500";
        
        for (int i = 0; i < _hazards.Count; i++)
        {
            if (_hazards[i].Code == targetCode)
            {
                return i;
            }
        }
        
        return -1;
    }

    [Benchmark]
    public int BinarySearch_FindHazard()
    {
        var targetCode = "COMP-HZ-000500";
        
        // Note: This assumes the list is sorted by Code
        var sortedList = _hazards.OrderBy(h => h.Code).ToList();
        
        int left = 0, right = sortedList.Count - 1;
        
        while (left <= right)
        {
            int mid = (left + right) / 2;
            int comparison = string.Compare(sortedList[mid].Code, targetCode);
            
            if (comparison == 0)
                return mid;
            else if (comparison < 0)
                left = mid + 1;
            else
                right = mid - 1;
        }
        
        return -1;
    }

    [Benchmark]
    public int HashSetLookup_FindHazard()
    {
        var targetCode = "COMP-HZ-000500";
        var hashSet = _hazards.ToHashSet();
        
        var target = _hazards.FirstOrDefault(h => h.Code == targetCode);
        return hashSet.Contains(target) ? 1 : -1;
    }

    [Benchmark]
    public int DictionaryLookup_FindHazard()
    {
        var targetCode = "COMP-HZ-000500";
        var dictionary = _hazards.ToDictionary(h => h.Code);
        
        return dictionary.TryGetValue(targetCode, out _) ? 1 : -1;
    }
}