# SMS Audit Management - Database Triggers and Views

## Database Triggers

### 1. Auto-update Audit Findings Summary Trigger

```sql
-- ============================================= 
-- SMS AUDIT MANAGEMENT DATABASE TRIGGERS
-- ============================================= 

USE [PDXSMS_V2]
GO

-- ============================================= 
-- TRIGGER: Auto-update Audit Findings Summary
-- ============================================= 
CREATE OR ALTER TRIGGER [tr_SMSAuditFindings_UpdateSummary]
ON [dbo].[tbld_SMSAuditFindings]
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Handle INSERT and UPDATE
    IF EXISTS(SELECT 1 FROM inserted)
    BEGIN
        UPDATE a
        SET 
            [fldi_TotalFindings] = fs.TotalFindings,
            [fldi_CriticalFindings] = fs.CriticalFindings,
            [fldi_MajorFindings] = fs.MajorFindings,
            [fldi_MinorFindings] = fs.MinorFindings,
            [fldi_Observations] = fs.Observations
        FROM [dbo].[tbld_SMSAudits] a
        INNER JOIN (
            SELECT 
                i.[fldv_AuditCode],
                COUNT(*) as TotalFindings,
                SUM(CASE WHEN f.[fldv_Severity] = 'Critical' THEN 1 ELSE 0 END) as CriticalFindings,
                SUM(CASE WHEN f.[fldv_Severity] = 'Major' THEN 1 ELSE 0 END) as MajorFindings,
                SUM(CASE WHEN f.[fldv_Severity] = 'Minor' THEN 1 ELSE 0 END) as MinorFindings,
                SUM(CASE WHEN f.[fldv_Severity] = 'Observation' THEN 1 ELSE 0 END) as Observations
            FROM inserted i
            INNER JOIN [dbo].[tbld_SMSAuditFindings] f ON i.[fldv_AuditCode] = f.[fldv_AuditCode]
            GROUP BY i.[fldv_AuditCode]
        ) fs ON a.[fldv_Code] = fs.[fldv_AuditCode];
    END
    
    -- Handle DELETE
    IF EXISTS(SELECT 1 FROM deleted) AND NOT EXISTS(SELECT 1 FROM inserted)
    BEGIN
        UPDATE a
        SET 
            [fldi_TotalFindings] = fs.TotalFindings,
            [fldi_CriticalFindings] = fs.CriticalFindings,
            [fldi_MajorFindings] = fs.MajorFindings,
            [fldi_MinorFindings] = fs.MinorFindings,
            [fldi_Observations] = fs.Observations
        FROM [dbo].[tbld_SMSAudits] a
        INNER JOIN (
            SELECT 
                d.[fldv_AuditCode],
                ISNULL(COUNT(f.[fldi_ID]), 0) as TotalFindings,
                ISNULL(SUM(CASE WHEN f.[fldv_Severity] = 'Critical' THEN 1 ELSE 0 END), 0) as CriticalFindings,
                ISNULL(SUM(CASE WHEN f.[fldv_Severity] = 'Major' THEN 1 ELSE 0 END), 0) as MajorFindings,
                ISNULL(SUM(CASE WHEN f.[fldv_Severity] = 'Minor' THEN 1 ELSE 0 END), 0) as MinorFindings,
                ISNULL(SUM(CASE WHEN f.[fldv_Severity] = 'Observation' THEN 1 ELSE 0 END), 0) as Observations
            FROM deleted d
            LEFT JOIN [dbo].[tbld_SMSAuditFindings] f ON d.[fldv_AuditCode] = f.[fldv_AuditCode]
            GROUP BY d.[fldv_AuditCode]
        ) fs ON a.[fldv_Code] = fs.[fldv_AuditCode];
    END
END;
GO
```

### 2. Audit Status Update Trigger

