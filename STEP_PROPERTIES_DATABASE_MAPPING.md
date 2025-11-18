# Risk Assessment Wizard - Step Properties Database Mapping

## ?? **Complete Property List for Database Schema**

### **?? Step 1 - System Description Properties**

| C# Property | Database Field | Type | Description |
|-------------|---------------|------|-------------|
| `LeadAssessor` | `fldv_LeadAssessor` | nvarchar(50) | Lead assessor user ID |
| `SystemDescription` | `fldv_SystemDescription` | ntext | System description (1000 chars) |
| `SystemBoundaries` | `fldv_SystemBoundaries` | ntext | System boundaries (1000 chars) |
| `SystemPurpose` | `fldv_SystemPurpose` | ntext | System purpose (1000 chars) |
| `FiveMPersonnel` | `fldv_FiveMPersonnel` | ntext | 5M Personnel factors |
| `FiveMEquipment` | `fldv_FiveMEquipment` | ntext | 5M Equipment factors |
| `FiveMProcedures` | `fldv_FiveMProcedures` | ntext | 5M Procedure factors |
| `FiveMResources` | `fldv_FiveMResources` | ntext | 5M Resource factors |
| `FiveMPhysicalEnvironment` | `fldv_FiveMPhysicalEnvironment` | ntext | 5M Physical environment |
| `FiveMOperationalEnvironment` | `fldv_FiveMOperationalEnvironment` | ntext | 5M Operational environment |
| `StakeholderGroups` | `fldv_StakeholderGroups` | nvarchar(max) | Comma-separated stakeholder groups |
| `SelectedIndividualStakeholders` | `fldv_SelectedIndividualStakeholders` | nvarchar(max) | Selected individual stakeholders |

### **?? Step 2 - Hazard Identification Properties**

| C# Property | Database Field | Type | Description |
|-------------|---------------|------|-------------|
| `HazardIds` | `fldv_HazardIds` | nvarchar(max) | JSON array of hazard IDs |
| `HazardDescriptions` | `fldv_HazardDescriptions` | ntext | JSON array of hazard descriptions |
| `HazardCategories` | `fldv_HazardCategories` | nvarchar(max) | JSON array of hazard categories |
| `GetValidHazardCount()` | `fldi_ValidHazardCount` | int | Count of valid hazards |

### **?? Step 3 - Risk Analysis Properties**

| C# Property | Database Field | Type | Description |
|-------------|---------------|------|-------------|
| `RiskAnalysisMethod` | `fldv_RiskAnalysisMethod` | nvarchar(100) | Risk analysis method (default: SMS Risk Matrix) |
| `RiskCriteria` | `fldv_RiskCriteria` | ntext | Risk criteria description |
| `HazardAnalyses` | `fldv_HazardAnalysesData` | ntext | JSON of HazardRiskAnalysis objects |
| `HazardWorstOutcomes` | `fldv_HazardWorstOutcomes` | ntext | JSON dictionary {hazardId: worstOutcome} |
| `HazardRootCauses` | `fldv_HazardRootCauses` | ntext | JSON dictionary {hazardId: rootCause} |

**HazardRiskAnalysis Object Properties:**
- `HazardId` - Hazard identifier
- `HazardDescription` - Hazard description
- `HazardCategory` - Hazard category
- `WorstCredibleOutcome` - Worst credible outcome (min 10 chars)
- `RootCauseAnalysis` - Root cause analysis (min 10 chars)
- `AdditionalComments` - Additional comments
- `AnalysisDate` - When analysis was performed

### **?? Step 4 - Risk Assessment Properties**

| C# Property | Database Field | Type | Description |
|-------------|---------------|------|-------------|
| `TolerabilityFramework` | `fldv_TolerabilityFramework` | nvarchar(100) | Tolerability framework (default: PDX-SMS Default) |
| `RiskAcceptanceCriteria` | `fldv_RiskAcceptanceCriteria` | ntext | Risk acceptance criteria |
| `HazardPanelMembers` | `fldv_HazardPanelMembers` | ntext | JSON dictionary {hazardId: [memberIds]} |
| `PanelScores` | `fldv_HazardPanelScores` | ntext | JSON dictionary {hazardId: [scoreObjects]} |
| `HazardAverageScores` | `fldv_HazardAverageScores` | ntext | JSON dictionary {hazardId: averageScore} |
| `FinalSeverityScore` | `fldi_FinalSeverityScore` | int | Final severity score (1-5) |
| `FinalLikelihoodScore` | `fldi_FinalLikelihoodScore` | int | Final likelihood score (1-5) |
| `FinalRiskLevel` | `fldv_FinalRiskLevel` | nvarchar(10) | Final risk level (1A, 2B, etc.) |
| `RiskTolerability` | `fldv_RiskTolerability` | nvarchar(50) | Risk tolerability (default: ALARP) |
| `AssessmentRationale` | `fldv_AssessmentRationale` | ntext | Assessment rationale |

**PanelMemberScoreData Object Properties:**
- `PanelMemberId` - Panel member identifier  
- `MemberName` - Panel member name
- `SeverityScore` - Severity score (1-5)
- `LikelihoodScore` - Likelihood score (1-5)
- `CalculatedScore` - Calculated risk score
- `RiskLevel` - Risk level string
- `SubmittedDate` - When score was submitted
- `IsComplete` - Whether score is complete

### **?? Step 5 - Risk Mitigation Properties**

