namespace BenchmarkProject;

using System.Net.NetworkInformation;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using BenchmarkDotNet.Reports;

using SMS_Application.Configuration;
using SMS_Application.Interfaces;
using SMS_Application.Messaging;
using SMS_Application.Services;
using SMS_Application.Messaging.Commands;
using SMS_Application.Messaging.Queries;

using SMS_Domain.Common;
using SMS_Domain.Entities;
using SMS_Domain.Interfaces;

using SMS_Infrastructure.Configuration;

using SMS_Shared;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SMS_Shared.Configuration;

public static class ServiceProviderContainer
{
    public static void Initialize()
    {
        ServiceCollection services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
              .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
              .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
              .Build();

        services.AddSingleton<IConfiguration>(configuration);

        DependencyInjection.Initialize(services, configuration);

        ServiceProvider = services.BuildServiceProvider();
    }

    public static IServiceProvider ServiceProvider { get; private set; }
}

public static class DependencyInjection
{
    public static IServiceCollection Initialize(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSharedServices(configuration);
        services.AddInfrastructureServices(configuration);
        services.AddApplicationServices();

        return services;
    }
}

public class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("?? SMS Performance Benchmarking Suite");
        Console.WriteLine("=====================================");
        
        // Initialize SMS services
        Console.WriteLine("?? Initializing SMS services...");
        ServiceProviderContainer.Initialize();
        
        // Validate environment
        if (!SMSBenchmarkUtilities.ValidateEnvironment())
        {
            Console.WriteLine("Environment validation failed! Please check your configuration.");
            return;
        }
        
        Console.WriteLine("SMS services initialized successfully!");
        
        // Parse command line arguments for benchmark selection
        var benchmarkType = GetBenchmarkTypeFromArgs(args);
        
        Console.WriteLine($"?? Running benchmark type: {benchmarkType}");
        Console.WriteLine("?? This may take several minutes depending on the benchmark scope...");
        Console.WriteLine();
        
        var config = new SMSBenchmarkConfig();
        
        try
        {
            Summary summary = benchmarkType switch
            {
                BenchmarkType.All => SMSBenchmarkRunner.RunAll().FirstOrDefault(),
                BenchmarkType.Mediator => BenchmarkRunner.Run<SMSMediatorBenchmarks>(config),
                BenchmarkType.Database => BenchmarkRunner.Run<SMSDatabaseBenchmarks>(config),
                BenchmarkType.Entity => BenchmarkRunner.Run<SMSEntityBenchmarks>(config),
                BenchmarkType.Memory => BenchmarkRunner.Run<SMSMemoryBenchmarks>(config),
                BenchmarkType.Concurrency => BenchmarkRunner.Run<SMSConcurrencyBenchmarks>(config),
                BenchmarkType.Caching => BenchmarkRunner.Run<SMSCachingBenchmarks>(config),
                BenchmarkType.Serialization => BenchmarkRunner.Run<SMSSerializationBenchmarks>(config),
                BenchmarkType.ValueObject => BenchmarkRunner.Run<SMSValueObjectBenchmarks>(config),
                BenchmarkType.Comparison => BenchmarkRunner.Run<SMSComparisonBenchmarks>(config),
                BenchmarkType.Lightweight => RunLightweightBenchmarks(config),
                _ => BenchmarkRunner.Run(typeof(Program).Assembly, config).FirstOrDefault()
            };
            
            Console.WriteLine();
            Console.WriteLine("?? SMS Benchmarking Complete!");
            Console.WriteLine("?? Results have been exported to the BenchmarkDotNet.Artifacts folder");
            Console.WriteLine("?? Check for HTML, Markdown, CSV, and JSON reports");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Benchmark execution failed: {ex.Message}");
            Console.WriteLine($"?? Stack trace: {ex.StackTrace}");
        }
    }
    
    private static BenchmarkType GetBenchmarkTypeFromArgs(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("?  No benchmark type specified. Running all benchmarks.");
            Console.WriteLine("?? Available options: --mediator, --database, --entity, --memory, --concurrency, --caching, --serialization, --valueobject, --comparison, --lightweight");
            return BenchmarkType.All;
        }
        
        var arg = args[0].ToLowerInvariant();
        
        return arg switch
        {
            "--mediator" or "-m" => BenchmarkType.Mediator,
            "--database" or "-d" => BenchmarkType.Database,
            "--entity" or "-e" => BenchmarkType.Entity,
            "--memory" or "-mem" => BenchmarkType.Memory,
            "--concurrency" or "-c" => BenchmarkType.Concurrency,
            "--caching" or "-cache" => BenchmarkType.Caching,
            "--serialization" or "-s" => BenchmarkType.Serialization,
            "--valueobject" or "-vo" => BenchmarkType.ValueObject,
            "--comparison" or "-comp" => BenchmarkType.Comparison,
            "--lightweight" or "-l" => BenchmarkType.Lightweight,
            "--all" or "-a" => BenchmarkType.All,
            _ => BenchmarkType.All
        };
    }
    
    private static Summary RunLightweightBenchmarks(IConfig config)
    {
        Console.WriteLine("????? Running lightweight benchmarks (suitable for CI/CD)...");
        
        // Run a subset of benchmarks with reduced iterations
        var lightConfig = ManualConfig.Create(config)
            .AddJob(Job.Dry.WithId("SMS-Lightweight"));
            
        return BenchmarkRunner.Run<SMSEntityBenchmarks>(lightConfig);
    }
}

