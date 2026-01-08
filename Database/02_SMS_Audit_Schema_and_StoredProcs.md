# SMS Audit - Table Schema and Stored Procedures

## Table Definition: tbld_SMSAudits

```sql
-- ============================================= 
-- SMS AUDIT TABLE SCHEMA
-- ============================================= 

USE [PDXSMS_V2]
GO

CREATE TABLE [dbo].[tbld_SMSAudits]
(
    -- Primary Key
    [fldi_ID] INT IDENTITY(1,1) NOT NULL,
    
    -- Business Key (Code)
    [fldv_Code] NVARCHAR(50) NOT NULL,
    
    -- Core Fields
    [fldv_Name] NVARCHAR(200) NOT NULL,
    [fldv_Description] NVARCHAR(800) NULL,
    [fldv_AuditPlanCode] NVARCHAR(50) NULL, -- FK to tbld_SMSAuditPlans
    [fldv_AuditType] NVARCHAR(60) NOT NULL,
    [fldv_Scope] NVARCHAR(800) NOT NULL,
    [fldv_Objectives] NVARCHAR(800) NULL,
    
    -- Timeline Fields
    [fldd_ScheduledStartDate] DATETIME NOT NULL,
    [fldd_ScheduledEndDate] DATETIME NOT NULL,
    [fldd_ActualStartDate] DATETIME NULL,
    [fldd_ActualEndDate] DATETIME NULL,
    
    -- Responsibility Fields
    [fldv_LeadAuditor] NVARCHAR(200) NOT NULL,
    [fldv_AuditorTeam] NVARCHAR(800) NULL,
    [fldv_ResponsibleDepartment] NVARCHAR(200) NOT NULL,
    [fldv_ContactPerson] NVARCHAR(200) NULL,
    [fldv_AuditLocation] NVARCHAR(200) NULL,
    
    -- Status Fields
    [fldv_Status] NVARCHAR(50) NOT NULL DEFAULT 'Scheduled',
    [fldv_Priority] NVARCHAR(20) NOT NULL DEFAULT 'Medium',
    
    -- Findings Summary Fields
    [fldi_TotalFindings] INT NOT NULL DEFAULT 0,
    [fldi_CriticalFindings] INT NOT NULL DEFAULT 0,
    [fldi_MajorFindings] INT NOT NULL DEFAULT 0,
    [fldi_MinorFindings] INT NOT NULL DEFAULT 0,
    [fldi_Observations] INT NOT NULL DEFAULT 0,
    
    -- Report Fields
    [fldd_ReportSubmittedDate] DATETIME NULL,
    [fldv_ExecutiveSummary] NVARCHAR(2000) NULL,
    [fldv_Notes] NVARCHAR(2000) NULL,
    
    -- Audit fields (inherited from BaseAuditableEntity)
    [fldv_CreatedBy] VARCHAR(50) NOT NULL DEFAULT 'SYSTEM',
    [fldd_CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
    [fldv_UpdatedBy] VARCHAR(50) NULL,
    [fldd_UpdatedDate] DATETIME NULL,
    
    -- Primary Key Constraint
    CONSTRAINT [PK_tbld_SMSAudits] PRIMARY KEY CLUSTERED ([fldi_ID] ASC),
    
    -- Unique Constraint on Code
    CONSTRAINT [UC_tbld_SMSAudits_Code] UNIQUE NONCLUSTERED ([fldv_Code] ASC),
    
    -- Foreign Key Constraint
    CONSTRAINT [FK_tbld_SMSAudits_AuditPlan] FOREIGN KEY ([fldv_AuditPlanCode]) 
        REFERENCES [dbo].[tbld_SMSAuditPlans] ([fldv_Code])
)
ON [PRIMARY]
GO

-- Indexes for performance
CREATE INDEX [IX_tbld_SMSAudits_Status] ON [dbo].[tbld_SMSAudits] ([fldv_Status])
CREATE INDEX [IX_tbld_SMSAudits_AuditType] ON [dbo].[tbld_SMSAudits] ([fldv_AuditType])
CREATE INDEX [IX_tbld_SMSAudits_Department] ON [dbo].[tbld_SMSAudits] ([fldv_ResponsibleDepartment])
CREATE INDEX [IX_tbld_SMSAudits_LeadAuditor] ON [dbo].[tbld_SMSAudits] ([fldv_LeadAuditor])
CREATE INDEX [IX_tbld_SMSAudits_Dates] ON [dbo].[tbld_SMSAudits] ([fldd_ScheduledStartDate], [fldd_ScheduledEndDate])
CREATE INDEX [IX_tbld_SMSAudits_AuditPlan] ON [dbo].[tbld_SMSAudits] ([fldv_AuditPlanCode])
GO
```

