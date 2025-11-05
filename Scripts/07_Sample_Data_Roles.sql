-- =============================================
-- SMS USER MANAGEMENT - SAMPLE DATA INSERTION
-- Database: PDXSMS_V2
-- Description: Insert sample data for SMS Roles and initial user assignments
-- Author: System Generated
-- Date: Generated for SMS User Management System
-- =============================================

USE [PDXSMS_V2]
GO

-- =============================================
-- SAMPLE SMS ROLES DATA
-- =============================================

PRINT 'Inserting Sample SMS Roles...'

-- Safety Category Roles
IF NOT EXISTS (SELECT 1 FROM [dbo].[tbld_SMSRoles] WHERE [fldv_RoleName] = 'Safety Manager')
BEGIN
    EXEC [dbo].[pr_SMSRole_Insert] 
        @pCode = NULL,
        @pRoleName = 'Safety Manager',
        @pAuthorityLevel = 'Manager',
        @pRoleCategory = 'Safety',
        @pDescription = 'Manages overall safety programs and compliance',
        @pIsActive = 1,
        @pCreatedBy = 'SYSTEM_SETUP',
        @pCreatedDate = '2024-01-01T00:00:00',
        @pNewID = NULL,
        @pNewSMSRoleCode = NULL;
    PRINT '- Created Safety Manager role'
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[tbld_SMSRoles] WHERE [fldv_RoleName] = 'Safety Officer')
BEGIN
    EXEC [dbo].[pr_SMSRole_Insert] 
        @pCode = NULL,
        @pRoleName = 'Safety Officer',
        @pAuthorityLevel = 'Supervisor',
        @pRoleCategory = 'Safety',
        @pDescription = 'Conducts safety inspections and incident investigations',
        @pIsActive = 1,
        @pCreatedBy = 'SYSTEM_SETUP',
        @pCreatedDate = '2024-01-01T00:00:00',
        @pNewID = NULL,
        @pNewSMSRoleCode = NULL;
    PRINT '- Created Safety Officer role'
END

-- Security Category Roles
IF NOT EXISTS (SELECT 1 FROM [dbo].[tbld_SMSRoles] WHERE [fldv_RoleName] = 'Security Manager')
BEGIN
    EXEC [dbo].[pr_SMSRole_Insert] 
        @pCode = NULL,
        @pRoleName = 'Security Manager',
        @pAuthorityLevel = 'Manager',
        @pRoleCategory = 'Security',
        @pDescription = 'Oversees security operations and access control',
        @pIsActive = 1,
        @pCreatedBy = 'SYSTEM_SETUP',
        @pCreatedDate = '2024-01-01T00:00:00',
        @pNewID = NULL,
        @pNewSMSRoleCode = NULL;
    PRINT '- Created Security Manager role'
END

-- Operations Category Roles
IF NOT EXISTS (SELECT 1 FROM [dbo].[tbld_SMSRoles] WHERE [fldv_RoleName] = 'Operations Manager')
BEGIN
    EXEC [dbo].[pr_SMSRole_Insert] 
        @pCode = NULL,
        @pRoleName = 'Operations Manager',
        @pAuthorityLevel = 'Manager',
        @pRoleCategory = 'Operations',
        @pDescription = 'Manages daily airport operations and coordination',
        @pIsActive = 1,
        @pCreatedBy = 'SYSTEM_SETUP',
        @pCreatedDate = '2024-01-01T00:00:00',
        @pNewID = NULL,
        @pNewSMSRoleCode = NULL;
    PRINT '- Created Operations Manager role'
END

-- Quality Category Roles
IF NOT EXISTS (SELECT 1 FROM [dbo].[tbld_SMSRoles] WHERE [fldv_RoleName] = 'Quality Assurance Analyst')
BEGIN
    EXEC [dbo].[pr_SMSRole_Insert] 
        @pCode = NULL,
        @pRoleName = 'Quality Assurance Analyst',
        @pAuthorityLevel = 'Standard',
        @pRoleCategory = 'Quality',
        @pDescription = 'Performs quality audits and compliance monitoring',
        @pIsActive = 1,
        @pCreatedBy = 'SYSTEM_SETUP',
        @pCreatedDate = '2024-01-01T00:00:00',
        @pNewID = NULL,
        @pNewSMSRoleCode = NULL;
    PRINT '- Created Quality Assurance Analyst role'
END

