-- **************************************************
-- ALTER EXISTING HAZARDS AND SCORING PANEL TABLES
-- Updates tbld_Hazards and tbld_ScoringPanel for Steps 1-5 support
-- **************************************************
USE [PDXSMS_V2]
GO

PRINT '***************************************************';
PRINT 'ENHANCING HAZARDS AND SCORING PANEL TABLES FOR STEPS 1-5';
PRINT '***************************************************';

-- **************************************************
-- UPDATE tbld_Hazards TABLE - Add All Enhanced Fields
-- **************************************************

PRINT 'Updating tbld_Hazards table...';

-- First, properly size existing fields
ALTER TABLE [dbo].[tbld_Hazards]
ALTER COLUMN [fldv_Code] [nvarchar](50) NOT NULL;

ALTER TABLE [dbo].[tbld_Hazards]
ALTER COLUMN [fldv_Name] [nvarchar](200) NULL;

ALTER TABLE [dbo].[tbld_Hazards]
ALTER COLUMN [fldc_Description] [ntext] NULL;

ALTER TABLE [dbo].[tbld_Hazards]
ALTER COLUMN [fldv_ReportCode] [nvarchar](50) NOT NULL;

ALTER TABLE [dbo].[tbld_Hazards]
ALTER COLUMN [fldv_ScoringPanelCode] [nvarchar](50) NULL;

ALTER TABLE [dbo].[tbld_Hazards]
ALTER COLUMN [fldv_AverageScore] [decimal](5,2) NULL;

-- Add Enhanced Hazard Classification Fields
ALTER TABLE [dbo].[tbld_Hazards]
ADD [fldv_HazardType] [nvarchar](100) NULL;

ALTER TABLE [dbo].[tbld_Hazards]
ADD [fldv_Category] [nvarchar](100) NULL;

ALTER TABLE [dbo].[tbld_Hazards]
ADD [fldv_Status] [nvarchar](50) NULL DEFAULT 'Active';

ALTER TABLE [dbo].[tbld_Hazards]
ADD [fldv_Priority] [nvarchar](50) NULL;

ALTER TABLE [dbo].[tbld_Hazards]
ADD [fldv_RiskLevel] [nvarchar](50) NULL;

-- Add Reporting and Investigation Fields  
ALTER TABLE [dbo].[tbld_Hazards]
ADD [fldv_ReportedBy] [nvarchar](50) NULL;

ALTER TABLE [dbo].[tbld_Hazards]
ADD [fldd_ReportedOn] [datetime] NULL;

ALTER TABLE [dbo].[tbld_Hazards]
ADD [fldv_ReportingDepartment] [nvarchar](100) NULL;

ALTER TABLE [dbo].[tbld_Hazards]
ADD [fldb_IsConfidential] [bit] NULL DEFAULT 0;

ALTER TABLE [dbo].[tbld_Hazards]
ADD [fldb_IsAnonymous] [bit] NULL DEFAULT 0;

-- Add Step 3 Risk Analysis Fields (Hazard-Specific)
ALTER TABLE [dbo].[tbld_Hazards]
ADD [fldc_WorstCredibleOutcome] [ntext] NULL;

ALTER TABLE [dbo].[tbld_Hazards]
ADD [fldc_RootCause] [ntext] NULL;

ALTER TABLE [dbo].[tbld_Hazards]
ADD [fldc_AdditionalComments] [ntext] NULL;

-- Add Investigation Tracking Fields
ALTER TABLE [dbo].[tbld_Hazards]
ADD [fldb_RequiresInvestigation] [bit] NULL DEFAULT 0;

ALTER TABLE [dbo].[tbld_Hazards]
ADD [fldd_InvestigationCompletedDate] [datetime] NULL;

ALTER TABLE [dbo].[tbld_Hazards]
ADD [fldc_InvestigationNotes] [ntext] NULL;

-- Add Location Information Fields
ALTER TABLE [dbo].[tbld_Hazards]
ADD [fldv_Location] [nvarchar](200) NULL;

ALTER TABLE [dbo].[tbld_Hazards]
ADD [fldv_LocationArea] [nvarchar](100) NULL;

ALTER TABLE [dbo].[tbld_Hazards]
ADD [fldv_LocationSubArea] [nvarchar](100) NULL;

