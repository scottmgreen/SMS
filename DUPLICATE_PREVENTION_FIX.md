# Risk Assessment Creation Logic - FIXED

## What Was Implemented

You identified a critical issue: **The system was always creating new RiskAssessments without checking if one already existed for the same hazard**, which could lead to duplicate assessments.

## The Solution

### ? **Added Proper Existence Check**

**Before (Problematic):**
```csharp
// Always tried to create new, no check for existing
if ((string.IsNullOrWhiteSpace(Id) || Id.Equals("new", StringComparison.OrdinalIgnoreCase)) && !string.IsNullOrWhiteSpace(HazardId))
{
    var createResult = await CreateRiskAssessmentFromHazardAsync(HazardId);
    // ... create new assessment
}
```

**After (Fixed):**
```csharp
if ((string.IsNullOrWhiteSpace(Id) || Id.Equals("new", StringComparison.OrdinalIgnoreCase)) && !string.IsNullOrWhiteSpace(HazardId))
{
    _logger.LogInformation("Checking for existing RiskAssessment for Hazard: {HazardId}", HazardId);
    
    // CRITICAL: Check if RiskAssessment already exists for this hazard
    var existingAssessmentResult = await FindExistingRiskAssessmentByHazardAsync(HazardId);
    
    if (existingAssessmentResult.IsSuccess)
    {
        // Use existing assessment
        Id = existingAssessmentResult.Value;
        _logger.LogInformation("Found existing RiskAssessment {AssessmentId} for Hazard {HazardId} - loading it", Id, HazardId);
        
        return RedirectToPage("/SafetyRiskManagement/RiskAssessmentWizard", 
            new { id = Id, stepNumber = StepNumber });
    }
    else
    {
        // Create new assessment since none exists
        _logger.LogInformation("No existing RiskAssessment found for Hazard: {HazardId} - creating new one", HazardId);
        var createResult = await CreateRiskAssessmentFromHazardAsync(HazardId);
        // ... create new assessment
    }
}
```

### ? **Added FindExistingRiskAssessmentByHazardAsync Method**

```csharp
/// <summary>
/// CRITICAL: Find existing RiskAssessment by HazardId before creating new one
/// Prevents duplicate assessments for the same hazard
/// </summary>
private async Task<Result<string>> FindExistingRiskAssessmentByHazardAsync(string hazardId)
{
    try
    {
        _logger.LogInformation("Searching for existing RiskAssessment for Hazard: {HazardId}", hazardId);

        // Get all risk assessments and check for matching hazard
        var allAssessmentsQuery = new GetAllRiskAssessmentsQuery();
        var assessmentsResult = await _mediator.SendAsync(allAssessmentsQuery, CancellationToken.None);

        if (assessmentsResult.IsFailure)
        {
            _logger.LogWarning("Could not retrieve risk assessments to check for existing: {Error}", 
                assessmentsResult.Error?.Message);
            return Result<string>.Failure<string>(assessmentsResult.Error!);
        }

        var existingAssessment = assessmentsResult.Value
            .FirstOrDefault(ra => ra.HazardCode == hazardId || ra.PrimaryHazardId == hazardId);

        if (existingAssessment != null)
        {
            _logger.LogInformation("Found existing RiskAssessment {AssessmentId} for Hazard {HazardId} (Status: {Status})",
                existingAssessment.Code, hazardId, existingAssessment.Status);
            return Result<string>.Success(existingAssessment.Code!);
        }

        _logger.LogInformation("No existing RiskAssessment found for Hazard: {HazardId}", hazardId);
        return Result<string>.Failure<string>(new SMS_Domain.Common.Error("NOT_FOUND", "No existing assessment found"));
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error searching for existing RiskAssessment for Hazard {HazardId}", hazardId);
        return Result<string>.Failure<string>(
            new SMS_Domain.Common.Error("SEARCH_FAILED", $"Error searching for existing assessment: {ex.Message}"));
    }
}
```

### ? **Added Missing Query Handler**

Created `GetAllRiskAssessmentsQueryHandler` to support the existence check:

```csharp
public class GetAllRiskAssessmentsQueryHandler : BaseQueryBundle, IRequestHandler<GetAllRiskAssessmentsQuery, Result<List<RiskAssessment>>>
{
    private readonly RiskAssessmentDataService _dataService;
    private readonly ILogger<GetAllRiskAssessmentsQueryHandler> _logger;

    public GetAllRiskAssessmentsQueryHandler(RiskAssessmentDataService dataService, ILogger<GetAllRiskAssessmentsQueryHandler> logger)
    {
        _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<List<RiskAssessment>>> HandleAsync(GetAllRiskAssessmentsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Processing GetAllRiskAssessmentsQuery");

            var result = await _dataService.GetAllRiskAssessmentsAsync(ct).ConfigureAwait(false);

            if (result.IsSuccess)
            {
                _logger.LogInformation("Successfully retrieved {Count} RiskAssessments", result.Value?.Count ?? 0);
            }
            else
            {
                _logger.LogError("Failed to retrieve RiskAssessments. Error: {Error}", result.Error?.Message);
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("GetAllRiskAssessmentsQuery operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while retrieving all RiskAssessments");
            return Result<List<RiskAssessment>>.Failure<List<RiskAssessment>>(DomainErrors.RiskAssessmentError.NotFound);
        }
    }
}
```

## How It Works Now

### **Complete Flow:**

1. **User clicks "Proceed to Assessment" in ReportValidation**
2. **ReportValidation redirects with `id = "new"` and `hazardId = "{hazardId}"`**
3. **RiskAssessmentWizard.OnGetAsync() detects this scenario**
4. **? NEW: System checks if RiskAssessment already exists for that hazard**
   - Calls `FindExistingRiskAssessmentByHazardAsync(hazardId)`
   - Searches all RiskAssessments for matching `HazardCode` or `PrimaryHazardId`
5. **If existing assessment found:**
   - ? **Loads the existing assessment** instead of creating duplicate
   - Redirects to wizard with the existing assessment ID
   - User can continue working on the existing assessment
6. **If no existing assessment found:**
   - ? **Creates new assessment** as before
   - Links it to the hazard
   - Redirects to wizard with new assessment ID

### **Key Benefits:**

? **Prevents duplicate assessments** for the same hazard
? **Preserves existing work** - users can resume interrupted assessments
? **Maintains data integrity** - one assessment per hazard
? **Better user experience** - no confusion from multiple assessments
? **Proper logging** for troubleshooting and audit trails

### **Files Modified:**

1. **`RiskAssessmentWizard.cshtml.cs`** - Added existence check logic
2. **`RiskAssessmentQueryHandlers.cs`** - Added missing query handler

### **The Result:**

Now when validating and moving to RiskAssessment, the system **properly checks** if there's already a RiskAssessment created for that hazard. If so, it loads the existing one. If not, **THEN** it creates a new one. This prevents duplicates and ensures users can resume their work seamlessly.

This addresses your exact concern: *"When Validating and Moving to the RiskAssessment the system needs to check if there is already a RiskAssessment created. If so then it should be loaded, if not THEN a CreateRiskAssessmentCommand should be used."*