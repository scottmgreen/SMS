using BenchmarkDotNet.Attributes;
using SMS_Application.Interfaces;
using SMS_Application.Messaging.Commands;
using SMS_Application.Messaging.Queries;
using SMS_Domain.Entities;
using SMS_Domain.Common;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace BenchmarkProject;

// ==============================================
// SMS DATABASE INTEGRATION BENCHMARKS
// ==============================================

/// <summary>
/// Benchmarks that test the full database integration stack
/// Including CQRS handlers, repositories, and actual database operations
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
[RankColumn]
public class SMSDatabaseBenchmarks
{
    private IMediator _mediator;
    private List<Hazard> _testHazards;
    private List<Report> _testReports;

    [Params(5, 10, 25)]
    public int BatchSize { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _mediator = ServiceProviderContainer.ServiceProvider.GetRequiredService<IMediator>();
        
        // Pre-create test entities for consistent benchmarking
        _testHazards = CreateBenchmarkHazards(50);
        _testReports = CreateBenchmarkReports(50);
    }

    [Benchmark]
    public async Task<BenchmarkResult> FullCRUDCycle_Hazard()
    {
        var stopwatch = Stopwatch.StartNew();
        var results = new BenchmarkResult();
        
        // CREATE
        var hazard = CreateBenchmarkHazard($"CRUD-HZ-{Guid.NewGuid().ToString("N")[..8]}");
        var createCommand = new CreateHazardCommand(hazard);
        var createResult = await _mediator.SendAsync(createCommand, CancellationToken.None);
        
        if (createResult.IsSuccess)
        {
            results.OperationsCompleted++;
            
            // READ
            var readQuery = new GetHazardByIdQuery((HazardID)hazard.Id);
            var readResult = await _mediator.SendAsync(readQuery, CancellationToken.None);
            
            if (readResult.IsSuccess)
            {
                results.OperationsCompleted++;
                
                // UPDATE
                var updatedHazard = readResult.Value;
                updatedHazard.Description = $"Updated at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}";
                var updateCommand = new UpdateHazardCommand(updatedHazard);
                var updateResult = await _mediator.SendAsync(updateCommand, CancellationToken.None);
                
                if (updateResult.IsSuccess)
                {
                    results.OperationsCompleted++;
                    
                    // DELETE
                    var deleteCommand = new DeleteHazardCommand((HazardID)hazard.Id);
                    var deleteResult = await _mediator.SendAsync(deleteCommand, CancellationToken.None);
                    
                    if (deleteResult.IsSuccess && deleteResult.Value)
                    {
                        results.OperationsCompleted++;
                    }
                }
            }
        }
        
        stopwatch.Stop();
        results.TotalDuration = stopwatch.Elapsed;
        return results;
    }

    [Benchmark]
    public async Task<BenchmarkResult> FullCRUDCycle_Report()
    {
        var stopwatch = Stopwatch.StartNew();
        var results = new BenchmarkResult();
        
        // CREATE
        var report = CreateBenchmarkReport($"CRUD-RPT-{Guid.NewGuid().ToString("N")[..8]}");
        var createCommand = new CreateReportCommand(report);
        var createResult = await _mediator.SendAsync(createCommand, CancellationToken.None);
        
        if (createResult.IsSuccess)
        {
            results.OperationsCompleted++;
            
            // READ
            var readQuery = new GetReportByIdQuery((ReportID)report.Id);
            var readResult = await _mediator.SendAsync(readQuery, CancellationToken.None);
            
            if (readResult.IsSuccess)
            {
                results.OperationsCompleted++;
                
                // UPDATE
                var updatedReport = readResult.Value;
                updatedReport.Description = $"Updated at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}";
                var updateCommand = new UpdateReportCommand(updatedReport);
                var updateResult = await _mediator.SendAsync(updateCommand, CancellationToken.None);
                
                if (updateResult.IsSuccess)
                {
                    results.OperationsCompleted++;
                    
                    // DELETE
                    var deleteCommand = new DeleteReportCommand((ReportID)report.Id);
                    var deleteResult = await _mediator.SendAsync(deleteCommand, CancellationToken.None);
                    
                    if (deleteResult.IsSuccess && deleteResult.Value)
                    {
                        results.OperationsCompleted++;
                    }
                }
            }
        }
        
        stopwatch.Stop();
        results.TotalDuration = stopwatch.Elapsed;
        return results;
    }

