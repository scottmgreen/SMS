-- **************************************************
-- ENHANCE tbld_ScoringPanel TABLE FOR STEPS 1-5
-- Missing enhancement from Batch 1 and Batch 2
-- **************************************************
USE [PDXSMS_V2]
GO

PRINT '***************************************************';
PRINT 'ENHANCING tbld_ScoringPanel TABLE FOR STEPS 1-5';
PRINT 'Adding missing enhanced fields for comprehensive panel scoring';
PRINT '***************************************************';

-- **************************************************
-- UPDATE tbld_ScoringPanel TABLE - Add All Enhanced Fields
-- **************************************************

PRINT 'Enhancing tbld_ScoringPanel table...';

-- First, properly size existing fields and change data types for proper scoring
ALTER TABLE [dbo].[tbld_ScoringPanel]
ALTER COLUMN [fldv_Code] [nvarchar](50) NULL;

ALTER TABLE [dbo].[tbld_ScoringPanel]
ALTER COLUMN [fldv_HazardCode] [nvarchar](50) NULL;

ALTER TABLE [dbo].[tbld_ScoringPanel]
ALTER COLUMN [fldv_SMSUserCode] [nvarchar](50) NULL;

-- IMPORTANT: Change scoring fields from nchar to proper data types
ALTER TABLE [dbo].[tbld_ScoringPanel]
ALTER COLUMN [fldv_Likelyhood] [int] NULL; -- Keep misspelling for compatibility

ALTER TABLE [dbo].[tbld_ScoringPanel]
ALTER COLUMN [fldv_Severity] [int] NULL;

ALTER TABLE [dbo].[tbld_ScoringPanel]
ALTER COLUMN [fldv_Score] [decimal](5,2) NULL;

-- Add Enhanced Panel Scoring Fields for Steps 1-5
ALTER TABLE [dbo].[tbld_ScoringPanel]
ADD [fldv_RiskAssessmentCode] [nvarchar](50) NULL; -- Link to specific risk assessment

ALTER TABLE [dbo].[tbld_ScoringPanel]
ADD [fldv_PanelMemberName] [nvarchar](200) NULL; -- Cache member name for performance

ALTER TABLE [dbo].[tbld_ScoringPanel]
ADD [fldv_MemberRole] [nvarchar](100) NULL; -- Role/position of panel member

ALTER TABLE [dbo].[tbld_ScoringPanel]
ADD [fldv_MemberDepartment] [nvarchar](100) NULL; -- Department of panel member

ALTER TABLE [dbo].[tbld_ScoringPanel]
ADD [fldd_ScoreSubmittedDate] [datetime] NULL DEFAULT GETUTCDATE();

ALTER TABLE [dbo].[tbld_ScoringPanel]
ADD [fldv_ScoreRationale] [ntext] NULL; -- Optional rationale for the score

ALTER TABLE [dbo].[tbld_ScoringPanel]
ADD [fldv_ScoringRound] [nvarchar](20) NULL DEFAULT 'Initial'; -- Initial, Residual, etc.

ALTER TABLE [dbo].[tbld_ScoringPanel]
ADD [fldb_IsConsensusScore] [bit] NULL DEFAULT 0; -- Whether this is a consensus score

ALTER TABLE [dbo].[tbld_ScoringPanel]
ADD [fldb_IsActive] [bit] NULL DEFAULT 1; -- Whether this score is currently active

ALTER TABLE [dbo].[tbld_ScoringPanel]
ADD [fldv_Status] [nvarchar](50) NULL DEFAULT 'Submitted'; -- Submitted, Reviewed, Approved

-- Add Quality Assurance Fields
ALTER TABLE [dbo].[tbld_ScoringPanel]
ADD [fldv_ReviewedBy] [nvarchar](50) NULL;

ALTER TABLE [dbo].[tbld_ScoringPanel]
ADD [fldd_ReviewedDate] [datetime] NULL;

ALTER TABLE [dbo].[tbld_ScoringPanel]
ADD [fldv_ReviewComments] [nvarchar](1000) NULL;

