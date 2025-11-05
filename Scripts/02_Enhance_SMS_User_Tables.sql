-- =============================================
-- SMS USER TABLE ENHANCEMENTS
-- Database: PDXSMS_V2
-- Description: Add enhanced security and audit fields to existing user tables
-- Author: System Generated
-- Date: Generated for SMS User Management System
-- =============================================

USE [PDXSMS_V2]
GO

-- =============================================
-- Enhance tbld_SMSApplicationUsers
-- =============================================
PRINT 'Enhancing tbld_SMSApplicationUsers with security fields...'

-- Add password management fields
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('[dbo].[tbld_SMSApplicationUsers]') AND name = 'fldb_RequiresPasswordChange')
BEGIN
    ALTER TABLE [dbo].[tbld_SMSApplicationUsers] ADD [fldb_RequiresPasswordChange] [bit] NOT NULL DEFAULT(0)
    PRINT '- Added fldb_RequiresPasswordChange field'
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('[dbo].[tbld_SMSApplicationUsers]') AND name = 'fldd_PasswordLastChanged')
BEGIN
    ALTER TABLE [dbo].[tbld_SMSApplicationUsers] ADD [fldd_PasswordLastChanged] [datetime2](7) NULL
    PRINT '- Added fldd_PasswordLastChanged field'
END

-- Add account security fields
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('[dbo].[tbld_SMSApplicationUsers]') AND name = 'fldi_FailedLoginAttempts')
BEGIN
    ALTER TABLE [dbo].[tbld_SMSApplicationUsers] ADD [fldi_FailedLoginAttempts] [int] NOT NULL DEFAULT(0)
    PRINT '- Added fldi_FailedLoginAttempts field'
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('[dbo].[tbld_SMSApplicationUsers]') AND name = 'fldd_AccountLockedUntil')
BEGIN
    ALTER TABLE [dbo].[tbld_SMSApplicationUsers] ADD [fldd_AccountLockedUntil] [datetime2](7) NULL
    PRINT '- Added fldd_AccountLockedUntil field'
END

-- Add audit trail enhancement
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('[dbo].[tbld_SMSApplicationUsers]') AND name = 'fldi_LoginCount')
BEGIN
    ALTER TABLE [dbo].[tbld_SMSApplicationUsers] ADD [fldi_LoginCount] [int] NOT NULL DEFAULT(0)
    PRINT '- Added fldi_LoginCount field'
END

-- =============================================
-- Enhance tbld_SMSOrganizationalUsers
-- =============================================
PRINT 'Enhancing tbld_SMSOrganizationalUsers with security fields...'

-- Add password management fields
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('[dbo].[tbld_SMSOrganizationalUsers]') AND name = 'fldb_RequiresPasswordChange')
BEGIN
    ALTER TABLE [dbo].[tbld_SMSOrganizationalUsers] ADD [fldb_RequiresPasswordChange] [bit] NOT NULL DEFAULT(0)
    PRINT '- Added fldb_RequiresPasswordChange field'
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('[dbo].[tbld_SMSOrganizationalUsers]') AND name = 'fldd_PasswordLastChanged')
BEGIN
    ALTER TABLE [dbo].[tbld_SMSOrganizationalUsers] ADD [fldd_PasswordLastChanged] [datetime2](7) NULL
    PRINT '- Added fldd_PasswordLastChanged field'  
END

-- Add account security fields
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('[dbo].[tbld_SMSOrganizationalUsers]') AND name = 'fldi_FailedLoginAttempts')
BEGIN
    ALTER TABLE [dbo].[tbld_SMSOrganizationalUsers] ADD [fldi_FailedLoginAttempts] [int] NOT NULL DEFAULT(0)
    PRINT '- Added fldi_FailedLoginAttempts field'
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('[dbo].[tbld_SMSOrganizationalUsers]') AND name = 'fldd_AccountLockedUntil')
BEGIN
    ALTER TABLE [dbo].[tbld_SMSOrganizationalUsers] ADD [fldd_AccountLockedUntil] [datetime2](7) NULL
    PRINT '- Added fldd_AccountLockedUntil field'
END

-- Add audit trail enhancement
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('[dbo].[tbld_SMSOrganizationalUsers]') AND name = 'fldi_LoginCount')
BEGIN
    ALTER TABLE [dbo].[tbld_SMSOrganizationalUsers] ADD [fldi_LoginCount] [int] NOT NULL DEFAULT(0)
    PRINT '- Added fldi_LoginCount field'
END

-- =============================================
-- Enhance tbld_SMSStakeholderUsers
-- =============================================
PRINT 'Enhancing tbld_SMSStakeholderUsers with security fields...'

