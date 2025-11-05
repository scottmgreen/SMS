-- =============================================
-- SMS USER MANAGEMENT SYSTEM - ENTITY REGISTRY UPDATES
-- Database: PDXSMS_V2
-- Description: Update entity registry for SMS Role and User Role code generation
-- Author: System Generated
-- Date: Generated for SMS User Management System
-- =============================================

USE [PDXSMS_V2]
GO

-- =============================================
-- Add SMS Role Management Entities to Registry
-- =============================================

PRINT 'Updating Entity Registry for SMS Role Management...'

-- Check if SMSRole entry exists
IF NOT EXISTS (SELECT 1 FROM [dbo].[tbld_EntityRegistry] WHERE [EntityName] = 'SMSRole')
BEGIN
    INSERT INTO [dbo].[tbld_EntityRegistry] ([EntityName], [TableName], [CodePrefix])
    VALUES ('SMSRole', 'tbld_SMSRoles', 'SR')
    PRINT '- Added SMSRole entity (Prefix: SR)'
END
ELSE
BEGIN
    PRINT '- SMSRole entity already exists in registry'
END

-- Check if SMSUserRole entry exists
IF NOT EXISTS (SELECT 1 FROM [dbo].[tbld_EntityRegistry] WHERE [EntityName] = 'SMSUserRole')
BEGIN
    INSERT INTO [dbo].[tbld_EntityRegistry] ([EntityName], [TableName], [CodePrefix])
    VALUES ('SMSUserRole', 'tbld_SMSUserRoles', 'UR')
    PRINT '- Added SMSUserRole entity (Prefix: UR)'
END
ELSE
BEGIN
    PRINT '- SMSUserRole entity already exists in registry'
END

-- =============================================
-- Verify existing SMS User entities are registered
-- =============================================

PRINT 'Verifying existing SMS User entities in registry...'

-- Check if SMSApplicationUser entry exists
IF NOT EXISTS (SELECT 1 FROM [dbo].[tbld_EntityRegistry] WHERE [EntityName] = 'SMSApplicationUser')
BEGIN
    INSERT INTO [dbo].[tbld_EntityRegistry] ([EntityName], [TableName], [CodePrefix])
    VALUES ('SMSApplicationUser', 'tbld_SMSApplicationUsers', 'AU')
    PRINT '- Added SMSApplicationUser entity (Prefix: AU)'
END
ELSE
BEGIN
    PRINT '- SMSApplicationUser entity already exists in registry'
END

-- Check if SMSOrganizationalUser entry exists
IF NOT EXISTS (SELECT 1 FROM [dbo].[tbld_EntityRegistry] WHERE [EntityName] = 'SMSOrganizationalUser')
BEGIN
    INSERT INTO [dbo].[tbld_EntityRegistry] ([EntityName], [TableName], [CodePrefix])
    VALUES ('SMSOrganizationalUser', 'tbld_SMSOrganizationalUsers', 'OU')
    PRINT '- Added SMSOrganizationalUser entity (Prefix: OU)'
END
ELSE
BEGIN
    PRINT '- SMSOrganizationalUser entity already exists in registry'
END

-- Check if SMSStakeholderUser entry exists
IF NOT EXISTS (SELECT 1 FROM [dbo].[tbld_EntityRegistry] WHERE [EntityName] = 'SMSStakeholderUser')
BEGIN
    INSERT INTO [dbo].[tbld_EntityRegistry] ([EntityName], [TableName], [CodePrefix])
    VALUES ('SMSStakeholderUser', 'tbld_SMSStakeholderUsers', 'SU')
    PRINT '- Added SMSStakeholderUser entity (Prefix: SU)'
END
ELSE
BEGIN
    PRINT '- SMSStakeholderUser entity already exists in registry'
END

-- =============================================
-- Display current SMS User Management registry
-- =============================================

PRINT ''
PRINT '============================================='
PRINT 'Current SMS User Management Entity Registry:'
PRINT '============================================='

SELECT 
    [EntityName],
    [TableName],
    [CodePrefix],
    CASE 
        WHEN [EntityName] LIKE '%User%' THEN 'User Management'
        WHEN [EntityName] LIKE '%Role%' THEN 'Role Management'
        ELSE 'Other'
    END AS Category
FROM [dbo].[tbld_EntityRegistry]
WHERE [EntityName] LIKE 'SMS%User%' 
   OR [EntityName] LIKE 'SMS%Role%'
ORDER BY Category, [EntityName];

PRINT ''
PRINT 'Entity Registry Update Complete!'
PRINT '============================================='