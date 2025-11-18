-- **************************************************
-- ENHANCE tbld_Reports TABLE FOR VALIDATION ONLY (FINAL CLEANED)
-- Reports as simple wrapper with validation - NO RiskAssessment duplication
-- **************************************************
USE [PDXSMS_V2]
GO

PRINT '***************************************************';
PRINT 'ENHANCING tbld_Reports TABLE FOR VALIDATION WORKFLOW';
PRINT 'Reports as simple wrapper for Hazards - validation fields only';
PRINT 'NO RiskAssessment duplication (handled per Hazard)';
PRINT 'NO Investigation duplication (handled by tbld_Investigations)';
PRINT '***************************************************';

-- **************************************************
-- UPDATE tbld_Reports TABLE - Validation Wrapper Only
-- **************************************************

PRINT 'Enhancing tbld_Reports table for validation workflow...';

-- Ensure we have proper data types for existing fields
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbld_Reports' AND COLUMN_NAME = 'fldv_Code')
BEGIN
    ALTER TABLE [dbo].[tbld_Reports]
    ALTER COLUMN [fldv_Code] [nvarchar](50) NOT NULL;
END

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbld_Reports' AND COLUMN_NAME = 'fldv_Name')
BEGIN
    ALTER TABLE [dbo].[tbld_Reports]
    ALTER COLUMN [fldv_Name] [nvarchar](200) NULL;
END

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbld_Reports' AND COLUMN_NAME = 'fldv_Description')
BEGIN
    ALTER TABLE [dbo].[tbld_Reports]
    ALTER COLUMN [fldv_Description] [ntext] NULL;
END

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbld_Reports' AND COLUMN_NAME = 'fldv_Status')
BEGIN
    ALTER TABLE [dbo].[tbld_Reports]
    ALTER COLUMN [fldv_Status] [nvarchar](50) NULL;
END

IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbld_Reports' AND COLUMN_NAME = 'fldv_Stage')
BEGIN
    ALTER TABLE [dbo].[tbld_Reports]
    ALTER COLUMN [fldv_Stage] [nvarchar](50) NULL;
END

-- Add Basic Report Management Fields
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbld_Reports' AND COLUMN_NAME = 'fldv_ReportType')
BEGIN
    ALTER TABLE [dbo].[tbld_Reports]
    ADD [fldv_ReportType] [nvarchar](50) NULL DEFAULT 'Incident'; -- Incident, Hazard, Investigation, etc.
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbld_Reports' AND COLUMN_NAME = 'fldv_Priority')
BEGIN
    ALTER TABLE [dbo].[tbld_Reports]
    ADD [fldv_Priority] [nvarchar](50) NULL DEFAULT 'Medium';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbld_Reports' AND COLUMN_NAME = 'fldv_Department')
BEGIN
    ALTER TABLE [dbo].[tbld_Reports]
    ADD [fldv_Department] [nvarchar](100) NULL;
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbld_Reports' AND COLUMN_NAME = 'fldv_ReportedBy')
BEGIN
    ALTER TABLE [dbo].[tbld_Reports]
    ADD [fldv_ReportedBy] [nvarchar](50) NULL;
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbld_Reports' AND COLUMN_NAME = 'fldd_ReportedDate')
BEGIN
    ALTER TABLE [dbo].[tbld_Reports]
    ADD [fldd_ReportedDate] [datetime] NULL;
END

-- Add Report Validation Integration Fields (THE CRITICAL FIELDS FROM YOUR CONTEXT)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbld_Reports' AND COLUMN_NAME = 'fldv_ValidatedBy')
BEGIN
    ALTER TABLE [dbo].[tbld_Reports]
    ADD [fldv_ValidatedBy] [nvarchar](50) NULL;
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbld_Reports' AND COLUMN_NAME = 'fldd_ValidatedDate')
BEGIN
    ALTER TABLE [dbo].[tbld_Reports]
    ADD [fldd_ValidatedDate] [datetime] NULL;
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbld_Reports' AND COLUMN_NAME = 'fldv_ValidationComments')
BEGIN
    ALTER TABLE [dbo].[tbld_Reports]
    ADD [fldv_ValidationComments] [ntext] NULL;
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbld_Reports' AND COLUMN_NAME = 'fldv_ValidationType')
BEGIN
    ALTER TABLE [dbo].[tbld_Reports]
    ADD [fldv_ValidationType] [nvarchar](50) NULL DEFAULT 'Standard';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbld_Reports' AND COLUMN_NAME = 'fldv_ValidationDecision')