-- Add Scoring Session Information
ALTER TABLE [dbo].[tbld_ScoringPanel]
ADD [fldv_ScoringSessionId] [nvarchar](50) NULL; -- Group scores by session

ALTER TABLE [dbo].[tbld_ScoringPanel]
ADD [fldi_TimeSpentMinutes] [int] NULL; -- Time spent on scoring

ALTER TABLE [dbo].[tbld_ScoringPanel]
ADD [fldv_ScoringNotes] [ntext] NULL; -- Additional notes from panel member

-- Add Risk Matrix Context Fields
ALTER TABLE [dbo].[tbld_ScoringPanel]
ADD [fldv_RiskMatrix] [nvarchar](50) NULL DEFAULT 'SMS 5x5'; -- Which risk matrix used

ALTER TABLE [dbo].[tbld_ScoringPanel]
ADD [fldv_SeverityDefinition] [nvarchar](500) NULL; -- Definition used for severity

ALTER TABLE [dbo].[tbld_ScoringPanel]
ADD [fldv_LikelihoodDefinition] [nvarchar](500) NULL; -- Definition used for likelihood

-- Add Confidence and Experience Fields
ALTER TABLE [dbo].[tbld_ScoringPanel]
ADD [fldi_ConfidenceLevel] [int] NULL; -- 1-5 confidence in score

ALTER TABLE [dbo].[tbld_ScoringPanel]
ADD [fldv_ExperienceWithHazardType] [nvarchar](50) NULL; -- High, Medium, Low

ALTER TABLE [dbo].[tbld_ScoringPanel]
ADD [fldv_ScoreJustification] [ntext] NULL; -- Detailed justification for score

-- Add Missing CRITICAL Audit Field
ALTER TABLE [dbo].[tbld_ScoringPanel]
ADD [fldd_CreatedDate] [datetime] NOT NULL DEFAULT GETUTCDATE();

-- Add Other Audit Fields (if not already present)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbld_ScoringPanel' AND COLUMN_NAME = 'fldv_CreatedBy')
BEGIN
    ALTER TABLE [dbo].[tbld_ScoringPanel]
    ADD [fldv_CreatedBy] [nvarchar](50) NOT NULL DEFAULT 'SYSTEM';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbld_ScoringPanel' AND COLUMN_NAME = 'fldv_UpdatedBy')
BEGIN
    ALTER TABLE [dbo].[tbld_ScoringPanel]
    ADD [fldv_UpdatedBy] [nvarchar](50) NULL;
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbld_ScoringPanel' AND COLUMN_NAME = 'fldd_UpdatedDate')
BEGIN
    ALTER TABLE [dbo].[tbld_ScoringPanel]
    ADD [fldd_UpdatedDate] [datetime] NULL;
END

-- **************************************************
-- CREATE ENHANCED PERFORMANCE INDEXES
-- **************************************************

PRINT 'Creating enhanced performance indexes for tbld_ScoringPanel...';

-- Primary lookup indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_tbld_ScoringPanel_HazardCode_SMSUser')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_tbld_ScoringPanel_HazardCode_SMSUser] 
    ON [dbo].[tbld_ScoringPanel] ([fldv_HazardCode], [fldv_SMSUserCode])
    INCLUDE ([fldv_Severity], [fldv_Likelyhood], [fldv_Score], [fldd_ScoreSubmittedDate], [fldb_IsActive]);
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_tbld_ScoringPanel_RiskAssessment')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_tbld_ScoringPanel_RiskAssessment] 
    ON [dbo].[tbld_ScoringPanel] ([fldv_RiskAssessmentCode])
    INCLUDE ([fldv_HazardCode], [fldv_SMSUserCode], [fldv_Status], [fldb_IsActive]);
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_tbld_ScoringPanel_Session_Active')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_tbld_ScoringPanel_Session_Active] 
    ON [dbo].[tbld_ScoringPanel] ([fldv_ScoringSessionId], [fldb_IsActive])
    INCLUDE ([fldv_HazardCode], [fldv_Status], [fldd_ScoreSubmittedDate]);