## Stored Procedures

### 1. SMS Audit Insert

```sql
-- ============================================= 
-- SMS AUDIT CRUD STORED PROCEDURES
-- ============================================= 

USE [PDXSMS_V2]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- ============================================= 
-- SMS AUDIT INSERT OPERATION
-- ============================================= 
CREATE OR ALTER PROCEDURE [dbo].[pr_SMSAudit_Insert]
    @pCode NVARCHAR(50) = NULL,
    @pName NVARCHAR(200),
    @pDescription NVARCHAR(800) = NULL,
    @pAuditPlanCode NVARCHAR(50) = NULL,
    @pAuditType NVARCHAR(60),
    @pScope NVARCHAR(800),
    @pObjectives NVARCHAR(800) = NULL,
    @pScheduledStartDate DATETIME,
    @pScheduledEndDate DATETIME,
    @pLeadAuditor NVARCHAR(200),
    @pAuditorTeam NVARCHAR(800) = NULL,
    @pResponsibleDepartment NVARCHAR(200),
    @pContactPerson NVARCHAR(200) = NULL,
    @pAuditLocation NVARCHAR(200) = NULL,
    @pStatus NVARCHAR(50) = 'Scheduled',
    @pPriority NVARCHAR(20) = 'Medium',
    @pNotes NVARCHAR(2000) = NULL,
    @pCreatedBy VARCHAR(50) = 'SYSTEM',
    @pCreatedDate DATETIME = NULL,
    @pNewID INT OUTPUT,
    @pNewAuditCode NVARCHAR(50) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(10) = COALESCE(@pCreatedBy, 'SYSTEM'),
            @FunctionName VARCHAR(50) = 'pr_SMSAudit_Insert';
    
    IF @pCreatedDate IS NULL SET @pCreatedDate = GETDATE();
    
    BEGIN TRY
        EXEC [dbo].[sp_AddAuditLogEntry] 
            @pUserID = @UserID, 
            @pMessageType = 'SMS_CRUD', 
            @pSeverity = 'Information', 
            @pModule = 'SMS_AuditManagement', 
            @pFunction = @FunctionName, 
            @pDescription = 'Starting SMS Audit insert operation';

        -- Generate Audit Code
        EXEC [pr_GenerateFormattedCode] 
            @EntityName = 'SMSAudit',           -- Uses registry: Table='tbld_SMSAudits', Prefix='AU'
            @GeneratedCode = @pNewAuditCode OUTPUT;

        INSERT INTO [dbo].[tbld_SMSAudits] (
            [fldv_Code], [fldv_Name], [fldv_Description], [fldv_AuditPlanCode], [fldv_AuditType], 
            [fldv_Scope], [fldv_Objectives], [fldd_ScheduledStartDate], [fldd_ScheduledEndDate], 
            [fldv_LeadAuditor], [fldv_AuditorTeam], [fldv_ResponsibleDepartment], [fldv_ContactPerson], 
            [fldv_AuditLocation], [fldv_Status], [fldv_Priority], [fldv_Notes], [fldv_CreatedBy], [fldd_CreatedDate]
        )
        VALUES (
            @pNewAuditCode, @pName, @pDescription, @pAuditPlanCode, @pAuditType, 
            @pScope, @pObjectives, @pScheduledStartDate, @pScheduledEndDate, 
            @pLeadAuditor, @pAuditorTeam, @pResponsibleDepartment, @pContactPerson, 
            @pAuditLocation, @pStatus, @pPriority, @pNotes, @pCreatedBy, @pCreatedDate
        );

        SET @pNewID = SCOPE_IDENTITY();
        SET @AuditMessage = 'Successfully inserted SMS Audit with ID: ' + CAST(@pNewID AS VARCHAR(10)) + ', Code: ' + COALESCE(@pNewAuditCode, 'NULL');
        
        EXEC [dbo].[sp_AddAuditLogEntry] 
            @pUserID = @UserID, 
            @pMessageType = 'SMS_CRUD', 
            @pSeverity = 'Information', 
            @pModule = 'SMS_AuditManagement', 
            @pFunction = @FunctionName, 
            @pDescription = @AuditMessage;
            
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error inserting SMS Audit: ' + @ErrorMessage;
        
        EXEC [dbo].[sp_AddAuditLogEntry] 
            @pUserID = @UserID, 
            @pMessageType = 'SMS_CRUD', 
            @pSeverity = 'Error', 
            @pModule = 'SMS_AuditManagement', 
            @pFunction = @FunctionName, 
            @pDescription = @AuditMessage;
            
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO
```