BEGIN
    ALTER TABLE [dbo].[tbld_Reports]
    ADD [fldv_ValidationDecision] [nvarchar](50) NULL; -- Approved, Rejected, Pending
END

-- REMOVED: Risk Assessment Integration Fields 
-- These belong at the Hazard level, not Report level:
-- - fldv_AssignedRiskAssessmentId (RiskAssessment is per Hazard via HazardCode)
-- - fldv_RiskAssessmentStatus (Status is per individual Hazard's assessment)
-- - fldd_RiskAssessmentStartDate (Each Hazard can have different start dates)
-- - fldd_RiskAssessmentCompletedDate (Each Hazard completes independently)

-- Add Confidentiality and Access Control Fields
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbld_Reports' AND COLUMN_NAME = 'fldb_IsConfidential')
BEGIN
    ALTER TABLE [dbo].[tbld_Reports]
    ADD [fldb_IsConfidential] [bit] NULL DEFAULT 0;
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbld_Reports' AND COLUMN_NAME = 'fldb_IsAnonymous')
BEGIN
    ALTER TABLE [dbo].[tbld_Reports]
    ADD [fldb_IsAnonymous] [bit] NULL DEFAULT 0;
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbld_Reports' AND COLUMN_NAME = 'fldv_AccessLevel')
BEGIN
    ALTER TABLE [dbo].[tbld_Reports]
    ADD [fldv_AccessLevel] [nvarchar](50) NULL DEFAULT 'Standard'; -- Public, Standard, Restricted, Confidential
END

-- Add Workflow and Review Fields (Report-level review, not Hazard-level)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbld_Reports' AND COLUMN_NAME = 'fldv_ReviewedBy')
BEGIN
    ALTER TABLE [dbo].[tbld_Reports]
    ADD [fldv_ReviewedBy] [nvarchar](50) NULL;
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbld_Reports' AND COLUMN_NAME = 'fldd_ReviewedDate')
BEGIN
    ALTER TABLE [dbo].[tbld_Reports]
    ADD [fldd_ReviewedDate] [datetime] NULL;
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbld_Reports' AND COLUMN_NAME = 'fldv_ReviewComments')
BEGIN
    ALTER TABLE [dbo].[tbld_Reports]
    ADD [fldv_ReviewComments] [ntext] NULL;
END

-- Add Follow-up and Resolution Fields (Report-level closure)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbld_Reports' AND COLUMN_NAME = 'fldb_RequiresFollowup')
BEGIN
    ALTER TABLE [dbo].[tbld_Reports]
    ADD [fldb_RequiresFollowup] [bit] NULL DEFAULT 0;
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbld_Reports' AND COLUMN_NAME = 'fldd_FollowupDueDate')
BEGIN
    ALTER TABLE [dbo].[tbld_Reports]
    ADD [fldd_FollowupDueDate] [datetime] NULL;
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbld_Reports' AND COLUMN_NAME = 'fldv_ResolutionSummary')
BEGIN
    ALTER TABLE [dbo].[tbld_Reports]
    ADD [fldv_ResolutionSummary] [ntext] NULL;
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbld_Reports' AND COLUMN_NAME = 'fldd_ClosedDate')
BEGIN
    ALTER TABLE [dbo].[tbld_Reports]
    ADD [fldd_ClosedDate] [datetime] NULL;
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbld_Reports' AND COLUMN_NAME = 'fldv_ClosedBy')
BEGIN
    ALTER TABLE [dbo].[tbld_Reports]
    ADD [fldv_ClosedBy] [nvarchar](50) NULL;
END

-- Add Audit Fields (if not already present)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbld_Reports' AND COLUMN_NAME = 'fldv_CreatedBy')
BEGIN
    ALTER TABLE [dbo].[tbld_Reports]
    ADD [fldv_CreatedBy] [nvarchar](50) NOT NULL DEFAULT 'SYSTEM';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbld_Reports' AND COLUMN_NAME = 'fldd_CreatedDate')
BEGIN
    ALTER TABLE [dbo].[tbld_Reports]
    ADD [fldd_CreatedDate] [datetime] NOT NULL DEFAULT GETUTCDATE();
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbld_Reports' AND COLUMN_NAME = 'fldv_UpdatedBy')
BEGIN
    ALTER TABLE [dbo].[tbld_Reports]
    ADD [fldv_UpdatedBy] [nvarchar](50) NULL;
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbld_Reports' AND COLUMN_NAME = 'fldd_UpdatedDate')
BEGIN
    ALTER TABLE [dbo].[tbld_Reports]
    ADD [fldd_UpdatedDate] [datetime] NULL;
END

-- **************************************************
-- CREATE PRIMARY KEY AND CONSTRAINTS
-- **************************************************

PRINT 'Creating primary key and constraints for tbld_Reports...';

-- Add Primary Key if not exists
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE TABLE_NAME = 'tbld_Reports' AND CONSTRAINT_TYPE = 'PRIMARY KEY')
BEGIN
    ALTER TABLE [dbo].[tbld_Reports]
    ADD CONSTRAINT [PK_tbld_Reports] PRIMARY KEY CLUSTERED ([fldi_ID]);
