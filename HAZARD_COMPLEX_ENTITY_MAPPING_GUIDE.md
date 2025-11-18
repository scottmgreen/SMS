# Enhanced Hazard Entity Mapping and Complex Entity Population Guide

## Overview
The Hazard entity has been enhanced to support complex relationships, particularly with HazardLocation. This guide shows how to properly use the enhanced mapping and Application Service patterns.

## Architecture Pattern

```
???????????????????    ????????????????????    ???????????????????????
?   Mappers.cs    ?    ?  HazardService   ?    ? HazardLocationService?
?   (Infrastructure) ? ?  (Application)   ?    ?   (Application)     ?
???????????????????    ????????????????????    ???????????????????????
         ?                       ?                        ?
         ?                       ?                        ?
         ?                       ?                        ?
???????????????????    ????????????????????    ???????????????????????
? MapToHazard()   ?    ?Complex Entity    ?    ? GetByHazardCode()   ?
? Basic SQL?Entity?    ?Population        ?    ? Location Data       ?
???????????????????    ????????????????????    ???????????????????????
```

## Enhanced MapToHazard Method

### Basic Mapping (Infrastructure Layer)
```csharp
// Infrastructure\Common\Mappers.cs
public static Hazard MapToHazard(SqlDataReader reader)
{
    // Maps basic Hazard fields from tbld_Hazards
    // ? Handles all enhanced Step 3 fields
    // ? Handles 5M Component Smart Enum
    // ? Handles HazardStatus/HazardPriority enums
    // ? Does NOT populate HazardLocation (by design)
    
    var hazard = new Hazard(hazardId);
    // ... basic field mapping
    
    // NOTE: hazard.HazardLocation is NOT set here
    // Must be populated via Application Service
    return hazard;
}
```

### Complex Entity Mapping (Application Layer)
```csharp
// Infrastructure\Common\Mappers.cs
public static async Task<Hazard> MapToHazardWithLocationAsync(SqlDataReader reader, HazardLocationService hazardLocationService)
{
    // First get basic hazard
    var hazard = MapToHazard(reader);
    
    // Then populate complex HazardLocation via Application Service
    var locationsResult = await hazardLocationService.GetHazardLocationsByHazardCodeAsync(hazard.Code);
    if (locationsResult.IsSuccess && locationsResult.Value.Any())
    {
        hazard.HazardLocation = locationsResult.Value.OrderByDescending(l => l.DateSelected).First();
    }
    
    return hazard;
}
```

## Enhanced HazardService Usage

### Basic Hazard Retrieval (No Location)
```csharp
public class SomeController
{
    private readonly HazardService _hazardService;
    
    // When you DON'T need location data
    public async Task<IActionResult> GetBasicHazard(string hazardId)
    {
        var result = await _hazardService.GetHazardByIdAsync(new HazardID(hazardId));
        if (result.IsSuccess)
        {
            var hazard = result.Value;
            // hazard.HazardLocation will be null
            // Use hazard.Location, hazard.LocationArea, hazard.LocationSubArea (legacy fields)
        }
        
        return View(hazard);
    }
}
```

### Complex Hazard Retrieval (With Location)
```csharp
public class SomeController
{
    private readonly HazardService _hazardService;
    
    // When you DO need complete location data
    public async Task<IActionResult> GetCompleteHazard(string hazardId)
    {
        var result = await _hazardService.GetHazardWithLocationByIdAsync(new HazardID(hazardId));
        if (result.IsSuccess)
        {
            var hazard = result.Value;
            // hazard.HazardLocation is populated with complete HazardLocation entity
            // Access: hazard.HazardLocation.Latitude, hazard.HazardLocation.Longitude, etc.
        }
        
        return View(hazard);
    }
}
```

### Collection Retrieval with Efficient Location Population
```csharp
public class HazardController
{
    private readonly HazardService _hazardService;
    
    // Efficient collection retrieval with location data
    public async Task<IActionResult> GetHazardsList()
    {
        // ? Efficient: Uses Task.WhenAll for parallel location loading
        var result = await _hazardService.GetAllHazardsWithLocationAsync();
        if (result.IsSuccess)
        {
            var hazards = result.Value;
            // All hazards have HazardLocation populated
        }
        
        return View(hazards);
    }
    
    // Or if you only need basic list (better performance)
    public async Task<IActionResult> GetBasicHazardsList()
    {
        // ? Faster: No location data loaded
        var result = await _hazardService.GetAllHazardsAsync();
        // Use for lists where location detail isn't shown
    }
}
```

## Repository Pattern Integration