public enum BenchmarkType
{
    All,
    Mediator,
    Database,
    Entity,
    Memory,
    Concurrency,
    Caching,
    Serialization,
    ValueObject,
    Comparison,
    Lightweight
}

// ==============================================
// SMS MEDIATOR PERFORMANCE BENCHMARKS
// ==============================================

[MemoryDiagnoser]
[SimpleJob]
[RankColumn]
[BenchmarkCategory(SMSBenchmarkCategories.MEDIATOR)]
public class SMSMediatorBenchmarks
{
    private IMediator _mediator;
    private List<HazardID> _hazardIds;
    private List<ReportID> _reportIds;
    private List<InvestigationID> _investigationIds;

    [Params(1, 10, 100)]
    public int Iterations { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _mediator = ServiceProviderContainer.ServiceProvider.GetRequiredService<IMediator>();
        
        // Pre-generate test IDs to avoid ID generation overhead in benchmarks
        _hazardIds = Enumerable.Range(1, 1000).Select(i => new HazardID($"HZ-{i:D4}")).ToList();
        _reportIds = Enumerable.Range(1, 1000).Select(i => new ReportID($"RPT-{i:D4}")).ToList();
        _investigationIds = Enumerable.Range(1, 1000).Select(i => new InvestigationID($"INV-{i:D4}")).ToList();
    }

    [Benchmark]
    public async Task<int> GetAllHazards_Bulk()
    {
        int successCount = 0;
        var query = new GetAllHazardsQuery();
        
        for (int i = 0; i < Iterations; i++)
        {
            var result = await _mediator.SendAsync(query, CancellationToken.None);
            if (result.IsSuccess) successCount++;
        }
        
        return successCount;
    }

    [Benchmark]
    public async Task<int> GetHazardById_Bulk()
    {
        int successCount = 0;
        
        for (int i = 0; i < Iterations; i++)
        {
            var hazardId = _hazardIds[i % _hazardIds.Count];
            var query = new GetHazardByIdQuery(hazardId);
            var result = await _mediator.SendAsync(query, CancellationToken.None);
            if (result.IsSuccess) successCount++;
        }
        
        return successCount;
    }

    [Benchmark]
    public async Task<int> GetAllReports_Bulk()
    {
        int successCount = 0;
        var query = new GetAllReportsQuery();
        
        for (int i = 0; i < Iterations; i++)
        {
            var result = await _mediator.SendAsync(query, CancellationToken.None);
            if (result.IsSuccess) successCount++;
        }
        
        return successCount;
    }

    [Benchmark]
    public async Task<int> GetAllInvestigations_Bulk()
    {
        int successCount = 0;
        var query = new GetAllInvestigationsQuery();
        
        for (int i = 0; i < Iterations; i++)
        {
            var result = await _mediator.SendAsync(query, CancellationToken.None);
            if (result.IsSuccess) successCount++;
        }
        
        return successCount;
    }
}

// ==============================================
// SMS COMMAND PERFORMANCE BENCHMARKS
// ==============================================

[MemoryDiagnoser]
[SimpleJob]
[RankColumn]
[BenchmarkCategory(SMSBenchmarkCategories.CRUD)]
public class SMSCommandBenchmarks
{
    private IMediator _mediator;