END

-- Ensure unique report codes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_tbld_Reports_Code')
BEGIN
    ALTER TABLE [dbo].[tbld_Reports]
    ADD CONSTRAINT [UQ_tbld_Reports_Code] UNIQUE ([fldv_Code]);
END

-- **************************************************
-- CREATE PERFORMANCE INDEXES (VALIDATION FOCUSED)
-- **************************************************

PRINT 'Creating performance indexes for tbld_Reports (validation focused)...';

-- Core lookup indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_tbld_Reports_Code')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_tbld_Reports_Code] 
    ON [dbo].[tbld_Reports] ([fldv_Code])
    INCLUDE ([fldv_Status], [fldv_Stage], [fldv_Priority], [fldv_ReportType]);
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_tbld_Reports_Status_Stage')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_tbld_Reports_Status_Stage] 
    ON [dbo].[tbld_Reports] ([fldv_Status], [fldv_Stage])
    INCLUDE ([fldv_Code], [fldv_Priority], [fldd_ReportedDate]);
END

-- VALIDATION WORKFLOW INDEXES (Key for your ReportValidation functionality)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_tbld_Reports_ValidatedBy')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_tbld_Reports_ValidatedBy] 
    ON [dbo].[tbld_Reports] ([fldv_ValidatedBy])
    INCLUDE ([fldv_ValidationDecision], [fldd_ValidatedDate], [fldv_Status]);
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_tbld_Reports_ValidationDecision')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_tbld_Reports_ValidationDecision] 
    ON [dbo].[tbld_Reports] ([fldv_ValidationDecision])
    INCLUDE ([fldv_ValidatedBy], [fldd_ValidatedDate], [fldv_Status]);
END

-- REMOVED: Risk Assessment indexes (no longer needed in Reports table)

-- **************************************************
-- UPDATE pr_Report_GetById STORED PROCEDURE (VALIDATION FOCUSED)
-- **************************************************

PRINT 'Creating validation-focused pr_Report_GetById stored procedure...';

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('dbo.pr_Report_GetById', 'P') IS NOT NULL
    DROP PROCEDURE dbo.pr_Report_GetById
GO