### 2. SMS Audit Get By Code

```sql
-- ============================================= 
-- SMS AUDIT GET BY CODE OPERATION
-- ============================================= 
CREATE OR ALTER PROCEDURE [dbo].[pr_SMSAudit_GetByCode]
    @pCode NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [fldi_ID],
        [fldv_Code],
        [fldv_Name],
        [fldv_Description],
        [fldv_AuditPlanCode],
        [fldv_AuditType],
        [fldv_Scope],
        [fldv_Objectives],
        [fldd_ScheduledStartDate],
        [fldd_ScheduledEndDate],
        [fldd_ActualStartDate],
        [fldd_ActualEndDate],
        [fldv_LeadAuditor],
        [fldv_AuditorTeam],
        [fldv_ResponsibleDepartment],
        [fldv_ContactPerson],
        [fldv_AuditLocation],
        [fldv_Status],
        [fldv_Priority],
        [fldi_TotalFindings],
        [fldi_CriticalFindings],
        [fldi_MajorFindings],
        [fldi_MinorFindings],
        [fldi_Observations],
        [fldd_ReportSubmittedDate],
        [fldv_ExecutiveSummary],
        [fldv_Notes],
        [fldv_CreatedBy],
        [fldd_CreatedDate],
        [fldv_UpdatedBy],
        [fldd_UpdatedDate]
    FROM [dbo].[tbld_SMSAudits]
    WHERE [fldv_Code] = @pCode;
END;
GO
```

### 3. SMS Audit Get All

```sql
-- ============================================= 
-- SMS AUDIT GET ALL OPERATION
-- ============================================= 
CREATE OR ALTER PROCEDURE [dbo].[pr_SMSAudit_GetAll]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [fldi_ID],
        [fldv_Code],
        [fldv_Name],
        [fldv_Description],
        [fldv_AuditPlanCode],
        [fldv_AuditType],
        [fldv_Scope],
        [fldv_Objectives],
        [fldd_ScheduledStartDate],
        [fldd_ScheduledEndDate],
        [fldd_ActualStartDate],
        [fldd_ActualEndDate],
        [fldv_LeadAuditor],
        [fldv_AuditorTeam],
        [fldv_ResponsibleDepartment],
        [fldv_ContactPerson],
        [fldv_AuditLocation],
        [fldv_Status],
        [fldv_Priority],
        [fldi_TotalFindings],
        [fldi_CriticalFindings],
        [fldi_MajorFindings],
        [fldi_MinorFindings],
        [fldi_Observations],
        [fldd_ReportSubmittedDate],
        [fldv_ExecutiveSummary],
        [fldv_Notes],
        [fldv_CreatedBy],
        [fldd_CreatedDate],
        [fldv_UpdatedBy],
        [fldd_UpdatedDate]
    FROM [dbo].[tbld_SMSAudits]
    ORDER BY [fldd_CreatedDate] DESC;
END;
GO
```

### 4. SMS Audit Update

