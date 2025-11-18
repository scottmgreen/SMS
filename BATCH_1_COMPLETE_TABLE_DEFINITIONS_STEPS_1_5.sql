-- **************************************************
-- BATCH 1: COMPLETE TABLE DEFINITIONS FOR STEPS 1-5
-- Enhanced table definitions for comprehensive risk assessment workflow
-- **************************************************
USE [PDXSMS_V2]
GO

PRINT '***************************************************';
PRINT 'BATCH 1: ENHANCING ALL TABLES FOR STEPS 1-5';
PRINT 'Updating: tbld_Hazards, tbld_Mitigations, tbld_MitigationAssignments';
PRINT '***************************************************';

-- **************************************************
-- ENHANCE tbld_Hazards TABLE - Complete Definition
-- **************************************************

PRINT 'Enhancing tbld_Hazards table...';

-- Resize existing fields
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

-- Add Enhanced Classification Fields
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

-- Add Reporting Fields
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

-- Add Investigation Fields
ALTER TABLE [dbo].[tbld_Hazards]
ADD [fldb_RequiresInvestigation] [bit] NULL DEFAULT 0;

ALTER TABLE [dbo].[tbld_Hazards]
ADD [fldd_InvestigationCompletedDate] [datetime] NULL;

ALTER TABLE [dbo].[tbld_Hazards]
ADD [fldc_InvestigationNotes] [ntext] NULL;

-- Add Location Fields
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
-- ENHANCE tbld_Mitigations TABLE - Complete Definition
-- **************************************************

PRINT 'Enhancing tbld_Mitigations table...';

-- Resize existing fields
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
ADD [fldv_Type] [nvarchar](50) NULL; -- Current, Proposed, Implemented

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldv_Status] [nvarchar](50) NULL DEFAULT 'Proposed';

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldv_Priority] [nvarchar](50) NULL;

-- Add Step 5 Implementation Planning Fields
ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldv_RiskAssessmentCode] [nvarchar](50) NULL;

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldd_TargetDate] [datetime] NULL;

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldd_ImplementationDate] [datetime] NULL;

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldd_CompletionDate] [datetime] NULL;

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldv_AssignedDepartment] [nvarchar](100) NULL;

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldv_AssignedTo] [nvarchar](50) NULL;

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

-- Add Effectiveness Fields
ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldv_EffectivenessRating] [nvarchar](20) NULL;

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldc_EffectivenessNotes] [ntext] NULL;

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldd_EffectivenessReviewDate] [datetime] NULL;

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldv_EffectivenessReviewedBy] [nvarchar](50) NULL;

-- Add Monitoring Fields
ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldc_MonitoringRequirements] [ntext] NULL;

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldv_MonitoringFrequency] [nvarchar](50) NULL;

-- Add Risk Reduction Fields
ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldi_ExpectedSeverityReduction] [int] NULL;

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldi_ExpectedLikelihoodReduction] [int] NULL;

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldi_ActualSeverityReduction] [int] NULL;

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldi_ActualLikelihoodReduction] [int] NULL;

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldv_ResidualRiskLevel] [nvarchar](50) NULL;

-- Add Dependencies Fields
ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldv_Prerequisites] [ntext] NULL;

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldv_Dependencies] [nvarchar](500) NULL;

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldb_HasDependencies] [bit] NULL DEFAULT 0;

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldb_IsPrerequisite] [bit] NULL DEFAULT 0;

-- Add Implementation Planning Fields
ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldc_ImplementationPlan] [ntext] NULL;

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldc_CommunicationPlan] [ntext] NULL;

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldc_TrainingRequirements] [ntext] NULL;

ALTER TABLE [dbo].[tbld_Mitigations]
ADD [fldc_DocumentationUpdates] [ntext] NULL;

-- Add Validation Fields
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

-- Add General Fields
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
-- ENHANCE tbld_MitigationAssignments TABLE - Complete Definition  
-- **************************************************

