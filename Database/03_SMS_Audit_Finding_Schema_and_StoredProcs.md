# SMS Audit Finding - Table Schema and Stored Procedures

## Table Definition: tbld_SMSAuditFindings

```sql
-- ============================================= 
-- SMS AUDIT FINDING TABLE SCHEMA
-- ============================================= 

USE [PDXSMS_V2]
GO

CREATE TABLE [dbo].[tbld_SMSAuditFindings]
(
    -- Primary Key
    [fldi_ID] INT IDENTITY(1,1) NOT NULL,
    
    -- Business Key (Code)
    [fldv_Code] NVARCHAR(50) NOT NULL,
    
    -- Foreign Keys
    [fldv_AuditCode] NVARCHAR(50) NOT NULL, -- FK to tbld_SMSAudits
    
    -- Core Fields
    [fldv_Title] NVARCHAR(200) NOT NULL,
    [fldv_Description] NVARCHAR(2000) NOT NULL,
    [fldv_Severity] NVARCHAR(20) NOT NULL, -- Critical, Major, Minor, Observation
    [fldv_Category] NVARCHAR(100) NOT NULL,
    [fldv_Status] NVARCHAR(50) NOT NULL DEFAULT 'Open',
    
    -- Timeline Fields
    [fldd_DiscoveredDate] DATETIME NOT NULL,
    [fldd_TargetResolutionDate] DATETIME NULL,
    [fldd_ActualResolutionDate] DATETIME NULL,
    
    -- Responsibility Fields
    [fldv_ResponsiblePerson] NVARCHAR(200) NULL,
    
    -- Action Fields
    [fldv_CorrectiveAction] NVARCHAR(2000) NULL,
    [fldv_RootCauseAnalysis] NVARCHAR(2000) NULL,
    
    -- Verification Fields
    [fldb_VerificationRequired] BIT NOT NULL DEFAULT 0,
    [fldv_VerifiedBy] NVARCHAR(200) NULL,
    [fldd_VerificationDate] DATETIME NULL,
    
    -- Additional Fields
    [fldv_Notes] NVARCHAR(2000) NULL,
    
    -- Audit fields (inherited from BaseAuditableEntity)
    [fldv_CreatedBy] VARCHAR(50) NOT NULL DEFAULT 'SYSTEM',
    [fldd_CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
    [fldv_UpdatedBy] VARCHAR(50) NULL,
    [fldd_UpdatedDate] DATETIME NULL,
    
    -- Primary Key Constraint
    CONSTRAINT [PK_tbld_SMSAuditFindings] PRIMARY KEY CLUSTERED ([fldi_ID] ASC),
    
    -- Unique Constraint on Code
    CONSTRAINT [UC_tbld_SMSAuditFindings_Code] UNIQUE NONCLUSTERED ([fldv_Code] ASC),
    
    -- Foreign Key Constraint
    CONSTRAINT [FK_tbld_SMSAuditFindings_Audit] FOREIGN KEY ([fldv_AuditCode]) 
        REFERENCES [dbo].[tbld_SMSAudits] ([fldv_Code])
)
ON [PRIMARY]
GO

-- Indexes for performance
CREATE INDEX [IX_tbld_SMSAuditFindings_AuditCode] ON [dbo].[tbld_SMSAuditFindings] ([fldv_AuditCode])
CREATE INDEX [IX_tbld_SMSAuditFindings_Status] ON [dbo].[tbld_SMSAuditFindings] ([fldv_Status])
CREATE INDEX [IX_tbld_SMSAuditFindings_Severity] ON [dbo].[tbld_SMSAuditFindings] ([fldv_Severity])
CREATE INDEX [IX_tbld_SMSAuditFindings_Category] ON [dbo].[tbld_SMSAuditFindings] ([fldv_Category])
CREATE INDEX [IX_tbld_SMSAuditFindings_ResponsiblePerson] ON [dbo].[tbld_SMSAuditFindings] ([fldv_ResponsiblePerson])
CREATE INDEX [IX_tbld_SMSAuditFindings_TargetDate] ON [dbo].[tbld_SMSAuditFindings] ([fldd_TargetResolutionDate])
GO
```