    [Params(1, 5, 10)]
    public int Iterations { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _mediator = ServiceProviderContainer.ServiceProvider.GetRequiredService<IMediator>();
    }

    [Benchmark]
    public async Task<int> CreateHazard_Bulk()
    {
        int successCount = 0;
        
        for (int i = 0; i < Iterations; i++)
        {
            var hazard = CreateTestHazard($"BENCH-HZ-{Guid.NewGuid().ToString("N")[..8]}");
            var command = new CreateHazardCommand(hazard);
            var result = await _mediator.SendAsync(command, CancellationToken.None);
            if (result.IsSuccess) successCount++;
        }
        
        return successCount;
    }

    [Benchmark]
    public async Task<int> CreateReport_Bulk()
    {
        int successCount = 0;
        
        for (int i = 0; i < Iterations; i++)
        {
            var report = CreateTestReport($"BENCH-RPT-{Guid.NewGuid().ToString("N")[..8]}");
            var command = new CreateReportCommand(report);
            var result = await _mediator.SendAsync(command, CancellationToken.None);
            if (result.IsSuccess) successCount++;
        }
        
        return successCount;
    }

    private Hazard CreateTestHazard(string idValue)
    {
        var hazard = new Hazard(new HazardID(idValue))
        {
            Code = idValue,
            Name = $"Benchmark Test Hazard {idValue}",
            Description = $"This is a benchmark test hazard created for performance testing purposes",
            ReportCode = $"RPT-{idValue}",
            ScoringPanelCode = "SP-001",
            AverageScore = "3.5"
        };
        return hazard;
    }

    private Report CreateTestReport(string idValue)
    {
        var report = new Report(new ReportID(idValue))
        {
            Code = idValue,
            Name = $"Benchmark Test Report {idValue}",
            Description = $"This is a benchmark test report created for performance testing purposes",
            Status = "Draft",
            Stage = "Initial"
        };
        return report;
    }
}

// ==============================================
// SMS CONCURRENT OPERATIONS BENCHMARKS
// ==============================================

[MemoryDiagnoser]
[SimpleJob]
[RankColumn]
[BenchmarkCategory(SMSBenchmarkCategories.CONCURRENCY)]
public class SMSConcurrencyBenchmarks
{
    private IMediator _mediator;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(10, 10); // Limit concurrent operations

