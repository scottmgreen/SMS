# ?? **STORED PROCEDURES - CORRECTIONS & ADDITIONS ONLY**

## ?? **ADDITIONS NEEDED for StoredProcs.cs**

Based on the database stored procedures provided, the following entries need to be **ADDED** to `Infrastructure/Common/StoredProcs.cs`:

### **?? Missing SMS User Role Procedures (ALREADY ADDED)**
```csharp
// ? These were already added in previous corrections:
// pr_SMSUserRole_GetAll, pr_SMSUserRole_GetById, pr_SMSUserRole_GetByUserId, etc.
```

### **?? NEW ADDITIONS NEEDED:**

#### **1. Report Management Procedures**
```csharp
private static readonly Lazy<string> _pr_AddReport = new Lazy<string>(() => "pr_AddReport");
public static string pr_AddReport => _pr_AddReport.Value;

private static readonly Lazy<string> _pr_Report_GetAll = new Lazy<string>(() => "pr_Report_GetAll");
public static string pr_Report_GetAll => _pr_Report_GetAll.Value;

private static readonly Lazy<string> _pr_Report_GetById = new Lazy<string>(() => "pr_Report_GetById");
public static string pr_Report_GetById => _pr_Report_GetById.Value;

private static readonly Lazy<string> _pr_Report_Insert = new Lazy<string>(() => "pr_Report_Insert");
public static string pr_Report_Insert => _pr_Report_Insert.Value;

private static readonly Lazy<string> _pr_Report_Update = new Lazy<string>(() => "pr_Report_Update");
public static string pr_Report_Update => _pr_Report_Update.Value;

private static readonly Lazy<string> _pr_Report_Delete = new Lazy<string>(() => "pr_Report_Delete");
public static string pr_Report_Delete => _pr_Report_Delete.Value;
```

#### **2. Hazard Management Procedures**
```csharp
private static readonly Lazy<string> _pr_Hazard_GetAll = new Lazy<string>(() => "pr_Hazard_GetAll");
public static string pr_Hazard_GetAll => _pr_Hazard_GetAll.Value;

private static readonly Lazy<string> _pr_Hazard_GetById = new Lazy<string>(() => "pr_Hazard_GetById");
public static string pr_Hazard_GetById => _pr_Hazard_GetById.Value;

private static readonly Lazy<string> _pr_Hazard_Insert = new Lazy<string>(() => "pr_Hazard_Insert");
public static string pr_Hazard_Insert => _pr_Hazard_Insert.Value;

private static readonly Lazy<string> _pr_Hazard_Update = new Lazy<string>(() => "pr_Hazard_Update");
public static string pr_Hazard_Update => _pr_Hazard_Update.Value;

private static readonly Lazy<string> _pr_Hazard_Delete = new Lazy<string>(() => "pr_Hazard_Delete");
public static string pr_Hazard_Delete => _pr_Hazard_Delete.Value;
```

#### **3. Investigation Procedures**
```csharp
private static readonly Lazy<string> _pr_Investigation_GetAll = new Lazy<string>(() => "pr_Investigation_GetAll");
public static string pr_Investigation_GetAll => _pr_Investigation_GetAll.Value;

private static readonly Lazy<string> _pr_Investigation_GetById = new Lazy<string>(() => "pr_Investigation_GetById");
public static string pr_Investigation_GetById => _pr_Investigation_GetById.Value;

private static readonly Lazy<string> _pr_Investigation_Insert = new Lazy<string>(() => "pr_Investigation_Insert");
public static string pr_Investigation_Insert => _pr_Investigation_Insert.Value;

private static readonly Lazy<string> _pr_Investigation_Update = new Lazy<string>(() => "pr_Investigation_Update");
public static string pr_Investigation_Update => _pr_Investigation_Update.Value;

private static readonly Lazy<string> _pr_Investigation_Delete = new Lazy<string>(() => "pr_Investigation_Delete");
public static string pr_Investigation_Delete => _pr_Investigation_Delete.Value;
```

#### **4. Interview Procedures**
```csharp
private static readonly Lazy<string> _pr_Interview_GetAll = new Lazy<string>(() => "pr_Interview_GetAll");
public static string pr_Interview_GetAll => _pr_Interview_GetAll.Value;

private static readonly Lazy<string> _pr_Interview_GetById = new Lazy<string>(() => "pr_Interview_GetById");
public static string pr_Interview_GetById => _pr_Interview_GetById.Value;

private static readonly Lazy<string> _pr_Interview_Insert = new Lazy<string>(() => "pr_Interview_Insert");
public static string pr_Interview_Insert => _pr_Interview_Insert.Value;

private static readonly Lazy<string> _pr_Interview_Update = new Lazy<string>(() => "pr_Interview_Update");
public static string pr_Interview_Update => _pr_Interview_Update.Value;

private static readonly Lazy<string> _pr_Interview_Delete = new Lazy<string>(() => "pr_Interview_Delete");
public static string pr_Interview_Delete => _pr_Interview_Delete.Value;
```