END

-- Status and workflow indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_tbld_ScoringPanel_Status_Review')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_tbld_ScoringPanel_Status_Review] 
    ON [dbo].[tbld_ScoringPanel] ([fldv_Status], [fldv_ReviewedBy])
    INCLUDE ([fldv_HazardCode], [fldd_ScoreSubmittedDate], [fldd_ReviewedDate]);
END

-- **************************************************
-- CREATE PRIMARY KEY AND CONSTRAINTS
-- **************************************************

PRINT 'Creating primary key and constraints for tbld_ScoringPanel...';

-- Add Primary Key if not exists
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE TABLE_NAME = 'tbld_ScoringPanel' AND CONSTRAINT_TYPE = 'PRIMARY KEY')
BEGIN
    ALTER TABLE [dbo].[tbld_ScoringPanel]
    ADD CONSTRAINT [PK_tbld_ScoringPanel] PRIMARY KEY CLUSTERED ([fldi_ID]);
END

-- Ensure unique panel member scores per hazard (prevent duplicates)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_tbld_ScoringPanel_HazardUser_Round')
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UQ_tbld_ScoringPanel_HazardUser_Round]
    ON [dbo].[tbld_ScoringPanel] ([fldv_HazardCode], [fldv_SMSUserCode], [fldv_ScoringRound])
    WHERE [fldb_IsActive] = 1;
END

-- Add check constraints for scoring ranges
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.CHECK_CONSTRAINTS WHERE CONSTRAINT_NAME = 'CK_tbld_ScoringPanel_Severity')
BEGIN
    ALTER TABLE [dbo].[tbld_ScoringPanel]
    ADD CONSTRAINT [CK_tbld_ScoringPanel_Severity] CHECK ([fldv_Severity] >= 1 AND [fldv_Severity] <= 5);
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.CHECK_CONSTRAINTS WHERE CONSTRAINT_NAME = 'CK_tbld_ScoringPanel_Likelihood')
BEGIN
    ALTER TABLE [dbo].[tbld_ScoringPanel]
    ADD CONSTRAINT [CK_tbld_ScoringPanel_Likelihood] CHECK ([fldv_Likelyhood] >= 1 AND [fldv_Likelyhood] <= 5);
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.CHECK_CONSTRAINTS WHERE CONSTRAINT_NAME = 'CK_tbld_ScoringPanel_Confidence')
BEGIN
    ALTER TABLE [dbo].[tbld_ScoringPanel]
    ADD CONSTRAINT [CK_tbld_ScoringPanel_Confidence] CHECK ([fldi_ConfidenceLevel] >= 1 AND [fldi_ConfidenceLevel] <= 5);
END

-- **************************************************
-- UPDATE pr_ScoringPanel_GetById STORED PROCEDURE
-- **************************************************

PRINT 'Creating enhanced pr_ScoringPanel_GetById stored procedure...';

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('dbo.pr_ScoringPanel_GetById', 'P') IS NOT NULL
    DROP PROCEDURE dbo.pr_ScoringPanel_GetById
GO

