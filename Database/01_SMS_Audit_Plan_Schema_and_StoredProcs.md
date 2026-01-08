# SMS Audit Plan - Table Schema and Stored Procedures

## Table Definition: tbld_SMSAuditPlans

```sql
-- ============================================= 
-- SMS AUDIT PLAN TABLE SCHEMA
-- ============================================= 

USE [PDXSMS_V2]
GO

CREATE TABLE [dbo].[tbld_SMSAuditPlans]
(
    -- Primary Key
    [fldi_ID] INT IDENTITY(1,1) NOT NULL,
    
    -- Business Key (Code)
    [fldv_Code] NVARCHAR(50) NOT NULL,
    
    -- Core Fields
    [fldv_Name] NVARCHAR(200) NOT NULL,
    [fldv_Description] NVARCHAR(800) NULL,
    [fldv_AuditType] NVARCHAR(60) NOT NULL,
    [fldv_Scope] NVARCHAR(800) NOT NULL,
    [fldv_Objectives] NVARCHAR(800) NULL,
    
    -- Timeline Fields
    [fldd_PlannedStartDate] DATETIME NOT NULL,
    [fldd_PlannedEndDate] DATETIME NOT NULL,
    
    -- Responsibility Fields
    [fldv_LeadAuditor] NVARCHAR(200) NOT NULL,
    [fldv_AuditorTeam] NVARCHAR(800) NULL,
    [fldv_ResponsibleDepartment] NVARCHAR(200) NOT NULL,
    
    -- Status Fields
    [fldv_Status] NVARCHAR(50) NOT NULL DEFAULT 'Draft',
    [fldv_Priority] NVARCHAR(20) NOT NULL DEFAULT 'Medium',
    [fldv_RecurrencePattern] NVARCHAR(60) NULL,
    
    -- Approval Fields
    [fldb_RequiresApproval] BIT NOT NULL DEFAULT 0,
    [fldv_ApprovedBy] NVARCHAR(200) NULL,
    [fldd_ApprovedDate] DATETIME NULL,
    
    -- Planning Fields
    [fldi_ExpectedDurationHours] INT NULL,
    [fldv_Notes] NVARCHAR(2000) NULL,
    
    -- Audit fields (inherited from BaseAuditableEntity)
    [fldv_CreatedBy] VARCHAR(50) NOT NULL DEFAULT 'SYSTEM',
    [fldd_CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
    [fldv_UpdatedBy] VARCHAR(50) NULL,
    [fldd_UpdatedDate] DATETIME NULL,
    
    -- Primary Key Constraint
    CONSTRAINT [PK_tbld_SMSAuditPlans] PRIMARY KEY CLUSTERED ([fldi_ID] ASC),
    
    -- Unique Constraint on Code
    CONSTRAINT [UC_tbld_SMSAuditPlans_Code] UNIQUE NONCLUSTERED ([fldv_Code] ASC)
)
ON [PRIMARY]
GO

-- Indexes for performance
CREATE INDEX [IX_tbld_SMSAuditPlans_Status] ON [dbo].[tbld_SMSAuditPlans] ([fldv_Status])
CREATE INDEX [IX_tbld_SMSAuditPlans_AuditType] ON [dbo].[tbld_SMSAuditPlans] ([fldv_AuditType])
CREATE INDEX [IX_tbld_SMSAuditPlans_Department] ON [dbo].[tbld_SMSAuditPlans] ([fldv_ResponsibleDepartment])
CREATE INDEX [IX_tbld_SMSAuditPlans_LeadAuditor] ON [dbo].[tbld_SMSAuditPlans] ([fldv_LeadAuditor])
CREATE INDEX [IX_tbld_SMSAuditPlans_Dates] ON [dbo].[tbld_SMSAuditPlans] ([fldd_PlannedStartDate], [fldd_PlannedEndDate])
GO
```

## Stored Procedures

### 1. SMS Audit Plan Insert

