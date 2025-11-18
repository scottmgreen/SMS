# Modular Step Save Approach - Much Better for Debugging & Testing

## ?? **Problem Solved**
The original monolithic `SaveCurrentStepAsync()` approach was difficult to debug because:
- Couldn't easily test individual step saves in isolation
- When `Assessment` was null, couldn't determine which step was failing
- Mixed concerns across multiple steps in one method
- Hard to add step-specific logic

## ? **New Modular Approach**

### **Step-Specific Save Methods**
```csharp
// Now you can easily test and debug each step individually:
private async Task<(bool success, string message)> SaveStep1Async()
private async Task<(bool success, string message)> SaveStep2Async()  
private async Task<(bool success, string message)> SaveStep3Async()
private async Task<(bool success, string message)> SaveStep4Async()
private async Task<(bool success, string message)> SaveStep5Async()
```

### **Benefits of This Approach**

#### **1. Easy to Debug**
```csharp
// You can now set breakpoints in specific step saves
var saveResult = await SaveStep1Async();
if (!saveResult.success)
{
    // Immediately know Step 1 failed and why
    TempData["ErrorMessage"] = $"Step 1 failed: {saveResult.message}";
    return Page();
}
```

#### **2. Easy to Unit Test**
```csharp
[Test]
public async Task SaveStep1Async_WithValidData_ShouldSucceed()
{
    // Arrange
    var model = new RiskAssessmentWizardModel(mockMediator, mockLogger);
    model.Step1 = new Step1Model 
    { 
        LeadAssessor = "test-user",
        SystemDescription = "Valid description here..."
    };
    
    // Act
    var result = await model.SaveStep1Async();
    
    // Assert
    Assert.True(result.success);
    Assert.Contains("Step 1 saved successfully", result.message);
}
```

#### **3. Step-Specific Logic**
Each step can have its own specific requirements:

```csharp
private async Task<(bool success, string message)> SaveStep1Async()
{
    // Step 1 specific: Create new assessment if needed
    if (Assessment == null)
    {
        Assessment = await CreateNewRiskAssessmentAsync();
    }
    // ... rest of Step 1 logic
}

private async Task<(bool success, string message)> SaveStep3Async()
{
    // Step 3 specific: Requires hazards from Step 2
    var validation = Step3.Validate(RelatedHazards);
    if (!validation.isValid)
    {
        return (false, validation.message);
    }
    // ... rest of Step 3 logic
}
```

### **How It Fixes the Assessment = null Issue**

#### **Before (Problematic)**
```csharp
public async Task<IActionResult> OnPostSaveAndNavigateNextAsync()
{
    // Assessment is null - but which step? Hard to tell!
    if (Assessment == null)
    {
        await LoadAssessmentDataAsync(); // Might still be null
    }
    
    // Generic save that doesn't handle step-specific needs
    await SaveCurrentStepAsync(); // Fails, but why?
}
```

#### **After (Clear & Debuggable)**
```csharp
public async Task<IActionResult> OnPostSaveAndNavigateNextAsync()
{
    // Use step-specific save - immediately know which step fails
    var saveResult = await SaveCurrentStepAsync(); // Dispatches to SaveStepXAsync()
    
    if (!saveResult.success)
    {
        TempData["ErrorMessage"] = $"Step {StepNumber} save failed: {saveResult.message}";
        return Page(); // Clear error message shows exactly what failed
    }
    
    // Success - navigate to next step
    TempData["SuccessMessage"] = saveResult.message;
    return RedirectToPage("/SafetyRiskManagement/RiskAssessmentWizard", 
        new { id = Id, stepNumber = StepNumber + 1 });
}
```

### **Step-Specific Error Handling**

Each step can handle its specific error scenarios:

```csharp
private async Task<(bool success, string message)> SaveStep1Async()
{
    // Validate Step 1 model binding
    if (Step1 == null)
    {
        return (false, "Step 1 data is not properly bound - check form field names");
    }

    // Validate Step 1 business rules
    var validation = Step1.Validate();
    if (!validation.isValid)
    {
        return (false, $"Step 1 validation failed: {validation.message}");
    }

    // Step 1 specific: Create assessment if this is the first step
    if (Assessment == null)
    {
        Assessment = await CreateNewRiskAssessmentAsync();
        if (Assessment == null)
        {
            return (false, "Failed to create new risk assessment - check database connection");
        }
    }

    // Apply and save
    Step1.ApplyToAssessment(Assessment);
    await SaveAssessmentToDatabaseAsync();
    
    return (true, "Step 1 (System Description) saved successfully");
}
```

## ?? **Testing Individual Steps**

Now you can easily test each step in isolation:

```bash
# Test only Step 1 save functionality
dotnet test --filter "SaveStep1Async"

# Test only Step 3 save functionality  
dotnet test --filter "SaveStep3Async"
```

## ?? **Debugging is Much Easier**

### **Old Way: Monolithic & Hard to Debug**
1. Set breakpoint in `OnPostSaveAndNavigateNextAsync()`
2. Step through generic `SaveCurrentStepAsync()`
3. Try to figure out which step is failing and why
4. Assessment = null - but no idea if it's Step 1, 2, 3, 4, or 5 issue

### **New Way: Step-Specific & Clear**
1. Set breakpoint in `SaveStep3Async()` (or whatever step you're testing)
2. Immediately see Step 3 specific validation and logic
3. Clear error messages: "Step 3 data is not properly bound" vs generic failure
4. Each step handles its own Assessment creation/loading logic

## ?? **Result**

**Much better separation of concerns:**
- Step 1: Creates new assessments
- Step 2: Adds hazard data
- Step 3: Adds risk analysis data
- Step 4: Adds risk assessment data  
- Step 5: Adds mitigation data

**Much easier debugging:**
- Clear error messages per step
- Individual step testing
- Step-specific breakpoints
- Isolated failure modes

This approach makes the codebase **much more maintainable and debuggable**! ??