CREATE PROCEDURE [dbo].[pr_Report_GetById]
    @pID VARCHAR(60),
    @pUserID VARCHAR(10) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @FunctionName VARCHAR(50) = 'pr_Report_GetById';
    
    BEGIN TRY
        SET @AuditMessage = 'Retrieving Report (Validation Wrapper) with ID: ' + @pID;
        
        -- Main Report Query - Simple Validation Wrapper (No RiskAssessment duplication)
        SELECT 
            -- Core Fields
            [fldi_ID],
            [fldv_Code],
            [fldv_Name],
            [fldv_Description],
            [fldv_Status],
            [fldv_Stage],
            
            -- Basic Management Fields
            [fldv_ReportType],
            [fldv_Priority],
            [fldv_Department],
            [fldv_ReportedBy],
            [fldd_ReportedDate],
            
            -- VALIDATION FIELDS (Critical for your ReportValidation workflow)
            [fldv_ValidatedBy],
            [fldd_ValidatedDate],
            [fldv_ValidationComments],
            [fldv_ValidationType],
            [fldv_ValidationDecision],
            
            -- REMOVED: Risk Assessment fields (handled per Hazard)
            -- Risk assessment data retrieved via HazardCode -> RiskAssessment relationship
            
            -- Confidentiality and Access Control
            [fldb_IsConfidential],
            [fldb_IsAnonymous],
            [fldv_AccessLevel],
            
            -- REMOVED: Investigation fields (handled by tbld_Investigations)
            -- Investigation data retrieved separately via InvestigationRepository
            
            -- Workflow and Review (Report-level)
            [fldv_ReviewedBy],
            [fldd_ReviewedDate],
            [fldv_ReviewComments],
            
            -- Follow-up and Resolution (Report-level)
            [fldb_RequiresFollowup],
            [fldd_FollowupDueDate],
            [fldv_ResolutionSummary],
            [fldd_ClosedDate],
            [fldv_ClosedBy],
            
            -- Audit Fields
            [fldv_CreatedBy],
            [fldd_CreatedDate],
            [fldv_UpdatedBy],
            [fldd_UpdatedDate]
            
        FROM [dbo].[tbld_Reports] 
        WHERE [fldv_Code] = @pID;
        
        SET @AuditMessage = 'Successfully retrieved Report (Validation Wrapper) with ID: ' + @pID;
        
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error retrieving Report ID ' + @pID + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Reports', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;

GO

-- **************************************************
-- UPDATE pr_Report_Update STORED PROCEDURE (VALIDATION FOCUSED)
-- **************************************************

PRINT 'Creating validation-focused pr_Report_Update stored procedure...';

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('dbo.pr_Report_Update', 'P') IS NOT NULL
    DROP PROCEDURE dbo.pr_Report_Update
GO

CREATE PROCEDURE [dbo].[pr_Report_Update]
    @pID VARCHAR(60),
    @pReportCode VARCHAR(50),
    @pReportName VARCHAR(200),
    @pReportDescription NTEXT,
    @pReportStatus VARCHAR(50),
    @pReportStage VARCHAR(50),
    -- VALIDATION FIELDS (Critical from your ReportValidation context)
    @pValidatedBy VARCHAR(50) = NULL,
    @pValidatedDate DATETIME = NULL,
    @pValidationComments NTEXT = NULL,
    @pValidationType VARCHAR(50) = 'Standard',
    @pValidationDecision VARCHAR(50) = NULL,
    -- Basic report fields (no RiskAssessment - handled per Hazard)
    @pReportType VARCHAR(50) = NULL,
    @pPriority VARCHAR(50) = NULL,
    @pDepartment VARCHAR(100) = NULL,
    -- REMOVED: RiskAssessment parameters (handled per Hazard)
    @pUpdatedBy VARCHAR(50),
    @pUpdatedDate DATETIME,
    @pUserID VARCHAR(10) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @FunctionName VARCHAR(50) = 'pr_Report_Update';
    
    BEGIN TRY
        SET @AuditMessage = 'Updating Report (Validation Wrapper) with ID: ' + @pID;
        
        UPDATE [dbo].[tbld_Reports] 
        SET 
            [fldv_Code] = @pReportCode,
            [fldv_Name] = @pReportName,
            [fldv_Description] = @pReportDescription,
            [fldv_Status] = @pReportStatus,
            [fldv_Stage] = @pReportStage,
            
            -- CRITICAL VALIDATION FIELDS from your context
            [fldv_ValidatedBy] = @pValidatedBy,
            [fldd_ValidatedDate] = @pValidatedDate,
            [fldv_ValidationComments] = @pValidationComments,
            [fldv_ValidationType] = @pValidationType,
            [fldv_ValidationDecision] = @pValidationDecision,
            
            -- Basic report management (no RiskAssessment duplication)
            [fldv_ReportType] = @pReportType,
            [fldv_Priority] = @pPriority,
            [fldv_Department] = @pDepartment,
            
            -- REMOVED: RiskAssessment fields (handled per Hazard)
            -- RiskAssessment data managed via Hazard -> RiskAssessment relationship
            
            -- Audit fields
            [fldv_UpdatedBy] = @pUpdatedBy,
            [fldd_UpdatedDate] = @pUpdatedDate
            
        WHERE [fldv_Code] = @pID;
        
        SET @AuditMessage = 'Successfully updated Report (Validation Wrapper) with ID: ' + @pID;
        
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error updating Report ID ' + @pID + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Reports', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;

