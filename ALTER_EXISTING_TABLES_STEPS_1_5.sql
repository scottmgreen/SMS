-- **************************************************
-- ALTER EXISTING TABLES FOR RISK ASSESSMENT WIZARD
-- Updates tbld_ReportValidations and tbld_RiskAssessments
-- for Steps 1-5 support
-- **************************************************
USE [PDXSMS_V2]
GO

-- **************************************************
-- UPDATE tbld_RiskAssessments TABLE
-- **************************************************

-- First, let's properly size the existing fields and add missing ones
ALTER TABLE [dbo].[tbld_RiskAssessments]
ALTER COLUMN [fldv_Code] [nvarchar](50) NULL;

ALTER TABLE [dbo].[tbld_RiskAssessments]
ALTER COLUMN [fldv_Name] [nvarchar](200) NULL;

ALTER TABLE [dbo].[tbld_RiskAssessments]
ALTER COLUMN [fldv_Description] [ntext] NULL;

ALTER TABLE [dbo].[tbld_RiskAssessments]
ALTER COLUMN [fldv_HazardCode] [nvarchar](50) NULL;

ALTER TABLE [dbo].[tbld_RiskAssessments]
ALTER COLUMN [fldv_AssessmentType] [nvarchar](50) NULL;

ALTER TABLE [dbo].[tbld_RiskAssessments]
ALTER COLUMN [fldv_Status] [nvarchar](50) NULL;

ALTER TABLE [dbo].[tbld_RiskAssessments]
ALTER COLUMN [fldv_Stage] [nvarchar](50) NULL;

-- Add missing core fields
ALTER TABLE [dbo].[tbld_RiskAssessments]
ADD [fldv_LeadAssessorId] [nvarchar](50) NULL;

ALTER TABLE [dbo].[tbld_RiskAssessments]
ADD [fldv_PrimaryHazardId] [nvarchar](50) NULL;

ALTER TABLE [dbo].[tbld_RiskAssessments]
ADD [fldv_RiskAssessmentCategory] [nvarchar](50) NULL;

ALTER TABLE [dbo].[tbld_RiskAssessments]
ADD [fldi_CurrentStep] [int] NULL DEFAULT 1;

ALTER TABLE [dbo].[tbld_RiskAssessments]
ADD [fldd_CompletedDate] [datetime] NULL;

ALTER TABLE [dbo].[tbld_RiskAssessments]
ADD [fldv_CompletedBy] [nvarchar](50) NULL;

ALTER TABLE [dbo].[tbld_RiskAssessments]
ADD [fldv_ParentAssessmentId] [nvarchar](50) NULL;

-- **************************************************
-- ADD STEP 1 FIELDS TO tbld_RiskAssessments
-- **************************************************
ALTER TABLE [dbo].[tbld_RiskAssessments]
ADD [fldv_SystemDescription] [ntext] NULL;

ALTER TABLE [dbo].[tbld_RiskAssessments]
ADD [fldv_SystemBoundaries] [ntext] NULL;

ALTER TABLE [dbo].[tbld_RiskAssessments]
ADD [fldv_SystemPurpose] [ntext] NULL;

ALTER TABLE [dbo].[tbld_RiskAssessments]
ADD [fldv_PersonnelFactors] [ntext] NULL;

ALTER TABLE [dbo].[tbld_RiskAssessments]
ADD [fldv_EquipmentFactors] [ntext] NULL;

ALTER TABLE [dbo].[tbld_RiskAssessments]
ADD [fldv_ProcedureFactors] [ntext] NULL;

ALTER TABLE [dbo].[tbld_RiskAssessments]
ADD [fldv_ResourceFactors] [ntext] NULL;

ALTER TABLE [dbo].[tbld_RiskAssessments]
ADD [fldv_EnvironmentFactors] [ntext] NULL;

-- **************************************************
-- ADD STEP 3 FIELDS TO tbld_RiskAssessments  
-- **************************************************
ALTER TABLE [dbo].[tbld_RiskAssessments]
ADD [fldv_RiskAnalysisMethod] [nvarchar](100) NULL DEFAULT 'SMS Risk Matrix';

ALTER TABLE [dbo].[tbld_RiskAssessments]
ADD [fldv_RiskCriteria] [ntext] NULL;

-- **************************************************
-- ADD STEP 4 FIELDS TO tbld_RiskAssessments
-- **************************************************
ALTER TABLE [dbo].[tbld_RiskAssessments]
ADD [fldv_TolerabilityFramework] [nvarchar](100) NULL DEFAULT 'PDX-SMS Default';

ALTER TABLE [dbo].[tbld_RiskAssessments]
ADD [fldv_RiskAcceptanceCriteria] [ntext] NULL;