-- Add password management fields
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('[dbo].[tbld_SMSStakeholderUsers]') AND name = 'fldb_RequiresPasswordChange')
BEGIN
    ALTER TABLE [dbo].[tbld_SMSStakeholderUsers] ADD [fldb_RequiresPasswordChange] [bit] NOT NULL DEFAULT(0)
    PRINT '- Added fldb_RequiresPasswordChange field'
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('[dbo].[tbld_SMSStakeholderUsers]') AND name = 'fldd_PasswordLastChanged')
BEGIN
    ALTER TABLE [dbo].[tbld_SMSStakeholderUsers] ADD [fldd_PasswordLastChanged] [datetime2](7) NULL
    PRINT '- Added fldd_PasswordLastChanged field'
END

-- Add account security fields
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('[dbo].[tbld_SMSStakeholderUsers]') AND name = 'fldi_FailedLoginAttempts')
BEGIN
    ALTER TABLE [dbo].[tbld_SMSStakeholderUsers] ADD [fldi_FailedLoginAttempts] [int] NOT NULL DEFAULT(0)
    PRINT '- Added fldi_FailedLoginAttempts field'
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('[dbo].[tbld_SMSStakeholderUsers]') AND name = 'fldd_AccountLockedUntil')
BEGIN
    ALTER TABLE [dbo].[tbld_SMSStakeholderUsers] ADD [fldd_AccountLockedUntil] [datetime2](7) NULL
    PRINT '- Added fldd_AccountLockedUntil field'
END

-- Add audit trail enhancement
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('[dbo].[tbld_SMSStakeholderUsers]') AND name = 'fldi_LoginCount')
BEGIN
    ALTER TABLE [dbo].[tbld_SMSStakeholderUsers] ADD [fldi_LoginCount] [int] NOT NULL DEFAULT(0)
    PRINT '- Added fldi_LoginCount field'
END

-- =============================================
-- Create Performance Indexes for Enhanced Fields
-- =============================================
PRINT 'Creating performance indexes for enhanced security fields...'

-- Indexes for Account Security
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_tbld_SMSApplicationUsers_AccountLockedUntil')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_tbld_SMSApplicationUsers_AccountLockedUntil] 
    ON [dbo].[tbld_SMSApplicationUsers] ([fldd_AccountLockedUntil])
    WHERE [fldd_AccountLockedUntil] IS NOT NULL
    PRINT '- Created IX_tbld_SMSApplicationUsers_AccountLockedUntil index'
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_tbld_SMSOrganizationalUsers_AccountLockedUntil')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_tbld_SMSOrganizationalUsers_AccountLockedUntil] 
    ON [dbo].[tbld_SMSOrganizationalUsers] ([fldd_AccountLockedUntil])
    WHERE [fldd_AccountLockedUntil] IS NOT NULL
    PRINT '- Created IX_tbld_SMSOrganizationalUsers_AccountLockedUntil index'
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_tbld_SMSStakeholderUsers_AccountLockedUntil')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_tbld_SMSStakeholderUsers_AccountLockedUntil] 
    ON [dbo].[tbld_SMSStakeholderUsers] ([fldd_AccountLockedUntil])
    WHERE [fldd_AccountLockedUntil] IS NOT NULL
    PRINT '- Created IX_tbld_SMSStakeholderUsers_AccountLockedUntil index'
END

-- Indexes for Password Management
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_tbld_SMSApplicationUsers_RequiresPasswordChange')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_tbld_SMSApplicationUsers_RequiresPasswordChange] 
    ON [dbo].[tbld_SMSApplicationUsers] ([fldb_RequiresPasswordChange])
    WHERE [fldb_RequiresPasswordChange] = 1
    PRINT '- Created IX_tbld_SMSApplicationUsers_RequiresPasswordChange index'
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_tbld_SMSOrganizationalUsers_RequiresPasswordChange')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_tbld_SMSOrganizationalUsers_RequiresPasswordChange] 
    ON [dbo].[tbld_SMSOrganizationalUsers] ([fldb_RequiresPasswordChange])
    WHERE [fldb_RequiresPasswordChange] = 1
    PRINT '- Created IX_tbld_SMSOrganizationalUsers_RequiresPasswordChange index'
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_tbld_SMSStakeholderUsers_RequiresPasswordChange')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_tbld_SMSStakeholderUsers_RequiresPasswordChange] 
    ON [dbo].[tbld_SMSStakeholderUsers] ([fldb_RequiresPasswordChange])
    WHERE [fldb_RequiresPasswordChange] = 1
    PRINT '- Created IX_tbld_SMSStakeholderUsers_RequiresPasswordChange index'
END

PRINT '============================================='
PRINT 'SMS User Table Enhancements Complete'
PRINT '============================================='
PRINT 'Enhanced all 3 user tables with:'
PRINT '- Password management tracking'
PRINT '- Account security controls'
PRINT '- Enhanced audit capabilities'
PRINT '- Performance optimized indexes'
PRINT '============================================='