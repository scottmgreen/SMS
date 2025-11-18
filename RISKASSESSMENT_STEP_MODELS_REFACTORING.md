# RiskAssessmentWizard Step Models Refactoring - SIMPLIFIED ARCHITECTURE

## Problem: "God Class" Anti-Pattern

The original RiskAssessmentWizard was a **"God Class"** with too many responsibilities:

? **Before - Single Monolithic Class:**
- 500+ lines of code in one class
- Step 1-5 properties mixed together
- Complex validation logic scattered throughout 
- Difficult to maintain and extend
- Single Responsibility Principle violated

## Solution: Step Models Within Same File

? **After - Organized Step Models:**
```csharp
public class Step1Model { } // System Description & 5M Framework
public class Step2Model { } // Hazard Identification  
public class Step3Model { } // Risk Analysis
public class Step4Model { } // Risk Assessment
public class Step5Model { } // Risk Mitigation

public class RiskAssessmentWizardModel : PageModel // Main coordinator
```

## Architecture Benefits

### ? **1. Single Responsibility Principle**

**Before (God Class):**
```csharp
public class RiskAssessmentWizardModel : PageModel
{
    // 50+ properties mixed together
    public string LeadAssessor { get; set; }
    public string SystemDescription { get; set; }
    public List<string> HazardIds { get; set; }
    public string RiskAnalysisMethod { get; set; }
    public string TolerabilityFramework { get; set; }
    public string ImplementationStrategy { get; set; }
    
    // Complex validation mixing all steps
    private (bool isValid, string message) ValidateStep1() { /* 50 lines */ }
    private (bool isValid, string message) ValidateStep2() { /* 30 lines */ }
    // etc...
}
```

**After (Step Models):**
```csharp
public class Step1Model 
{
    // Only Step 1 properties
    public string LeadAssessor { get; set; }
    public string SystemDescription { get; set; }
    // Only Step 1 methods
    public (bool isValid, string message) Validate() { /* Step 1 logic only */ }
    public void ApplyToAssessment(RiskAssessment assessment) { /* Step 1 logic only */ }
}

public class RiskAssessmentWizardModel : PageModel 
{
    // Clean coordinator
    public Step1Model Step1 { get; set; } = new();
    public Step2Model Step2 { get; set; } = new();
    // Simple delegation
    private (bool isValid, string message) ValidateCurrentStep()
    {
        return StepNumber switch
        {
            1 => Step1.Validate(),
            2 => Step2.Validate(),
            // etc...
        };
    }
}
```

### ? **2. Clean Model Binding**

**PageModel Integration:**
```csharp
public class RiskAssessmentWizardModel : PageModel
{
    #region Step Models - Organized by Responsibility

    [BindProperty]
    public Step1Model Step1 { get; set; } = new();

    [BindProperty] 
    public Step2Model Step2 { get; set; } = new();
    
    // etc...

    #endregion
}
```

### ? **3. Backward Compatibility**

**Legacy Property Support:**
```csharp
// These properties maintain compatibility with existing views
[Obsolete("Use Step1.LeadAssessor instead")]
public string LeadAssessor 
{ 
    get => Step1.LeadAssessor; 
    set => Step1.LeadAssessor = value; 
}
```

### ? **4. Step-Specific Logic Encapsulation**

**Each Step Model Contains:**
- **Properties** - Only fields relevant to that step
- **Validation** - Business rules specific to that step  
- **ApplyToAssessment()** - How to update the domain entity
- **LoadFromAssessment()** - How to populate from existing data

## Implementation Details

### **Step 1 Model - System Description**
```csharp
public class Step1Model 
{
    [Required(ErrorMessage = "Lead Assessor is required.")]
    public string LeadAssessor { get; set; } = string.Empty;
    
    [Required, StringLength(1000, MinimumLength = 10)]
    public string SystemDescription { get; set; } = string.Empty;
    
    // 5M Framework properties...
    
    public (bool isValid, string message) Validate()
    {
        // Step 1 specific validation logic
        // Checks that at least 4 of 10 fields are completed
    }
    
    public void ApplyToAssessment(RiskAssessment assessment)
    {
        // Applies Step 1 data to domain entity
        assessment.UpdateSystemDescription(...);
        assessment.CompleteStep(1);
    }
}
```

### **Simplified Main PageModel**
```csharp
public class RiskAssessmentWizardModel : PageModel
{
    // Step models handle their own data and logic
    [BindProperty] public Step1Model Step1 { get; set; } = new();
    [BindProperty] public Step2Model Step2 { get; set; } = new();
    
    // Main class focuses on coordination
    public async Task<IActionResult> OnPostSaveAndNavigateNextAsync()
    {
        // 1. Validate using step model
        if (!ModelState.IsValid) return Page();
        
        // 2. Load assessment if needed  
        if (Assessment == null) await LoadAssessmentDataAsync();
        
        // 3. Save using step model
        var saveResult = await SaveCurrentStepSimpleAsync();
        
        // 4. Navigate
        return RedirectToPage(...);
    }
    
    private async Task<(bool success, string message)> SaveStep1SimpleAsync()
    {
        // Step model handles the complexity
        Step1.ApplyToAssessment(Assessment);
        return await SaveAssessmentToDatabaseSimpleAsync();
    }
}
```

## Code Metrics Improvement

| Metric | Before (God Class) | After (Step Models) | Improvement |
|--------|-------------------|-------------------|-------------|
| **Lines per class** | 500+ lines | ~100 lines main + ~50 per step | 80% reduction |
| **Responsibilities** | 5+ (all steps mixed) | 1 (coordination only) | Single responsibility |
| **Cyclomatic complexity** | High (nested validation) | Low (simple delegation) | Much simpler |
| **Testability** | Hard (large dependencies) | Easy (focused units) | Easy to unit test |
| **Maintainability** | Low (find step logic) | High (clear separation) | Much easier |

## Benefits Achieved

### ? **For Developers:**
- **Easier to find** Step 1 logic (it's in Step1Model)
- **Easier to test** each step independently
- **Easier to modify** one step without affecting others
- **Clearer code organization** - no more hunting through 500 lines

### ? **For Architecture:**
- **Single Responsibility** - each class has one job
- **Open/Closed Principle** - can extend steps without changing main class
- **Clean separation** of concerns
- **Better model binding** - ASP.NET Core can bind to step models

### ? **For Maintenance:**
- **Add new step** - just create Step6Model 
- **Modify step validation** - only touch that step's model
- **Debug step issues** - smaller, focused code to review
- **Reuse step logic** - step models could be used elsewhere

## Files Modified

1. **`RiskAssessmentWizard.cshtml.cs`** - Refactored to use step models
   - Added Step1Model, Step2Model, Step3Model, Step4Model, Step5Model classes
   - Added backward compatibility properties
   - Simplified validation and save methods to delegate to step models

## The Result

The RiskAssessmentWizard is now **much easier to work with:**

- ? **Step 1 logic** is contained in Step1Model
- ? **Step 2 logic** is contained in Step2Model  
- ? **Main PageModel** focuses on coordination
- ? **Existing views** still work (backward compatibility)
- ? **New development** is much simpler
- ? **Testing** can focus on individual steps
- ? **Maintenance** is straightforward

The "God Class" anti-pattern has been eliminated while maintaining full functionality! ??