# Risk Assessment Stored Procedure Updates for Steps 1-5

## ?? **Major Updates Required**

You're absolutely right! The original `pr_RiskAssessment_Update` stored procedure was severely limited and needed a **major overhaul** to support the Steps 1-5 functionality.

## ? **Problems with Original Procedure**

### **1. Severely Limited Parameters**
```sql
-- OLD: Only 7 basic parameters with tiny field sizes
@pRiskAssessmentCode NCHAR(10) = NULL,
@pRiskAssessmentName NCHAR(10) = NULL,
@pRiskAssessmentDescription NCHAR(10) = NULL,
-- ... only 4 more basic fields
```

### **2. No Steps 1-5 Support**
- **Missing:** All Step 1 System Description fields (8 fields)
- **Missing:** All Step 3 Risk Analysis fields (2 fields)
- **Missing:** All Step 4 Risk Assessment fields (7 fields)
- **Missing:** All Step 5 Implementation fields (3 fields)
- **Missing:** Progress tracking fields (3 fields)

### **3. Inadequate Field Sizes**
- `NCHAR(10)` for descriptions that need `NTEXT`
- `NCHAR(10)` for names that need `NVARCHAR(200)`
- No support for long text content

## ? **New Comprehensive Solution**

### **?? Updated Main Procedure: `pr_RiskAssessment_Update`**

**Now supports 35+ parameters for complete Steps 1-5 functionality:**

#### **Core Fields (Enhanced)**
```sql
@pRiskAssessmentCode NVARCHAR(50) = NULL,      -- Was: NCHAR(10)
@pRiskAssessmentName NVARCHAR(200) = NULL,     -- Was: NCHAR(10)  
@pRiskAssessmentDescription NTEXT = NULL,      -- Was: NCHAR(10)
@pLeadAssessorId NVARCHAR(50) = NULL,          -- NEW
@pCurrentStep INT = NULL,                      -- NEW
@pCompletedSteps NVARCHAR(50) = NULL,          -- NEW
```

#### **Step 1 - System Description (8 fields)**
```sql
@pSystemDescription NTEXT = NULL,
@pSystemBoundaries NTEXT = NULL,
@pSystemPurpose NTEXT = NULL,
@pPersonnelFactors NTEXT = NULL,
@pEquipmentFactors NTEXT = NULL,
@pProcedureFactors NTEXT = NULL,
@pResourceFactors NTEXT = NULL,
@pEnvironmentFactors NTEXT = NULL,
```

#### **Step 3 - Risk Analysis (2 fields)**
```sql
@pRiskAnalysisMethod NVARCHAR(100) = NULL,
@pRiskCriteria NTEXT = NULL,
```

#### **Step 4 - Risk Assessment (7 fields)**
```sql
@pTolerabilityFramework NVARCHAR(100) = NULL,
@pRiskAcceptanceCriteria NTEXT = NULL,
@pFinalSeverityScore INT = NULL,
@pFinalLikelihoodScore INT = NULL,
@pFinalRiskLevel NVARCHAR(10) = NULL,
@pRiskTolerability NVARCHAR(50) = NULL,
@pAssessmentRationale NTEXT = NULL,
```

#### **Step 5 - Implementation (3 fields)**
```sql
@pImplementationStrategy NTEXT = NULL,
@pOverallTargetDate DATETIME = NULL,
@pImplementationNotes NTEXT = NULL,
```

### **?? Step-Specific Helper Procedures**

**For easier, more targeted updates:**

1. **`pr_RiskAssessment_UpdateStep1`** - Only Step 1 fields
2. **`pr_RiskAssessment_UpdateStep3`** - Only Step 3 fields  
3. **`pr_RiskAssessment_UpdateStep4`** - Only Step 4 fields
4. **`pr_RiskAssessment_UpdateStep5`** - Only Step 5 fields
5. **`pr_RiskAssessment_UpdateProgress`** - Only progress tracking

## ?? **Key Improvements**