```sql
-- ============================================= 
-- SMS AUDIT PLAN CRUD STORED PROCEDURES
-- ============================================= 

USE [PDXSMS_V2]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- ============================================= 
-- SMS AUDIT PLAN INSERT OPERATION
-- ============================================= 
CREATE OR ALTER PROCEDURE [dbo].[pr_SMSAuditPlan_Insert]
    @pCode NVARCHAR(50) = NULL,
    @pName NVARCHAR(200),
    @pDescription NVARCHAR(800) = NULL,
    @pAuditType NVARCHAR(60),
    @pScope NVARCHAR(800),
    @pObjectives NVARCHAR(800) = NULL,
    @pPlannedStartDate DATETIME,
    @pPlannedEndDate DATETIME,
    @pLeadAuditor NVARCHAR(200),
    @pAuditorTeam NVARCHAR(800) = NULL,
    @pResponsibleDepartment NVARCHAR(200),
    @pStatus NVARCHAR(50) = 'Draft',
    @pPriority NVARCHAR(20) = 'Medium',
    @pRecurrencePattern NVARCHAR(60) = NULL,
    @pRequiresApproval BIT = 0,
    @pExpectedDurationHours INT = NULL,
    @pNotes NVARCHAR(2000) = NULL,
    @pCreatedBy VARCHAR(50) = 'SYSTEM',
    @pCreatedDate DATETIME = NULL,
    @pNewID INT OUTPUT,
    @pNewAuditPlanCode NVARCHAR(50) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(10) = COALESCE(@pCreatedBy, 'SYSTEM'),
            @FunctionName VARCHAR(50) = 'pr_SMSAuditPlan_Insert';
    
    IF @pCreatedDate IS NULL SET @pCreatedDate = GETDATE();
    
    BEGIN TRY
        EXEC [dbo].[sp_AddAuditLogEntry] 
            @pUserID = @UserID, 
            @pMessageType = 'SMS_CRUD', 
            @pSeverity = 'Information', 
            @pModule = 'SMS_AuditManagement', 
            @pFunction = @FunctionName, 
            @pDescription = 'Starting SMS Audit Plan insert operation';

        -- Generate Audit Plan Code
        EXEC [pr_GenerateFormattedCode] 
            @EntityName = 'SMSAuditPlan',           -- Uses registry: Table='tbld_SMSAuditPlans', Prefix='AP'
            @GeneratedCode = @pNewAuditPlanCode OUTPUT;

        INSERT INTO [dbo].[tbld_SMSAuditPlans] (
            [fldv_Code], [fldv_Name], [fldv_Description], [fldv_AuditType], [fldv_Scope], 
            [fldv_Objectives], [fldd_PlannedStartDate], [fldd_PlannedEndDate], [fldv_LeadAuditor], 
            [fldv_AuditorTeam], [fldv_ResponsibleDepartment], [fldv_Status], [fldv_Priority], 
            [fldv_RecurrencePattern], [fldb_RequiresApproval], [fldi_ExpectedDurationHours], 
            [fldv_Notes], [fldv_CreatedBy], [fldd_CreatedDate]
        )
        VALUES (
            @pNewAuditPlanCode, @pName, @pDescription, @pAuditType, @pScope, 
            @pObjectives, @pPlannedStartDate, @pPlannedEndDate, @pLeadAuditor, 
            @pAuditorTeam, @pResponsibleDepartment, @pStatus, @pPriority, 
            @pRecurrencePattern, @pRequiresApproval, @pExpectedDurationHours, 
            @pNotes, @pCreatedBy, @pCreatedDate
        );

        SET @pNewID = SCOPE_IDENTITY();
        SET @AuditMessage = 'Successfully inserted SMS Audit Plan with ID: ' + CAST(@pNewID AS VARCHAR(10)) + ', Code: ' + COALESCE(@pNewAuditPlanCode, 'NULL');
        
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
        SET @AuditMessage = 'Error inserting SMS Audit Plan: ' + @ErrorMessage;
        
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

### 2. SMS Audit Plan Get By Code

```sql
-- ============================================= 
-- SMS AUDIT PLAN GET BY CODE OPERATION
-- ============================================= 
CREATE OR ALTER PROCEDURE [dbo].[pr_SMSAuditPlan_GetByCode]
    @pCode NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [fldi_ID],
        [fldv_Code],
        [fldv_Name],
        [fldv_Description],
        [fldv_AuditType],
        [fldv_Scope],
        [fldv_Objectives],
        [fldd_PlannedStartDate],
        [fldd_PlannedEndDate],
        [fldv_LeadAuditor],
        [fldv_AuditorTeam],
        [fldv_ResponsibleDepartment],
        [fldv_Status],
        [fldv_Priority],
        [fldv_RecurrencePattern],
        [fldb_RequiresApproval],
        [fldv_ApprovedBy],
        [fldd_ApprovedDate],
        [fldi_ExpectedDurationHours],
        [fldv_Notes],
        [fldv_CreatedBy],
        [fldd_CreatedDate],
        [fldv_UpdatedBy],
        [fldd_UpdatedDate]
    FROM [dbo].[tbld_SMSAuditPlans]
    WHERE [fldv_Code] = @pCode;