## Stored Procedures

### 1. SMS Audit Finding Insert

```sql
-- ============================================= 
-- SMS AUDIT FINDING CRUD STORED PROCEDURES
-- ============================================= 

USE [PDXSMS_V2]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- ============================================= 
-- SMS AUDIT FINDING INSERT OPERATION
-- ============================================= 
CREATE OR ALTER PROCEDURE [dbo].[pr_SMSAuditFinding_Insert]
    @pCode NVARCHAR(50) = NULL,
    @pAuditCode NVARCHAR(50),
    @pTitle NVARCHAR(200),
    @pDescription NVARCHAR(2000),
    @pSeverity NVARCHAR(20),
    @pCategory NVARCHAR(100),
    @pStatus NVARCHAR(50) = 'Open',
    @pDiscoveredDate DATETIME,
    @pTargetResolutionDate DATETIME = NULL,
    @pResponsiblePerson NVARCHAR(200) = NULL,
    @pCorrectiveAction NVARCHAR(2000) = NULL,
    @pRootCauseAnalysis NVARCHAR(2000) = NULL,
    @pVerificationRequired BIT = 0,
    @pNotes NVARCHAR(2000) = NULL,
    @pCreatedBy VARCHAR(50) = 'SYSTEM',
    @pCreatedDate DATETIME = NULL,
    @pNewID INT OUTPUT,
    @pNewAuditFindingCode NVARCHAR(50) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(10) = COALESCE(@pCreatedBy, 'SYSTEM'),
            @FunctionName VARCHAR(50) = 'pr_SMSAuditFinding_Insert';
    
    IF @pCreatedDate IS NULL SET @pCreatedDate = GETDATE();
    
    BEGIN TRY
        EXEC [dbo].[sp_AddAuditLogEntry] 
            @pUserID = @UserID, 
            @pMessageType = 'SMS_CRUD', 
            @pSeverity = 'Information', 
            @pModule = 'SMS_AuditManagement', 
            @pFunction = @FunctionName, 
            @pDescription = 'Starting SMS Audit Finding insert operation';

        -- Generate Audit Finding Code
        EXEC [pr_GenerateFormattedCode] 
            @EntityName = 'SMSAuditFinding',           -- Uses registry: Table='tbld_SMSAuditFindings', Prefix='AF'
            @GeneratedCode = @pNewAuditFindingCode OUTPUT;

        INSERT INTO [dbo].[tbld_SMSAuditFindings] (
            [fldv_Code], [fldv_AuditCode], [fldv_Title], [fldv_Description], [fldv_Severity], 
            [fldv_Category], [fldv_Status], [fldd_DiscoveredDate], [fldd_TargetResolutionDate], 
            [fldv_ResponsiblePerson], [fldv_CorrectiveAction], [fldv_RootCauseAnalysis], 
            [fldb_VerificationRequired], [fldv_Notes], [fldv_CreatedBy], [fldd_CreatedDate]
        )
        VALUES (
            @pNewAuditFindingCode, @pAuditCode, @pTitle, @pDescription, @pSeverity, 
            @pCategory, @pStatus, @pDiscoveredDate, @pTargetResolutionDate, 
            @pResponsiblePerson, @pCorrectiveAction, @pRootCauseAnalysis, 
            @pVerificationRequired, @pNotes, @pCreatedBy, @pCreatedDate
        );

        SET @pNewID = SCOPE_IDENTITY();
        
        -- Update audit findings summary
        EXEC [dbo].[pr_SMSAudit_UpdateFindingsSummary] @pAuditCode = @pAuditCode;
        
        SET @AuditMessage = 'Successfully inserted SMS Audit Finding with ID: ' + CAST(@pNewID AS VARCHAR(10)) + ', Code: ' + COALESCE(@pNewAuditFindingCode, 'NULL');
        
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
        SET @AuditMessage = 'Error inserting SMS Audit Finding: ' + @ErrorMessage;
        
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

### 2. SMS Audit Finding Get By Code

```sql
-- ============================================= 
-- SMS AUDIT FINDING GET BY CODE OPERATION
-- ============================================= 
CREATE OR ALTER PROCEDURE [dbo].[pr_SMSAuditFinding_GetByCode]
    @pCode NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [fldi_ID],
        [fldv_Code],
        [fldv_AuditCode],
        [fldv_Title],
        [fldv_Description],
        [fldv_Severity],
        [fldv_Category],
        [fldv_Status],
        [fldd_DiscoveredDate],
        [fldd_TargetResolutionDate],
        [fldd_ActualResolutionDate],
        [fldv_ResponsiblePerson],
        [fldv_CorrectiveAction],
        [fldv_RootCauseAnalysis],
        [fldb_VerificationRequired],
        [fldv_VerifiedBy],
        [fldd_VerificationDate],
        [fldv_Notes],
        [fldv_CreatedBy],
        [fldd_CreatedDate],
        [fldv_UpdatedBy],
        [fldd_UpdatedDate]
    FROM [dbo].[tbld_SMSAuditFindings]
    WHERE [fldv_Code] = @pCode;
