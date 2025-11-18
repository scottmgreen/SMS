## ?? **ParameterNames.cs and StoredProcs.cs Update Summary**

Both `ParameterNames.cs` and `StoredProcs.cs` have been **successfully updated** to support the new Steps 1-5 stored procedures for Risk Assessment.

### ? **Files Updated:**

#### **?? ParameterNames.cs**
- **Added 40+ new parameter constants** for Steps 1-5 Risk Assessment stored procedures
- **Added missing parameter constants** for Report, Mitigation, Interview, Investigation, HazardFile, and ScoringPanel operations
- **Restored accidentally removed parameters** that were causing build errors

#### **?? StoredProcs.cs**
- **Added 5 new step-specific procedures:**
  - `pr_RiskAssessment_UpdateStep1`
  - `pr_RiskAssessment_UpdateStep3` 
  - `pr_RiskAssessment_UpdateStep4`
  - `pr_RiskAssessment_UpdateStep5`
  - `pr_RiskAssessment_UpdateProgress`

### ??? **Build Errors Fixed:**

#### **Parameter Name Issues Resolved:**
- ? All Report parameter constants (`pmReportCode`, `pmReportName`, etc.)
- ? All Mitigation parameter constants (`pmMitigationCode`, etc.)
- ? All Interview parameter constants (`pmInterviewCode`, `pmInterviewStatus`, etc.)
- ? All Investigation parameter constants (`pmInvestigationStatus`, `pmInvestigationPlan`, etc.)
- ? All HazardFile parameter constants (`pmHazardFileCode`, `pmHazardFileUploadedBy`, etc.)
- ? All ReportValidation parameter constants (`pmReportValidationCode`, etc.)
- ? All ScoringPanel parameter constants (`pmScoringPanelCode`, etc.)

#### **Stored Procedure Constants Added:**
- ? `StoredProcs.pr_RiskAssessment_UpdateStep1`
- ? `StoredProcs.pr_RiskAssessment_UpdateStep3`
- ? `StoredProcs.pr_RiskAssessment_UpdateStep4`
- ? `StoredProcs.pr_RiskAssessment_UpdateStep5`
- ? `StoredProcs.pr_RiskAssessment_UpdateProgress`

### ?? **Next Issue: Field Name Constants**

**Current Error:** The `Mappers.cs` file is looking for basic Risk Assessment field constants that were accidentally removed:
- `FieldNames.fRiskAssessmentCode`
- `FieldNames.fRiskAssessmentName` 
- `FieldNames.fRiskAssessmentDescription`
- `FieldNames.fRiskAssessmentHazardCode`
- `FieldNames.fRiskAssessmentType`
- `FieldNames.fRiskAssessmentStatus`
- `FieldNames.fRiskAssessmentStage`

**Solution:** Add these basic field constants back to `FieldNames.cs`. The enhanced Steps 1-5 field constants were added correctly, but the basic ones used by Mappers.cs were accidentally removed.

### ?? **Usage Examples:**

```csharp
// Repository can now use step-specific stored procedures
await ExecuteStoredProcedureAsync(StoredProcs.pr_RiskAssessment_UpdateStep1, step1Parameters);
await ExecuteStoredProcedureAsync(StoredProcs.pr_RiskAssessment_UpdateStep3, step3Parameters);
await ExecuteStoredProcedureAsync(StoredProcs.pr_RiskAssessment_UpdateProgress, progressParameters);

// All parameter constants available
var parameters = new Dictionary<string, object>
{
    { ParameterNames.pmSystemDescription, step1.SystemDescription },
    { ParameterNames.pmPersonnelFactors, step1.FiveMPersonnel },
    { ParameterNames.pmCurrentStep, 1 },
    { ParameterNames.pmCompletionPercentage, 20 }
};
```

### ? **Status:**
- **ParameterNames.cs:** ? **COMPLETE**
- **StoredProcs.cs:** ? **COMPLETE** 
- **FieldNames.cs:** ?? **Needs basic field constants restored**

Once the basic field constants are added back to `FieldNames.cs`, the build should succeed and the Step 1-5 stored procedure integration will be complete! ??