using BenchmarkDotNet.Attributes;
using SMS_Domain.Entities;
using SMS_Domain.Common;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BenchmarkProject;

// ==============================================
// SMS ENTITY CREATION & MANIPULATION BENCHMARKS
// ==============================================

/// <summary>
/// Benchmarks focused on SMS entity creation, manipulation, and collection operations
/// Tests the performance characteristics of core domain entities
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
[RankColumn]
public class SMSEntityBenchmarks
{
    [Params(100, 1000, 5000)]
    public int EntityCount { get; set; }

    [Benchmark]
    public List<Hazard> CreateHazardCollection()
    {
        var hazards = new List<Hazard>(EntityCount);
        
        for (int i = 0; i < EntityCount; i++)
        {
            var hazard = new Hazard(new HazardID($"PERF-HZ-{i:D6}"))
            {
                Code = $"PERF-HZ-{i:D6}",
                Name = $"Performance Test Hazard {i}",
                Description = $"This hazard was created for performance testing iteration {i}",
                ReportCode = $"RPT-{i:D6}",
                ScoringPanelCode = $"SP-{i % 10:D2}",
                AverageScore = (2.5 + (i % 3)).ToString("F1")
            };
            hazards.Add(hazard);
        }
        
        return hazards;
    }

    [Benchmark]
    public List<Report> CreateReportCollection()
    {
        var reports = new List<Report>(EntityCount);
        
        for (int i = 0; i < EntityCount; i++)
        {
            var report = new Report(new ReportID($"PERF-RPT-{i:D6}"))
            {
                Code = $"PERF-RPT-{i:D6}",
                Name = $"Performance Test Report {i}",
                Description = $"This report was created for performance testing iteration {i}",
                Status = i % 2 == 0 ? "Draft" : "Published",
                Stage = i % 3 == 0 ? "Initial" : "Review"
            };
            reports.Add(report);
        }
        
        return reports;
    }

    [Benchmark]
    public ConcurrentBag<Hazard> CreateConcurrentHazardCollection()
    {
        var hazards = new ConcurrentBag<Hazard>();
        
        Parallel.For(0, EntityCount, i =>
        {
            var hazard = new Hazard(new HazardID($"CONC-HZ-{i:D6}"))
            {
                Code = $"CONC-HZ-{i:D6}",
                Name = $"Concurrent Hazard {i}",
                Description = $"Concurrent creation test - iteration {i}",
                ReportCode = $"CRPT-{i:D6}",
                ScoringPanelCode = "SP-CONC",
                AverageScore = "3.0"
            };
            hazards.Add(hazard);
        });
        
        return hazards;
    }

    [Benchmark]
    public Dictionary<HazardID, Hazard> CreateHazardLookupDictionary()
    {
        var hazardDict = new Dictionary<HazardID, Hazard>(EntityCount);
        
        for (int i = 0; i < EntityCount; i++)
        {
            var hazardId = new HazardID($"LOOKUP-HZ-{i:D6}");
            var hazard = new Hazard(hazardId)
            {
                Code = $"LOOKUP-HZ-{i:D6}",
                Name = $"Lookup Test Hazard {i}",
                Description = "Dictionary lookup performance test",
                ReportCode = $"LRPT-{i:D6}",
                ScoringPanelCode = "SP-LOOKUP",
                AverageScore = "2.8"
            };
            hazardDict.Add(hazardId, hazard);
        }
        
        return hazardDict;
    }

    [Benchmark]
    public int EntityPropertyAccess()
    {
        var hazards = CreateHazardCollection();
        int totalLength = 0;
        
        foreach (var hazard in hazards)
        {
            // Test property access performance
            totalLength += hazard.Code?.Length ?? 0;
            totalLength += hazard.Name?.Length ?? 0;
            totalLength += hazard.Description?.Length ?? 0;
            totalLength += hazard.ReportCode?.Length ?? 0;
        }
        
        return totalLength;
    }