-- Add Audit Fields (if not already present)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbld_Hazards' AND COLUMN_NAME = 'fldv_CreatedBy')
BEGIN
    ALTER TABLE [dbo].[tbld_Hazards]
    ADD [fldv_CreatedBy] [nvarchar](50) NOT NULL DEFAULT 'SYSTEM';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbld_Hazards' AND COLUMN_NAME = 'fldd_CreatedDate')
BEGIN
    ALTER TABLE [dbo].[tbld_Hazards]
    ADD [fldd_CreatedDate] [datetime] NOT NULL DEFAULT GETUTCDATE();
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbld_Hazards' AND COLUMN_NAME = 'fldv_UpdatedBy')
BEGIN
    ALTER TABLE [dbo].[tbld_Hazards]
    ADD [fldv_UpdatedBy] [nvarchar](50) NULL;
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbld_Hazards' AND COLUMN_NAME = 'fldd_UpdatedDate')
BEGIN
    ALTER TABLE [dbo].[tbld_Hazards]
    ADD [fldd_UpdatedDate] [datetime] NULL;
END

-- **************************************************
-- UPDATE tbld_ScoringPanel TABLE - Add Enhanced Fields  
-- **************************************************

PRINT 'Updating tbld_ScoringPanel table...';

-- First, properly size existing fields
ALTER TABLE [dbo].[tbld_ScoringPanel]
ALTER COLUMN [fldv_Code] [nvarchar](50) NULL;

ALTER TABLE [dbo].[tbld_ScoringPanel]
ALTER COLUMN [fldv_HazardCode] [nvarchar](50) NULL;

ALTER TABLE [dbo].[tbld_ScoringPanel]
ALTER COLUMN [fldv_SMSUserCode] [nvarchar](50) NULL;

ALTER TABLE [dbo].[tbld_ScoringPanel]
ALTER COLUMN [fldv_Likelyhood] [int] NULL; -- Change from nchar to int for proper scoring

ALTER TABLE [dbo].[tbld_ScoringPanel]
ALTER COLUMN [fldv_Severity] [int] NULL; -- Change from nchar to int for proper scoring

ALTER TABLE [dbo].[tbld_ScoringPanel]
ALTER COLUMN [fldv_Score] [decimal](5,2) NULL; -- Change from nchar to decimal for calculated scores

-- Add Enhanced Panel Scoring Fields
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

-- Add Audit Fields (if not already present)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbld_ScoringPanel' AND COLUMN_NAME = 'fldv_CreatedBy')
BEGIN
    ALTER TABLE [dbo].[tbld_ScoringPanel]
    ADD [fldv_CreatedBy] [nvarchar](50) NOT NULL DEFAULT 'SYSTEM';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbld_ScoringPanel' AND COLUMN_NAME = 'fldd_CreatedDate')
BEGIN
    ALTER TABLE [dbo].[tbld_ScoringPanel]
    ADD [fldd_CreatedDate] [datetime] NOT NULL DEFAULT GETUTCDATE();
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

PRINT 'Creating performance indexes...';

-- Hazards Table Indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_tbld_Hazards_Code')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_tbld_Hazards_Code] 
    ON [dbo].[tbld_Hazards] ([fldv_Code])
    INCLUDE ([fldv_Status], [fldv_HazardType], [fldv_Priority]);
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_tbld_Hazards_ReportCode')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_tbld_Hazards_ReportCode] 
    ON [dbo].[tbld_Hazards] ([fldv_ReportCode])
    INCLUDE ([fldv_Status], [fldv_RiskLevel]);
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_tbld_Hazards_Status_Type')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_tbld_Hazards_Status_Type] 
    ON [dbo].[tbld_Hazards] ([fldv_Status], [fldv_HazardType])
    INCLUDE ([fldv_Code], [fldv_Priority], [fldv_RiskLevel]);
END

-- ScoringPanel Table Indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_tbld_ScoringPanel_HazardCode_SMSUser')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_tbld_ScoringPanel_HazardCode_SMSUser] 
    ON [dbo].[tbld_ScoringPanel] ([fldv_HazardCode], [fldv_SMSUserCode])
    INCLUDE ([fldv_Severity], [fldv_Likelyhood], [fldv_Score], [fldd_ScoreSubmittedDate]);
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

-- **************************************************
-- CREATE PRIMARY KEYS IF NOT EXISTS
-- **************************************************

