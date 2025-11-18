## ?? **Repository & DataService Updates for Steps 1-5 Complete!**

The **RiskAssessmentRepository** and **RiskAssessmentDataService** have been successfully updated to support the enhanced Steps 1-5 functionality with proper stored procedure integration.

## ? **What Was Updated:**

### **1. Infrastructure/Persistence/RiskAssessmentRepository.cs**

#### **Enhanced CRUD Operations:**
```csharp
// BEFORE: Basic parameters only
cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAssessmentCode, riskAssessment.Code));
cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAssessmentName, riskAssessment.Name));

// AFTER: Full Steps 1-5 parameter support
// Core Assessment Fields
cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmLeadAssessorId, riskAssessment.LeadAssessorId));
cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmCurrentStep, riskAssessment.CurrentStep));
cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAssessmentCategory, riskAssessment.RiskAssessmentCategory.ToString()));

// Step 1 - System Description Fields
cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmSystemDescription, riskAssessment.SystemDescription));
cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmPersonnelFactors, riskAssessment.FiveMPersonnel));
// ... All 5M Framework fields

// Steps 3-5 Fields
cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAnalysisMethod, riskAssessment.RiskAnalysisMethod));
cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmImplementationStrategy, riskAssessment.ImplementationStrategy));
// ... etc.
```

#### **NEW Step-Specific Update Methods:**
```csharp
// ? Step 1: System Description & 5M Framework
public async Task<Result<RiskAssessment>> UpdateStep1Async(
    string riskAssessmentId,
    string leadAssessorId,
    string systemDescription,
    string systemBoundaries,
    string systemPurpose,
    string fiveMPersonnel,
    string fiveMEquipment,
    string fiveMProcedures,
    string fiveMResources,
    string fiveMPhysicalEnvironment,
    string fiveMOperationalEnvironment,
    string updatedBy = "SYSTEM",
    CancellationToken ct = default)

// ? Step 3: Risk Analysis
public async Task<Result<RiskAssessment>> UpdateStep3Async(...)

// ? Step 4: Risk Assessment
public async Task<Result<RiskAssessment>> UpdateStep4Async(...)

// ? Step 5: Implementation Planning
public async Task<Result<RiskAssessment>> UpdateStep5Async(...)

// ? Progress Tracking
public async Task<Result<RiskAssessment>> UpdateProgressAsync(...)
```

### **2. Infrastructure/Services/RiskAssessmentDataService.cs**

#### **Enhanced Service with Step-Specific Methods:**
```csharp
// ? Step-specific save methods
public async Task<Result<RiskAssessment>> SaveStep1Async(...)
public async Task<Result<RiskAssessment>> SaveStep3Async(...)
public async Task<Result<RiskAssessment>> SaveStep4Async(...)
public async Task<Result<RiskAssessment>> SaveStep5Async(...)

// ? Progress tracking
public async Task<Result<RiskAssessment>> UpdateProgressAsync(...)

// ? Validation methods
public async Task<Result<bool>> ValidateStepCanBeSavedAsync(...)
public async Task<Result<Dictionary<int, bool>>> GetStepCompletionStatusAsync(...)

// ? Generic step dispatcher
public async Task<Result<RiskAssessment>> SaveStepDataAsync(
    string riskAssessmentId,
    int stepNumber,
    Dictionary<string, object> stepData,
    string updatedBy = "SYSTEM",
    CancellationToken ct = default)
```

#### **Convenience Methods for UI Integration:**
```csharp
// ? Direct data save (no model dependencies)
public async Task<Result<RiskAssessment>> SaveFromStep1DataAsync(
    string riskAssessmentId,
    string leadAssessor,
    string systemDescription,
    // ... all Step 1 fields
)

// ? Generic routing based on step number
return stepNumber switch
{
    1 => await SaveStep1Async(...),
    3 => await SaveStep3Async(...),
    4 => await SaveStep4Async(...),
    5 => await SaveStep5Async(...),
    _ => Result<RiskAssessment>.Failure<RiskAssessment>(DomainErrors.RiskAssessmentError.InvalidStep)
};
```

## ?? **Key Technical Fixes:**

### **1. Enum Handling:**
```csharp
// BEFORE: Caused compilation errors
cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAssessmentStatus, riskAssessment.Status ?? (object)DBNull.Value));

// AFTER: Properly converts enum to string
cmd.Parameters.Add(DataAccess.Parameter(ParameterNames.pmRiskAssessmentStatus, riskAssessment.Status.ToString()));
```

### **2. Property Name Consistency:**
```csharp
// BEFORE: Used non-existent property
riskAssessment.Category

// AFTER: Used correct property
riskAssessment.RiskAssessmentCategory.ToString()
```

### **3. Progress Calculation:**
```csharp
// ? Dynamic completion percentage calculation
var completedStepsString = string.Join(",", riskAssessment.CompletedSteps);
var completionPercentage = riskAssessment.CompletedSteps.Count * 20; // 5 steps = 100%
```

### **4. Layer Separation:**
```csharp
// BEFORE: Infrastructure referencing Presentation layer
SMS.Presentation.Pages.SafetyRiskManagement.Models.Step1Model step1Model

// AFTER: Infrastructure using primitives only
Dictionary<string, object> stepData
```

## ?? **How This Integrates:**

### **For Step Models (Presentation Layer):**
```csharp
// Step1Model.ApplyToAssessment() can now call:
await _dataService.SaveStep1Async(
    assessmentId,
    step1Model.LeadAssessor,
    step1Model.SystemDescription,
    step1Model.SystemBoundaries,
    step1Model.SystemPurpose,
    step1Model.FiveMPersonnel,
    step1Model.FiveMEquipment,
    step1Model.FiveMProcedures,
    step1Model.FiveMResources,
    step1Model.FiveMPhysicalEnvironment,
    step1Model.FiveMOperationalEnvironment
);
```

### **For RiskAssessmentWizard.cshtml.cs:**
```csharp
// SaveCurrentStepAsync() can now route to specific save methods:
private async Task<(bool success, string message)> SaveStep1Async()
{
    var result = await _dataService.SaveStep1Async(
        Id,
        Step1.LeadAssessor,
        Step1.SystemDescription,
        // ... all Step 1 fields
    );
    
    if (result.IsSuccess)
    {
        Assessment = result.Value; // Update cached assessment
        return (true, "Step 1 saved successfully");
    }
    
    return (false, result.Error.Message);
}
```

## ? **Build Status: SUCCESSFUL!** ??

The entire **Steps 1-5 Risk Assessment infrastructure** is now complete with:
- ? **Enhanced database schema** (tables updated)
- ? **Updated stored procedures** (step-specific updates)
- ? **Enhanced ParameterNames** (all Steps 1-5 fields)
- ? **Updated Mappers** (Steps 1-5 field mapping)
- ? **Enhanced Repository** (step-specific CRUD methods)
- ? **Enhanced DataService** (step-specific business logic)

## ?? **Ready For:**
- ? **UI Integration** - Controllers and Page Models can use step-specific save methods
- ? **Business Logic** - Complex validation and workflow rules
- ? **Testing** - Each step can be tested independently
- ? **Future Enhancements** - Easy to add new steps or modify existing ones

Your **Risk Assessment Wizard** now has a robust, scalable infrastructure that properly separates concerns and supports the complete Steps 1-5 workflow! ??