    [Benchmark]
    public async Task<int> BulkCreateOperations_Hazard()
    {
        var successCount = 0;
        var tasks = new List<Task<Result<Hazard>>>();
        
        for (int i = 0; i < BatchSize; i++)
        {
            var hazard = CreateBenchmarkHazard($"BULK-HZ-{Guid.NewGuid().ToString("N")[..8]}");
            var command = new CreateHazardCommand(hazard);
            tasks.Add(_mediator.SendAsync(command, CancellationToken.None));
        }
        
        var results = await Task.WhenAll(tasks);
        
        foreach (var result in results)
        {
            if (result.IsSuccess) successCount++;
        }
        
        // Cleanup - attempt to delete created records
        var cleanupTasks = new List<Task>();
        foreach (var result in results)
        {
            if (result.IsSuccess)
            {
                var deleteCommand = new DeleteHazardCommand((HazardID)result.Value.Id);
                cleanupTasks.Add(_mediator.SendAsync(deleteCommand, CancellationToken.None));
            }
        }
        
        await Task.WhenAll(cleanupTasks);
        
        return successCount;
    }

    [Benchmark]
    public async Task<int> BulkCreateOperations_Report()
    {
        var successCount = 0;
        var tasks = new List<Task<Result<Report>>>();
        
        for (int i = 0; i < BatchSize; i++)
        {
            var report = CreateBenchmarkReport($"BULK-RPT-{Guid.NewGuid().ToString("N")[..8]}");
            var command = new CreateReportCommand(report);
            tasks.Add(_mediator.SendAsync(command, CancellationToken.None));
        }
        
        var results = await Task.WhenAll(tasks);
        
        foreach (var result in results)
        {
            if (result.IsSuccess) successCount++;
        }
        
        // Cleanup - attempt to delete created records
        var cleanupTasks = new List<Task>();
        foreach (var result in results)
        {
            if (result.IsSuccess)
            {
                var deleteCommand = new DeleteReportCommand((ReportID)result.Value.Id);
                cleanupTasks.Add(_mediator.SendAsync(deleteCommand, CancellationToken.None));
            }
        }
        
        await Task.WhenAll(cleanupTasks);
        
        return successCount;
    }

    [Benchmark]
    public async Task<TimeSpan> DatabaseConnectionLatency()
    {
        var stopwatch = Stopwatch.StartNew();
        
        // Simple query to test connection latency
        var query = new GetAllHazardsQuery();
        await _mediator.SendAsync(query, CancellationToken.None);
        
        stopwatch.Stop();
        return stopwatch.Elapsed;
    }

    [Benchmark]
    public async Task<int> ConcurrentReads_Performance()
    {
        const int concurrentQueries = 10;
        var tasks = new List<Task<Result<List<Hazard>>>>();
        
        for (int i = 0; i < concurrentQueries; i++)
        {
            var query = new GetAllHazardsQuery();
            tasks.Add(_mediator.SendAsync(query, CancellationToken.None));
        }
        
        var results = await Task.WhenAll(tasks);
        return results.Count(r => r.IsSuccess);
    }

    [Benchmark]
    public async Task<QueryPerformanceResult> ComplexQueryPerformance()
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new QueryPerformanceResult();
        
        // Execute multiple different queries to test various performance aspects
        var hazardQuery = new GetAllHazardsQuery();
        var reportQuery = new GetAllReportsQuery();
        var investigationQuery = new GetAllInvestigationsQuery();
        
        var hazardTask = _mediator.SendAsync(hazardQuery, CancellationToken.None);
        var reportTask = _mediator.SendAsync(reportQuery, CancellationToken.None);
        var investigationTask = _mediator.SendAsync(investigationQuery, CancellationToken.None);
        
        var hazardResult = await hazardTask;
        var reportResult = await reportTask;
        var investigationResult = await investigationTask;
        
        result.HazardCount = hazardResult.IsSuccess ? hazardResult.Value?.Count ?? 0 : 0;
        result.ReportCount = reportResult.IsSuccess ? reportResult.Value?.Count ?? 0 : 0;
        result.InvestigationCount = investigationResult.IsSuccess ? investigationResult.Value?.Count ?? 0 : 0;
        result.SuccessfulQueries = (hazardResult.IsSuccess ? 1 : 0) + 
                                  (reportResult.IsSuccess ? 1 : 0) + 
                                  (investigationResult.IsSuccess ? 1 : 0);
        
        stopwatch.Stop();
        result.TotalDuration = stopwatch.Elapsed;
        
