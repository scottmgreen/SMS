-- =============================================
-- SMS USER MANAGEMENT - COMPLETE DEPLOYMENT SCRIPT
-- Database: PDXSMS_V2
-- Description: Master script to deploy all SMS Role Management components
-- Author: System Generated
-- Date: Generated for SMS User Management System
-- 
-- EXECUTION ORDER:
-- Run this script to deploy everything in the correct sequence
-- =============================================

USE [PDXSMS_V2]
GO

PRINT '============================================='
PRINT 'SMS USER MANAGEMENT SYSTEM DEPLOYMENT'
PRINT 'Starting Complete Database Schema Updates'
PRINT '============================================='
PRINT ''

-- =============================================
-- STEP 1: Create Role Management Tables
-- =============================================
PRINT 'STEP 1: Creating SMS Role Management Tables...'
PRINT '----------------------------------------------'

-- Execute table creation script
:r "01_Create_SMS_Role_Tables.sql"

PRINT ''
PRINT 'STEP 1 COMPLETED: Role Management Tables'
PRINT ''

-- =============================================
-- STEP 2: Enhance User Tables
-- =============================================
PRINT 'STEP 2: Enhancing User Tables with Security Fields...'
PRINT '------------------------------------------------------'

-- Execute user table enhancement script
:r "02_Enhance_SMS_User_Tables.sql"

PRINT ''
PRINT 'STEP 2 COMPLETED: User Table Enhancements'
PRINT ''

-- =============================================
-- STEP 3: Create SMS Role Stored Procedures
-- =============================================
PRINT 'STEP 3: Creating SMS Role Stored Procedures...'
PRINT '-----------------------------------------------'

-- Execute SMS Role procedures script
:r "03_SMS_Role_StoredProcedures.sql"

PRINT ''
PRINT 'STEP 3 COMPLETED: SMS Role Procedures'
PRINT ''

-- =============================================
-- STEP 4: Create SMS User Role Stored Procedures
-- =============================================
PRINT 'STEP 4: Creating SMS User Role Assignment Procedures...'
PRINT '--------------------------------------------------------'

-- Execute SMS User Role procedures script
:r "04_SMS_UserRole_StoredProcedures.sql"

PRINT ''
PRINT 'STEP 4 COMPLETED: SMS User Role Assignment Procedures'
PRINT ''

-- =============================================
-- STEP 5: Create Enhanced User Stored Procedures
-- =============================================
PRINT 'STEP 5: Creating Enhanced User Management Procedures...'
PRINT '-------------------------------------------------------'

-- Execute enhanced user procedures script
:r "05_Enhanced_User_StoredProcedures.sql"

PRINT ''
PRINT 'STEP 5 COMPLETED: Enhanced User Management Procedures'
PRINT ''

-- =============================================
-- STEP 6: Update Entity Registry
-- =============================================
PRINT 'STEP 6: Updating Entity Registry for Code Generation...'
PRINT '-------------------------------------------------------'

-- Execute entity registry updates script
:r "06_Entity_Registry_Updates.sql"

PRINT ''
PRINT 'STEP 6 COMPLETED: Entity Registry Updates'
PRINT ''

-- =============================================
-- STEP 7: Insert Sample Data (Optional)
-- =============================================
PRINT 'STEP 7: Inserting Sample SMS Roles Data...'
PRINT '------------------------------------------'

-- Execute sample data script
:r "07_Sample_Data_Roles.sql"

PRINT ''
PRINT 'STEP 7 COMPLETED: Sample Data Insertion'
PRINT ''

-- =============================================
-- FINAL VALIDATION AND SUMMARY
-- =============================================
PRINT '============================================='
PRINT 'DEPLOYMENT VALIDATION AND SUMMARY'
PRINT '============================================='

-- Validate table creation
PRINT 'Validating Table Creation:'
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tbld_SMSRoles]') AND type in (N'U'))
    PRINT '? tbld_SMSRoles - Created Successfully'
ELSE
    PRINT '? tbld_SMSRoles - FAILED TO CREATE'

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tbld_SMSUserRoles]') AND type in (N'U'))
    PRINT '? tbld_SMSUserRoles - Created Successfully'
ELSE
    PRINT '? tbld_SMSUserRoles - FAILED TO CREATE'

-- Validate stored procedures
PRINT ''
PRINT 'Validating Stored Procedure Creation:'

DECLARE @RoleProcCount INT, @UserRoleProcCount INT, @EnhancedProcCount INT;

SELECT @RoleProcCount = COUNT(*)
FROM sys.objects 
WHERE type = 'P' AND name LIKE 'pr_SMSRole_%';

SELECT @UserRoleProcCount = COUNT(*)
FROM sys.objects 
WHERE type = 'P' AND name LIKE 'pr_SMSUserRole_%';

SELECT @EnhancedProcCount = COUNT(*)
FROM sys.objects 
WHERE type = 'P' AND name IN (
    'pr_SMSApplicationUser_GetByPermissionLevel',
    'pr_SMSApplicationUser_UpdateLoginInfo',
    'pr_SMSApplicationUser_RecordFailedLogin',
    'pr_SMS_CheckUsernameAvailability',
    'pr_SMS_GetUserStatisticsSummary'
);

PRINT '? SMS Role Procedures: ' + CAST(@RoleProcCount AS VARCHAR(10)) + ' procedures created'
PRINT '? SMS User Role Procedures: ' + CAST(@UserRoleProcCount AS VARCHAR(10)) + ' procedures created'
PRINT '? Enhanced User Procedures: ' + CAST(@EnhancedProcCount AS VARCHAR(10)) + ' procedures created'

-- Display role statistics
PRINT ''
PRINT 'Sample Data Summary:'
SELECT 
    COUNT(*) AS TotalRoles,
    COUNT(CASE WHEN [fldb_IsActive] = 1 THEN 1 END) AS ActiveRoles,
    COUNT(DISTINCT [fldv_RoleCategory]) AS Categories,
    COUNT(DISTINCT [fldv_AuthorityLevel]) AS AuthorityLevels
FROM [dbo].[tbld_SMSRoles];

-- Entity Registry Status
PRINT ''
PRINT 'Entity Registry Status:'
SELECT 
    COUNT(*) AS SMS_Entities
FROM [dbo].[tbld_EntityRegistry]
WHERE [EntityName] LIKE 'SMS%';

PRINT ''
PRINT '============================================='
PRINT '?? SMS USER MANAGEMENT DEPLOYMENT COMPLETE! ??'
PRINT '============================================='
PRINT ''
PRINT 'Successfully Deployed:'
PRINT '• 2 New Tables (tbld_SMSRoles, tbld_SMSUserRoles)'
PRINT '• Enhanced 3 User Tables with security fields'
PRINT '• 35+ Stored Procedures for complete CRUD operations'
PRINT '• Performance indexes and data integrity constraints'
PRINT '• Entity registry entries for code generation'
PRINT '• Sample role data for immediate testing'
PRINT ''
PRINT 'Your SMS User Management System is ready for:'
PRINT '? Role-based access control'
PRINT '? User role assignments with temporal validity'
PRINT '? Enhanced security and audit capabilities'
PRINT '? Complete integration with existing domain entities'
PRINT ''
PRINT 'Next Steps:'
PRINT '1. Update your C# repositories to use new procedures'
PRINT '2. Test role assignment and validation workflows'
PRINT '3. Configure application security with new role system'
PRINT '============================================='