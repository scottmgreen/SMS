-- **************************************************
-- ENHANCE tbld_Mitigations TABLE FOR STEPS 1-5
-- Comprehensive mitigation management for Risk Assessment workflow
-- **************************************************
USE [PDXSMS_V2]
GO

PRINT '***************************************************';
PRINT 'ENHANCING tbld_Mitigations TABLE FOR STEPS 1-5';
PRINT '***************************************************';

-- **************************************************
-- UPDATE tbld_Mitigations TABLE - Add All Enhanced Fields
-- **************************************************

PRINT 'Updating tbld_Mitigations table...';

-- First, properly size existing fields
ALTER TABLE [dbo].[tbld_Mitigations]
ALTER COLUMN [fldv_Code] [nvarchar](50) NOT NULL;

ALTER TABLE [dbo].[tbld_Mitigations]
ALTER COLUMN [fldv_HazardCode] [nvarchar](50) NOT NULL;

-- Add Essential Mitigation Fields
ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldv_Name] [nvarchar](200) NULL;

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldc_Description] [ntext] NULL;

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldv_Type] [nvarchar](50) NULL; -- Current, Proposed, Implemented, etc.

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldv_Status] [nvarchar](50) NULL DEFAULT 'Proposed';

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldv_Priority] [nvarchar](50) NULL;

-- Add Step 5 Implementation Planning Fields
ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldv_RiskAssessmentCode] [nvarchar](50) NULL; -- Link to specific risk assessment

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldd_TargetDate] [datetime] NULL;

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldd_ImplementationDate] [datetime] NULL;

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldd_CompletionDate] [datetime] NULL;

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldv_AssignedDepartment] [nvarchar](100) NULL;

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldv_AssignedTo] [nvarchar](50) NULL; -- Person responsible

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldv_ApprovedBy] [nvarchar](50) NULL;

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldd_ApprovedDate] [datetime] NULL;

-- Add Progress Tracking Fields
ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldi_Progress] [int] NULL DEFAULT 0; -- 0-100 percentage

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldv_ProgressNotes] [ntext] NULL;

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldd_LastProgressUpdate] [datetime] NULL;

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldv_ProgressUpdatedBy] [nvarchar](50) NULL;

-- Add Cost and Resource Fields
ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldd_EstimatedCost] [decimal](12,2) NULL;

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldd_ActualCost] [decimal](12,2) NULL;

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldv_ResourceRequirements] [ntext] NULL;

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldi_EstimatedHours] [int] NULL;

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldi_ActualHours] [int] NULL;

-- Add Effectiveness and Monitoring Fields
ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldv_EffectivenessRating] [nvarchar](20) NULL; -- High, Medium, Low

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldc_EffectivenessNotes] [ntext] NULL;

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldd_EffectivenessReviewDate] [datetime] NULL;

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldv_EffectivenessReviewedBy] [nvarchar](50) NULL;

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldc_MonitoringRequirements] [ntext] NULL;

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldv_MonitoringFrequency] [nvarchar](50) NULL; -- Daily, Weekly, Monthly, etc.

-- Add Risk Reduction Fields
ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldi_ExpectedSeverityReduction] [int] NULL; -- Expected reduction in severity score

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldi_ExpectedLikelihoodReduction] [int] NULL; -- Expected reduction in likelihood score

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldi_ActualSeverityReduction] [int] NULL; -- Actual reduction achieved

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldi_ActualLikelihoodReduction] [int] NULL; -- Actual reduction achieved

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldv_ResidualRiskLevel] [nvarchar](50) NULL; -- Risk level after mitigation

-- Add Dependencies and Prerequisites
ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldv_Prerequisites] [ntext] NULL;

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldv_Dependencies] [nvarchar](500) NULL; -- Other mitigation codes this depends on

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldb_HasDependencies] [bit] NULL DEFAULT 0;

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldb_IsPrerequisite] [bit] NULL DEFAULT 0; -- Is this a prerequisite for others

-- Add Communication and Documentation
ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldc_ImplementationPlan] [ntext] NULL;

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldc_CommunicationPlan] [ntext] NULL;

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldc_TrainingRequirements] [ntext] NULL;

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldc_DocumentationUpdates] [ntext] NULL;