PRINT 'Enhancing tbld_MitigationAssignments table...';

-- Resize existing fields
ALTER TABLE [dbo].[tbld_MitigationAssignments]
ALTER COLUMN [fldv_Code] [nvarchar](50) NOT NULL;

ALTER TABLE [dbo].[tbld_MitigationAssignments]
ALTER COLUMN [fldv_MitigationCode] [nvarchar](50) NOT NULL;

ALTER TABLE [dbo].[tbld_MitigationAssignments]
ALTER COLUMN [fldv_DepartmentCode] [nvarchar](50) NULL;

-- Add Assignment Details Fields
ALTER TABLE [dbo].[tbld_MitigationAssignments]
ADD [fldv_AssignedToUserId] [nvarchar](50) NULL;

ALTER TABLE [dbo].[tbld_MitigationAssignments]
ADD [fldv_AssignedToUserName] [nvarchar](200) NULL;

ALTER TABLE [dbo].[tbld_MitigationAssignments]
ADD [fldv_AssignedRole] [nvarchar](100) NULL;

ALTER TABLE [dbo].[tbld_MitigationAssignments]
ADD [fldv_DepartmentName] [nvarchar](200) NULL;

-- Add Assignment Workflow Fields
ALTER TABLE [dbo].[tbld_MitigationAssignments]
ADD [fldv_AssignedBy] [nvarchar](50) NULL;

ALTER TABLE [dbo].[tbld_MitigationAssignments]
ADD [fldd_AssignedDate] [datetime] NULL;

ALTER TABLE [dbo].[tbld_MitigationAssignments]
ADD [fldd_DueDate] [datetime] NULL;

ALTER TABLE [dbo].[tbld_MitigationAssignments]
ADD [fldv_Status] [nvarchar](50) NULL DEFAULT 'Assigned';

ALTER TABLE [dbo].[tbld_MitigationAssignments]
ADD [fldv_Priority] [nvarchar](50) NULL;

-- Add Progress Tracking Fields
ALTER TABLE [dbo].[tbld_MitigationAssignments]
ADD [fldi_Progress] [int] NULL DEFAULT 0;

ALTER TABLE [dbo].[tbld_MitigationAssignments]
ADD [fldv_ProgressNotes] [ntext] NULL;

ALTER TABLE [dbo].[tbld_MitigationAssignments]
ADD [fldd_LastProgressUpdate] [datetime] NULL;

ALTER TABLE [dbo].[tbld_MitigationAssignments]
ADD [fldv_ProgressUpdatedBy] [nvarchar](50) NULL;

-- Add Communication Fields
ALTER TABLE [dbo].[tbld_MitigationAssignments]
ADD [fldc_AssignmentInstructions] [ntext] NULL;

ALTER TABLE [dbo].[tbld_MitigationAssignments]
ADD [fldc_CompletionNotes] [ntext] NULL;

ALTER TABLE [dbo].[tbld_MitigationAssignments]
ADD [fldd_CompletedDate] [datetime] NULL;

ALTER TABLE [dbo].[tbld_MitigationAssignments]
ADD [fldv_CompletedBy] [nvarchar](50) NULL;

-- Add Review and Approval Fields
ALTER TABLE [dbo].[tbld_MitigationAssignments]
ADD [fldv_ReviewStatus] [nvarchar](50) NULL;

ALTER TABLE [dbo].[tbld_MitigationAssignments]
ADD [fldv_ReviewedBy] [nvarchar](50) NULL;

ALTER TABLE [dbo].[tbld_MitigationAssignments]
ADD [fldd_ReviewedDate] [datetime] NULL;

ALTER TABLE [dbo].[tbld_MitigationAssignments]
ADD [fldc_ReviewComments] [ntext] NULL;

-- Add Resource and Cost Tracking
ALTER TABLE [dbo].[tbld_MitigationAssignments]
ADD [fldi_EstimatedHours] [int] NULL;

ALTER TABLE [dbo].[tbld_MitigationAssignments]
ADD [fldi_ActualHours] [int] NULL;