| C# Property | Database Field | Type | Description |
|-------------|---------------|------|-------------|
| `ImplementationStrategy` | `fldv_ImplementationStrategy` | ntext | Implementation strategy |
| `OverallTargetDate` | `fldd_OverallTargetDate` | datetime | Overall target completion date |
| `ImplementationNotes` | `fldv_ImplementationNotes` | ntext | Implementation notes |
| `SavedMitigationStrategies` | `fldv_SavedMitigationStrategies` | ntext | JSON dictionary {hazardId: [strategies]} |

### **?? Assessment Progress & Metadata Properties**

| C# Property | Database Field | Type | Description |
|-------------|---------------|------|-------------|
| `StepNumber` | `fldi_CurrentStep` | int | Current step number (1-5) |
| `CompletedSteps` | `fldv_CompletedSteps` | nvarchar(50) | Comma-separated completed steps "1,2,3" |
| `GetCompletionPercentage()` | `fldi_CompletionPercentage` | int | Overall completion percentage |
| `AssessmentType` | `fldv_AssessmentType` | nvarchar(50) | Initial/Residual |
| `AssessmentCategory` | `fldv_AssessmentCategory` | nvarchar(50) | FiveStep/Simplified/etc |
| `ParentAssessmentId` | `fldv_ParentAssessmentId` | nvarchar(50) | For residual assessments |

## ?? **JSON Data Examples**

### **Step 2 - Hazard Data**
```json
// HazardIds
["HAZ-001", "HAZ-002", "HAZ-003"]

// HazardDescriptions  
["Aircraft collision on runway", "Ground vehicle incident", "Weather delay"]

// HazardCategories
["Aircraft Operations", "Ground Operations", "Weather Related"]
```

### **Step 3 - Risk Analysis Data**
```json
// HazardAnalysesData
{
  "HAZ-001": {
    "HazardId": "HAZ-001",
    "HazardDescription": "Aircraft collision on runway",
    "HazardCategory": "Aircraft Operations",
    "WorstCredibleOutcome": "Multiple fatalities and aircraft destruction",
    "RootCauseAnalysis": "Communication breakdown between ATC and pilots",
    "AdditionalComments": "Weather conditions may be a contributing factor",
    "AnalysisDate": "2024-11-17T10:30:00Z"
  },
  "HAZ-002": {
    // ... additional hazard analysis
  }
}
```

### **Step 4 - Panel Scoring Data**
```json
// HazardPanelScores
{
  "HAZ-001": [
    {
      "PanelMemberId": "USER-001",
      "MemberName": "John Smith",
      "SeverityScore": 5,
      "LikelihoodScore": 3,
      "CalculatedScore": 15,
      "RiskLevel": "3C",
      "SubmittedDate": "2024-11-17T11:00:00Z",
      "IsComplete": true
    }
  ]
}
```

### **Step 5 - Mitigation Strategies Data**
```json
// SavedMitigationStrategies
{
  "HAZ-001": [
    "Enhanced ATC communication protocols",
    "Radar-based collision avoidance systems", 
    "Mandatory pilot training updates"
  ],
  "HAZ-002": [
    "Ground vehicle GPS tracking",
    "Designated vehicle lanes"
  ]
}
```

## ?? **Database Field Mapping for Repository**

Update your `FieldNames.cs` to include these new fields:

```csharp
// Step 1 Fields
public static string fLeadAssessor => "fldv_LeadAssessor";
public static string fSystemDescription => "fldv_SystemDescription";
public static string fSystemBoundaries => "fldv_SystemBoundaries";
public static string fSystemPurpose => "fldv_SystemPurpose";
public static string fFiveMPersonnel => "fldv_FiveMPersonnel";
public static string fFiveMEquipment => "fldv_FiveMEquipment";
public static string fFiveMProcedures => "fldv_FiveMProcedures";
public static string fFiveMResources => "fldv_FiveMResources";
public static string fFiveMPhysicalEnvironment => "fldv_FiveMPhysicalEnvironment";
public static string fFiveMOperationalEnvironment => "fldv_FiveMOperationalEnvironment";

// Step 2 Fields  
public static string fHazardIds => "fldv_HazardIds";
public static string fHazardDescriptions => "fldv_HazardDescriptions";
public static string fHazardCategories => "fldv_HazardCategories";

// Step 3 Fields
public static string fRiskAnalysisMethod => "fldv_RiskAnalysisMethod";
public static string fHazardAnalysesData => "fldv_HazardAnalysesData";
public static string fHazardWorstOutcomes => "fldv_HazardWorstOutcomes";
public static string fHazardRootCauses => "fldv_HazardRootCauses";

// Step 4 Fields
public static string fTolerabilityFramework => "fldv_TolerabilityFramework";
public static string fHazardPanelScores => "fldv_HazardPanelScores";
public static string fFinalSeverityScore => "fldi_FinalSeverityScore";
public static string fFinalLikelihoodScore => "fldi_FinalLikelihoodScore";

// Step 5 Fields
public static string fImplementationStrategy => "fldv_ImplementationStrategy";
public static string fOverallTargetDate => "fldd_OverallTargetDate";
public static string fSavedMitigationStrategies => "fldv_SavedMitigationStrategies";

// Progress Fields
public static string fCurrentStep => "fldi_CurrentStep";
public static string fCompletedSteps => "fldv_CompletedSteps";
public static string fCompletionPercentage => "fldi_CompletionPercentage";
```

This comprehensive schema update supports all the Step 1-5 properties that are being bound in your Risk Assessment Wizard! ??