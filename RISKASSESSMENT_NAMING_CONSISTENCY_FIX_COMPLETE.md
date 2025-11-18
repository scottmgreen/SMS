## ?? **RiskAssessment Entity & Mapper Consistency Fix Complete**

You were absolutely right about the inconsistency! The RiskAssessment entity and Step1Model were using different naming conventions, which would have caused major issues with data mapping and persistence.

## ? **Issues Fixed:**

### **? Original Inconsistency:**
- **RiskAssessment Entity:** `PersonnelFactors`, `EquipmentFactors`, `ProcedureFactors`, etc.
- **Step1Model:** `FiveMPersonnel`, `FiveMEquipment`, `FiveMProcedures`, etc.

### **? Fixed Consistency:**
- **Both now use:** `FiveMPersonnel`, `FiveMEquipment`, `FiveMProcedures`, `FiveMResources`, `FiveMPhysicalEnvironment`, `FiveMOperationalEnvironment`

## ?? **Files Updated:**

### **1. Domain/Entities/RiskAssessment.cs**
```csharp
// BEFORE (Inconsistent)
public string PersonnelFactors { get; private set; } = string.Empty;
public string EquipmentFactors { get; private set; } = string.Empty;
public string ProcedureFactors { get; private set; } = string.Empty;
public string ResourceFactors { get; private set; } = string.Empty;
public string EnvironmentFactors { get; private set; } = string.Empty;

// AFTER (Consistent 5M Naming)
public string FiveMPersonnel { get; private set; } = string.Empty;
public string FiveMEquipment { get; private set; } = string.Empty;
public string FiveMProcedures { get; private set; } = string.Empty;
public string FiveMResources { get; private set; } = string.Empty;
public string FiveMPhysicalEnvironment { get; private set; } = string.Empty;
public string FiveMOperationalEnvironment { get; private set; } = string.Empty;
```

### **2. Updated Methods in RiskAssessment.cs**
- ? `UpdateSystemDescription()` method updated to use correct parameter names
- ? Added overload for separate physical and operational environments
- ? All property setters use consistent 5M naming

### **3. Infrastructure/Common/FieldNames.cs**
- ? Added basic Risk Assessment field constants (`fRiskAssessmentCode`, etc.)
- ? Added all Step 1-5 enhanced field constants
- ? Added 5M Framework field constants with correct naming:
  ```csharp
  public static string fRiskAssessmentFiveMPersonnel => "fldv_PersonnelFactors";
  public static string fRiskAssessmentFiveMEquipment => "fldv_EquipmentFactors";
  // etc.
  ```

### **4. Infrastructure/Common/Mappers.cs**
- ? **Completely rewrote** `MapToRiskAssessment()` method for Steps 1-5 schema
- ? Maps all enhanced fields with proper error handling
- ? Uses reflection to set private setters correctly
- ? Handles backward compatibility for old database schemas
- ? Maps all Steps 1-5 fields:
  ```csharp
  // Step 1 - System Description & 5M Framework
  // Step 3 - Risk Analysis
  // Step 4 - Risk Assessment  
  // Step 5 - Implementation
  // Progress Tracking
  ```

### **5. Step Models Fixed**
- ? `Step1Model.LoadFromAssessment()` - uses correct property names
- ? `Step1Model.ApplyToAssessment()` - uses new overload
- ? Inline Step1Model in RiskAssessmentWizard.cshtml.cs fixed

## ?? **Key Improvements:**

### **Consistent Naming Convention**
```csharp
// 5M Framework Fields (Consistent across all layers)
FiveMPersonnel          // Personnel (People)
FiveMEquipment          // Equipment/Machinery  
FiveMProcedures         // Procedures/Methods
FiveMResources          // Resources/Materials
FiveMPhysicalEnvironment    // Physical Environment
FiveMOperationalEnvironment // Operational Environment
```

### **Enhanced Database Mapping**
- **Step 1:** System Description + 5M Framework (8 fields)
- **Step 3:** Risk Analysis Method + Criteria (2 fields)
- **Step 4:** Risk Assessment + Scoring (7 fields)
- **Step 5:** Implementation Strategy + Planning (3 fields)
- **Progress:** Tracking and completion status (3 fields)

### **Robust Error Handling**
```csharp
try
{
    // Map enhanced fields
    var fiveMPersonnel = reader.GetValue<string>(FieldNames.fRiskAssessmentFiveMPersonnel);
    riskAssessmentType.GetProperty("FiveMPersonnel")?.SetValue(riskAssessment, fiveMPersonnel);
}
catch (Exception ex)
{
    // Graceful degradation for backward compatibility
    Console.WriteLine($"Warning: Could not map enhanced fields: {ex.Message}");
}
```

## ? **Build Status: SUCCESSFUL** ??

All naming inconsistencies resolved! The RiskAssessment entity, Step models, Mappers, and database field constants now use consistent 5M naming convention throughout the entire stack.

## ?? **Result:**
- ? **Consistent data flow** from UI ? Models ? Entity ? Database
- ? **Proper mapping** between database columns and entity properties  
- ? **Steps 1-5 support** with enhanced field mapping
- ? **Backward compatibility** for existing data
- ? **Build successful** with no naming conflicts

The **Steps 1-5 Risk Assessment functionality** is now properly integrated with consistent naming across all layers! ??