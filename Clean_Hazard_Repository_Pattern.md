# Clean Hazard Repository Pattern - Fixing the GetHazardsByReportId Mess

## Problem
The original `GetHazardsByReportId` implementation was trying to LEFT JOIN with mitigations, which created:
- Complex SQL with multiple rows per hazard
- Messy data mapping logic
- Performance issues
- Difficult-to-maintain code

## Solution
**Separation of Concerns** - Load hazards and mitigations separately:

### 1. Clean Hazard Loading
```sql
-- pr_Hazard_GetByReportId (CLEAN VERSION)
SELECT 
    h.[fldi_ID], h.[fldv_Code], h.[fldv_Name], h.[fldc_Description],
    -- ... all hazard fields ...
    r.[fldv_WorstCredibleOutcome], r.[fldv_RootCause], r.[fldv_AdditionalComments]
FROM [dbo].[tbld_Hazards] h
LEFT JOIN [dbo].[tbld_RiskAnalysis] r ON h.[fldv_Code] = r.[fldv_HazardCode]
WHERE h.[fldv_ReportCode] = @pID
ORDER BY h.[fldd_CreatedDate];
```

### 2. Separate Mitigation Loading (When Needed)
```csharp
// Load hazards first (clean)
var hazards = await _hazardRepository.GetHazardsByReportIdAsync(reportId);

// Load mitigations separately (only if needed)
foreach (var hazard in hazards)
{
    var mitigations = await _mitigationRepository.GetByHazardCodeAsync(hazard.Code);
    // Process mitigations as needed
}
```

## Benefits

### ? **Clean Data Mapping**
- One row per hazard = simple mapping
- No complex result set processing
- Clear separation of entity responsibilities

### ? **Better Performance**
- No unnecessary JOINs when mitigations aren't needed
- Optimized queries for specific use cases
- Reduced data transfer

### ? **Maintainable Code**
- Simple, focused methods
- Easy to understand and debug
- Clear intent for each operation

### ? **Flexible Usage**
- `GetHazardsByReportIdAsync()` - Clean hazards only
- `GetHazardsByReportIdWithMitigationsAsync()` - Hazards + mitigations when needed

## Usage Examples

### Basic Usage (Most Common)
```csharp
// Just get the hazards - no complex joins
var hazards = await _hazardDataService.GetHazardsByReportIdAsync(reportId);
```

### With Mitigations (When Needed)
```csharp
// Get hazards with mitigations loaded separately
var hazards = await _hazardDataService.GetHazardsByReportIdWithMitigationsAsync(reportId);
```

### Performance-Conscious Loading
```csharp
// Load base hazards first
var hazards = await _hazardDataService.GetHazardsByReportIdAsync(reportId);

// Only load mitigations for hazards that need them
foreach (var hazard in hazards.Where(h => h.RequiresMitigationReview))
{
    // Load mitigations on-demand
    var mitigations = await _mitigationService.GetByHazardCodeAsync(hazard.Code);
}
```

## Migration Notes

### Before (Messy)
- Complex JOIN with mitigations
- Multiple rows per hazard 
- Complex mapping logic
- Performance issues

### After (Clean)
- Simple hazard-only query
- One row per hazard
- Clean domain mapping
- Performance optimized

This pattern follows **Domain-Driven Design** principles by keeping entity loading focused and avoiding complex cross-aggregate queries.