END;
GO
```

### 3. SMS Audit Plan Get All

```sql
-- ============================================= 
-- SMS AUDIT PLAN GET ALL OPERATION
-- ============================================= 
CREATE OR ALTER PROCEDURE [dbo].[pr_SMSAuditPlan_GetAll]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [fldi_ID],
        [fldv_Code],
        [fldv_Name],
        [fldv_Description],
        [fldv_AuditType],
        [fldv_Scope],
        [fldv_Objectives],
        [fldd_PlannedStartDate],
        [fldd_PlannedEndDate],
        [fldv_LeadAuditor],
        [fldv_AuditorTeam],
        [fldv_ResponsibleDepartment],
        [fldv_Status],
        [fldv_Priority],
        [fldv_RecurrencePattern],
        [fldb_RequiresApproval],
        [fldv_ApprovedBy],
        [fldd_ApprovedDate],
        [fldi_ExpectedDurationHours],
        [fldv_Notes],
        [fldv_CreatedBy],
        [fldd_CreatedDate],
        [fldv_UpdatedBy],
        [fldd_UpdatedDate]
    FROM [dbo].[tbld_SMSAuditPlans]
    ORDER BY [fldd_CreatedDate] DESC;
END;
GO
```

### 4. SMS Audit Plan Update

```sql
-- ============================================= 
-- SMS AUDIT PLAN UPDATE OPERATION
-- ============================================= 
CREATE OR ALTER PROCEDURE [dbo].[pr_SMSAuditPlan_Update]
    @pID INT,
    @pCode NVARCHAR(50),
    @pName NVARCHAR(200),
    @pDescription NVARCHAR(800) = NULL,
    @pAuditType NVARCHAR(60),
    @pScope NVARCHAR(800),
    @pObjectives NVARCHAR(800) = NULL,
    @pPlannedStartDate DATETIME,
    @pPlannedEndDate DATETIME,
    @pLeadAuditor NVARCHAR(200),
    @pAuditorTeam NVARCHAR(800) = NULL,
    @pResponsibleDepartment NVARCHAR(200),
    @pStatus NVARCHAR(50),
    @pPriority NVARCHAR(20),
    @pRecurrencePattern NVARCHAR(60) = NULL,
    @pRequiresApproval BIT,
    @pApprovedBy NVARCHAR(200) = NULL,
    @pApprovedDate DATETIME = NULL,
    @pExpectedDurationHours INT = NULL,
    @pNotes NVARCHAR(2000) = NULL,
    @pUpdatedBy VARCHAR(50),
    @pUpdatedDate DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(10) = COALESCE(@pUpdatedBy, 'SYSTEM'),
            @FunctionName VARCHAR(50) = 'pr_SMSAuditPlan_Update';
    
    IF @pUpdatedDate IS NULL SET @pUpdatedDate = GETDATE();
    
    BEGIN TRY
        EXEC [dbo].[sp_AddAuditLogEntry] 
            @pUserID = @UserID, 
            @pMessageType = 'SMS_CRUD', 
            @pSeverity = 'Information', 
            @pModule = 'SMS_AuditManagement', 
            @pFunction = @FunctionName, 
            @pDescription = 'Starting SMS Audit Plan update operation';

        UPDATE [dbo].[tbld_SMSAuditPlans]
        SET 
            [fldv_Name] = @pName,
            [fldv_Description] = @pDescription,
            [fldv_AuditType] = @pAuditType,
            [fldv_Scope] = @pScope,
            [fldv_Objectives] = @pObjectives,
            [fldd_PlannedStartDate] = @pPlannedStartDate,
            [fldd_PlannedEndDate] = @pPlannedEndDate,
            [fldv_LeadAuditor] = @pLeadAuditor,
            [fldv_AuditorTeam] = @pAuditorTeam,
            [fldv_ResponsibleDepartment] = @pResponsibleDepartment,
            [fldv_Status] = @pStatus,
            [fldv_Priority] = @pPriority,
            [fldv_RecurrencePattern] = @pRecurrencePattern,
            [fldb_RequiresApproval] = @pRequiresApproval,
            [fldv_ApprovedBy] = @pApprovedBy,
            [fldd_ApprovedDate] = @pApprovedDate,
            [fldi_ExpectedDurationHours] = @pExpectedDurationHours,
            [fldv_Notes] = @pNotes,
            [fldv_UpdatedBy] = @pUpdatedBy,
            [fldd_UpdatedDate] = @pUpdatedDate
        WHERE [fldi_ID] = @pID;

        SET @AuditMessage = 'Successfully updated SMS Audit Plan with ID: ' + CAST(@pID AS VARCHAR(10)) + ', Code: ' + COALESCE(@pCode, 'NULL');
        
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
        SET @AuditMessage = 'Error updating SMS Audit Plan: ' + @ErrorMessage;
        
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

