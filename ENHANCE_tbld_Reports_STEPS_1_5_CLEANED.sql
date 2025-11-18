-- **************************************************
-- ENHANCE tbld_Reports TABLE FOR STEPS 1-5 INTEGRATION (CLEANED)
-- Complete Reports table enhancement without investigation duplication
-- **************************************************
USE [PDXSMS_V2]
GO

PRINT '***************************************************';
PRINT 'ENHANCING tbld_Reports TABLE FOR STEPS 1-5 INTEGRATION';
PRINT 'Adding enhanced fields for comprehensive report management';
PRINT 'EXCLUDING investigation fields (handled by tbld_Investigations)';
PRINT '***************************************************';

-- **************************************************
-- UPDATE tbld_Reports TABLE - Add All Enhanced Fields (No Investigation Duplication)
-- **************************************************

PRINT 'Enhancing tbld_Reports table...';

-- First check current basic table definition and enhance existing fields
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbld_Reports' AND COLUMN_NAME = 'fldv_Code')
BEGIN
    -- If table doesn't exist or is missing core fields, we need to define it properly
    PRINT 'Current tbld_Reports table needs enhancement for core fields';
END

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

-- Add Enhanced Report Management Fields for Steps 1-5
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

-- Add Report Validation Integration Fields (these are the critical fields from your context)
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

-- Add Risk Assessment Integration Fields
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbld_Reports' AND COLUMN_NAME = 'fldv_AssignedRiskAssessmentId')
BEGIN
    ALTER TABLE [dbo].[tbld_Reports]
    ADD [fldv_AssignedRiskAssessmentId] [nvarchar](50) NULL;
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbld_Reports' AND COLUMN_NAME = 'fldv_RiskAssessmentStatus')
BEGIN
    ALTER TABLE [dbo].[tbld_Reports]
    ADD [fldv_RiskAssessmentStatus] [nvarchar](50) NULL; -- Not Started, In Progress, Completed
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbld_Reports' AND COLUMN_NAME = 'fldd_RiskAssessmentStartDate')
BEGIN
    ALTER TABLE [dbo].[tbld_Reports]
    ADD [fldd_RiskAssessmentStartDate] [datetime] NULL;
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbld_Reports' AND COLUMN_NAME = 'fldd_RiskAssessmentCompletedDate')
BEGIN
    ALTER TABLE [dbo].[tbld_Reports]
    ADD [fldd_RiskAssessmentCompletedDate] [datetime] NULL;
END

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

-- REMOVED: Investigation Integration Fields 
-- These are handled by the dedicated tbld_Investigations table:
-- - fldb_RequiresInvestigation (handled by Investigation creation workflow)
-- - fldv_InvestigationStatus (handled in tbld_Investigations.fldv_Status)
-- - fldv_AssignedInvestigator (handled in tbld_Investigations.fldv_AssignedInvestigatorId)

-- Add Workflow and Review Fields
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

-- Add Follow-up and Resolution Fields
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
-- CREATE PERFORMANCE INDEXES (CLEANED - No Investigation)
-- **************************************************

PRINT 'Creating enhanced performance indexes for tbld_Reports...';

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

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_tbld_Reports_ValidatedBy')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_tbld_Reports_ValidatedBy] 
    ON [dbo].[tbld_Reports] ([fldv_ValidatedBy])
    INCLUDE ([fldv_ValidationDecision], [fldd_ValidatedDate], [fldv_Status]);
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_tbld_Reports_RiskAssessment')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_tbld_Reports_RiskAssessment] 
    ON [dbo].[tbld_Reports] ([fldv_AssignedRiskAssessmentId])
    INCLUDE ([fldv_RiskAssessmentStatus], [fldd_RiskAssessmentStartDate], [fldd_RiskAssessmentCompletedDate]);
END

-- **************************************************
-- UPDATE pr_Report_GetById STORED PROCEDURE (CLEANED)
-- **************************************************

PRINT 'Creating enhanced pr_Report_GetById stored procedure...';

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
        SET @AuditMessage = 'Retrieving Enhanced Report with ID: ' + @pID;
        
        -- Main Report Query with Complete Enhanced Data (No Investigation Duplication)
        SELECT 
            -- Core Fields
            [fldi_ID],
            [fldv_Code],
            [fldv_Name],
            [fldv_Description],
            [fldv_Status],
            [fldv_Stage],
            
            -- Enhanced Management Fields
            [fldv_ReportType],
            [fldv_Priority],
            [fldv_Department],
            [fldv_ReportedBy],
            [fldd_ReportedDate],
            
            -- Validation Fields (critical for your ReportValidation integration)
            [fldv_ValidatedBy],
            [fldd_ValidatedDate],
            [fldv_ValidationComments],
            [fldv_ValidationType],
            [fldv_ValidationDecision],
            
            -- Risk Assessment Integration
            [fldv_AssignedRiskAssessmentId],
            [fldv_RiskAssessmentStatus],
            [fldd_RiskAssessmentStartDate],
            [fldd_RiskAssessmentCompletedDate],
            
            -- Confidentiality and Access Control
            [fldb_IsConfidential],
            [fldb_IsAnonymous],
            [fldv_AccessLevel],
            
            -- REMOVED: Investigation fields (handled by tbld_Investigations)
            -- Investigation data retrieved separately via InvestigationRepository
            
            -- Workflow and Review
            [fldv_ReviewedBy],
            [fldd_ReviewedDate],
            [fldv_ReviewComments],
            
            -- Follow-up and Resolution
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
        
        SET @AuditMessage = 'Successfully retrieved Enhanced Report with ID: ' + @pID;
        
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error retrieving Enhanced Report ID ' + @pID + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Reports', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;

GO