```sql
-- ============================================= 
-- SMS AUDIT UPDATE OPERATION
-- ============================================= 
CREATE OR ALTER PROCEDURE [dbo].[pr_SMSAudit_Update]
    @pID INT,
    @pCode NVARCHAR(50),
    @pName NVARCHAR(200),
    @pDescription NVARCHAR(800) = NULL,
    @pAuditPlanCode NVARCHAR(50) = NULL,
    @pAuditType NVARCHAR(60),
    @pScope NVARCHAR(800),
    @pObjectives NVARCHAR(800) = NULL,
    @pScheduledStartDate DATETIME,
    @pScheduledEndDate DATETIME,
    @pActualStartDate DATETIME = NULL,
    @pActualEndDate DATETIME = NULL,
    @pLeadAuditor NVARCHAR(200),
    @pAuditorTeam NVARCHAR(800) = NULL,
    @pResponsibleDepartment NVARCHAR(200),
    @pContactPerson NVARCHAR(200) = NULL,
    @pAuditLocation NVARCHAR(200) = NULL,
    @pStatus NVARCHAR(50),
    @pPriority NVARCHAR(20),
    @pExecutiveSummary NVARCHAR(2000) = NULL,
    @pNotes NVARCHAR(2000) = NULL,
    @pUpdatedBy VARCHAR(50),
    @pUpdatedDate DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(10) = COALESCE(@pUpdatedBy, 'SYSTEM'),
            @FunctionName VARCHAR(50) = 'pr_SMSAudit_Update';
    
    IF @pUpdatedDate IS NULL SET @pUpdatedDate = GETDATE();
    
    BEGIN TRY
        EXEC [dbo].[sp_AddAuditLogEntry] 
            @pUserID = @UserID, 
            @pMessageType = 'SMS_CRUD', 
            @pSeverity = 'Information', 
            @pModule = 'SMS_AuditManagement', 
            @pFunction = @FunctionName, 
            @pDescription = 'Starting SMS Audit update operation';

        UPDATE [dbo].[tbld_SMSAudits]
        SET 
            [fldv_Name] = @pName,
            [fldv_Description] = @pDescription,
            [fldv_AuditPlanCode] = @pAuditPlanCode,
            [fldv_AuditType] = @pAuditType,
            [fldv_Scope] = @pScope,
            [fldv_Objectives] = @pObjectives,
            [fldd_ScheduledStartDate] = @pScheduledStartDate,
            [fldd_ScheduledEndDate] = @pScheduledEndDate,
            [fldd_ActualStartDate] = @pActualStartDate,
            [fldd_ActualEndDate] = @pActualEndDate,
            [fldv_LeadAuditor] = @pLeadAuditor,
            [fldv_AuditorTeam] = @pAuditorTeam,
            [fldv_ResponsibleDepartment] = @pResponsibleDepartment,
            [fldv_ContactPerson] = @pContactPerson,
            [fldv_AuditLocation] = @pAuditLocation,
            [fldv_Status] = @pStatus,
            [fldv_Priority] = @pPriority,
            [fldv_ExecutiveSummary] = @pExecutiveSummary,
            [fldv_Notes] = @pNotes,
            [fldv_UpdatedBy] = @pUpdatedBy,
            [fldd_UpdatedDate] = @pUpdatedDate
        WHERE [fldi_ID] = @pID;

        SET @AuditMessage = 'Successfully updated SMS Audit with ID: ' + CAST(@pID AS VARCHAR(10)) + ', Code: ' + COALESCE(@pCode, 'NULL');
        
        EXEC [dbo].[sp_AddAuditLogEntry] 
            @pUserID = @UserID, 
            @pMessageType = 'SMS_CRUD', 
            @pSeverity = 'Information', 
            @pModule = 'SMS_AuditManagement', 
            @pFunction = @FunctionName, 
            @pDescription = @AuditMessage;
            
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error updating SMS Audit: ' + @ErrorMessage;
        
        EXEC [dbo].[sp_AddAuditLogEntry] 
            @pUserID = @UserID, 
            @pMessageType = 'SMS_CRUD', 
            @pSeverity = 'Error', 
            @pModule = 'SMS_AuditManagement', 
            @pFunction = @FunctionName, 
            @pDescription = @AuditMessage;
            
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO
```

### 5. SMS Audit Delete