### 5. SMS Audit Plan Delete

```sql
-- ============================================= 
-- SMS AUDIT PLAN DELETE OPERATION
-- ============================================= 
CREATE OR ALTER PROCEDURE [dbo].[pr_SMSAuditPlan_Delete]
    @pCode NVARCHAR(50),
    @pDeletedBy VARCHAR(50) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(10) = COALESCE(@pDeletedBy, 'SYSTEM'),
            @FunctionName VARCHAR(50) = 'pr_SMSAuditPlan_Delete';
    
    BEGIN TRY
        EXEC [dbo].[sp_AddAuditLogEntry] 
            @pUserID = @UserID, 
            @pMessageType = 'SMS_CRUD', 
            @pSeverity = 'Information', 
            @pModule = 'SMS_AuditManagement', 
            @pFunction = @FunctionName, 
            @pDescription = 'Starting SMS Audit Plan delete operation';

        DELETE FROM [dbo].[tbld_SMSAuditPlans]
        WHERE [fldv_Code] = @pCode;

        SET @AuditMessage = 'Successfully deleted SMS Audit Plan with Code: ' + COALESCE(@pCode, 'NULL');
        
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
        SET @AuditMessage = 'Error deleting SMS Audit Plan: ' + @ErrorMessage;
        
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

## Code Generation Registry Entry

Add this entry to the code generation registry for automatic code generation:

```sql
-- Registry entry for SMS Audit Plan code generation
INSERT INTO [dbo].[tbld_CodeGenerationRegistry] ([fldv_EntityName], [fldv_TableName], [fldv_Prefix], [fldi_StartingNumber], [fldi_CurrentNumber])
VALUES ('SMSAuditPlan', 'tbld_SMSAuditPlans', 'AP', 1000, 1000);
GO
```

## Sample Data (Optional)

```sql
-- Sample SMS Audit Plan data
INSERT INTO [dbo].[tbld_SMSAuditPlans] (
    [fldv_Code], [fldv_Name], [fldv_Description], [fldv_AuditType], [fldv_Scope], 
    [fldv_Objectives], [fldd_PlannedStartDate], [fldd_PlannedEndDate], [fldv_LeadAuditor], 
    [fldv_ResponsibleDepartment], [fldv_Status], [fldv_CreatedBy]
)
VALUES 
    ('AP1001', 'Q1 2025 Safety Management System Audit', 'Comprehensive annual SMS audit covering all departments', 'Internal', 'All SMS processes and procedures', 'Evaluate SMS effectiveness and compliance', '2025-01-15', '2025-01-30', 'John Smith', 'Safety', 'Approved', 'SYSTEM'),
    ('AP1002', 'Ground Operations Safety Audit', 'Focused audit on ground handling operations', 'Internal', 'Ground operations and equipment', 'Assess ground safety procedures', '2025-02-01', '2025-02-05', 'Jane Doe', 'Operations', 'Draft', 'SYSTEM');
GO
```