        return result;
    }

    private List<Hazard> CreateBenchmarkHazards(int count)
    {
        var hazards = new List<Hazard>();
        
        for (int i = 0; i < count; i++)
        {
            hazards.Add(CreateBenchmarkHazard($"BENCH-HZ-{i:D4}"));
        }
        
        return hazards;
    }

    private List<Report> CreateBenchmarkReports(int count)
    {
        var reports = new List<Report>();
        
        for (int i = 0; i < count; i++)
        {
            reports.Add(CreateBenchmarkReport($"BENCH-RPT-{i:D4}"));
        }
        
        return reports;
    }

    private Hazard CreateBenchmarkHazard(string id)
    {
        return new Hazard(new HazardID(id))
        {
            Code = id,
            Name = $"Benchmark Hazard {id}",
            Description = $"Performance test hazard for database benchmarking - {id}",
            ReportCode = $"RPT-{id}",
            ScoringPanelCode = "SP-BENCH",
            AverageScore = "3.2"
        };
    }

    private Report CreateBenchmarkReport(string id)
    {
        return new Report(new ReportID(id))
        {
            Code = id,
            Name = $"Benchmark Report {id}",
            Description = $"Performance test report for database benchmarking - {id}",
            Status = "Draft",
            Stage = "Benchmark"
        };
    }
}

// ==============================================
// BENCHMARK RESULT CLASSES
// ==============================================

public class BenchmarkResult
{
    public int OperationsCompleted { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public double OperationsPerSecond => OperationsCompleted > 0 && TotalDuration.TotalSeconds > 0 
        ? OperationsCompleted / TotalDuration.TotalSeconds 
        : 0;
}

public class QueryPerformanceResult
{
    public int HazardCount { get; set; }
    public int ReportCount { get; set; }
    public int InvestigationCount { get; set; }
    public int SuccessfulQueries { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public int TotalRecords => HazardCount + ReportCount + InvestigationCount;
}

// ==============================================
// SMS CACHING BENCHMARKS
// ==============================================

/// <summary>
/// Benchmarks to test caching scenarios and memory efficiency
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
[RankColumn]
public class SMSCachingBenchmarks
{
    private IMediator _mediator;
    private readonly Dictionary<string, object> _simpleCache = new();
    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, object> _concurrentCache = new();

    [Params(100, 500, 1000)]
    public int Iterations { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _mediator = ServiceProviderContainer.ServiceProvider.GetRequiredService<IMediator>();
    }

    [Benchmark]
    public async Task<int> CacheVsDatabase_HazardRetrieval()
    {
        var cacheHits = 0;
        var cacheKey = "all_hazards";
        
        for (int i = 0; i < Iterations; i++)
        {
            // Try cache first
            if (_simpleCache.TryGetValue(cacheKey, out var cachedValue))
            {
                cacheHits++;
            }
            else
            {
                // Cache miss - fetch from database
                var query = new GetAllHazardsQuery();
                var result = await _mediator.SendAsync(query, CancellationToken.None);
                
                if (result.IsSuccess)
                {
                    _simpleCache[cacheKey] = result.Value;
                }
            }
        }
        
        return cacheHits;
    }

    [Benchmark]
    public async Task<int> ConcurrentCache_Performance()
    {
        var cacheHits = 0;
        var tasks = new List<Task>();
        
        for (int i = 0; i < Iterations; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                var cacheKey = $"hazard_{i % 10}"; // Create cache contention with limited keys
                
                if (_concurrentCache.TryGetValue(cacheKey, out _))
                {
                    Interlocked.Increment(ref cacheHits);
                }
                else
                {
                    var query = new GetAllHazardsQuery();
                    var result = await _mediator.SendAsync(query, CancellationToken.None);
                    
                    if (result.IsSuccess)
                    {
                        _concurrentCache.TryAdd(cacheKey, result.Value);
                    }
                }
            }));
        }
        
        await Task.WhenAll(tasks);
        return cacheHits;
    }

    [Benchmark]
    public void CacheEviction_LRUSimulation()
    {
        const int maxCacheSize = 100;
        var cache = new Dictionary<string, (object Value, DateTime LastAccess)>();
        
        // Fill cache beyond capacity
        for (int i = 0; i < maxCacheSize * 2; i++)
        {
            var key = $"item_{i}";
            var value = $"cached_value_{i}";
            
            if (cache.Count >= maxCacheSize)
            {
                // Remove least recently used item
                var lruKey = cache
                    .OrderBy(kvp => kvp.Value.LastAccess)
                    .First()
                    .Key;
                cache.Remove(lruKey);
            }
            
            cache[key] = (value, DateTime.UtcNow);
        }
    }
}