### In HazardRepository
```csharp
public async Task<Result<Hazard>> GetHazardByIdAsync(HazardID id, CancellationToken ct = default)
{
    // Repository only handles basic SQL?Entity mapping
    using (SqlDataReader reader = await cmd.ExecuteReaderAsync(ct))
    {
        while (await reader.ReadAsync())
        {
            // ? Use basic mapper in Repository layer
            response = Mappers.MapToHazard(reader);
        }
    }
    
    // ? Do NOT populate HazardLocation here
    // That's the Application Service's job
    return Result<Hazard>.Success(response);
}
```

### In HazardDataService
```csharp
public async Task<Result<Hazard>> GetHazardByIdAsync(HazardID id, CancellationToken ct = default)
{
    // DataService passes through to Repository
    return await _repo.GetHazardByIdAsync(id, ct);
}
```

## Step-by-Step Usage Examples

### Example 1: Risk Assessment Wizard Step 2
```csharp
// SMS_Presentation\Pages\SafetyRiskManagement\Models\Step2Model.cs
public class Step2Model
{
    public async Task PopulateHazardsAsync()
    {
        // ? Get hazards with locations for map display
        var hazardsResult = await _hazardService.GetAllHazardsWithLocationAsync();
        if (hazardsResult.IsSuccess)
        {
            this.IdentifiedHazards = hazardsResult.Value.Select(h => new HazardData
            {
                HazardId = h.Code,
                Description = h.Description,
                Category = h.Category,
                // ? Can access location data
                Latitude = h.HazardLocation?.Latitude,
                Longitude = h.HazardLocation?.Longitude
            }).ToList();
        }
    }
}
```

### Example 2: Hazard Details Page
```csharp
// SMS_Presentation\Pages\SafetyRiskManagement\HazardDetails.cshtml.cs
public class HazardDetailsModel : PageModel
{
    public async Task<IActionResult> OnGetAsync(string hazardId)
    {
        // ? Get complete hazard with location for details page
        var result = await _hazardService.GetHazardWithLocationByIdAsync(new HazardID(hazardId));
        if (result.IsSuccess)
        {
            this.Hazard = result.Value;
            // Can display map with hazard.HazardLocation data
            // Can show complete location details
        }
        
        return Page();
    }
}
```

### Example 3: Performance-Optimized List
```csharp
// SMS_Presentation\Pages\SafetyRiskManagement\HazardList.cshtml.cs
public class HazardListModel : PageModel
{
    public async Task<IActionResult> OnGetAsync()
    {
        // ? For list view, use basic retrieval for better performance
        var result = await _hazardService.GetAllHazardsAsync();
        if (result.IsSuccess)
        {
            this.Hazards = result.Value;
            // Use legacy location fields for simple display
            // hazard.Location, hazard.LocationArea, hazard.LocationSubArea
        }
        
        return Page();
    }
}
```

## Service Registration

### In Program.cs or Startup.cs
```csharp
// Register services for dependency injection
services.AddScoped<HazardService>();
services.AddScoped<HazardLocationService>();
services.AddScoped<HazardDataService>();
services.AddScoped<HazardLocationDataService>();
services.AddScoped<HazardRepository>();
services.AddScoped<HazardLocationRepository>();
```

## Key Benefits

1. **Separation of Concerns**: Repository handles SQL mapping, Application Service handles complex entity population
2. **Performance**: Only load complex entities when needed
3. **Maintainability**: Clear pattern for adding other complex entities (Mitigations, Files, etc.)
4. **Testability**: Each layer can be tested independently
5. **Scalability**: Async patterns prevent blocking on complex entity loading

## Migration Path

### Existing Code
```csharp
// OLD: Basic mapping only
var hazard = await _hazardRepository.GetByIdAsync(hazardId);
// hazard.HazardLocation is null
```

### Enhanced Code
```csharp
// NEW: Choose based on needs
var hazard = await _hazardService.GetHazardByIdAsync(hazardId);           // Basic
var hazard = await _hazardService.GetHazardWithLocationByIdAsync(hazardId); // With location
```

## Error Handling

```csharp
public async Task<IActionResult> ExampleWithErrorHandling()
{
    try
    {
        var result = await _hazardService.GetHazardWithLocationByIdAsync(hazardId);
        if (result.IsFailure)
        {
            // Handle domain errors
            return BadRequest(result.Error);
        }
        
        var hazard = result.Value;
        if (hazard.HazardLocation == null)
        {
            // Location data wasn't available, but hazard was retrieved
            // Fall back to legacy location fields or show warning
        }
        
        return View(hazard);
    }
    catch (Exception ex)
    {
        // Handle unexpected errors
        _logger.LogError(ex, "Error retrieving hazard with location");
        return StatusCode(500);
    }
}
```

This enhanced pattern provides a robust, scalable solution for handling complex entity relationships while maintaining clean separation of concerns and optimal performance.