ALTER TABLE [dbo].[tbld_RiskAssessments]
ADD [fldi_FinalSeverityScore] [int] NULL;

ALTER TABLE [dbo].[tbld_RiskAssessments]
ADD [fldi_FinalLikelihoodScore] [int] NULL;

ALTER TABLE [dbo].[tbld_RiskAssessments]
ADD [fldv_FinalRiskLevel] [nvarchar](10) NULL;

ALTER TABLE [dbo].[tbld_RiskAssessments]
ADD [fldv_RiskTolerability] [nvarchar](50) NULL DEFAULT 'ALARP';

ALTER TABLE [dbo].[tbld_RiskAssessments]
ADD [fldv_AssessmentRationale] [ntext] NULL;

-- **************************************************
-- ADD STEP 5 FIELDS TO tbld_RiskAssessments
-- **************************************************
ALTER TABLE [dbo].[tbld_RiskAssessments]
ADD [fldv_ImplementationStrategy] [ntext] NULL;

ALTER TABLE [dbo].[tbld_RiskAssessments]
ADD [fldd_OverallTargetDate] [datetime] NULL;

ALTER TABLE [dbo].[tbld_RiskAssessments]
ADD [fldv_ImplementationNotes] [ntext] NULL;

-- **************************************************
-- ADD PROGRESS TRACKING FIELDS TO tbld_RiskAssessments
-- **************************************************
ALTER TABLE [dbo].[tbld_RiskAssessments]
ADD [fldv_CompletedSteps] [nvarchar](50) NULL;

ALTER TABLE [dbo].[tbld_RiskAssessments]
ADD [fldi_CompletionPercentage] [int] NULL DEFAULT 0;

-- **************************************************
-- ADD AUDIT FIELDS TO tbld_RiskAssessments (if not already present)
-- **************************************************
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbld_RiskAssessments' AND COLUMN_NAME = 'fldv_CreatedBy')
BEGIN
    ALTER TABLE [dbo].[tbld_RiskAssessments]
    ADD [fldv_CreatedBy] [nvarchar](50) NOT NULL DEFAULT 'SYSTEM';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbld_RiskAssessments' AND COLUMN_NAME = 'fldd_CreatedDate')
BEGIN
    ALTER TABLE [dbo].[tbld_RiskAssessments]
    ADD [fldd_CreatedDate] [datetime] NOT NULL DEFAULT GETUTCDATE();
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbld_RiskAssessments' AND COLUMN_NAME = 'fldv_UpdatedBy')
BEGIN
    ALTER TABLE [dbo].[tbld_RiskAssessments]
    ADD [fldv_UpdatedBy] [nvarchar](50) NULL;
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbld_RiskAssessments' AND COLUMN_NAME = 'fldd_UpdatedDate')
BEGIN
    ALTER TABLE [dbo].[tbld_RiskAssessments]
    ADD [fldd_UpdatedDate] [datetime] NULL;
END

-- **************************************************
-- UPDATE tbld_ReportValidations FOR ADDITIONAL FIELDS
-- **************************************************

-- Add Step 2 hazard data fields
ALTER TABLE [dbo].[tbld_ReportValidations]
ADD [fldv_HazardIds] [nvarchar](max) NULL;

ALTER TABLE [dbo].[tbld_ReportValidations]
ADD [fldv_HazardDescriptions] [ntext] NULL;

ALTER TABLE [dbo].[tbld_ReportValidations]
ADD [fldv_HazardCategories] [nvarchar](max) NULL;

ALTER TABLE [dbo].[tbld_ReportValidations]
ADD [fldi_ValidHazardCount] [int] NULL;

-- Add Step 3 hazard-specific analysis fields
ALTER TABLE [dbo].[tbld_ReportValidations]
ADD [fldv_HazardAnalysesData] [ntext] NULL;

ALTER TABLE [dbo].[tbld_ReportValidations]
ADD [fldv_HazardWorstOutcomes] [ntext] NULL;

ALTER TABLE [dbo].[tbld_ReportValidations]
ADD [fldv_HazardRootCauses] [ntext] NULL;

-- Add Step 4 panel scoring fields  
ALTER TABLE [dbo].[tbld_ReportValidations]
ADD [fldv_HazardPanelMembers] [ntext] NULL;

ALTER TABLE [dbo].[tbld_ReportValidations]
ADD [fldv_HazardPanelScores] [ntext] NULL;

ALTER TABLE [dbo].[tbld_ReportValidations]
ADD [fldv_HazardAverageScores] [ntext] NULL;

-- Add Step 5 mitigation fields
ALTER TABLE [dbo].[tbld_ReportValidations]
ADD [fldv_SavedMitigationStrategies] [ntext] NULL;

ALTER TABLE [dbo].[tbld_ReportValidations]
ADD [fldv_HazardMitigationStrategyIds] [ntext] NULL;

