-- =============================================
-- Enhanced Interview Table ALTER Script
-- SMS Investigation Interview Management System  
-- Updated to support comprehensive interview workflow
-- Modifies existing tbld_Interviews table
-- =============================================

USE [PDXSMS_V2]
GO

PRINT 'Starting enhancement of existing tbld_Interviews table...'
GO

-- First, let's standardize the primary key name and add missing constraints
IF NOT EXISTS (SELECT * FROM sys.key_constraints WHERE name = 'PK_tbld_Interviews')
BEGIN
    ALTER TABLE [dbo].[tbld_Interviews]
    ADD CONSTRAINT [PK_tbld_Interviews] PRIMARY KEY CLUSTERED ([fldi_ID] ASC)
END
GO

-- Rename and resize existing columns to match enhanced schema
-- Change fldv_Code to fldv_InterviewCode and make it proper size
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbld_Interviews') AND name = 'fldv_Code')
BEGIN
    EXEC sp_rename 'tbld_Interviews.fldv_Code', 'fldv_InterviewCode', 'COLUMN'
    ALTER TABLE [dbo].[tbld_Interviews] 
    ALTER COLUMN [fldv_InterviewCode] [nvarchar](50) NOT NULL
END
GO

-- Resize fldv_InvestigationCode to proper size and make it NOT NULL
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbld_Interviews') AND name = 'fldv_InvestigationCode')
BEGIN
    ALTER TABLE [dbo].[tbld_Interviews] 
    ALTER COLUMN [fldv_InvestigationCode] [nvarchar](50) NOT NULL
END
GO

-- Resize fldv_SMSInvestigatorCode to proper size and make it NOT NULL
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbld_Interviews') AND name = 'fldv_SMSInvestigatorCode')
BEGIN
    ALTER TABLE [dbo].[tbld_Interviews] 
    ALTER COLUMN [fldv_SMSInvestigatorCode] [nvarchar](50) NOT NULL
END
GO

-- Resize fldv_PersonInterviewed to proper size and make it NOT NULL
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbld_Interviews') AND name = 'fldv_PersonInterviewed')
BEGIN
    ALTER TABLE [dbo].[tbld_Interviews] 
    ALTER COLUMN [fldv_PersonInterviewed] [nvarchar](200) NOT NULL
END
GO

-- Resize existing notes columns to support large text
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbld_Interviews') AND name = 'fldv_PersonInterviewedNotes')
BEGIN
    ALTER TABLE [dbo].[tbld_Interviews] 
    ALTER COLUMN [fldv_PersonInterviewedNotes] [nvarchar](max) NULL
END
GO

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbld_Interviews') AND name = 'fldv_InvestigatorNotes')
BEGIN
    ALTER TABLE [dbo].[tbld_Interviews] 
    ALTER COLUMN [fldv_InvestigatorNotes] [nvarchar](max) NULL
END
GO

-- Add new Interview Subject Detail columns
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbld_Interviews') AND name = 'fldv_PersonInterviewedRole')
BEGIN
    ALTER TABLE [dbo].[tbld_Interviews] 
    ADD [fldv_PersonInterviewedRole] [nvarchar](100) NULL
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbld_Interviews') AND name = 'fldv_PersonInterviewedDepartment')
BEGIN
    ALTER TABLE [dbo].[tbld_Interviews] 
    ADD [fldv_PersonInterviewedDepartment] [nvarchar](100) NULL
END
GO

-- Add Interview Management Properties
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbld_Interviews') AND name = 'fldv_Status')
BEGIN
    ALTER TABLE [dbo].[tbld_Interviews] 
    ADD [fldv_Status] [nvarchar](50) NOT NULL DEFAULT 'Planned'
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbld_Interviews') AND name = 'fldd_InterviewDate')
BEGIN
    ALTER TABLE [dbo].[tbld_Interviews] 
    ADD [fldd_InterviewDate] [datetime2](7) NULL
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbld_Interviews') AND name = 'fldi_DurationMinutes')
BEGIN
    ALTER TABLE [dbo].[tbld_Interviews] 
    ADD [fldi_DurationMinutes] [int] NULL
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbld_Interviews') AND name = 'fldv_InterviewLocation')
BEGIN
    ALTER TABLE [dbo].[tbld_Interviews] 
    ADD [fldv_InterviewLocation] [nvarchar](200) NULL
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbld_Interviews') AND name = 'fldv_Type')
BEGIN
    ALTER TABLE [dbo].[tbld_Interviews] 
    ADD [fldv_Type] [nvarchar](50) NOT NULL DEFAULT 'Witness'
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbld_Interviews') AND name = 'fldb_IsConfidential')
BEGIN
    ALTER TABLE [dbo].[tbld_Interviews] 
    ADD [fldb_IsConfidential] [bit] NOT NULL DEFAULT 0