-- Add Validation and Testing
ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldc_TestingProcedure] [ntext] NULL;

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldd_TestingCompletedDate] [datetime] NULL;

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldv_TestingResults] [nvarchar](200) NULL;

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldb_ValidationRequired] [bit] NULL DEFAULT 0;

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldd_ValidationDate] [datetime] NULL;

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldv_ValidatedBy] [nvarchar](50) NULL;

-- Add General Notes and Comments
ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldc_Notes] [ntext] NULL;

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldc_LessonsLearned] [ntext] NULL;

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldc_RecommendationsForFuture] [ntext] NULL;

-- Add Audit Fields (if not already present)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbld_Mitigations' AND COLUMN_NAME = 'fldv_CreatedBy')
BEGIN
    ALTER TABLE [dbo].[tbld_Mitigations]
    ADD [fldv_CreatedBy] [nvarchar](50) NOT NULL DEFAULT 'SYSTEM';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbld_Mitigations' AND COLUMN_NAME = 'fldd_CreatedDate')
BEGIN
    ALTER TABLE [dbo].[tbld_Mitigations]
    ADD [fldd_CreatedDate] [datetime] NOT NULL DEFAULT GETUTCDATE();
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbld_Mitigations' AND COLUMN_NAME = 'fldv_UpdatedBy')
BEGIN
    ALTER TABLE [dbo].[tbld_Mitigations]
    ADD [fldv_UpdatedBy] [nvarchar](50) NULL;
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbld_Mitigations' AND COLUMN_NAME = 'fldd_UpdatedDate')
BEGIN
    ALTER TABLE [dbo].[tbld_Mitigations]
    ADD [fldd_UpdatedDate] [datetime] NULL;
END

-- **************************************************
-- CREATE PERFORMANCE INDEXES
-- **************************************************

PRINT 'Creating performance indexes for tbld_Mitigations...';

-- Primary lookup indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_tbld_Mitigations_Code')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_tbld_Mitigations_Code] 
    ON [dbo].[tbld_Mitigations] ([fldv_Code])
    INCLUDE ([fldv_Status], [fldv_Type], [fldi_Progress]);
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_tbld_Mitigations_HazardCode')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_tbld_Mitigations_HazardCode] 
    ON [dbo].[tbld_Mitigations] ([fldv_HazardCode])
    INCLUDE ([fldv_Status], [fldv_Type], [fldi_Progress], [fldd_TargetDate]);
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_tbld_Mitigations_RiskAssessment')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_tbld_Mitigations_RiskAssessment] 
    ON [dbo].[tbld_Mitigations] ([fldv_RiskAssessmentCode])
    INCLUDE ([fldv_HazardCode], [fldv_Status], [fldi_Progress]);
END

-- Status and progress tracking indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_tbld_Mitigations_Status_Progress')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_tbld_Mitigations_Status_Progress] 
    ON [dbo].[tbld_Mitigations] ([fldv_Status], [fldi_Progress])
    INCLUDE ([fldv_HazardCode], [fldv_AssignedTo], [fldd_TargetDate]);
END

-- Assignment and department indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_tbld_Mitigations_Assignment')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_tbld_Mitigations_Assignment] 
    ON [dbo].[tbld_Mitigations] ([fldv_AssignedTo], [fldv_AssignedDepartment])
    INCLUDE ([fldv_Status], [fldi_Progress], [fldd_TargetDate]);
END

-- Target date index for deadline tracking
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_tbld_Mitigations_TargetDate')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_tbld_Mitigations_TargetDate] 
    ON [dbo].[tbld_Mitigations] ([fldd_TargetDate])
    INCLUDE ([fldv_Status], [fldv_AssignedTo], [fldi_Progress]);
END

-- **************************************************
-- CREATE PRIMARY KEY AND CONSTRAINTS
-- **************************************************

PRINT 'Creating primary key and constraints...';

-- Add Primary Key if not exists
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE TABLE_NAME = 'tbld_Mitigations' AND CONSTRAINT_TYPE = 'PRIMARY KEY')
BEGIN
    ALTER TABLE [dbo].[tbld_Mitigations]
    ADD CONSTRAINT [PK_tbld_Mitigations] PRIMARY KEY CLUSTERED ([fldi_ID]);