-- **************************************************
-- UPDATE pr_Report_Update STORED PROCEDURE (CLEANED)
-- **************************************************

PRINT 'Creating enhanced pr_Report_Update stored procedure...';

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
    -- Enhanced validation fields (these are the critical ones from your context)
    @pValidatedBy VARCHAR(50) = NULL,
    @pValidatedDate DATETIME = NULL,
    @pValidationComments NTEXT = NULL,
    @pValidationType VARCHAR(50) = 'Standard',
    @pValidationDecision VARCHAR(50) = NULL,
    -- Additional enhanced fields (no investigation - handled by tbld_Investigations)
    @pReportType VARCHAR(50) = NULL,
    @pPriority VARCHAR(50) = NULL,
    @pDepartment VARCHAR(100) = NULL,
    @pAssignedRiskAssessmentId VARCHAR(50) = NULL,
    @pRiskAssessmentStatus VARCHAR(50) = NULL,
    @pUpdatedBy VARCHAR(50),
    @pUpdatedDate DATETIME,
    @pUserID VARCHAR(10) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @FunctionName VARCHAR(50) = 'pr_Report_Update';
    
    BEGIN TRY
        SET @AuditMessage = 'Updating Enhanced Report with ID: ' + @pID;
        
        UPDATE [dbo].[tbld_Reports] 
        SET 
            [fldv_Code] = @pReportCode,
            [fldv_Name] = @pReportName,
            [fldv_Description] = @pReportDescription,
            [fldv_Status] = @pReportStatus,
            [fldv_Stage] = @pReportStage,
            
            -- Critical validation fields from your context
            [fldv_ValidatedBy] = @pValidatedBy,
            [fldd_ValidatedDate] = @pValidatedDate,
            [fldv_ValidationComments] = @pValidationComments,
            [fldv_ValidationType] = @pValidationType,
            [fldv_ValidationDecision] = @pValidationDecision,
            
            -- Enhanced fields (no investigation - handled separately)
            [fldv_ReportType] = @pReportType,
            [fldv_Priority] = @pPriority,
            [fldv_Department] = @pDepartment,
            [fldv_AssignedRiskAssessmentId] = @pAssignedRiskAssessmentId,
            [fldv_RiskAssessmentStatus] = @pRiskAssessmentStatus,
            
            -- Audit fields
            [fldv_UpdatedBy] = @pUpdatedBy,
            [fldd_UpdatedDate] = @pUpdatedDate
            
        WHERE [fldv_Code] = @pID;
        
        SET @AuditMessage = 'Successfully updated Enhanced Report with ID: ' + @pID;
        
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error updating Enhanced Report ID ' + @pID + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Reports', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;

GO

-- **************************************************
-- VERIFICATION QUERIES
-- **************************************************

PRINT 'Running verification queries...';

-- Check enhanced Reports table structure
PRINT 'Enhanced Reports Table Structure (Investigation fields excluded):';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'tbld_Reports'
ORDER BY ORDINAL_POSITION;

-- Test enhanced reports query
SELECT 
    R.fldv_Code,
    R.fldv_Name,
    R.fldv_Status,
    R.fldv_Stage,
    R.fldv_ValidatedBy,
    R.fldd_ValidatedDate,
    R.fldv_ValidationDecision,
    R.fldv_AssignedRiskAssessmentId,
    R.fldv_RiskAssessmentStatus,
    R.fldd_CreatedDate
FROM [dbo].[tbld_Reports] R
ORDER BY R.fldd_CreatedDate DESC;

PRINT '***************************************************';
PRINT 'tbld_Reports ENHANCEMENT COMPLETED SUCCESSFULLY!';
PRINT '***************************************************';
PRINT 'ENHANCED CAPABILITIES ADDED:';
PRINT '? Validation Integration: ValidatedBy, ValidatedDate, ValidationComments, ValidationType';
PRINT '? Risk Assessment Integration: Direct linking to risk assessments';
PRINT '? Workflow Management: Review and approval workflows';
PRINT '? Access Control: Confidentiality and access level management';
PRINT '? Follow-up Tracking: Resolution and closure management';
PRINT '? Audit Trail: Complete creation and modification tracking';
PRINT '? Performance Indexes: Strategic indexes for key lookup patterns';
PRINT '';
PRINT 'CRITICAL INTEGRATION FIELDS ADDED:';
PRINT '? fldv_ValidatedBy - Matches ReportValidation.ValidatedBy requirement';
PRINT '? fldd_ValidatedDate - Matches ReportValidation.ValidatedDate requirement';
PRINT '? fldv_ValidationComments - Matches ReportValidation.ValidationComments requirement';
PRINT '? fldv_ValidationType - Matches ReportValidation.ValidationType requirement';
PRINT '';
PRINT 'PROPER ARCHITECTURAL SEPARATION:';
PRINT '? Investigation fields REMOVED - handled by dedicated tbld_Investigations table';
PRINT '? No duplication of investigation data between tables';
PRINT '? Clean separation of concerns: Reports vs Investigations';
PRINT '? Investigation data accessed via InvestigationRepository';
PRINT '';
PRINT 'STORED PROCEDURES ENHANCED:';
PRINT '? pr_Report_GetById: Complete data retrieval with all validation fields (no investigation duplication)';
PRINT '? pr_Report_Update: Enhanced parameter support for all validation fields (clean architecture)';
PRINT '';
PRINT 'ARCHITECTURE INTEGRATION:';
PRINT '- Compatible with existing ReportRepository and ReportValidationRepository';
PRINT '- Ready for enhanced Report validation workflows';
PRINT '- Supports Steps 1-5 risk assessment workflow integration';
PRINT '- Clean separation with tbld_Investigations table';
PRINT '- Investigation data retrieved via dedicated InvestigationRepository';
PRINT '***************************************************';

GO