END;
GO
```

### 3. SMS Audit Finding Get All

```sql
-- ============================================= 
-- SMS AUDIT FINDING GET ALL OPERATION
-- ============================================= 
CREATE OR ALTER PROCEDURE [dbo].[pr_SMSAuditFinding_GetAll]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [fldi_ID],
        [fldv_Code],
        [fldv_AuditCode],
        [fldv_Title],
        [fldv_Description],
        [fldv_Severity],
        [fldv_Category],
        [fldv_Status],
        [fldd_DiscoveredDate],
        [fldd_TargetResolutionDate],
        [fldd_ActualResolutionDate],
        [fldv_ResponsiblePerson],
        [fldv_CorrectiveAction],
        [fldv_RootCauseAnalysis],
        [fldb_VerificationRequired],
        [fldv_VerifiedBy],
        [fldd_VerificationDate],
        [fldv_Notes],
        [fldv_CreatedBy],
        [fldd_CreatedDate],
        [fldv_UpdatedBy],
        [fldd_UpdatedDate]
    FROM [dbo].[tbld_SMSAuditFindings]
    ORDER BY [fldd_CreatedDate] DESC;
END;
GO
```

### 4. SMS Audit Finding Get By Audit

```sql
-- ============================================= 
-- SMS AUDIT FINDING GET BY AUDIT OPERATION
-- ============================================= 
CREATE OR ALTER PROCEDURE [dbo].[pr_SMSAuditFinding_GetByAudit]
    @pAuditCode NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [fldi_ID],
        [fldv_Code],
        [fldv_AuditCode],
        [fldv_Title],
        [fldv_Description],
        [fldv_Severity],
        [fldv_Category],
        [fldv_Status],
        [fldd_DiscoveredDate],
        [fldd_TargetResolutionDate],
        [fldd_ActualResolutionDate],
        [fldv_ResponsiblePerson],
        [fldv_CorrectiveAction],
        [fldv_RootCauseAnalysis],
        [fldb_VerificationRequired],
        [fldv_VerifiedBy],
        [fldd_VerificationDate],
        [fldv_Notes],
        [fldv_CreatedBy],
        [fldd_CreatedDate],
        [fldv_UpdatedBy],
        [fldd_UpdatedDate]
    FROM [dbo].[tbld_SMSAuditFindings]
    WHERE [fldv_AuditCode] = @pAuditCode
    ORDER BY [fldv_Severity] DESC, [fldd_DiscoveredDate] ASC;
