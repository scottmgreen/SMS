## Integration Test ID Generation Utilities

### ?? **CENTRALIZED IN DATABASETESTBASE**

All integration tests now use centralized test ID generation utilities located in `DatabaseTestBase`. This ensures:
- **Consistent ID Format** across all tests
- **No Conflicts** between different test classes
- **Easy Maintenance** - single place to update ID generation logic
- **Flexible Options** for different testing scenarios

### ?? **AVAILABLE METHODS**

#### 1. **`GenerateTestId()`**
```csharp
protected string GenerateTestId()
```
- **Format**: `TEST-####` (e.g., `TEST-0742`, `TEST-9851`)
- **Range**: `TEST-0000` to `TEST-9999`
- **Use Case**: Standard test ID for most integration tests

#### 2. **`GenerateTestId(string prefix)`**
```csharp
protected string GenerateTestId(string prefix)
```
- **Format**: `{prefix}-####` (e.g., `PERF-0742`, `LOAD-9851`)
- **Range**: `{prefix}-0000` to `{prefix}-9999`
- **Use Case**: Custom prefix for specific test types

#### 3. **`GenerateUniqueTestId()`**
```csharp
protected string GenerateUniqueTestId()
```
- **Format**: `TEST-####-HHMMSS` (e.g., `TEST-0742-143022`)
- **Uniqueness**: Includes timestamp for guaranteed uniqueness
- **Use Case**: When you need absolutely unique IDs (e.g., stress tests)

#### 4. **`GenerateUniqueTestId(string prefix)`**
```csharp
protected string GenerateUniqueTestId(string prefix)
```
- **Format**: `{prefix}-####-HHMMSS` (e.g., `PERF-0742-143022`)
- **Uniqueness**: Custom prefix + timestamp
- **Use Case**: Unique IDs with custom prefixes

### ?? **USAGE EXAMPLES**

#### **Standard Usage**
```csharp
[Fact]
public async Task Repository_CreateHazard_ShouldSucceed()
{
    // Standard test ID
    var testId = GenerateTestId();
    var hazard = CreateTestHazard();
    hazard.Name = $"Test Hazard {testId}";
    
    // Test continues...
}
```

#### **Performance Testing**
```csharp
[Fact]
public async Task Performance_CreateMultipleHazards_ShouldCompleteInTime()
{
    for (int i = 0; i < 100; i++)
    {
        var testId = GenerateTestId("PERF");
        var hazard = CreateTestHazard();
        hazard.Name = $"Performance Test Hazard {testId}";
        
        // Create and cleanup...
    }
}
```

#### **Stress Testing with Unique IDs**
```csharp
[Fact]
public async Task Stress_CreateManyHazards_WithUniqueIds()
{
    var tasks = new List<Task>();
    
    for (int i = 0; i < 1000; i++)
    {
        tasks.Add(Task.Run(async () =>
        {
            var uniqueId = GenerateUniqueTestId("STRESS");
            var hazard = CreateTestHazard();
            hazard.Name = $"Stress Test {uniqueId}";
            
            // Concurrent operations...
        }));
    }
    
    await Task.WhenAll(tasks);
}
```

### ?? **CURRENT IMPLEMENTATION STATUS**

All integration test classes inherit from `DatabaseTestBase` and can use these methods:

? **HazardDatabaseIntegrationTests** - Uses centralized `GenerateTestId()`  
? **AirportSharedDatasetDatabaseIntegrationTests** - Uses centralized methods  
? **ReportDatabaseIntegrationTests** - Uses centralized methods  
? **MitigationDatabaseIntegrationTests** - Uses centralized methods  
? **InvestigationDatabaseIntegrationTests** - Uses centralized methods  
? **ScoringPanelDatabaseIntegrationTests** - Uses centralized methods  
? **InterviewDatabaseIntegrationTests** - Uses centralized methods  

### ?? **BENEFITS ACHIEVED**

1. **Consistency** - All tests use the same ID format
2. **No Conflicts** - Range 0-9999 provides 10,000 unique combinations per run
3. **Debugging** - Easy to identify test-generated data by prefixes
4. **Maintenance** - Single location for ID generation logic
5. **Flexibility** - Multiple methods for different scenarios
6. **Thread Safety** - Uses `Random.Shared` which is thread-safe in .NET 6+

### ?? **RECOMMENDATIONS**

- **Use `GenerateTestId()`** for standard integration tests
- **Use `GenerateTestId("PREFIX")`** for categorized tests (PERF, LOAD, etc.)
- **Use `GenerateUniqueTestId()`** for stress/concurrent tests
- **Always cleanup** test data using the generated IDs
- **Include test ID in entity names/descriptions** for easier debugging

This centralized approach ensures all your integration tests follow the same patterns and are easy to maintain!