```sql
-- ============================================= 
-- SMS AUDIT DELETE OPERATION
-- ============================================= 
CREATE OR ALTER PROCEDURE [dbo].[pr_SMSAudit_Delete]
    @pCode NVARCHAR(50),
    @pDeletedBy VARCHAR(50) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(10) = COALESCE(@pDeletedBy, 'SYSTEM'),
            @FunctionName VARCHAR(50) = 'pr_SMSAudit_Delete';
    
    BEGIN TRY
        EXEC [dbo].[sp_AddAuditLogEntry] 
            @pUserID = @UserID, 
            @pMessageType = 'SMS_CRUD', 
            @pSeverity = 'Information', 
            @pModule = 'SMS_AuditManagement', 
            @pFunction = @FunctionName, 
            @pDescription = 'Starting SMS Audit delete operation';

        -- Check for dependent records (findings, evidence)
        IF EXISTS (SELECT 1 FROM [dbo].[tbld_SMSAuditFindings] WHERE [fldv_AuditCode] = @pCode)
            OR EXISTS (SELECT 1 FROM [dbo].[tbld_SMSAuditEvidence] WHERE [fldv_AuditCode] = @pCode)
        BEGIN
            RAISERROR('Cannot delete audit with existing findings or evidence. Delete dependent records first.', 16, 1);
            RETURN;
        END

        DELETE FROM [dbo].[tbld_SMSAudits]
        WHERE [fldv_Code] = @pCode;

        SET @AuditMessage = 'Successfully deleted SMS Audit with Code: ' + COALESCE(@pCode, 'NULL');
        
        EXEC [dbo].[sp_AddAuditLogEntry] 
            @pUserID = @UserID, 
            @pMessageType = 'SMS_CRUD', 
            @pSeverity = 'Information', 
            @pModule = 'SMS_AuditManagement', 
            @pFunction = @FunctionName, 
            @pDescription = @AuditMessage;
            
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error deleting SMS Audit: ' + @ErrorMessage;
        
        EXEC [dbo].[sp_AddAuditLogEntry] 
            @pUserID = @UserID, 
            @pMessageType = 'SMS_CRUD', 
            @pSeverity = 'Error', 
            @pModule = 'SMS_AuditManagement', 
            @pFunction = @FunctionName, 
            @pDescription = @AuditMessage;
            
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO
```

## Utility Procedure - Update Findings Summary

```sql
-- ============================================= 
-- UTILITY: UPDATE AUDIT FINDINGS SUMMARY
-- ============================================= 
CREATE OR ALTER PROCEDURE [dbo].[pr_SMSAudit_UpdateFindingsSummary]
    @pAuditCode NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @TotalFindings INT = 0, @Critical INT = 0, @Major INT = 0, @Minor INT = 0, @Observations INT = 0;
    
    -- Count findings by severity
    SELECT 
        @TotalFindings = COUNT(*),
        @Critical = SUM(CASE WHEN [fldv_Severity] = 'Critical' THEN 1 ELSE 0 END),
        @Major = SUM(CASE WHEN [fldv_Severity] = 'Major' THEN 1 ELSE 0 END),
        @Minor = SUM(CASE WHEN [fldv_Severity] = 'Minor' THEN 1 ELSE 0 END),
        @Observations = SUM(CASE WHEN [fldv_Severity] = 'Observation' THEN 1 ELSE 0 END)
    FROM [dbo].[tbld_SMSAuditFindings]
    WHERE [fldv_AuditCode] = @pAuditCode;
    
    -- Update audit summary
    UPDATE [dbo].[tbld_SMSAudits]
    SET 
        [fldi_TotalFindings] = @TotalFindings,
        [fldi_CriticalFindings] = @Critical,
        [fldi_MajorFindings] = @Major,
        [fldi_MinorFindings] = @Minor,
        [fldi_Observations] = @Observations
    WHERE [fldv_Code] = @pAuditCode;
END;
GO
```

## Code Generation Registry Entry

```sql
-- Registry entry for SMS Audit code generation
INSERT INTO [dbo].[tbld_CodeGenerationRegistry] ([fldv_EntityName], [fldv_TableName], [fldv_Prefix], [fldi_StartingNumber], [fldi_CurrentNumber])
VALUES ('SMSAudit', 'tbld_SMSAudits', 'AU', 1000, 1000);
GO
```

## Sample Data (Optional)

```sql
-- Sample SMS Audit data
INSERT INTO [dbo].[tbld_SMSAudits] (
    [fldv_Code], [fldv_Name], [fldv_Description], [fldv_AuditPlanCode], [fldv_AuditType], 
    [fldv_Scope], [fldd_ScheduledStartDate], [fldd_ScheduledEndDate], [fldv_LeadAuditor], 
    [fldv_ResponsibleDepartment], [fldv_Status], [fldv_CreatedBy]
)
VALUES 
    ('AU1001', 'Q1 2025 SMS Implementation Audit', 'Audit of SMS implementation across all departments', 'AP1001', 'Internal', 'SMS processes and documentation', '2025-01-15', '2025-01-30', 'John Smith', 'Safety', 'In Progress', 'SYSTEM'),
    ('AU1002', 'Ground Operations Safety Review', 'Focused audit on ground handling safety procedures', 'AP1002', 'Internal', 'Ground operations safety', '2025-02-01', '2025-02-05', 'Jane Doe', 'Operations', 'Scheduled', 'SYSTEM');
GO
```