END;
GO
```

### 5. SMS Audit Finding Update

```sql
-- ============================================= 
-- SMS AUDIT FINDING UPDATE OPERATION
-- ============================================= 
CREATE OR ALTER PROCEDURE [dbo].[pr_SMSAuditFinding_Update]
    @pID INT,
    @pCode NVARCHAR(50),
    @pAuditCode NVARCHAR(50),
    @pTitle NVARCHAR(200),
    @pDescription NVARCHAR(2000),
    @pSeverity NVARCHAR(20),
    @pCategory NVARCHAR(100),
    @pStatus NVARCHAR(50),
    @pDiscoveredDate DATETIME,
    @pTargetResolutionDate DATETIME = NULL,
    @pActualResolutionDate DATETIME = NULL,
    @pResponsiblePerson NVARCHAR(200) = NULL,
    @pCorrectiveAction NVARCHAR(2000) = NULL,
    @pRootCauseAnalysis NVARCHAR(2000) = NULL,
    @pVerificationRequired BIT = 0,
    @pVerifiedBy NVARCHAR(200) = NULL,
    @pVerificationDate DATETIME = NULL,
    @pNotes NVARCHAR(2000) = NULL,
    @pUpdatedBy VARCHAR(50),
    @pUpdatedDate DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(10) = COALESCE(@pUpdatedBy, 'SYSTEM'),
            @FunctionName VARCHAR(50) = 'pr_SMSAuditFinding_Update';
    
    IF @pUpdatedDate IS NULL SET @pUpdatedDate = GETDATE();
    
    BEGIN TRY
        EXEC [dbo].[sp_AddAuditLogEntry] 
            @pUserID = @UserID, 
            @pMessageType = 'SMS_CRUD', 
            @pSeverity = 'Information', 
            @pModule = 'SMS_AuditManagement', 
            @pFunction = @FunctionName, 
            @pDescription = 'Starting SMS Audit Finding update operation';

        UPDATE [dbo].[tbld_SMSAuditFindings]
        SET 
            [fldv_AuditCode] = @pAuditCode,
            [fldv_Title] = @pTitle,
            [fldv_Description] = @pDescription,
            [fldv_Severity] = @pSeverity,
            [fldv_Category] = @pCategory,
            [fldv_Status] = @pStatus,
            [fldd_DiscoveredDate] = @pDiscoveredDate,
            [fldd_TargetResolutionDate] = @pTargetResolutionDate,
            [fldd_ActualResolutionDate] = @pActualResolutionDate,
            [fldv_ResponsiblePerson] = @pResponsiblePerson,
            [fldv_CorrectiveAction] = @pCorrectiveAction,
            [fldv_RootCauseAnalysis] = @pRootCauseAnalysis,
            [fldb_VerificationRequired] = @pVerificationRequired,
            [fldv_VerifiedBy] = @pVerifiedBy,
            [fldd_VerificationDate] = @pVerificationDate,
            [fldv_Notes] = @pNotes,
            [fldv_UpdatedBy] = @pUpdatedBy,
            [fldd_UpdatedDate] = @pUpdatedDate
        WHERE [fldi_ID] = @pID;

        -- Update audit findings summary
        EXEC [dbo].[pr_SMSAudit_UpdateFindingsSummary] @pAuditCode = @pAuditCode;
        
        SET @AuditMessage = 'Successfully updated SMS Audit Finding with ID: ' + CAST(@pID AS VARCHAR(10)) + ', Code: ' + COALESCE(@pCode, 'NULL');
        
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
        SET @AuditMessage = 'Error updating SMS Audit Finding: ' + @ErrorMessage;
        
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

### 6. SMS Audit Finding Delete