ALTER TABLE [dbo].[tbld_MitigationAssignments]
ADD [fldd_EstimatedCost] [decimal](10,2) NULL;

ALTER TABLE [dbo].[tbld_MitigationAssignments]
ADD [fldd_ActualCost] [decimal](10,2) NULL;

-- Add Escalation Fields
ALTER TABLE [dbo].[tbld_MitigationAssignments]
ADD [fldb_IsEscalated] [bit] NULL DEFAULT 0;

ALTER TABLE [dbo].[tbld_MitigationAssignments]
ADD [fldd_EscalatedDate] [datetime] NULL;

ALTER TABLE [dbo].[tbld_MitigationAssignments]
ADD [fldv_EscalatedTo] [nvarchar](50) NULL;

ALTER TABLE [dbo].[tbld_MitigationAssignments]
ADD [fldc_EscalationReason] [ntext] NULL;

-- Add Active/Inactive Management
ALTER TABLE [dbo].[tbld_MitigationAssignments]
ADD [fldb_IsActive] [bit] NULL DEFAULT 1;

ALTER TABLE [dbo].[tbld_MitigationAssignments]
ADD [fldd_InactiveDate] [datetime] NULL;

ALTER TABLE [dbo].[tbld_MitigationAssignments]
ADD [fldv_InactiveReason] [ntext] NULL;

-- Add Audit Fields (if not already present)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbld_MitigationAssignments' AND COLUMN_NAME = 'fldv_CreatedBy')
BEGIN
    ALTER TABLE [dbo].[tbld_MitigationAssignments]
    ADD [fldv_CreatedBy] [nvarchar](50) NOT NULL DEFAULT 'SYSTEM';
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbld_MitigationAssignments' AND COLUMN_NAME = 'fldd_CreatedDate')
BEGIN
    ALTER TABLE [dbo].[tbld_MitigationAssignments]
    ADD [fldd_CreatedDate] [datetime] NOT NULL DEFAULT GETUTCDATE();
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbld_MitigationAssignments' AND COLUMN_NAME = 'fldv_UpdatedBy')
BEGIN
    ALTER TABLE [dbo].[tbld_MitigationAssignments]
    ADD [fldv_UpdatedBy] [nvarchar](50) NULL;
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tbld_MitigationAssignments' AND COLUMN_NAME = 'fldd_UpdatedDate')
BEGIN
    ALTER TABLE [dbo].[tbld_MitigationAssignments]
    ADD [fldd_UpdatedDate] [datetime] NULL;
END

-- **************************************************
-- CREATE INDEXES AND CONSTRAINTS
-- **************************************************

PRINT 'Creating indexes and constraints...';

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

-- Mitigations Table Indexes
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
    INCLUDE ([fldv_Status], [fldi_Progress], [fldd_TargetDate]);
END

-- Mitigation Assignments Table Indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_tbld_MitigationAssignments_Code')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_tbld_MitigationAssignments_Code] 
    ON [dbo].[tbld_MitigationAssignments] ([fldv_Code])
    INCLUDE ([fldv_Status], [fldi_Progress]);
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_tbld_MitigationAssignments_MitigationCode')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_tbld_MitigationAssignments_MitigationCode] 
    ON [dbo].[tbld_MitigationAssignments] ([fldv_MitigationCode])
    INCLUDE ([fldv_Status], [fldi_Progress], [fldd_DueDate]);
END