    [Params(5, 10, 20)]
    public int ConcurrentCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _mediator = ServiceProviderContainer.ServiceProvider.GetRequiredService<IMediator>();
    }

    [Benchmark]
    public async Task<int> ConcurrentHazardQueries()
    {
        var tasks = new List<Task<Result<List<Hazard>>>>();
        
        for (int i = 0; i < ConcurrentCount; i++)
        {
            tasks.Add(ExecuteHazardQueryWithSemaphore());
        }
        
        var results = await Task.WhenAll(tasks);
        return results.Count(r => r.IsSuccess);
    }

    [Benchmark]
    public async Task<int> ConcurrentReportQueries()
    {
        var tasks = new List<Task<Result<List<Report>>>>();
        
        for (int i = 0; i < ConcurrentCount; i++)
        {
            tasks.Add(ExecuteReportQueryWithSemaphore());
        }
        
        var results = await Task.WhenAll(tasks);
        return results.Count(r => r.IsSuccess);
    }

    private async Task<Result<List<Hazard>>> ExecuteHazardQueryWithSemaphore()
    {
        await _semaphore.WaitAsync();
        try
        {
            var query = new GetAllHazardsQuery();
            return await _mediator.SendAsync(query, CancellationToken.None);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<Result<List<Report>>> ExecuteReportQueryWithSemaphore()
    {
        await _semaphore.WaitAsync();
        try
        {
            var query = new GetAllReportsQuery();
            return await _mediator.SendAsync(query, CancellationToken.None);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _semaphore?.Dispose();
    }
}

// ==============================================
// SMS MEMORY ALLOCATION BENCHMARKS
// ==============================================

[MemoryDiagnoser]
[SimpleJob]
[RankColumn]
[BenchmarkCategory(SMSBenchmarkCategories.MEMORY)]
public class SMSMemoryBenchmarks
{
    private IMediator _mediator;

    [GlobalSetup]
    public void Setup()
    {
        _mediator = ServiceProviderContainer.ServiceProvider.GetRequiredService<IMediator>();
    }

    [Benchmark]
    public async Task<int> LargeHazardCollection_MemoryUsage()
    {
        // Test memory allocation patterns for large collections
        var query = new GetAllHazardsQuery();
        var result = await _mediator.SendAsync(query, CancellationToken.None);
        
        if (result.IsSuccess && result.Value != null)
        {
            // Force enumeration to measure actual memory impact
            var processedCount = result.Value
                .Where(h => !string.IsNullOrEmpty(h.Name))
                .Select(h => h.Name)
                .ToList()
                .Count;
            
            return processedCount;
        }
        
        return 0;
    }

    [Benchmark]
    public async Task<int> LargeReportCollection_MemoryUsage()
    {
        var query = new GetAllReportsQuery();
        var result = await _mediator.SendAsync(query, CancellationToken.None);
        
        if (result.IsSuccess && result.Value != null)
        {
            var processedCount = result.Value
                .Where(r => r.Status == "Draft")
                .Select(r => r.Name)
                .ToList()
                .Count;
            
            return processedCount;
        }
        
        return 0;
    }

    [Benchmark]
    public void EntityCreation_AllocationPattern()
    {
        // Test object allocation patterns
        var hazards = new List<Hazard>(1000);
        
        for (int i = 0; i < 1000; i++)
        {
            var hazard = new Hazard(new HazardID($"ALLOC-HZ-{i:D4}"))
            {
                Code = $"ALLOC-HZ-{i:D4}",
                Name = $"Allocation Test Hazard {i}",
                Description = "Testing memory allocation patterns for hazard creation",
                ReportCode = $"RPT-ALLOC-{i:D4}",
                ScoringPanelCode = "SP-MEM-TEST",
                AverageScore = "2.0"
            };
            hazards.Add(hazard);
        }
        
        // Force GC to measure actual allocations
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }
}

// ==============================================
// SMS LOGGING PERFORMANCE BENCHMARKS
// ==============================================

[MemoryDiagnoser]
[SimpleJob]
[RankColumn]
[BenchmarkCategory(SMSBenchmarkCategories.LOGGING)]
public class SMSLoggingBenchmarks
{
    private readonly ILoggerFactory _loggerFactory = LoggerFactory.Create(builder =>
    {
        builder.AddConsole().AddFakeLogger().SetMinimumLevel(LogLevel.Information);
    });

    private readonly ILogger<SMSLoggingBenchmarks> _logger;
    private const string LogMessageTemplate = "SMS Operation: {Operation} completed for Entity {EntityId} with Status {Status} in {Duration}ms";

    [Params(100, 1000, 5000)]
    public int LogCount { get; set; }

    public SMSLoggingBenchmarks()
    {
        _logger = new Logger<SMSLoggingBenchmarks>(_loggerFactory);
    }

    [Benchmark]
    public void StructuredLogging_Performance()
    {
        for (int i = 0; i < LogCount; i++)
        {
            _logger.LogInformation(LogMessageTemplate, 
                "GetHazard", 
                $"HZ-{i:D4}", 
                "Success", 
                Random.Shared.Next(1, 100));
        }
    }

    [Benchmark]
    public void ConditionalLogging_Performance()
    {
        for (int i = 0; i < LogCount; i++)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation(LogMessageTemplate,
                    "GetReport",
                    $"RPT-{i:D4}",
                    "Success",
                    Random.Shared.Next(1, 100));
            }
        }
    }

    [Benchmark]
    public void HighPerformanceLogging_Performance()
    {
        // Using LoggerMessage.Define for high-performance logging
        var logAction = LoggerMessage.Define<string, string, string, int>(
            LogLevel.Information,
            new EventId(1, "SMSOperation"),
            LogMessageTemplate);

        for (int i = 0; i < LogCount; i++)
        {
            logAction(_logger, "GetInvestigation", $"INV-{i:D4}", "Success", Random.Shared.Next(1, 100), null);
        }
    }
}

// ==============================================
// SMS REPOSITORY PERFORMANCE BENCHMARKS  
// ==============================================

[MemoryDiagnoser]
[SimpleJob]
[RankColumn]
[BenchmarkCategory(SMSBenchmarkCategories.DATABASE)]
public class SMSRepositoryBenchmarks
{
    private IMediator _mediator;