```sql
-- ============================================= 
-- SMS AUDIT FINDING DELETE OPERATION
-- ============================================= 
CREATE OR ALTER PROCEDURE [dbo].[pr_SMSAuditFinding_Delete]
    @pCode NVARCHAR(50),
    @pDeletedBy VARCHAR(50) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(10) = COALESCE(@pDeletedBy, 'SYSTEM'),
            @FunctionName VARCHAR(50) = 'pr_SMSAuditFinding_Delete',
            @AuditCode NVARCHAR(50);
    
    BEGIN TRY
        EXEC [dbo].[sp_AddAuditLogEntry] 
            @pUserID = @UserID, 
            @pMessageType = 'SMS_CRUD', 
            @pSeverity = 'Information', 
            @pModule = 'SMS_AuditManagement', 
            @pFunction = @FunctionName, 
            @pDescription = 'Starting SMS Audit Finding delete operation';

        -- Get audit code before deletion for summary update
        SELECT @AuditCode = [fldv_AuditCode] 
        FROM [dbo].[tbld_SMSAuditFindings] 
        WHERE [fldv_Code] = @pCode;

        -- Check for dependent evidence records
        IF EXISTS (SELECT 1 FROM [dbo].[tbld_SMSAuditEvidence] WHERE [fldv_FindingCode] = @pCode)
        BEGIN
            RAISERROR('Cannot delete finding with existing evidence. Delete dependent evidence first.', 16, 1);
            RETURN;
        END

        DELETE FROM [dbo].[tbld_SMSAuditFindings]
        WHERE [fldv_Code] = @pCode;

        -- Update audit findings summary if we have the audit code
        IF @AuditCode IS NOT NULL
        BEGIN
            EXEC [dbo].[pr_SMSAudit_UpdateFindingsSummary] @pAuditCode = @AuditCode;
        END

        SET @AuditMessage = 'Successfully deleted SMS Audit Finding with Code: ' + COALESCE(@pCode, 'NULL');
        
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
        SET @AuditMessage = 'Error deleting SMS Audit Finding: ' + @ErrorMessage;
        
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

## Utility Procedures

### Get Overdue Findings

```sql
-- ============================================= 
-- UTILITY: GET OVERDUE FINDINGS
-- ============================================= 
CREATE OR ALTER PROCEDURE [dbo].[pr_SMSAuditFinding_GetOverdue]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        f.[fldi_ID],
        f.[fldv_Code],
        f.[fldv_AuditCode],
        f.[fldv_Title],
        f.[fldv_Description],
        f.[fldv_Severity],
        f.[fldv_Category],
        f.[fldv_Status],
        f.[fldd_DiscoveredDate],
        f.[fldd_TargetResolutionDate],
        f.[fldv_ResponsiblePerson],
        a.[fldv_Name] AS [AuditName],
        a.[fldv_LeadAuditor],
        DATEDIFF(DAY, f.[fldd_TargetResolutionDate], GETDATE()) AS [DaysOverdue]
    FROM [dbo].[tbld_SMSAuditFindings] f
    INNER JOIN [dbo].[tbld_SMSAudits] a ON f.[fldv_AuditCode] = a.[fldv_Code]
    WHERE f.[fldv_Status] NOT IN ('Closed', 'Resolved')
      AND f.[fldd_TargetResolutionDate] < GETDATE()
    ORDER BY f.[fldd_TargetResolutionDate] ASC;
END;
GO
```

## Code Generation Registry Entry

```sql
-- Registry entry for SMS Audit Finding code generation
INSERT INTO [dbo].[tbld_CodeGenerationRegistry] ([fldv_EntityName], [fldv_TableName], [fldv_Prefix], [fldi_StartingNumber], [fldi_CurrentNumber])
VALUES ('SMSAuditFinding', 'tbld_SMSAuditFindings', 'AF', 1000, 1000);
GO
```

## Sample Data (Optional)

```sql
-- Sample SMS Audit Finding data
INSERT INTO [dbo].[tbld_SMSAuditFindings] (
    [fldv_Code], [fldv_AuditCode], [fldv_Title], [fldv_Description], [fldv_Severity], 
    [fldv_Category], [fldd_DiscoveredDate], [fldd_TargetResolutionDate], 
    [fldv_ResponsiblePerson], [fldv_CreatedBy]
)
VALUES 
    ('AF1001', 'AU1001', 'Missing SMS Documentation', 'Several departments lack current SMS process documentation', 'Major', 'Documentation', '2025-01-16', '2025-02-15', 'Department Manager', 'SYSTEM'),
    ('AF1002', 'AU1001', 'Training Records Incomplete', 'Training records for safety-critical personnel are not up to date', 'Minor', 'Training', '2025-01-17', '2025-02-01', 'Training Coordinator', 'SYSTEM'),
    ('AF1003', 'AU1002', 'Safety Equipment Inspection Overdue', 'Ground support equipment safety inspections are past due', 'Critical', 'Equipment', '2025-02-02', '2025-02-10', 'Maintenance Manager', 'SYSTEM');
GO
```