-- Management Category Roles
IF NOT EXISTS (SELECT 1 FROM [dbo].[tbld_SMSRoles] WHERE [fldv_RoleName] = 'Airport Director')
BEGIN
    EXEC [dbo].[pr_SMSRole_Insert] 
        @pCode = NULL,
        @pRoleName = 'Airport Director',
        @pAuthorityLevel = 'Executive',
        @pRoleCategory = 'Management',
        @pDescription = 'Executive oversight of all airport operations',
        @pIsActive = 1,
        @pCreatedBy = 'SYSTEM_SETUP',
        @pCreatedDate = '2024-01-01T00:00:00',
        @pNewID = NULL,
        @pNewSMSRoleCode = NULL;
    PRINT '- Created Airport Director role'
END

-- Administrative Category Roles
IF NOT EXISTS (SELECT 1 FROM [dbo].[tbld_SMSRoles] WHERE [fldv_RoleName] = 'System Administrator')
BEGIN
    EXEC [dbo].[pr_SMSRole_Insert] 
        @pCode = NULL,
        @pRoleName = 'System Administrator',
        @pAuthorityLevel = 'Administrator',
        @pRoleCategory = 'Administrative',
        @pDescription = 'Manages SMS system access and configuration',
        @pIsActive = 1,
        @pCreatedBy = 'SYSTEM_SETUP',
        @pCreatedDate = '2024-01-01T00:00:00',
        @pNewID = NULL,
        @pNewSMSRoleCode = NULL;
    PRINT '- Created System Administrator role'
END

-- Technical Category Roles
IF NOT EXISTS (SELECT 1 FROM [dbo].[tbld_SMSRoles] WHERE [fldv_RoleName] = 'Technical Specialist')
BEGIN
    EXEC [dbo].[pr_SMSRole_Insert] 
        @pCode = NULL,
        @pRoleName = 'Technical Specialist',
        @pAuthorityLevel = 'Elevated',
        @pRoleCategory = 'Technical',
        @pDescription = 'Provides technical expertise and system maintenance',
        @pIsActive = 1,
        @pCreatedBy = 'SYSTEM_SETUP',
        @pCreatedDate = '2024-01-01T00:00:00',
        @pNewID = NULL,
        @pNewSMSRoleCode = NULL;
    PRINT '- Created Technical Specialist role'
END

PRINT ''
PRINT '============================================='
PRINT 'Sample SMS Roles Creation Summary'
PRINT '============================================='

-- Display created roles
SELECT 
    [fldv_Code] AS RoleCode,
    [fldv_RoleName] AS RoleName,
    [fldv_AuthorityLevel] AS AuthorityLevel,
    [fldv_RoleCategory] AS Category,
    [fldv_Description] AS Description
FROM [dbo].[tbld_SMSRoles]
WHERE [fldv_CreatedBy] = 'SYSTEM_SETUP'
ORDER BY 
    CASE [fldv_AuthorityLevel]
        WHEN 'Executive' THEN 1
        WHEN 'Director' THEN 2
        WHEN 'Manager' THEN 3
        WHEN 'Supervisor' THEN 4
        WHEN 'Elevated' THEN 5
        WHEN 'Standard' THEN 6
        WHEN 'Administrator' THEN 7
        ELSE 8
    END,
    [fldv_RoleCategory],
    [fldv_RoleName];

-- Display role statistics
PRINT ''
PRINT 'Role Statistics:'
SELECT 
    [fldv_RoleCategory] AS Category,
    COUNT(*) AS RoleCount
FROM [dbo].[tbld_SMSRoles]
WHERE [fldb_IsActive] = 1
GROUP BY [fldv_RoleCategory]
ORDER BY RoleCount DESC;

PRINT ''
PRINT 'Authority Level Distribution:'
SELECT 
    [fldv_AuthorityLevel] AS AuthorityLevel,
    COUNT(*) AS RoleCount
FROM [dbo].[tbld_SMSRoles]
WHERE [fldb_IsActive] = 1
GROUP BY [fldv_AuthorityLevel]
ORDER BY 
    CASE [fldv_AuthorityLevel]
        WHEN 'Executive' THEN 1
        WHEN 'Director' THEN 2
        WHEN 'Manager' THEN 3
        WHEN 'Supervisor' THEN 4
        WHEN 'Elevated' THEN 5
        WHEN 'Standard' THEN 6
        WHEN 'Administrator' THEN 7
        ELSE 8
    END;

PRINT ''
PRINT '============================================='
PRINT 'Sample Data Insertion Complete!'
PRINT ''
PRINT 'Next Steps:'
PRINT '1. Create sample users using existing procedures'
PRINT '2. Assign roles using pr_SMSUserRole_Insert'
PRINT '3. Test role-based access in your application'
PRINT '============================================='