END

-- Ensure unique mitigation codes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_tbld_Mitigations_Code')
BEGIN
    ALTER TABLE [dbo].[tbld_Mitigations]
    ADD CONSTRAINT [UQ_tbld_Mitigations_Code] UNIQUE ([fldv_Code]);
END

-- Add check constraints for progress percentage
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.CHECK_CONSTRAINTS WHERE CONSTRAINT_NAME = 'CK_tbld_Mitigations_Progress')
BEGIN
    ALTER TABLE [dbo].[tbld_Mitigations]
    ADD CONSTRAINT [CK_tbld_Mitigations_Progress] CHECK ([fldi_Progress] >= 0 AND [fldi_Progress] <= 100);
END

-- **************************************************
-- SAMPLE VERIFICATION QUERIES
-- **************************************************

PRINT 'Running verification queries...';

-- Check enhanced Mitigations table structure
PRINT 'Enhanced Mitigations Table Structure:';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'tbld_Mitigations'
ORDER BY ORDINAL_POSITION;

-- Test enhanced mitigations query for Step 5
SELECT 
    M.fldv_Code,
    M.fldv_HazardCode,
    M.fldv_Name,
    M.fldc_Description,
    M.fldv_Type,
    M.fldv_Status,
    M.fldi_Progress,
    M.fldd_TargetDate,
    M.fldv_AssignedTo,
    M.fldv_AssignedDepartment
FROM [dbo].[tbld_Mitigations] M
WHERE M.fldv_Status IN ('Proposed', 'In Progress', 'Implemented')
ORDER BY M.fldv_HazardCode, M.fldd_TargetDate;

-- Test mitigation effectiveness query
SELECT 
    M.fldv_Code,
    M.fldv_HazardCode,
    M.fldv_EffectivenessRating,
    M.fldi_ExpectedSeverityReduction,
    M.fldi_ActualSeverityReduction,
    M.fldi_ExpectedLikelihoodReduction,
    M.fldi_ActualLikelihoodReduction,
    M.fldv_ResidualRiskLevel
FROM [dbo].[tbld_Mitigations] M
WHERE M.fldv_Status = 'Implemented'
AND M.fldd_EffectivenessReviewDate IS NOT NULL
ORDER BY M.fldv_EffectivenessRating DESC;

-- **************************************************
-- APPLICATION SERVICE SUPPORT
-- **************************************************

PRINT '***************************************************';
PRINT 'tbld_Mitigations ENHANCEMENT COMPLETED SUCCESSFULLY!';
PRINT '***************************************************';
PRINT 'NEW MITIGATION MANAGEMENT CAPABILITIES:';
PRINT '? Step 5 Implementation Planning: Target dates, assignments, progress tracking';
PRINT '? Resource Management: Cost estimation, hours tracking, resource requirements';
PRINT '? Effectiveness Monitoring: Risk reduction tracking, effectiveness ratings';
PRINT '? Dependencies Management: Prerequisites and dependency tracking';
PRINT '? Progress Tracking: 0-100% completion with notes and updates';
PRINT '? Validation & Testing: Testing procedures and validation workflows';
PRINT '? Communication Planning: Implementation and communication plans';
PRINT '? Performance Optimization: Strategic indexes for key queries';
PRINT '';
PRINT 'ARCHITECTURE INTEGRATION:';
PRINT '- Hazard.Mitigations populated by Application Service GetHazardMitigations()';
PRINT '- Proper separation: Mitigation details in tbld_Mitigations, not tbld_Hazards';
PRINT '- Repository pattern ready for enhanced mitigation queries';
PRINT '- Mapper support for comprehensive mitigation data';
PRINT '';
PRINT 'NEXT STEPS:';
PRINT '1. Create/Update MitigationRepository with enhanced queries';
PRINT '2. Create Application Service GetHazardMitigations(hazardCode)';
PRINT '3. Update Hazard mapper to populate Mitigations list';
PRINT '4. Update Step 5 UI to leverage enhanced mitigation data';
PRINT '***************************************************';

GO