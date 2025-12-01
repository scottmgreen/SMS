-- =============================================
-- SMS STAKEHOLDER GROUPS TABLE
-- Table: tbld_SMSStakeholderGroups
-- Purpose: Stores stakeholder groups for organizing external partners
-- =============================================

USE [PDXSMS_V2]
GO

-- Create the stakeholder groups table
CREATE TABLE [dbo].[tbld_SMSStakeholderGroups](
    [fldi_ID] [int] IDENTITY(1,1) NOT NULL,
    [fldv_Code] [varchar](60) NOT NULL,
    [fldv_GroupName] [varchar](100) NOT NULL,
    [fldv_Description] [varchar](500) NULL,
    [fldb_IsActive] [bit] NOT NULL DEFAULT 1,
    [fldv_CreatedBy] [varchar](50) NOT NULL DEFAULT 'SYSTEM',
    [fldd_CreatedDate] [datetime] NOT NULL DEFAULT GETDATE(),
    [fldv_UpdatedBy] [varchar](50) NULL,
    [fldd_UpdatedDate] [datetime] NULL,
    CONSTRAINT [PK_tbld_SMSStakeholderGroups] PRIMARY KEY CLUSTERED ([fldi_ID] ASC),
    CONSTRAINT [UK_tbld_SMSStakeholderGroups_Code] UNIQUE NONCLUSTERED ([fldv_Code] ASC),
    CONSTRAINT [UK_tbld_SMSStakeholderGroups_GroupName] UNIQUE NONCLUSTERED ([fldv_GroupName] ASC)
)
GO

-- Create indexes for performance
CREATE NONCLUSTERED INDEX [IX_tbld_SMSStakeholderGroups_IsActive] 
ON [dbo].[tbld_SMSStakeholderGroups] ([fldb_IsActive])
GO

CREATE NONCLUSTERED INDEX [IX_tbld_SMSStakeholderGroups_GroupName] 
ON [dbo].[tbld_SMSStakeholderGroups] ([fldv_GroupName])
GO

-- =============================================
-- SMS STAKEHOLDER USER GROUPS JUNCTION TABLE
-- Table: tbld_SMSStakeholderUserGroups
-- Purpose: Many-to-many relationship between stakeholder users and groups
-- =============================================

CREATE TABLE [dbo].[tbld_SMSStakeholderUserGroups](
    [fldi_ID] [int] IDENTITY(1,1) NOT NULL,
    [fldv_UserCode] [varchar](60) NOT NULL,
    [fldv_GroupCode] [varchar](60) NOT NULL,
    [fldd_AssignedDate] [datetime] NOT NULL DEFAULT GETDATE(),
    [fldv_AssignedBy] [varchar](50) NOT NULL DEFAULT 'SYSTEM',
    [fldv_CreatedBy] [varchar](50) NOT NULL DEFAULT 'SYSTEM',
    [fldd_CreatedDate] [datetime] NOT NULL DEFAULT GETDATE(),
    [fldv_UpdatedBy] [varchar](50) NULL,
    [fldd_UpdatedDate] [datetime] NULL,
    CONSTRAINT [PK_tbld_SMSStakeholderUserGroups] PRIMARY KEY CLUSTERED ([fldi_ID] ASC),
    CONSTRAINT [UK_tbld_SMSStakeholderUserGroups_UserGroup] UNIQUE NONCLUSTERED ([fldv_UserCode] ASC, [fldv_GroupCode] ASC),
    CONSTRAINT [FK_tbld_SMSStakeholderUserGroups_User] FOREIGN KEY ([fldv_UserCode]) 
        REFERENCES [dbo].[tbld_SMSStakeholderUsers] ([fldv_Code]) ON DELETE CASCADE,
    CONSTRAINT [FK_tbld_SMSStakeholderUserGroups_Group] FOREIGN KEY ([fldv_GroupCode]) 
        REFERENCES [dbo].[tbld_SMSStakeholderGroups] ([fldv_Code]) ON DELETE CASCADE
)
GO

-- Create indexes for performance
CREATE NONCLUSTERED INDEX [IX_tbld_SMSStakeholderUserGroups_UserCode] 
ON [dbo].[tbld_SMSStakeholderUserGroups] ([fldv_UserCode])
GO

CREATE NONCLUSTERED INDEX [IX_tbld_SMSStakeholderUserGroups_GroupCode] 
ON [dbo].[tbld_SMSStakeholderUserGroups] ([fldv_GroupCode])
GO

-- Insert some default stakeholder groups
INSERT INTO [dbo].[tbld_SMSStakeholderGroups] 
([fldv_Code], [fldv_GroupName], [fldv_Description], [fldb_IsActive], [fldv_CreatedBy])
VALUES 
('SG-001', 'Airlines', 'Commercial airline operators using PDX', 1, 'SYSTEM'),
('SG-002', 'Ground Handlers', 'Ground handling service providers', 1, 'SYSTEM'),
('SG-003', 'Contractors', 'Construction and maintenance contractors', 1, 'SYSTEM'),
('SG-004', 'Tenants', 'Terminal and facility tenants', 1, 'SYSTEM'),
('SG-005', 'Regulatory', 'FAA and other regulatory stakeholders', 1, 'SYSTEM'),
('SG-006', 'Emergency Services', 'Fire, police, and medical emergency services', 1, 'SYSTEM'),
('SG-007', 'Fuel Providers', 'Aircraft fuel and services providers', 1, 'SYSTEM'),
('SG-008', 'Cargo Operators', 'Cargo and freight handling operators', 1, 'SYSTEM')
GO

PRINT 'SMS Stakeholder Groups tables created successfully!'
PRINT 'Default stakeholder groups inserted!'