```sql
-- ============================================= 
-- TRIGGER: Auto-update Audit Status based on dates
-- ============================================= 
CREATE OR ALTER TRIGGER [tr_SMSAudits_UpdateStatus]
ON [dbo].[tbld_SMSAudits]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Auto-update status based on actual dates
    UPDATE [dbo].[tbld_SMSAudits]
    SET [fldv_Status] = CASE
        WHEN [fldd_ActualEndDate] IS NOT NULL THEN 'Completed'
        WHEN [fldd_ActualStartDate] IS NOT NULL AND [fldd_ActualEndDate] IS NULL THEN 'In Progress'
        WHEN [fldd_ActualStartDate] IS NULL AND [fldd_ScheduledStartDate] <= GETDATE() THEN 'Overdue'
        ELSE [fldv_Status]
    END
    WHERE [fldi_ID] IN (SELECT [fldi_ID] FROM inserted)
      AND ([fldd_ActualStartDate] IS NOT NULL OR [fldd_ActualEndDate] IS NOT NULL);
END;
GO
```

## Database Views

### 1. Audit Management Dashboard View

```sql
-- ============================================= 
-- SMS AUDIT MANAGEMENT DATABASE VIEWS
-- ============================================= 

-- ============================================= 
-- VIEW: Audit Management Dashboard
-- ============================================= 
CREATE OR ALTER VIEW [vw_SMS_AuditDashboard]
AS
SELECT 
    a.[fldi_ID],
    a.[fldv_Code] AS [AuditCode],
    a.[fldv_Name] AS [AuditName],
    a.[fldv_AuditType],
    a.[fldv_Status],
    a.[fldv_Priority],
    a.[fldv_LeadAuditor],
    a.[fldv_ResponsibleDepartment],
    a.[fldd_ScheduledStartDate],
    a.[fldd_ScheduledEndDate],
    a.[fldd_ActualStartDate],
    a.[fldd_ActualEndDate],
    a.[fldi_TotalFindings],
    a.[fldi_CriticalFindings],
    a.[fldi_MajorFindings],
    a.[fldi_MinorFindings],
    a.[fldi_Observations],
    
    -- Calculated fields
    CASE 
        WHEN a.[fldd_ActualEndDate] IS NOT NULL THEN 
            DATEDIFF(DAY, a.[fldd_ScheduledEndDate], a.[fldd_ActualEndDate])
        WHEN a.[fldd_ActualEndDate] IS NULL AND a.[fldd_ScheduledEndDate] < GETDATE() THEN
            DATEDIFF(DAY, a.[fldd_ScheduledEndDate], GETDATE())
        ELSE 0
    END AS [DaysOverdue],
    
    CASE 
        WHEN a.[fldd_ActualStartDate] IS NOT NULL AND a.[fldd_ActualEndDate] IS NOT NULL THEN
            DATEDIFF(DAY, a.[fldd_ActualStartDate], a.[fldd_ActualEndDate])
        WHEN a.[fldd_ActualStartDate] IS NOT NULL AND a.[fldd_ActualEndDate] IS NULL THEN
            DATEDIFF(DAY, a.[fldd_ActualStartDate], GETDATE())
        ELSE NULL
    END AS [ActualDurationDays],
    
    DATEDIFF(DAY, a.[fldd_ScheduledStartDate], a.[fldd_ScheduledEndDate]) AS [PlannedDurationDays],
    
    -- Progress indicators
    CASE 
        WHEN a.[fldv_Status] = 'Completed' THEN 100.0
        WHEN a.[fldv_Status] = 'In Progress' AND a.[fldd_ActualStartDate] IS NOT NULL THEN
            CASE 
                WHEN DATEDIFF(DAY, a.[fldd_ActualStartDate], GETDATE()) >= DATEDIFF(DAY, a.[fldd_ScheduledStartDate], a.[fldd_ScheduledEndDate]) THEN 90.0
                ELSE (CAST(DATEDIFF(DAY, a.[fldd_ActualStartDate], GETDATE()) AS FLOAT) / DATEDIFF(DAY, a.[fldd_ScheduledStartDate], a.[fldd_ScheduledEndDate])) * 80.0
            END
        WHEN a.[fldv_Status] = 'Scheduled' THEN 0.0
        ELSE 0.0
    END AS [ProgressPercentage],
    
    -- Risk indicators
    CASE 
        WHEN a.[fldi_CriticalFindings] > 0 THEN 'High'
        WHEN a.[fldi_MajorFindings] > 3 THEN 'High'
        WHEN a.[fldi_MajorFindings] > 0 OR a.[fldi_MinorFindings] > 5 THEN 'Medium'
        ELSE 'Low'
    END AS [RiskLevel],
    
    -- Audit Plan info
    ap.[fldv_Name] AS [AuditPlanName],
    ap.[fldv_Status] AS [AuditPlanStatus],
    
    -- Evidence count
    (SELECT COUNT(*) FROM [dbo].[tbld_SMSAuditEvidence] e WHERE e.[fldv_AuditCode] = a.[fldv_Code]) AS [EvidenceCount],
    
    a.[fldd_CreatedDate],
    a.[fldv_CreatedBy]
    
FROM [dbo].[tbld_SMSAudits] a
LEFT JOIN [dbo].[tbld_SMSAuditPlans] ap ON a.[fldv_AuditPlanCode] = ap.[fldv_Code];
GO
```

