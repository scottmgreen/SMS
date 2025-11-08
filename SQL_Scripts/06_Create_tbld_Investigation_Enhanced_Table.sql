-- =============================================
-- Enhanced Investigation Table ALTER Script
-- SMS Investigation Management System
-- Updated to support comprehensive investigation workflow
-- Modifies existing tbld_Investigations table
-- =============================================

USE [SMS_Database]
GO

PRINT 'Starting enhancement of existing tbld_Investigations table...'
GO

-- First, let's standardize the primary key name and add missing constraints
IF NOT EXISTS (SELECT * FROM sys.key_constraints WHERE name = 'PK_tbld_Investigations')
BEGIN
    ALTER TABLE [dbo].[tbld_Investigations]
    ADD CONSTRAINT [PK_tbld_Investigations] PRIMARY KEY CLUSTERED ([fldi_ID] ASC)
END
GO

-- Rename and resize existing columns to match enhanced schema
-- Change fldv_Code to fldv_InvestigationCode and make it proper size
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbld_Investigations') AND name = 'fldv_Code')
BEGIN
    EXEC sp_rename 'tbld_Investigations.fldv_Code', 'fldv_InvestigationCode', 'COLUMN'
    ALTER TABLE [dbo].[tbld_Investigations] 
    ALTER COLUMN [fldv_InvestigationCode] [nvarchar](50) NOT NULL
END
GO

-- Resize fldv_ReportCode to proper size and make nullable
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbld_Investigations') AND name = 'fldv_ReportCode')
BEGIN
    ALTER TABLE [dbo].[tbld_Investigations] 
    ALTER COLUMN [fldv_ReportCode] [nvarchar](50) NULL
END
GO

-- Resize fldv_InvestigationNotes to support large text
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbld_Investigations') AND name = 'fldv_InvestigationNotes')
BEGIN
    ALTER TABLE [dbo].[tbld_Investigations] 
    ALTER COLUMN [fldv_InvestigationNotes] [nvarchar](max) NULL
END
GO

-- Add missing core columns
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbld_Investigations') AND name = 'fldv_HazardCode')
BEGIN
    ALTER TABLE [dbo].[tbld_Investigations] 
    ADD [fldv_HazardCode] [nvarchar](50) NOT NULL DEFAULT 'HAZ-UNKNOWN'
END
GO

-- Add Investigation Management Properties
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbld_Investigations') AND name = 'fldv_AssignedInvestigatorId')
BEGIN
    ALTER TABLE [dbo].[tbld_Investigations] 
    ADD [fldv_AssignedInvestigatorId] [nvarchar](50) NOT NULL DEFAULT 'UNASSIGNED'
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbld_Investigations') AND name = 'fldv_Status')
BEGIN
    ALTER TABLE [dbo].[tbld_Investigations] 
    ADD [fldv_Status] [nvarchar](50) NOT NULL DEFAULT 'Assigned'
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbld_Investigations') AND name = 'fldd_CompletedDate')
BEGIN
    ALTER TABLE [dbo].[tbld_Investigations] 
    ADD [fldd_CompletedDate] [datetime2](7) NULL
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbld_Investigations') AND name = 'fldv_InvestigationPlan')
BEGIN
    ALTER TABLE [dbo].[tbld_Investigations] 
    ADD [fldv_InvestigationPlan] [nvarchar](max) NULL
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbld_Investigations') AND name = 'fldv_InvestigationObjectives')
BEGIN
    ALTER TABLE [dbo].[tbld_Investigations] 
    ADD [fldv_InvestigationObjectives] [nvarchar](max) NULL
END
GO

-- Add Investigation Decision Properties
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbld_Investigations') AND name = 'fldv_DecisionType')
BEGIN
    ALTER TABLE [dbo].[tbld_Investigations] 
    ADD [fldv_DecisionType] [nvarchar](100) NULL
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbld_Investigations') AND name = 'fldv_DecisionRationale')
BEGIN
    ALTER TABLE [dbo].[tbld_Investigations] 
    ADD [fldv_DecisionRationale] [nvarchar](max) NULL
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbld_Investigations') AND name = 'fldv_DecisionMaker')
BEGIN
    ALTER TABLE [dbo].[tbld_Investigations] 
    ADD [fldv_DecisionMaker] [nvarchar](50) NULL
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbld_Investigations') AND name = 'fldd_DecisionDate')
BEGIN
    ALTER TABLE [dbo].[tbld_Investigations] 
    ADD [fldd_DecisionDate] [datetime2](7) NULL
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbld_Investigations') AND name = 'fldv_NextSteps')
BEGIN
    ALTER TABLE [dbo].[tbld_Investigations] 
    ADD [fldv_NextSteps] [nvarchar](max) NULL
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbld_Investigations') AND name = 'fldv_ReferralDetails')
BEGIN
    ALTER TABLE [dbo].[tbld_Investigations] 
    ADD [fldv_ReferralDetails] [nvarchar](max) NULL