    [Params(10, 50, 100)]
    public int QueryCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _mediator = ServiceProviderContainer.ServiceProvider.GetRequiredService<IMediator>();
    }

    [Benchmark]
    public async Task<int> BulkHazardRetrieval_Performance()
    {
        int totalRecords = 0;
        var query = new GetAllHazardsQuery();

        for (int i = 0; i < QueryCount; i++)
        {
            var result = await _mediator.SendAsync(query, CancellationToken.None);
            if (result.IsSuccess && result.Value != null)
            {
                totalRecords += result.Value.Count;
            }
        }

        return totalRecords;
    }

    [Benchmark]
    public async Task<int> BulkReportRetrieval_Performance()
    {
        int totalRecords = 0;
        var query = new GetAllReportsQuery();

        for (int i = 0; i < QueryCount; i++)
        {
            var result = await _mediator.SendAsync(query, CancellationToken.None);
            if (result.IsSuccess && result.Value != null)
            {
                totalRecords += result.Value.Count;
            }
        }

        return totalRecords;
    }

    [Benchmark]
    public async Task<TimeSpan> MediatorResponseTime_Measurement()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        var query = new GetAllHazardsQuery();
        await _mediator.SendAsync(query, CancellationToken.None);
        
        stopwatch.Stop();
        return stopwatch.Elapsed;
    }
}

// ==============================================
// SMS STRESS TEST BENCHMARKS
// ==============================================

[MemoryDiagnoser]
[SimpleJob]
[RankColumn]
[BenchmarkCategory(SMSBenchmarkCategories.CONCURRENCY)]
public class SMSStressBenchmarks
{
    private IMediator _mediator;

    [Params(100, 500, 1000)]
    public int OperationCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _mediator = ServiceProviderContainer.ServiceProvider.GetRequiredService<IMediator>();
    }

    [Benchmark]
    public async Task<int> HighVolumeOperations_StressTest()
    {
        var successCount = 0;
        var tasks = new List<Task>();

        for (int i = 0; i < OperationCount; i++)
        {
            var taskType = i % 3; // Rotate between different operation types
            
            switch (taskType)
            {
                case 0:
                    tasks.Add(ExecuteHazardQueryAsync().ContinueWith(t => 
                    {
                        if (t.Result.IsSuccess) Interlocked.Increment(ref successCount);
                    }));
                    break;
                case 1:
                    tasks.Add(ExecuteReportQueryAsync().ContinueWith(t => 
                    {
                        if (t.Result.IsSuccess) Interlocked.Increment(ref successCount);
                    }));
                    break;
                case 2:
                    tasks.Add(ExecuteInvestigationQueryAsync().ContinueWith(t => 
                    {
                        if (t.Result.IsSuccess) Interlocked.Increment(ref successCount);
                    }));
                    break;
            }
        }

        await Task.WhenAll(tasks);
        return successCount;
    }

    private async Task<Result<List<Hazard>>> ExecuteHazardQueryAsync()
    {
        var query = new GetAllHazardsQuery();
        return await _mediator.SendAsync(query, CancellationToken.None);
    }

    private async Task<Result<List<Report>>> ExecuteReportQueryAsync()
    {
        var query = new GetAllReportsQuery();
        return await _mediator.SendAsync(query, CancellationToken.None);
    }

    private async Task<Result<List<Investigation>>> ExecuteInvestigationQueryAsync()
    {
        var query = new GetAllInvestigationsQuery();
        return await _mediator.SendAsync(query, CancellationToken.None);
    }
}

// ==============================================
// FAKE LOGGER INFRASTRUCTURE
// ==============================================

public static class FakeLoggerExtensions
{
    public static ILoggingBuilder AddFakeLogger(this ILoggingBuilder loggingBuilder)
    {
        loggingBuilder.Services.AddSingleton<ILoggerProvider, FakeLoggerProvider>();
        return loggingBuilder;
    }
}

public class FakeLogger : ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return new FakeDisposable();
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel != LogLevel.None;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        // No-op for performance testing
    }
}

public class FakeDisposable : IDisposable
{
    public void Dispose()
    {
        // No-op
    }
}

public class FakeLoggerProvider : ILoggerProvider
{
    public void Dispose()
    {
        // No-op
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new FakeLogger();
    }
}