CREATE PROCEDURE [dbo].[pr_ScoringPanel_GetById]
    @pID VARCHAR(60),
    @pUserID VARCHAR(10) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @FunctionName VARCHAR(50) = 'pr_ScoringPanel_GetById';
    
    BEGIN TRY
        SET @AuditMessage = 'Retrieving Enhanced ScoringPanel with ID: ' + @pID;
        
        -- Main Scoring Panel Query with Complete Enhanced Data
        SELECT 
            -- Core Fields
            [fldi_ID],
            [fldv_Code],
            [fldv_HazardCode],
            [fldv_SMSUserCode],
            [fldv_Likelyhood] AS LikelihoodScore, -- Note: keeping misspelling for compatibility
            [fldv_Severity] AS SeverityScore,
            [fldv_Score] AS CalculatedScore,
            
            -- Enhanced Fields
            [fldv_RiskAssessmentCode],
            [fldv_PanelMemberName],
            [fldv_MemberRole],
            [fldv_MemberDepartment],
            [fldd_ScoreSubmittedDate],
            [fldv_ScoreRationale],
            [fldv_ScoringRound],
            [fldb_IsConsensusScore],
            [fldb_IsActive],
            [fldv_Status],
            
            -- Quality Assurance Fields
            [fldv_ReviewedBy],
            [fldd_ReviewedDate],
            [fldv_ReviewComments],
            
            -- Session Fields
            [fldv_ScoringSessionId],
            [fldi_TimeSpentMinutes],
            [fldv_ScoringNotes],
            
            -- Risk Matrix Context
            [fldv_RiskMatrix],
            [fldv_SeverityDefinition],
            [fldv_LikelihoodDefinition],
            
            -- Confidence and Experience
            [fldi_ConfidenceLevel],
            [fldv_ExperienceWithHazardType],
            [fldv_ScoreJustification],
            
            -- Audit Fields
            [fldv_CreatedBy],
            [fldd_CreatedDate],
            [fldv_UpdatedBy],
            [fldd_UpdatedDate]
            
        FROM [dbo].[tbld_ScoringPanel] 
        WHERE [fldv_Code] = @pID;
        
        SET @AuditMessage = 'Successfully retrieved Enhanced ScoringPanel with ID: ' + @pID;
        
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error retrieving Enhanced ScoringPanel ID ' + @pID + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_ScoringPanels', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;

GO

-- **************************************************
-- VERIFICATION QUERIES
-- **************************************************

PRINT 'Running verification queries...';

-- Check enhanced ScoringPanel table structure
PRINT 'Enhanced ScoringPanel Table Structure:';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'tbld_ScoringPanel'
ORDER BY ORDINAL_POSITION;

-- Test enhanced scoring panel query
SELECT 
    SP.fldv_Code,
    SP.fldv_HazardCode,
    SP.fldv_SMSUserCode,
    SP.fldv_PanelMemberName,
    SP.fldv_Severity,
    SP.fldv_Likelyhood,
    SP.fldv_Score,
    SP.fldd_ScoreSubmittedDate,
    SP.fldv_ScoringRound,
    SP.fldv_Status,
    SP.fldb_IsActive
FROM [dbo].[tbld_ScoringPanel] SP
WHERE SP.fldb_IsActive = 1
ORDER BY SP.fldv_HazardCode, SP.fldd_ScoreSubmittedDate;

PRINT '***************************************************';
PRINT 'tbld_ScoringPanel ENHANCEMENT COMPLETED SUCCESSFULLY!';
PRINT '***************************************************';
PRINT 'ENHANCED CAPABILITIES ADDED:';
PRINT '? Step 4 Panel Scoring: Complete panel management & scoring tracking';
PRINT '? Proper Data Types: int for Severity/Likelihood, decimal for scores';
PRINT '? Risk Assessment Links: Direct connection to specific assessments';
PRINT '? Quality Assurance: Review workflows and score validation';
PRINT '? Session Management: Scoring session tracking and grouping';
PRINT '? Confidence Tracking: Panel member confidence and experience';
PRINT '? Audit Trail: Complete creation and modification tracking including fldd_CreatedDate';
PRINT '? Performance Indexes: Strategic indexes for key lookup patterns';
PRINT '? Data Integrity: Constraints for score ranges and uniqueness';
PRINT '';
PRINT 'CRITICAL FIX:';
PRINT '? fldd_CreatedDate: Added missing audit field for complete tracking';
PRINT '';
PRINT 'ARCHITECTURE INTEGRATION:';
PRINT '- Compatible with existing ScoringPanelRepository';
PRINT '- Ready for enhanced Step 4 panel scoring workflows';
PRINT '- Supports multi-hazard, multi-assessment scenarios';
PRINT '- Optimized for RiskAssessmentWizard Step 4 functionality';
PRINT '***************************************************';

GO