PRINT 'Ensuring primary keys exist...';

-- Add Primary Key to tbld_Hazards if not exists
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE TABLE_NAME = 'tbld_Hazards' AND CONSTRAINT_TYPE = 'PRIMARY KEY')
BEGIN
    ALTER TABLE [dbo].[tbld_Hazards]
    ADD CONSTRAINT [PK_tbld_Hazards] PRIMARY KEY CLUSTERED ([fldi_ID]);
END

-- Add Primary Key to tbld_ScoringPanel if not exists
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE TABLE_NAME = 'tbld_ScoringPanel' AND CONSTRAINT_TYPE = 'PRIMARY KEY')
BEGIN
    ALTER TABLE [dbo].[tbld_ScoringPanel]
    ADD CONSTRAINT [PK_tbld_ScoringPanel] PRIMARY KEY CLUSTERED ([fldi_ID]);
END

-- **************************************************
-- CREATE UNIQUE CONSTRAINTS
-- **************************************************

PRINT 'Creating unique constraints...';

-- Ensure unique hazard codes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_tbld_Hazards_Code')
BEGIN
    ALTER TABLE [dbo].[tbld_Hazards]
    ADD CONSTRAINT [UQ_tbld_Hazards_Code] UNIQUE ([fldv_Code]);
END

-- Ensure unique panel member scores per hazard (prevent duplicates)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_tbld_ScoringPanel_HazardUser_Round')
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UQ_tbld_ScoringPanel_HazardUser_Round]
    ON [dbo].[tbld_ScoringPanel] ([fldv_HazardCode], [fldv_SMSUserCode], [fldv_ScoringRound])
    WHERE [fldb_IsActive] = 1;
END

-- **************************************************
-- SAMPLE VERIFICATION QUERIES
-- **************************************************

PRINT 'Running verification queries...';

-- Check enhanced Hazards table structure
PRINT 'Enhanced Hazards Table Structure:';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'tbld_Hazards'
ORDER BY ORDINAL_POSITION;

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

-- Test enhanced hazards query for Step 3
SELECT 
    fldv_Code,
    fldv_Name,
    fldv_HazardType,
    fldv_Category,
    fldc_WorstCredibleOutcome,
    fldc_RootCause,
    fldv_Status,
    fldv_RiskLevel
FROM [dbo].[tbld_Hazards]
WHERE fldv_Status = 'Active';

-- Test enhanced scoring panel query for Step 4
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
    SP.fldv_Status
FROM [dbo].[tbld_ScoringPanel] SP
WHERE SP.fldb_IsActive = 1
ORDER BY SP.fldv_HazardCode, SP.fldd_ScoreSubmittedDate;

-- **************************************************
-- UPDATE FIELD MAPPING CONSTANTS
-- **************************************************

PRINT '***************************************************';
PRINT 'DATABASE SCHEMA ENHANCEMENT COMPLETED SUCCESSFULLY!';
PRINT '***************************************************';
PRINT 'ENHANCED TABLES:';
PRINT '- tbld_Hazards: 25+ new fields for comprehensive hazard management';
PRINT '- tbld_ScoringPanel: 15+ new fields for enhanced panel scoring';
PRINT '';
PRINT 'NEW CAPABILITIES ADDED:';
PRINT '? Step 3 Risk Analysis: Worst outcomes & root causes per hazard';
PRINT '? Step 4 Panel Scoring: Enhanced panel management & scoring tracking';
PRINT '? Hazard Classification: Type, category, 5M component mapping';
PRINT '? Investigation Tracking: Investigation requirements & completion';
PRINT '? Mitigation Planning: Current & proposed mitigations per hazard';
PRINT '? Location Information: Area, sub-area, and location details';
PRINT '? Quality Assurance: Review workflows and score validation';
PRINT '? Audit Trail: Complete creation and modification tracking';
PRINT '? Performance Optimization: Strategic indexes for key queries';
PRINT '';
PRINT 'NEXT STEPS:';
PRINT '1. Update Infrastructure\Common\FieldNames.cs with new field constants';
PRINT '2. Update Repository classes to use enhanced fields';
PRINT '3. Update DataService mappers for new fields';
PRINT '4. Test enhanced stored procedures with new schema';
PRINT '5. Update RiskAssessmentWizard UI to leverage enhanced data';
PRINT '***************************************************';

GO