    [Benchmark]
    public List<Hazard> EntityFiltering_LINQ()
    {
        var hazards = CreateHazardCollection();
        
        return hazards
            .Where(h => !string.IsNullOrEmpty(h.Name))
            .Where(h => h.AverageScore != null && double.Parse(h.AverageScore) > 2.0)
            .OrderBy(h => h.Name)
            .ToList();
    }

    [Benchmark]
    public List<Hazard> EntityFiltering_ForLoop()
    {
        var hazards = CreateHazardCollection();
        var filtered = new List<Hazard>();
        
        for (int i = 0; i < hazards.Count; i++)
        {
            var hazard = hazards[i];
            if (!string.IsNullOrEmpty(hazard.Name) && 
                hazard.AverageScore != null && 
                double.Parse(hazard.AverageScore) > 2.0)
            {
                filtered.Add(hazard);
            }
        }
        
        // Simple bubble sort for comparison
        for (int i = 0; i < filtered.Count - 1; i++)
        {
            for (int j = 0; j < filtered.Count - i - 1; j++)
            {
                if (string.Compare(filtered[j].Name, filtered[j + 1].Name) > 0)
                {
                    (filtered[j], filtered[j + 1]) = (filtered[j + 1], filtered[j]);
                }
            }
        }
        
        return filtered;
    }
}

// ==============================================
// SMS VALUE OBJECT BENCHMARKS
// ==============================================

/// <summary>
/// Benchmarks for SMS value objects (IDs) to test creation and comparison performance
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
[RankColumn]
public class SMSValueObjectBenchmarks
{
    [Params(1000, 10000, 50000)]
    public int IdCount { get; set; }

    [Benchmark]
    public List<HazardID> CreateHazardIds()
    {
        var ids = new List<HazardID>(IdCount);
        
        for (int i = 0; i < IdCount; i++)
        {
            ids.Add(new HazardID($"HZ-{i:D8}"));
        }
        
        return ids;
    }

    [Benchmark]
    public List<ReportID> CreateReportIds()
    {
        var ids = new List<ReportID>(IdCount);
        
        for (int i = 0; i < IdCount; i++)
        {
            ids.Add(new ReportID($"RPT-{i:D8}"));
        }
        
        return ids;
    }

    [Benchmark]
    public HashSet<HazardID> CreateUniqueHazardIdSet()
    {
        var idSet = new HashSet<HazardID>(IdCount);
        
        for (int i = 0; i < IdCount; i++)
        {
            idSet.Add(new HazardID($"SET-HZ-{i:D8}"));
        }
        
        return idSet;
    }

    [Benchmark]
    public int HazardIdComparisons()
    {
        var ids = CreateHazardIds();
        int equalCount = 0;
        
        for (int i = 0; i < ids.Count - 1; i++)
        {
            for (int j = i + 1; j < Math.Min(i + 100, ids.Count); j++) // Limit comparisons to prevent O(n²)
            {
                if (ids[i].Equals(ids[j]))
                {
                    equalCount++;
                }
            }
        }
        
        return equalCount;
    }

    [Benchmark]
    public Dictionary<HazardID, string> IdDictionaryOperations()
    {
        var dictionary = new Dictionary<HazardID, string>(IdCount);
        
        // Add operations
        for (int i = 0; i < IdCount; i++)
        {
            var id = new HazardID($"DICT-HZ-{i:D8}");
            dictionary.Add(id, $"Value for {id}");
        }
        
        // Lookup operations (test a subset)
        var lookupCount = Math.Min(1000, IdCount);
        for (int i = 0; i < lookupCount; i++)
        {
            var id = new HazardID($"DICT-HZ-{i:D8}");
            _ = dictionary.TryGetValue(id, out _);
        }
        
        return dictionary;
    }
}

// ==============================================
// SMS SERIALIZATION BENCHMARKS (DOMAIN ENTITIES)
// ==============================================

/// <summary>
/// Benchmarks for testing serialization performance of actual SMS Domain Entities
/// Uses custom JSON converters to handle parameterized constructors
/// </summary>
[MemoryDiagnoser]
[SimpleJob]
[RankColumn]
public class SMSSerializationBenchmarks
{
    private List<Hazard> _hazards;
    private List<Report> _reports;
    private JsonSerializerOptions _jsonOptions;
    private JsonSerializerOptions _readOnlyJsonOptions;