END
GO

-- Add Interview Preparation Properties
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbld_Interviews') AND name = 'fldv_PreparationNotes')
BEGIN
    ALTER TABLE [dbo].[tbld_Interviews] 
    ADD [fldv_PreparationNotes] [nvarchar](max) NULL
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbld_Interviews') AND name = 'fldv_QuestionsToAsk')
BEGIN
    ALTER TABLE [dbo].[tbld_Interviews] 
    ADD [fldv_QuestionsToAsk] [nvarchar](max) NULL
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbld_Interviews') AND name = 'fldv_BackgroundInformation')
BEGIN
    ALTER TABLE [dbo].[tbld_Interviews] 
    ADD [fldv_BackgroundInformation] [nvarchar](max) NULL
END
GO

-- Add Interview Results Properties
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbld_Interviews') AND name = 'fldv_KeyFindings')
BEGIN
    ALTER TABLE [dbo].[tbld_Interviews] 
    ADD [fldv_KeyFindings] [nvarchar](max) NULL
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbld_Interviews') AND name = 'fldv_FollowUpRequired')
BEGIN
    ALTER TABLE [dbo].[tbld_Interviews] 
    ADD [fldv_FollowUpRequired] [nvarchar](max) NULL
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbld_Interviews') AND name = 'fldv_AdditionalWitnesses')
BEGIN
    ALTER TABLE [dbo].[tbld_Interviews] 
    ADD [fldv_AdditionalWitnesses] [nvarchar](max) NULL
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbld_Interviews') AND name = 'fldd_CompletedDate')
BEGIN
    ALTER TABLE [dbo].[tbld_Interviews] 
    ADD [fldd_CompletedDate] [datetime2](7) NULL
END
GO

-- Add Standard Audit Fields
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbld_Interviews') AND name = 'fldv_CreatedBy')
BEGIN
    ALTER TABLE [dbo].[tbld_Interviews] 
    ADD [fldv_CreatedBy] [nvarchar](50) NOT NULL DEFAULT 'SYSTEM'
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbld_Interviews') AND name = 'fldd_CreatedDate')
BEGIN
    ALTER TABLE [dbo].[tbld_Interviews] 
    ADD [fldd_CreatedDate] [datetime2](7) NOT NULL DEFAULT GETUTCDATE()
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbld_Interviews') AND name = 'fldv_UpdatedBy')
BEGIN
    ALTER TABLE [dbo].[tbld_Interviews] 
    ADD [fldv_UpdatedBy] [nvarchar](50) NULL
END
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('tbld_Interviews') AND name = 'fldd_UpdatedDate')
BEGIN
    ALTER TABLE [dbo].[tbld_Interviews] 
    ADD [fldd_UpdatedDate] [datetime2](7) NULL
END
GO

-- Create unique index on Interview Code
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('tbld_Interviews') AND name = 'IX_tbld_Interviews_InterviewCode')
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [IX_tbld_Interviews_InterviewCode] 
    ON [dbo].[tbld_Interviews] ([fldv_InterviewCode] ASC)
    WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, 
          IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, 
          ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
END
GO

-- Create index on Investigation Code for efficient lookups
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('tbld_Interviews') AND name = 'IX_tbld_Interviews_InvestigationCode')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_tbld_Interviews_InvestigationCode] 
    ON [dbo].[tbld_Interviews] ([fldv_InvestigationCode] ASC)
    WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, 
          DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, 
          ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
END
GO