END
GO

-- Add Standard Audit Fields
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbld_Investigations') AND name = 'fldv_CreatedBy')
BEGIN
    ALTER TABLE [dbo].[tbld_Investigations] 
    ADD [fldv_CreatedBy] [nvarchar](50) NOT NULL DEFAULT 'SYSTEM'
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbld_Investigations') AND name = 'fldd_CreatedDate')
BEGIN
    ALTER TABLE [dbo].[tbld_Investigations] 
    ADD [fldd_CreatedDate] [datetime2](7) NOT NULL DEFAULT GETUTCDATE()
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbld_Investigations') AND name = 'fldv_UpdatedBy')
BEGIN
    ALTER TABLE [dbo].[tbld_Investigations] 
    ADD [fldv_UpdatedBy] [nvarchar](50) NULL
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbld_Investigations') AND name = 'fldd_UpdatedDate')
BEGIN
    ALTER TABLE [dbo].[tbld_Investigations] 
    ADD [fldd_UpdatedDate] [datetime2](7) NULL
END
GO

-- Create unique index on Investigation Code
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('tbld_Investigations') AND name = 'IX_tbld_Investigations_InvestigationCode')
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [IX_tbld_Investigations_InvestigationCode] 
    ON [dbo].[tbld_Investigations] ([fldv_InvestigationCode] ASC)
    WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, 
          IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, 
          ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
END
GO

-- Create index on Hazard Code for efficient lookups
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('tbld_Investigations') AND name = 'IX_tbld_Investigations_HazardCode')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_tbld_Investigations_HazardCode] 
    ON [dbo].[tbld_Investigations] ([fldv_HazardCode] ASC)
    WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, 
          DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, 
          ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
END
GO

-- Create index on Assigned Investigator for efficient lookups
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('tbld_Investigations') AND name = 'IX_tbld_Investigations_AssignedInvestigator')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_tbld_Investigations_AssignedInvestigator] 
    ON [dbo].[tbld_Investigations] ([fldv_AssignedInvestigatorId] ASC)
    WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, 
          DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, 
          ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
END
GO

-- Create index on Status for efficient filtering
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('tbld_Investigations') AND name = 'IX_tbld_Investigations_Status')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_tbld_Investigations_Status] 
    ON [dbo].[tbld_Investigations] ([fldv_Status] ASC)
    WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, 
          DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, 
          ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
END
GO

-- Add check constraints for data integrity
IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_tbld_Investigations_Status')
BEGIN
    ALTER TABLE [dbo].[tbld_Investigations]
    ADD CONSTRAINT [CK_tbld_Investigations_Status] 
    CHECK ([fldv_Status] IN ('Assigned', 'InProgress', 'OnHold', 'Completed', 'Cancelled'))
END
GO

IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_tbld_Investigations_DecisionType')
BEGIN
    ALTER TABLE [dbo].[tbld_Investigations]
    ADD CONSTRAINT [CK_tbld_Investigations_DecisionType] 
    CHECK ([fldv_DecisionType] IS NULL OR 
           [fldv_DecisionType] IN ('NoFurtherAction', 'ContinueMonitoring', 'RequiresMitigation', 
                                   'EscalateToRiskAssessment', 'ReferToExternalAgency'))
END
GO

-- Update any existing default values that might be temporary
UPDATE [dbo].[tbld_Investigations] 
SET [fldv_HazardCode] = 'HAZ-' + CAST([fldi_ID] AS NVARCHAR(10))
WHERE [fldv_HazardCode] = 'HAZ-UNKNOWN'
GO

UPDATE [dbo].[tbld_Investigations] 
SET [fldv_AssignedInvestigatorId] = 'INV-' + CAST([fldi_ID] AS NVARCHAR(10))
WHERE [fldv_AssignedInvestigatorId] = 'UNASSIGNED'
GO

-- Add table and column descriptions
EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Enhanced Investigation table for comprehensive hazard investigation management with workflow support', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'tbld_Investigations'
GO

EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Unique investigation code in format INV-YYYYMMDD-XXXXXXXX', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'tbld_Investigations',
    @level2type = N'COLUMN', @level2name = N'fldv_InvestigationCode'
GO

PRINT 'Enhanced tbld_Investigations table structure completed successfully!'
PRINT 'Added investigation workflow management, decision tracking, and audit capabilities.'
GO