    [GlobalSetup]
    public void Setup()
    {
        _hazards = CreateTestHazards(100);
        _reports = CreateTestReports(100);
        
        // Configure JSON options for Domain Entities
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = 
            {
                new HazardJsonConverter(),
                new ReportJsonConverter(),
                new HazardIDJsonConverter(),
                new ReportIDJsonConverter()
            }
        };

        // Separate options for serialization-only benchmarks (safer)
        _readOnlyJsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    [Benchmark]
    public string SerializeHazardsToJson()
    {
        return JsonSerializer.Serialize(_hazards, _readOnlyJsonOptions);
    }

    [Benchmark]
    public string SerializeReportsToJson()
    {
        return JsonSerializer.Serialize(_reports, _readOnlyJsonOptions);
    }

    [Benchmark]
    public int JsonStringAnalysis()
    {
        var json = JsonSerializer.Serialize(_hazards, _readOnlyJsonOptions);
        
        // Analyze JSON structure for performance insights
        var metrics = 0;
        metrics += json.Count(c => c == '{'); // Object count
        metrics += json.Count(c => c == '"'); // String count
        metrics += json.Split(',').Length;    // Property count
        metrics += json.Length;               // Total size
        
        return metrics;
    }

    [Benchmark]
    public int EntityToJsonMemoryPattern()
    {
        var processedEntities = 0;
        
        // Test memory allocation pattern for individual entity serialization
        foreach (var hazard in _hazards.Take(10)) // Limit to prevent long execution
        {
            var json = JsonSerializer.Serialize(hazard, _readOnlyJsonOptions);
            if (!string.IsNullOrEmpty(json))
            {
                processedEntities++;
            }
        }
        
        return processedEntities;
    }

    private List<Hazard> CreateTestHazards(int count)
    {
        var hazards = new List<Hazard>();
        
        for (int i = 0; i < count; i++)
        {
            var hazard = new Hazard(new HazardID($"SER-HZ-{i:D4}"))
            {
                Code = $"SER-HZ-{i:D4}",
                Name = $"Serialization Test Hazard {i}",
                Description = "This hazard is used for serialization performance testing with various JSON operations",
                ReportCode = $"SER-RPT-{i:D4}",
                ScoringPanelCode = $"SP-SER-{i % 5:D2}",
                AverageScore = (1.0 + (i % 5)).ToString("F1")
            };
            hazards.Add(hazard);
        }
        
        return hazards;
    }

    private List<Report> CreateTestReports(int count)
    {
        var reports = new List<Report>();
        
        for (int i = 0; i < count; i++)
        {
            var report = new Report(new ReportID($"SER-RPT-{i:D4}"))
            {
                Code = $"SER-RPT-{i:D4}",
                Name = $"Serialization Test Report {i}",
                Description = "This report is used for serialization performance testing with JSON operations",
                Status = i % 2 == 0 ? "Draft" : "Published",
                Stage = i % 3 == 0 ? "Initial" : (i % 3 == 1 ? "Review" : "Final")
            };
            reports.Add(report);
        }
        
        return reports;
    }
}

// ==============================================
// CUSTOM JSON CONVERTERS FOR DOMAIN ENTITIES
// ==============================================

/// <summary>
/// Custom JSON converter for HazardID value objects
/// </summary>
public class HazardIDJsonConverter : JsonConverter<HazardID>
{
    public override HazardID Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return new HazardID(reader.GetString() ?? string.Empty);
    }

    public override void Write(Utf8JsonWriter writer, HazardID value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}

/// <summary>
/// Custom JSON converter for ReportID value objects
/// </summary>
public class ReportIDJsonConverter : JsonConverter<ReportID>
{
    public override ReportID Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return new ReportID(reader.GetString() ?? string.Empty);
    }

    public override void Write(Utf8JsonWriter writer, ReportID value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Value);
    }
}

/// <summary>
/// Custom JSON converter for Hazard entities
/// Handles the parameterized constructor requirement
/// </summary>
public class HazardJsonConverter : JsonConverter<Hazard>
{
    public override Hazard Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;
        