### 2. Finding Management View

```sql
-- ============================================= 
-- VIEW: Finding Management Dashboard
-- ============================================= 
CREATE OR ALTER VIEW [vw_SMS_FindingManagement]
AS
SELECT 
    f.[fldi_ID],
    f.[fldv_Code] AS [FindingCode],
    f.[fldv_Title],
    f.[fldv_Severity],
    f.[fldv_Category],
    f.[fldv_Status],
    f.[fldd_DiscoveredDate],
    f.[fldd_TargetResolutionDate],
    f.[fldd_ActualResolutionDate],
    f.[fldv_ResponsiblePerson],
    
    -- Audit information
    a.[fldv_Code] AS [AuditCode],
    a.[fldv_Name] AS [AuditName],
    a.[fldv_AuditType],
    a.[fldv_LeadAuditor],
    a.[fldv_ResponsibleDepartment],
    
    -- Calculated fields
    CASE 
        WHEN f.[fldd_ActualResolutionDate] IS NOT NULL THEN 0
        WHEN f.[fldd_TargetResolutionDate] IS NOT NULL AND f.[fldd_TargetResolutionDate] < GETDATE() THEN
            DATEDIFF(DAY, f.[fldd_TargetResolutionDate], GETDATE())
        ELSE 0
    END AS [DaysOverdue],
    
    CASE 
        WHEN f.[fldd_ActualResolutionDate] IS NOT NULL THEN
            DATEDIFF(DAY, f.[fldd_DiscoveredDate], f.[fldd_ActualResolutionDate])
        WHEN f.[fldd_ActualResolutionDate] IS NULL THEN
            DATEDIFF(DAY, f.[fldd_DiscoveredDate], GETDATE())
        ELSE NULL
    END AS [DaysToResolve],
    
    -- Priority calculation
    CASE 
        WHEN f.[fldv_Severity] = 'Critical' THEN 1
        WHEN f.[fldv_Severity] = 'Major' THEN 2
        WHEN f.[fldv_Severity] = 'Minor' THEN 3
        WHEN f.[fldv_Severity] = 'Observation' THEN 4
        ELSE 5
    END AS [SeverityOrder],
    
    -- Status indicators
    CASE 
        WHEN f.[fldv_Status] IN ('Closed', 'Resolved') THEN 'Complete'
        WHEN f.[fldd_TargetResolutionDate] IS NOT NULL AND f.[fldd_TargetResolutionDate] < GETDATE() THEN 'Overdue'
        WHEN f.[fldd_TargetResolutionDate] IS NOT NULL AND DATEDIFF(DAY, GETDATE(), f.[fldd_TargetResolutionDate]) <= 7 THEN 'Due Soon'
        ELSE 'On Track'
    END AS [StatusIndicator],
    
    -- Evidence count
    (SELECT COUNT(*) FROM [dbo].[tbld_SMSAuditEvidence] e WHERE e.[fldv_FindingCode] = f.[fldv_Code]) AS [EvidenceCount],
    
    f.[fldd_CreatedDate],
    f.[fldv_CreatedBy]
    
FROM [dbo].[tbld_SMSAuditFindings] f
INNER JOIN [dbo].[tbld_SMSAudits] a ON f.[fldv_AuditCode] = a.[fldv_Code];
GO
```