ALTER TABLE [dbo].[tbld_ReportValidations]
ADD [fldv_MonitoringRequirements] [ntext] NULL;

-- Add progress tracking to report validations
ALTER TABLE [dbo].[tbld_ReportValidations]
ADD [fldi_CurrentStep] [int] NULL DEFAULT 1;

ALTER TABLE [dbo].[tbld_ReportValidations]
ADD [fldv_CompletedSteps] [nvarchar](50) NULL;

ALTER TABLE [dbo].[tbld_ReportValidations]
ADD [fldi_CompletionPercentage] [int] NULL DEFAULT 0;

ALTER TABLE [dbo].[tbld_ReportValidations]
ADD [fldv_AssessmentType] [nvarchar](50) NULL DEFAULT 'Initial';

ALTER TABLE [dbo].[tbld_ReportValidations]
ADD [fldv_AssessmentCategory] [nvarchar](50) NULL DEFAULT 'FiveStep';

ALTER TABLE [dbo].[tbld_ReportValidations]
ADD [fldv_ParentAssessmentId] [nvarchar](50) NULL;

-- **************************************************
-- CREATE PERFORMANCE INDEXES
-- **************************************************

-- Index on RiskAssessments Code for lookups
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_tbld_RiskAssessments_Code')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_tbld_RiskAssessments_Code] 
    ON [dbo].[tbld_RiskAssessments] ([fldv_Code])
    INCLUDE ([fldv_Status], [fldv_Stage], [fldi_CurrentStep]);
END

-- Index on RiskAssessments LeadAssessorId
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_tbld_RiskAssessments_LeadAssessor')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_tbld_RiskAssessments_LeadAssessor] 
    ON [dbo].[tbld_RiskAssessments] ([fldv_LeadAssessorId])
    INCLUDE ([fldv_Status], [fldi_CurrentStep], [fldd_CreatedDate]);
END

-- Index on RiskAssessments HazardCode
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_tbld_RiskAssessments_HazardCode')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_tbld_RiskAssessments_HazardCode] 
    ON [dbo].[tbld_RiskAssessments] ([fldv_HazardCode])
    INCLUDE ([fldv_Status], [fldi_CurrentStep]);
END

-- Index on ReportValidations ReportCode
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_tbld_ReportValidations_ReportCode')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_tbld_ReportValidations_ReportCode] 
    ON [dbo].[tbld_ReportValidations] ([fldv_ReportCode])
    INCLUDE ([fldv_Status], [fldv_Stage], [fldi_CurrentStep]);
END

-- Index on ReportValidations CurrentStep
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_tbld_ReportValidations_CurrentStep')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_tbld_ReportValidations_CurrentStep] 
    ON [dbo].[tbld_ReportValidations] ([fldi_CurrentStep], [fldv_Status])
    INCLUDE ([fldv_ReportCode], [fldi_CompletionPercentage]);
END

-- **************************************************
-- SAMPLE QUERIES TO VERIFY THE UPDATES
-- **************************************************

-- Check RiskAssessments table structure
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'tbld_RiskAssessments'
ORDER BY ORDINAL_POSITION;

-- Check ReportValidations table structure
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'tbld_ReportValidations'
ORDER BY ORDINAL_POSITION;

-- Test query for Step 1 data in RiskAssessments
SELECT 
    fldv_Code,
    fldv_Name,
    fldv_LeadAssessorId,
    fldv_SystemDescription,
    fldv_PersonnelFactors,
    fldi_CurrentStep,
    fldv_Status
FROM [dbo].[tbld_RiskAssessments]
WHERE fldi_CurrentStep >= 1;

-- Test query for Step 3 data in ReportValidations
SELECT 
    fldv_Code,
    fldv_ReportCode,
    fldv_HazardWorstOutcomes,
    fldv_HazardRootCauses,
    fldi_CurrentStep
FROM [dbo].[tbld_ReportValidations]
WHERE fldi_CurrentStep >= 3;

-- **************************************************
-- UPDATE FieldNames.cs CONSTANTS
-- **************************************************

PRINT '***************************************************';
PRINT 'Database schema updated successfully!';
PRINT '***************************************************';
PRINT 'NEXT STEPS:';
PRINT '1. Update Infrastructure\Common\FieldNames.cs with new field constants';
PRINT '2. Update your Repository classes to use the new fields';
PRINT '3. Test the step-by-step save functionality';
PRINT '***************************************************';
PRINT 'NEW FIELDS ADDED:';
PRINT '- tbld_RiskAssessments: 25+ new fields for Steps 1-5';
PRINT '- tbld_ReportValidations: 15+ new fields for multi-hazard support';
PRINT '- Performance indexes created for key lookup fields';
PRINT '***************************************************';

GO