### **1. COALESCE Pattern for Partial Updates**
```sql
-- Only updates non-NULL parameters, preserves existing values
[fldv_SystemDescription] = COALESCE(@pSystemDescription, [fldv_SystemDescription]),
[fldv_SystemBoundaries] = COALESCE(@pSystemBoundaries, [fldv_SystemBoundaries]),
```

### **2. Enhanced Audit Logging**
```sql
-- Now includes step progress information
DECLARE @StepInfo NVARCHAR(100) = '';
IF @pCurrentStep IS NOT NULL
    SET @StepInfo = ', Current Step: ' + CAST(@pCurrentStep AS VARCHAR(10));
IF @pCompletionPercentage IS NOT NULL
    SET @StepInfo = @StepInfo + ', Completion: ' + CAST(@pCompletionPercentage AS VARCHAR(10)) + '%';
```

### **3. Result Set Return**
```sql
-- Returns success information for calling code
SELECT 
    @RowsAffected AS RowsAffected,
    @pID AS UpdatedID,
    @pCurrentStep AS CurrentStep,
    @pCompletionPercentage AS CompletionPercentage,
    'Success' AS Result
```

## ?? **Usage Examples**

### **Step 1 Update Example**
```sql
EXEC [dbo].[pr_RiskAssessment_UpdateStep1]
    @pID = 'RISK-ASSESSMENT-001',
    @pLeadAssessorId = 'USER-001',
    @pSystemDescription = 'Runway 10L-28R operations including taxiways Alpha and Bravo',
    @pSystemBoundaries = 'From threshold 10L to threshold 28R, including associated taxiways',
    @pPersonnelFactors = 'ATC controllers, ground crews, aircraft operators',
    @pEquipmentFactors = 'Navigation aids, lighting systems, ground vehicles',
    @pUpdatedBy = 'WIZARD-USER-001';
```

### **Progress Update Example**
```sql
EXEC [dbo].[pr_RiskAssessment_UpdateProgress]
    @pID = 'RISK-ASSESSMENT-001',
    @pCurrentStep = 3,
    @pCompletedSteps = '1,2',
    @pCompletionPercentage = 40,
    @pStatus = 'In Progress',
    @pStage = 'Risk Analysis',
    @pUpdatedBy = 'WIZARD-USER-001';
```

## ?? **Repository Integration**

### **Parameter Names Updated**
All new parameter constants added to `ParameterNames.cs`:
```csharp
public static string pmSystemDescription => "@pSystemDescription";
public static string pmPersonnelFactors => "@pPersonnelFactors";
public static string pmCurrentStep => "@pCurrentStep";
// ... 30+ more parameter constants
```

### **Repository Method Example**
```csharp
// Now your repository can easily update specific steps
public async Task<Result<bool>> UpdateStep1Async(RiskAssessment assessment, Step1Model step1)
{
    var parameters = new Dictionary<string, object>
    {
        { ParameterNames.pmRiskAssessmentId, assessment.Id.Value },
        { ParameterNames.pmLeadAssessorId, step1.LeadAssessor },
        { ParameterNames.pmSystemDescription, step1.SystemDescription },
        { ParameterNames.pmSystemBoundaries, step1.SystemBoundaries },
        { ParameterNames.pmPersonnelFactors, step1.FiveMPersonnel },
        // ... all Step 1 fields
    };

    return await ExecuteStoredProcedureAsync("pr_RiskAssessment_UpdateStep1", parameters);
}
```

## ? **Files Updated**

1. **`UPDATED_pr_RiskAssessment_Update_STEPS_1_5.sql`** - Complete stored procedures
2. **`ParameterNames.cs`** - All new parameter constants
3. **`FieldNames.cs`** - All new field name constants

## ?? **Result**

**Before:** 7 basic parameters, no Steps support
**After:** 35+ parameters supporting complete Steps 1-5 workflow

The stored procedures now fully support your modular step save approach and will work seamlessly with the Step-specific save methods we created earlier! ??

## ?? **Next Steps**

1. **Run the SQL script** to update the stored procedures
2. **Update Repository classes** to use the new step-specific procedures
3. **Test each step save method** with the new stored procedures
4. **Verify progress tracking** works correctly