### 3. Evidence Management View

```sql
-- ============================================= 
-- VIEW: Evidence Management Dashboard
-- ============================================= 
CREATE OR ALTER VIEW [vw_SMS_EvidenceManagement]
AS
SELECT 
    e.[fldi_ID],
    e.[fldv_Code] AS [EvidenceCode],
    e.[fldv_Title],
    e.[fldv_EvidenceType],
    e.[fldv_Source],
    e.[fldv_CollectedBy],
    e.[fldd_CollectionDate],
    e.[fldv_ConfidentialityLevel],
    e.[fldi_RetentionPeriodMonths],
    e.[fldb_IsVerified],
    e.[fldb_IsArchived],
    
    -- Audit information
    a.[fldv_Code] AS [AuditCode],
    a.[fldv_Name] AS [AuditName],
    a.[fldv_LeadAuditor],
    
    -- Finding information (if linked)
    f.[fldv_Code] AS [FindingCode],
    f.[fldv_Title] AS [FindingTitle],
    f.[fldv_Severity] AS [FindingSeverity],
    
    -- File information
    e.[fldv_FilePath],
    e.[fldi_FileSize],
    e.[fldv_ContentType],
    e.[fldv_StorageLocation],
    
    -- Calculated fields
    DATEADD(MONTH, e.[fldi_RetentionPeriodMonths], e.[fldd_CollectionDate]) AS [RetentionExpiryDate],
    
    DATEDIFF(MONTH, GETDATE(), DATEADD(MONTH, e.[fldi_RetentionPeriodMonths], e.[fldd_CollectionDate])) AS [MonthsUntilExpiry],
    
    CASE 
        WHEN e.[fldb_IsArchived] = 1 THEN 'Archived'
        WHEN DATEADD(MONTH, e.[fldi_RetentionPeriodMonths], e.[fldd_CollectionDate]) <= DATEADD(MONTH, 6, GETDATE()) THEN 'Expiring Soon'
        WHEN DATEADD(MONTH, e.[fldi_RetentionPeriodMonths], e.[fldd_CollectionDate]) < GETDATE() THEN 'Expired'
        WHEN e.[fldb_IsVerified] = 0 THEN 'Needs Verification'
        ELSE 'Active'
    END AS [StatusIndicator],
    
    -- Size formatting
    CASE 
        WHEN e.[fldi_FileSize] > 1073741824 THEN CAST(ROUND(e.[fldi_FileSize] / 1073741824.0, 2) AS VARCHAR(20)) + ' GB'
        WHEN e.[fldi_FileSize] > 1048576 THEN CAST(ROUND(e.[fldi_FileSize] / 1048576.0, 2) AS VARCHAR(20)) + ' MB'
        WHEN e.[fldi_FileSize] > 1024 THEN CAST(ROUND(e.[fldi_FileSize] / 1024.0, 2) AS VARCHAR(20)) + ' KB'
        WHEN e.[fldi_FileSize] IS NOT NULL THEN CAST(e.[fldi_FileSize] AS VARCHAR(20)) + ' Bytes'
        ELSE NULL
    END AS [FormattedFileSize],
    
    e.[fldd_CreatedDate],
    e.[fldv_CreatedBy]
    
FROM [dbo].[tbld_SMSAuditEvidence] e
INNER JOIN [dbo].[tbld_SMSAudits] a ON e.[fldv_AuditCode] = a.[fldv_Code]
LEFT JOIN [dbo].[tbld_SMSAuditFindings] f ON e.[fldv_FindingCode] = f.[fldv_Code];
GO
```

### 4. Audit Performance Summary View