#### **5. Mitigation Procedures**
```csharp
private static readonly Lazy<string> _pr_Mitigation_GetAll = new Lazy<string>(() => "pr_Mitigation_GetAll");
public static string pr_Mitigation_GetAll => _pr_Mitigation_GetAll.Value;

private static readonly Lazy<string> _pr_Mitigation_GetById = new Lazy<string>(() => "pr_Mitigation_GetById");
public static string pr_Mitigation_GetById => _pr_Mitigation_GetById.Value;

private static readonly Lazy<string> _pr_Mitigation_Insert = new Lazy<string>(() => "pr_Mitigation_Insert");
public static string pr_Mitigation_Insert => _pr_Mitigation_Insert.Value;

private static readonly Lazy<string> _pr_Mitigation_Update = new Lazy<string>(() => "pr_Mitigation_Update");
public static string pr_Mitigation_Update => _pr_Mitigation_Update.Value;

private static readonly Lazy<string> _pr_Mitigation_Delete = new Lazy<string>(() => "pr_Mitigation_Delete");
public static string pr_Mitigation_Delete => _pr_Mitigation_Delete.Value;
```

#### **6. Mitigation Assignment Procedures**
```csharp
private static readonly Lazy<string> _pr_MitigationAssignment_GetAll = new Lazy<string>(() => "pr_MitigationAssignment_GetAll");
public static string pr_MitigationAssignment_GetAll => _pr_MitigationAssignment_GetAll.Value;

private static readonly Lazy<string> _pr_MitigationAssignment_GetById = new Lazy<string>(() => "pr_MitigationAssignment_GetById");
public static string pr_MitigationAssignment_GetById => _pr_MitigationAssignment_GetById.Value;

private static readonly Lazy<string> _pr_MitigationAssignment_Insert = new Lazy<string>(() => "pr_MitigationAssignment_Insert");
public static string pr_MitigationAssignment_Insert => _pr_MitigationAssignment_Insert.Value;

private static readonly Lazy<string> _pr_MitigationAssignment_Update = new Lazy<string>(() => "pr_MitigationAssignment_Update");
public static string pr_MitigationAssignment_Update => _pr_MitigationAssignment_Update.Value;

private static readonly Lazy<string> _pr_MitigationAssignment_Delete = new Lazy<string>(() => "pr_MitigationAssignment_Delete");
public static string pr_MitigationAssignment_Delete => _pr_MitigationAssignment_Delete.Value;
```

#### **7. Report Validation Procedures**
```csharp
private static readonly Lazy<string> _pr_ReportValidation_GetAll = new Lazy<string>(() => "pr_ReportValidation_GetAll");
public static string pr_ReportValidation_GetAll => _pr_ReportValidation_GetAll.Value;

private static readonly Lazy<string> _pr_ReportValidation_GetById = new Lazy<string>(() => "pr_ReportValidation_GetById");
public static string pr_ReportValidation_GetById => _pr_ReportValidation_GetById.Value;

private static readonly Lazy<string> _pr_ReportValidation_Insert = new Lazy<string>(() => "pr_ReportValidation_Insert");
public static string pr_ReportValidation_Insert => _pr_ReportValidation_Insert.Value;

private static readonly Lazy<string> _pr_ReportValidation_Update = new Lazy<string>(() => "pr_ReportValidation_Update");
public static string pr_ReportValidation_Update => _pr_ReportValidation_Update.Value;

private static readonly Lazy<string> _pr_ReportValidation_Delete = new Lazy<string>(() => "pr_ReportValidation_Delete");
public static string pr_ReportValidation_Delete => _pr_ReportValidation_Delete.Value;
```

#### **8. Risk Analysis Procedures**
```csharp
private static readonly Lazy<string> _pr_RiskAnalysis_GetAll = new Lazy<string>(() => "pr_RiskAnalysis_GetAll");
public static string pr_RiskAnalysis_GetAll => _pr_RiskAnalysis_GetAll.Value;

private static readonly Lazy<string> _pr_RiskAnalysis_GetById = new Lazy<string>(() => "pr_RiskAnalysis_GetById");
public static string pr_RiskAnalysis_GetById => _pr_RiskAnalysis_GetById.Value;

private static readonly Lazy<string> _pr_RiskAnalysis_Insert = new Lazy<string>(() => "pr_RiskAnalysis_Insert");
public static string pr_RiskAnalysis_Insert => _pr_RiskAnalysis_Insert.Value;

private static readonly Lazy<string> _pr_RiskAnalysis_Update = new Lazy<string>(() => "pr_RiskAnalysis_Update");
public static string pr_RiskAnalysis_Update => _pr_RiskAnalysis_Update.Value;

private static readonly Lazy<string> _pr_RiskAnalysis_Delete = new Lazy<string>(() => "pr_RiskAnalysis_Delete");
public static string pr_RiskAnalysis_Delete => _pr_RiskAnalysis_Delete.Value;
```