        // Extract ID for constructor
        var idElement = root.GetProperty("id");
        var idValue = idElement.GetProperty("value").GetString() ?? string.Empty;
        var hazardId = new HazardID(idValue);
        
        // Create entity with required constructor parameter
        var hazard = new Hazard(hazardId);
        
        // Set other properties
        if (root.TryGetProperty("code", out var code))
            hazard.Code = code.GetString() ?? string.Empty;
            
        if (root.TryGetProperty("name", out var name))
            hazard.Name = name.GetString();
            
        if (root.TryGetProperty("description", out var description))
            hazard.Description = description.GetString();
            
        if (root.TryGetProperty("reportCode", out var reportCode))
            hazard.ReportCode = reportCode.GetString() ?? string.Empty;
            
        if (root.TryGetProperty("scoringPanelCode", out var scoringPanelCode))
            hazard.ScoringPanelCode = scoringPanelCode.GetString();
            
        if (root.TryGetProperty("averageScore", out var averageScore))
            hazard.AverageScore = averageScore.GetString();
        
        return hazard;
    }

    public override void Write(Utf8JsonWriter writer, Hazard value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        
        // Write ID as nested object to match BaseID structure
        writer.WritePropertyName("id");
        writer.WriteStartObject();
        writer.WriteString("value", value.Id.Value);
        writer.WriteEndObject();
        
        writer.WriteString("code", value.Code);
        writer.WriteString("name", value.Name);
        writer.WriteString("description", value.Description);
        writer.WriteString("reportCode", value.ReportCode);
        writer.WriteString("scoringPanelCode", value.ScoringPanelCode);
        writer.WriteString("averageScore", value.AverageScore);
        writer.WriteString("createdBy", value.CreatedBy);
        
        if (value.CreatedDate.HasValue)
            writer.WriteString("createdDate", value.CreatedDate.Value);
            
        writer.WriteString("updatedBy", value.UpdatedBy);
        
        if (value.UpdatedDate.HasValue)
            writer.WriteString("updatedDate", value.UpdatedDate.Value);
        
        writer.WriteEndObject();
    }
}

/// <summary>
/// Custom JSON converter for Report entities
/// </summary>
public class ReportJsonConverter : JsonConverter<Report>
{
    public override Report Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;
        
        // Extract ID for constructor
        var idElement = root.GetProperty("id");
        var idValue = idElement.GetProperty("value").GetString() ?? string.Empty;
        var reportId = new ReportID(idValue);
        
        // Create entity with required constructor parameter
        var report = new Report(reportId);
        
        // Set other properties
        if (root.TryGetProperty("code", out var code))
            report.Code = code.GetString() ?? string.Empty;
            
        if (root.TryGetProperty("name", out var name))
            report.Name = name.GetString();
            
        if (root.TryGetProperty("description", out var description))
            report.Description = description.GetString();
            
        if (root.TryGetProperty("status", out var status))
            report.Status = status.GetString();
            
        if (root.TryGetProperty("stage", out var stage))
            report.Stage = stage.GetString();
        
        return report;
    }

    public override void Write(Utf8JsonWriter writer, Report value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        
        // Write ID as nested object to match BaseID structure
        writer.WritePropertyName("id");
        writer.WriteStartObject();
        writer.WriteString("value", value.Id.Value);
        writer.WriteEndObject();
        
        writer.WriteString("code", value.Code);
        writer.WriteString("name", value.Name);
        writer.WriteString("description", value.Description);
        writer.WriteString("status", value.Status);
        writer.WriteString("stage", value.Stage);
        writer.WriteString("createdBy", value.CreatedBy);
        
        if (value.CreatedDate.HasValue)
            writer.WriteString("createdDate", value.CreatedDate.Value);
            
        writer.WriteString("updatedBy", value.UpdatedBy);
        
        if (value.UpdatedDate.HasValue)
            writer.WriteString("updatedDate", value.UpdatedDate.Value);
        
        writer.WriteEndObject();
    }
}