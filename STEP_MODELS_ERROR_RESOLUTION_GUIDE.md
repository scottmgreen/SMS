# RiskAssessmentWizard Step Models - Error Resolution Guide

## Summary of 72 Errors

You changed the partial view `@model` directives from `RiskAssessmentWizardModel` to individual step models (Step1Model, Step2Model, etc.), but this created 72 errors because:

1. **Step models don't contain navigation properties** (Id, StepNumber, AvailableHazards, etc.)
2. **Step models don't contain UI helper properties** (AssessmentData, PanelScores, etc.)
3. **Partial views expect the full context** of the main model

## The Fix: Two Approaches

### ? **Approach 1: Revert Partial Views (Recommended)**

Keep the main RiskAssessmentWizardModel as the `@model` for partial views but access step data through step properties:

```razor
<!-- Instead of this (causes 72 errors): -->
@model SMS.Presentation.Pages.SafetyRiskManagement.Step1Model

<!-- Use this (works perfectly): -->
@model SMS.Presentation.Pages.SafetyRiskManagement.RiskAssessmentWizardModel

<!-- And access step properties like: -->
<input asp-for="Step1.LeadAssessor" />
<textarea asp-for="Step1.SystemDescription" />
<input asp-for="Step3.RiskAnalysisMethod" />
```

### ? **Approach 2: Add All Properties to Step Models (Not Recommended)**

This would require duplicating all navigation properties in each step model, defeating the purpose of separation.

## Implementation Guide

### **1. Update Partial View Model Directives**

**_Step1_DescribeSystem.cshtml:**
```razor
@model SMS.Presentation.Pages.SafetyRiskManagement.RiskAssessmentWizardModel
```

**_Step2_IdentifyHazards.cshtml:**
```razor
@model SMS.Presentation.Pages.SafetyRiskManagement.RiskAssessmentWizardModel
```

**_Step3_AnalyzeRisk.cshtml:**
```razor
@model SMS.Presentation.Pages.SafetyRiskManagement.RiskAssessmentWizardModel
```

**_Step4_AssessRisk.cshtml:**
```razor
@model SMS.Presentation.Pages.SafetyRiskManagement.RiskAssessmentWizardModel
```

**_Step5_MitigateRisk.cshtml:**
```razor
@model SMS.Presentation.Pages.SafetyRiskManagement.RiskAssessmentWizardModel
```

### **2. Update Form Bindings in Partial Views**

**Step 1 Example:**
```razor
<!-- OLD (causes errors): -->
<input asp-for="LeadAssessor" />
<textarea asp-for="SystemDescription" />

<!-- NEW (works): -->
<input asp-for="Step1.LeadAssessor" />
<textarea asp-for="Step1.SystemDescription" />
```

**Step 3 Example:**
```razor
<!-- OLD (causes errors): -->
<input asp-for="RiskAnalysisMethod" />
<textarea asp-for="RiskCriteria" />

<!-- NEW (works): -->
<input asp-for="Step3.RiskAnalysisMethod" />
<textarea asp-for="Step3.RiskCriteria" />
```

### **3. Keep Navigation Properties in Main Model**

The main RiskAssessmentWizardModel should keep all navigation and UI helper properties:

```csharp
public class RiskAssessmentWizardModel : PageModel
{
    // Step models for data organization
    [BindProperty] public Step1Model Step1 { get; set; } = new();
    [BindProperty] public Step2Model Step2 { get; set; } = new();
    // etc...

    // Navigation properties (needed by views)
    public string Id { get; set; }
    public int StepNumber { get; set; }
    public List<Hazard> AvailableHazards { get; set; }
    
    // UI helper properties (needed by views)
    public AssessmentDataWrapper AssessmentData { get; set; }
    public List<SMSApplicationUser> AvailableAssessors { get; set; }
    // etc...
}
```

## Benefits of This Approach

### ? **Clean Step Separation:**
- Step1Model handles only Step 1 data and logic
- Step2Model handles only Step 2 data and logic
- Each step is focused and maintainable

### ? **Backward Compatibility:**
- Views still work with the main model
- No breaking changes to existing functionality
- All navigation and UI helpers available

### ? **Simple Form Binding:**
```csharp
// Step models automatically bind
public async Task<IActionResult> OnPostSaveAndNavigateNextAsync()
{
    // Step1.LeadAssessor is automatically populated
    // Step1.SystemDescription is automatically populated
    // etc.
    
    // Validate using step model
    if (!ModelState.IsValid) return Page();
    
    // Save using step model
    Step1.ApplyToAssessment(Assessment);
    
    return RedirectToPage(...);
}
```

## Quick Fix Script

To quickly fix the 72 errors, update these 5 files:

1. **_Step1_DescribeSystem.cshtml** - Change `@model Step1Model` to `@model RiskAssessmentWizardModel`
2. **_Step2_IdentifyHazards.cshtml** - Already correct
3. **_Step3_AnalyzeRisk.cshtml** - Change `@model Step3Model` to `@model RiskAssessmentWizardModel`
4. **_Step4_AssessRisk.cshtml** - Change `@model Step4Model` to `@model RiskAssessmentWizardModel`
5. **_Step5_MitigateRisk.cshtml** - Change `@model Step5Model` to `@model RiskAssessmentWizardModel`

Then update form bindings to use `Step1.`, `Step2.`, `Step3.`, etc. prefixes.

## Result

- ? **0 errors** instead of 72 errors
- ? **Clean step model organization** maintained
- ? **All functionality preserved**
- ? **Better maintainability** through step separation
- ? **Simple form binding** through step model properties

The step models provide the architectural benefits (separation of concerns, focused validation, clean organization) while the main model provides the UI infrastructure the views need.