#### **9. Risk Assessment Procedures**
```csharp
private static readonly Lazy<string> _pr_RiskAssessment_GetAll = new Lazy<string>(() => "pr_RiskAssessment_GetAll");
public static string pr_RiskAssessment_GetAll => _pr_RiskAssessment_GetAll.Value;

private static readonly Lazy<string> _pr_RiskAssessment_GetById = new Lazy<string>(() => "pr_RiskAssessment_GetById");
public static string pr_RiskAssessment_GetById => _pr_RiskAssessment_GetById.Value;

private static readonly Lazy<string> _pr_RiskAssessment_Insert = new Lazy<string>(() => "pr_RiskAssessment_Insert");
public static string pr_RiskAssessment_Insert => _pr_RiskAssessment_Insert.Value;

private static readonly Lazy<string> _pr_RiskAssessment_Update = new Lazy<string>(() => "pr_RiskAssessment_Update");
public static string pr_RiskAssessment_Update => _pr_RiskAssessment_Update.Value;

private static readonly Lazy<string> _pr_RiskAssessment_Delete = new Lazy<string>(() => "pr_RiskAssessment_Delete");
public static string pr_RiskAssessment_Delete => _pr_RiskAssessment_Delete.Value;
```

#### **10. Scoring Panel Procedures**
```csharp
private static readonly Lazy<string> _pr_ScoringPanel_GetAll = new Lazy<string>(() => "pr_ScoringPanel_GetAll");
public static string pr_ScoringPanel_GetAll => _pr_ScoringPanel_GetAll.Value;

private static readonly Lazy<string> _pr_ScoringPanel_GetById = new Lazy<string>(() => "pr_ScoringPanel_GetById");
public static string pr_ScoringPanel_GetById => _pr_ScoringPanel_GetById.Value;

private static readonly Lazy<string> _pr_ScoringPanel_Insert = new Lazy<string>(() => "pr_ScoringPanel_Insert");
public static string pr_ScoringPanel_Insert => _pr_ScoringPanel_Insert.Value;

private static readonly Lazy<string> _pr_ScoringPanel_Update = new Lazy<string>(() => "pr_ScoringPanel_Update");
public static string pr_ScoringPanel_Update => _pr_ScoringPanel_Update.Value;

private static readonly Lazy<string> _pr_ScoringPanel_Delete = new Lazy<string>(() => "pr_ScoringPanel_Delete");
public static string pr_ScoringPanel_Delete => _pr_ScoringPanel_Delete.Value;
```

#### **11. SMS User Management Procedures**
```csharp
// SMS Application Users
private static readonly Lazy<string> _pr_SMSApplicationUser_GetAll = new Lazy<string>(() => "pr_SMSApplicationUser_GetAll");
public static string pr_SMSApplicationUser_GetAll => _pr_SMSApplicationUser_GetAll.Value;

private static readonly Lazy<string> _pr_SMSApplicationUser_GetById = new Lazy<string>(() => "pr_SMSApplicationUser_GetById");
public static string pr_SMSApplicationUser_GetById => _pr_SMSApplicationUser_GetById.Value;

private static readonly Lazy<string> _pr_SMSApplicationUser_Insert = new Lazy<string>(() => "pr_SMSApplicationUser_Insert");
public static string pr_SMSApplicationUser_Insert => _pr_SMSApplicationUser_Insert.Value;

private static readonly Lazy<string> _pr_SMSApplicationUser_Update = new Lazy<string>(() => "pr_SMSApplicationUser_Update");
public static string pr_SMSApplicationUser_Update => _pr_SMSApplicationUser_Update.Value;

private static readonly Lazy<string> _pr_SMSApplicationUser_Delete = new Lazy<string>(() => "pr_SMSApplicationUser_Delete");
public static string pr_SMSApplicationUser_Delete => _pr_SMSApplicationUser_Delete.Value;

// SMS Organizational Users
private static readonly Lazy<string> _pr_SMSOrganizationalUser_GetAll = new Lazy<string>(() => "pr_SMSOrganizationalUser_GetAll");
public static string pr_SMSOrganizationalUser_GetAll => _pr_SMSOrganizationalUser_GetAll.Value;

private static readonly Lazy<string> _pr_SMSOrganizationalUser_GetById = new Lazy<string>(() => "pr_SMSOrganizationalUser_GetById");
public static string pr_SMSOrganizationalUser_GetById => _pr_SMSOrganizationalUser_GetById.Value;

// SMS Stakeholder Users
private static readonly Lazy<string> _pr_SMSStakeholderUser_GetAll = new Lazy<string>(() => "pr_SMSStakeholderUser_GetAll");
public static string pr_SMSStakeholderUser_GetAll => _pr_SMSStakeholderUser_GetAll.Value;

private static readonly Lazy<string> _pr_SMSStakeholderUser_GetById = new Lazy<string>(() => "pr_SMSStakeholderUser_GetById");
public static string pr_SMSStakeholderUser_GetById => _pr_SMSStakeholderUser_GetById.Value;

// SMS Roles
private static readonly Lazy<string> _pr_SMSRole_GetAll = new Lazy<string>(() => "pr_SMSRole_GetAll");
public static string pr_SMSRole_GetAll => _pr_SMSRole_GetAll.Value;

private static readonly Lazy<string> _pr_SMSRole_GetById = new Lazy<string>(() => "pr_SMSRole_GetById");
public static string pr_SMSRole_GetById => _pr_SMSRole_GetById.Value;

private static readonly Lazy<string> _pr_SMSRole_Insert = new Lazy<string>(() => "pr_SMSRole_Insert");
public static string pr_SMSRole_Insert => _pr_SMSRole_Insert.Value;
```