GO

-- **************************************************
-- VERIFICATION QUERIES
-- **************************************************

PRINT 'Running verification queries...';

-- Check clean Reports table structure (validation wrapper only)
PRINT 'Clean Reports Table Structure (Validation Wrapper - No RiskAssessment duplication):';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'tbld_Reports'
ORDER BY ORDINAL_POSITION;

-- Test clean reports query
SELECT 
    R.fldv_Code,
    R.fldv_Name,
    R.fldv_Status,
    R.fldv_Stage,
    R.fldv_ValidatedBy,
    R.fldd_ValidatedDate,
    R.fldv_ValidationDecision,
    -- NO RiskAssessment fields (handled per Hazard)
    R.fldd_CreatedDate
FROM [dbo].[tbld_Reports] R
ORDER BY R.fldd_CreatedDate DESC;

PRINT '***************************************************';
PRINT 'tbld_Reports ENHANCEMENT COMPLETED - CLEAN ARCHITECTURE!';
PRINT '***************************************************';
PRINT 'CORRECT ARCHITECTURAL SEPARATION:';
PRINT '? Report: Simple validation wrapper for Hazards';
PRINT '? Hazard: Contains ReportCode, linked to RiskAssessments';
PRINT '? RiskAssessment: Per Hazard via HazardCode (Steps 1-5 data)';
PRINT '? Investigation: Per Report via ReportCode (tbld_Investigations)';
PRINT '';
PRINT 'VALIDATION INTEGRATION FIELDS (Critical for your workflow):';
PRINT '? fldv_ValidatedBy - Matches ReportValidation.ValidatedBy requirement';
PRINT '? fldd_ValidatedDate - Matches ReportValidation.ValidatedDate requirement';
PRINT '? fldv_ValidationComments - Matches ReportValidation.ValidationComments requirement';
PRINT '? fldv_ValidationType - Matches ReportValidation.ValidationType requirement';
PRINT '? fldv_ValidationDecision - Report-level validation decision';
PRINT '';
PRINT 'REMOVED DUPLICATIONS:';
PRINT '? Risk Assessment fields REMOVED - handled per Hazard (correct architecture)';
PRINT '? Investigation fields REMOVED - handled by tbld_Investigations (correct architecture)';
PRINT '';
PRINT 'CLEAN WORKFLOW:';
PRINT '1. Report created (simple wrapper)';
PRINT '2. Hazards linked to Report via ReportCode';
PRINT '3. Report validated (validation fields in Reports table)';
PRINT '4. Each Hazard gets individual RiskAssessment (via HazardCode)';
PRINT '5. Investigations created separately (tbld_Investigations)';
PRINT '';
PRINT 'STORED PROCEDURES:';
PRINT '? pr_Report_GetById: Clean validation wrapper data only';
PRINT '? pr_Report_Update: Validation parameters matching your context';
PRINT '';
PRINT 'PERFORMANCE OPTIMIZATION:';
PRINT '? Validation-focused indexes for ReportValidation workflow';
PRINT '? No redundant RiskAssessment indexes in Reports table';
PRINT '? Clean separation enables better query performance';
PRINT '***************************************************';

GO