```sql
-- ============================================= 
-- VIEW: Audit Performance Summary
-- ============================================= 
CREATE OR ALTER VIEW [vw_SMS_AuditPerformance]
AS
SELECT 
    -- Time period groupings
    YEAR(a.[fldd_ScheduledStartDate]) AS [AuditYear],
    DATEPART(QUARTER, a.[fldd_ScheduledStartDate]) AS [AuditQuarter],
    DATENAME(MONTH, a.[fldd_ScheduledStartDate]) AS [AuditMonth],
    
    -- Audit type and department
    a.[fldv_AuditType],
    a.[fldv_ResponsibleDepartment],
    a.[fldv_LeadAuditor],
    
    -- Counts
    COUNT(*) AS [TotalAudits],
    SUM(CASE WHEN a.[fldv_Status] = 'Completed' THEN 1 ELSE 0 END) AS [CompletedAudits],
    SUM(CASE WHEN a.[fldv_Status] = 'In Progress' THEN 1 ELSE 0 END) AS [InProgressAudits],
    SUM(CASE WHEN a.[fldv_Status] = 'Overdue' THEN 1 ELSE 0 END) AS [OverdueAudits],
    
    -- Finding statistics
    SUM(a.[fldi_TotalFindings]) AS [TotalFindings],
    SUM(a.[fldi_CriticalFindings]) AS [TotalCriticalFindings],
    SUM(a.[fldi_MajorFindings]) AS [TotalMajorFindings],
    SUM(a.[fldi_MinorFindings]) AS [TotalMinorFindings],
    
    -- Averages
    AVG(CAST(a.[fldi_TotalFindings] AS FLOAT)) AS [AvgFindingsPerAudit],
    AVG(CASE 
        WHEN a.[fldd_ActualStartDate] IS NOT NULL AND a.[fldd_ActualEndDate] IS NOT NULL 
        THEN CAST(DATEDIFF(DAY, a.[fldd_ActualStartDate], a.[fldd_ActualEndDate]) AS FLOAT)
        ELSE NULL 
    END) AS [AvgAuditDurationDays],
    
    -- Performance metrics
    CAST(SUM(CASE WHEN a.[fldv_Status] = 'Completed' THEN 1 ELSE 0 END) AS FLOAT) / COUNT(*) * 100 AS [CompletionPercentage],
    
    AVG(CASE 
        WHEN a.[fldd_ActualEndDate] IS NOT NULL THEN 
            CAST(DATEDIFF(DAY, a.[fldd_ScheduledEndDate], a.[fldd_ActualEndDate]) AS FLOAT)
        ELSE NULL
    END) AS [AvgDaysOverSchedule]
    
FROM [dbo].[tbld_SMSAudits] a
GROUP BY 
    YEAR(a.[fldd_ScheduledStartDate]),
    DATEPART(QUARTER, a.[fldd_ScheduledStartDate]),
    DATENAME(MONTH, a.[fldd_ScheduledStartDate]),
    a.[fldv_AuditType],
    a.[fldv_ResponsibleDepartment],
    a.[fldv_LeadAuditor];
GO
```

## Summary of Database Objects Created

### Tables
1. `tbld_SMSAuditPlans` - Audit planning and scheduling
2. `tbld_SMSAudits` - Main audit execution tracking
3. `tbld_SMSAuditFindings` - Audit findings and corrective actions
4. `tbld_SMSAuditEvidence` - Evidence collection and management

### Stored Procedures (Per Table)
- Insert (with code generation)
- Get by Code
- Get All
- Update
- Delete

### Utility Procedures
- `pr_SMSAudit_UpdateFindingsSummary` - Maintains findings counts
- `pr_SMSAuditFinding_GetOverdue` - Identifies overdue findings
- `pr_SMSAuditEvidence_Archive` - Archives evidence
- `pr_SMSAuditEvidence_GetRetentionReview` - Retention management

### Triggers
- `tr_SMSAuditFindings_UpdateSummary` - Auto-updates finding counts
- `tr_SMSAudits_UpdateStatus` - Auto-updates audit status

### Views
- `vw_SMS_AuditDashboard` - Comprehensive audit overview
- `vw_SMS_FindingManagement` - Finding tracking and status
- `vw_SMS_EvidenceManagement` - Evidence lifecycle management
- `vw_SMS_AuditPerformance` - Performance analytics

### Code Generation Registry Entries
- SMSAuditPlan (Prefix: AP)
- SMSAudit (Prefix: AU)
- SMSAuditFinding (Prefix: AF)
- SMSAuditEvidence (Prefix: AE)

This comprehensive database schema provides full CRUD operations, business logic enforcement, performance optimization, and management reporting capabilities for the SMS Audit Management system.