#### **12. Airport Shared Dataset Procedures**
```csharp
private static readonly Lazy<string> _pr_AirportSharedDataset_GetAll = new Lazy<string>(() => "pr_AirportSharedDataset_GetAll");
public static string pr_AirportSharedDataset_GetAll => _pr_AirportSharedDataset_GetAll.Value;

private static readonly Lazy<string> _pr_AirportSharedDataset_GetById = new Lazy<string>(() => "pr_AirportSharedDataset_GetById");
public static string pr_AirportSharedDataset_GetById => _pr_AirportSharedDataset_GetById.Value;

private static readonly Lazy<string> _pr_AirportSharedDataset_Insert = new Lazy<string>(() => "pr_AirportSharedDataset_Insert");
public static string pr_AirportSharedDataset_Insert => _pr_AirportSharedDataset_Insert.Value;

private static readonly Lazy<string> _pr_AirportSharedDataset_Update = new Lazy<string>(() => "pr_AirportSharedDataset_Update");
public static string pr_AirportSharedDataset_Update => _pr_AirportSharedDataset_Update.Value;

private static readonly Lazy<string> _pr_AirportSharedDataset_Delete = new Lazy<string>(() => "pr_AirportSharedDataset_Delete");
public static string pr_AirportSharedDataset_Delete => _pr_AirportSharedDataset_Delete.Value;
```

#### **13. Utility Procedures**
```csharp
private static readonly Lazy<string> _pr_GenerateFormattedCode = new Lazy<string>(() => "pr_GenerateFormattedCode");
public static string pr_GenerateFormattedCode => _pr_GenerateFormattedCode.Value;

private static readonly Lazy<string> _pr_SMS_CheckUsernameAvailability = new Lazy<string>(() => "pr_SMS_CheckUsernameAvailability");
public static string pr_SMS_CheckUsernameAvailability => _pr_SMS_CheckUsernameAvailability.Value;

private static readonly Lazy<string> _pr_SMS_GetUserStatisticsSummary = new Lazy<string>(() => "pr_SMS_GetUserStatisticsSummary");
public static string pr_SMS_GetUserStatisticsSummary => _pr_SMS_GetUserStatisticsSummary.Value;

private static readonly Lazy<string> _pr_TruncateAllTables = new Lazy<string>(() => "pr_TruncateAllTables");
public static string pr_TruncateAllTables => _pr_TruncateAllTables.Value;

private static readonly Lazy<string> _sp_AddAuditLogEntry = new Lazy<string>(() => "sp_AddAuditLogEntry");
public static string sp_AddAuditLogEntry => _sp_AddAuditLogEntry.Value;
```

---

## ?? **IMPLEMENTATION INSTRUCTIONS**

1. **ADD** all the above entries to your `Infrastructure/Common/StoredProcs.cs` file
2. **GROUP** them logically within the existing class structure
3. **MAINTAIN** the existing Lazy<string> pattern for consistency
4. **TEST** compilation after adding these entries

---

## ? **COMPLETION STATUS**

- **SMS User Role Procedures**: ? Already Added
- **Core SMS CRUD Procedures**: ?? Listed Above (Need Adding)
- **User Management Procedures**: ?? Listed Above (Need Adding)  
- **Airport Dataset Procedures**: ?? Listed Above (Need Adding)
- **Utility Procedures**: ?? Listed Above (Need Adding)

**Total New Additions Needed**: ~70+ stored procedure references

This completes the alignment between your C# codebase and the actual database stored procedures! ??