-- Create index on SMS Investigator for efficient lookups
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('tbld_Interviews') AND name = 'IX_tbld_Interviews_SMSInvestigatorCode')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_tbld_Interviews_SMSInvestigatorCode] 
    ON [dbo].[tbld_Interviews] ([fldv_SMSInvestigatorCode] ASC)
    WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, 
          DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, 
          ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
END
GO

-- Create index on Status for efficient filtering
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('tbld_Interviews') AND name = 'IX_tbld_Interviews_Status')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_tbld_Interviews_Status] 
    ON [dbo].[tbld_Interviews] ([fldv_Status] ASC)
    WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, 
          DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, 
          ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
END
GO

-- Create index on Interview Date for scheduling queries
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('tbld_Interviews') AND name = 'IX_tbld_Interviews_InterviewDate')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_tbld_Interviews_InterviewDate] 
    ON [dbo].[tbld_Interviews] ([fldd_InterviewDate] ASC)
    WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, 
          DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, 
          ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
END
GO

-- Add check constraints for data integrity
IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_tbld_Interviews_Status')
BEGIN
    ALTER TABLE [dbo].[tbld_Interviews]
    ADD CONSTRAINT [CK_tbld_Interviews_Status] 
    CHECK ([fldv_Status] IN ('Planned', 'Scheduled', 'InProgress', 'Completed', 'Cancelled'))
END
GO

IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_tbld_Interviews_Type')
BEGIN
    ALTER TABLE [dbo].[tbld_Interviews]
    ADD CONSTRAINT [CK_tbld_Interviews_Type] 
    CHECK ([fldv_Type] IN ('Witness', 'Expert', 'Stakeholder', 'FollowUp'))
END
GO

IF NOT EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_tbld_Interviews_DurationMinutes')
BEGIN
    ALTER TABLE [dbo].[tbld_Interviews]
    ADD CONSTRAINT [CK_tbld_Interviews_DurationMinutes] 
    CHECK ([fldi_DurationMinutes] IS NULL OR [fldi_DurationMinutes] > 0)
END
GO

-- Add foreign key constraint to Investigation table (if exists)
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tbld_Investigations]') AND type in (N'U'))
AND NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_tbld_Interviews_tbld_Investigations')
BEGIN
    ALTER TABLE [dbo].[tbld_Interviews]
    ADD CONSTRAINT [FK_tbld_Interviews_tbld_Investigations] 
    FOREIGN KEY ([fldv_InvestigationCode]) 
    REFERENCES [dbo].[tbld_Investigations]([fldv_InvestigationCode])
    ON DELETE CASCADE
END
GO

-- Update any existing records to have default values for required fields
UPDATE [dbo].[tbld_Interviews] 
SET [fldv_InvestigationCode] = 'INV-' + CAST([fldi_ID] AS NVARCHAR(10))
WHERE [fldv_InvestigationCode] IS NULL OR LTRIM(RTRIM([fldv_InvestigationCode])) = ''
GO

UPDATE [dbo].[tbld_Interviews] 
SET [fldv_SMSInvestigatorCode] = 'USR-' + CAST([fldi_ID] AS NVARCHAR(10))
WHERE [fldv_SMSInvestigatorCode] IS NULL OR LTRIM(RTRIM([fldv_SMSInvestigatorCode])) = ''
GO

UPDATE [dbo].[tbld_Interviews] 
SET [fldv_PersonInterviewed] = 'Unknown Person ' + CAST([fldi_ID] AS NVARCHAR(10))
WHERE [fldv_PersonInterviewed] IS NULL OR LTRIM(RTRIM([fldv_PersonInterviewed])) = ''
GO

-- Add table and column descriptions
EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Enhanced Interview table for comprehensive investigation interview management with scheduling and workflow support', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'tbld_Interviews'
GO

EXEC sys.sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Unique interview code in format INT-[InvestigationID]-HHMMSS', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'tbld_Interviews',
    @level2type = N'COLUMN', @level2name = N'fldv_InterviewCode'
GO

PRINT 'Enhanced tbld_Interviews table structure completed successfully!'
PRINT 'Added interview workflow management, scheduling, preparation tracking, and comprehensive audit capabilities.'
GO