-- Primary Keys (if not already exist)
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE TABLE_NAME = 'tbld_Hazards' AND CONSTRAINT_TYPE = 'PRIMARY KEY')
BEGIN
    ALTER TABLE [dbo].[tbld_Hazards]
    ADD CONSTRAINT [PK_tbld_Hazards] PRIMARY KEY CLUSTERED ([fldi_ID]);
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE TABLE_NAME = 'tbld_Mitigations' AND CONSTRAINT_TYPE = 'PRIMARY KEY')
BEGIN
    ALTER TABLE [dbo].[tbld_Mitigations]
    ADD CONSTRAINT [PK_tbld_Mitigations] PRIMARY KEY CLUSTERED ([fldi_ID]);
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE TABLE_NAME = 'tbld_MitigationAssignments' AND CONSTRAINT_TYPE = 'PRIMARY KEY')
BEGIN
    ALTER TABLE [dbo].[tbld_MitigationAssignments]
    ADD CONSTRAINT [PK_tbld_MitigationAssignments] PRIMARY KEY CLUSTERED ([fldi_ID]);
END

-- Unique Constraints
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_tbld_Hazards_Code')
BEGIN
    ALTER TABLE [dbo].[tbld_Hazards]
    ADD CONSTRAINT [UQ_tbld_Hazards_Code] UNIQUE ([fldv_Code]);
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_tbld_Mitigations_Code')
BEGIN
    ALTER TABLE [dbo].[tbld_Mitigations]
    ADD CONSTRAINT [UQ_tbld_Mitigations_Code] UNIQUE ([fldv_Code]);
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UQ_tbld_MitigationAssignments_Code')
BEGIN
    ALTER TABLE [dbo].[tbld_MitigationAssignments]
    ADD CONSTRAINT [UQ_tbld_MitigationAssignments_Code] UNIQUE ([fldv_Code]);
END

-- Check Constraints
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.CHECK_CONSTRAINTS WHERE CONSTRAINT_NAME = 'CK_tbld_Mitigations_Progress')
BEGIN
    ALTER TABLE [dbo].[tbld_Mitigations]
    ADD CONSTRAINT [CK_tbld_Mitigations_Progress] CHECK ([fldi_Progress] >= 0 AND [fldi_Progress] <= 100);
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.CHECK_CONSTRAINTS WHERE CONSTRAINT_NAME = 'CK_tbld_MitigationAssignments_Progress')
BEGIN
    ALTER TABLE [dbo].[tbld_MitigationAssignments]
    ADD CONSTRAINT [CK_tbld_MitigationAssignments_Progress] CHECK ([fldi_Progress] >= 0 AND [fldi_Progress] <= 100);
END

-- **************************************************
-- VERIFICATION QUERIES
-- **************************************************

PRINT 'Running verification queries...';

-- Check Hazards table structure
PRINT 'Enhanced Hazards Table Structure:';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'tbld_Hazards'
ORDER BY ORDINAL_POSITION;

-- Check Mitigations table structure
PRINT 'Enhanced Mitigations Table Structure:';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'tbld_Mitigations'
ORDER BY ORDINAL_POSITION;

-- Check Mitigation Assignments table structure
PRINT 'Enhanced Mitigation Assignments Table Structure:';
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'tbld_MitigationAssignments'
ORDER BY ORDINAL_POSITION;

PRINT '***************************************************';
PRINT 'BATCH 1 COMPLETED: ALL TABLES ENHANCED FOR STEPS 1-5';
PRINT '***************************************************';
PRINT 'ENHANCED TABLES:';
PRINT '? tbld_Hazards: 25+ fields for comprehensive hazard management';
PRINT '? tbld_Mitigations: 40+ fields for complete mitigation lifecycle';
PRINT '? tbld_MitigationAssignments: 25+ fields for assignment workflow';
PRINT '';
PRINT 'NEW CAPABILITIES:';
PRINT '? Step 3 Risk Analysis: Hazard-specific outcomes & root causes';
PRINT '? Step 5 Implementation: Complete mitigation planning & tracking';
PRINT '? Progress Tracking: 0-100% completion with detailed notes';
PRINT '? Resource Management: Cost estimation & hours tracking';
PRINT '? Assignment Workflow: Complete assignment lifecycle management';
PRINT '? Effectiveness Monitoring: Post-implementation review & validation';
PRINT '? Audit Trails: Full creation & modification tracking';
PRINT '? Performance Indexes: Optimized for key lookup patterns';
PRINT '***************************************************';

GO