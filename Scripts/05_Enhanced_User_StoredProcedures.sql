-- =============================================
-- ENHANCED USER STORED PROCEDURES
-- Database: PDXSMS_V2
-- Description: Additional stored procedures for enhanced user management features
-- Author: System Generated
-- Date: Generated for SMS User Management System
-- =============================================

USE [PDXSMS_V2]
GO

-- =============================================
-- ENHANCED SMS APPLICATION USER PROCEDURES
-- =============================================

-- =============================================
-- PROCEDURE: pr_SMSApplicationUser_GetByPermissionLevel
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[pr_SMSApplicationUser_GetByPermissionLevel]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[pr_SMSApplicationUser_GetByPermissionLevel]
GO

CREATE PROCEDURE [dbo].[pr_SMSApplicationUser_GetByPermissionLevel]
    @pPermissionLevel NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [fldi_ID],
        [fldv_Code],
        [fldv_FirstName],
        [fldv_LastName], 
        [fldv_UserName],
        [fldv_Password],
        [fldv_ApplicationRole],
        [fldv_PermissionLevel],
        [fldb_IsActive],
        [fldd_LastLoginDate],
        [fldb_RequiresPasswordChange],
        [fldd_PasswordLastChanged],
        [fldi_FailedLoginAttempts],
        [fldd_AccountLockedUntil],
        [fldi_LoginCount],
        [fldv_CreatedBy], 
        [fldd_CreatedDate], 
        [fldv_UpdatedBy], 
        [fldd_UpdatedDate]
    FROM [dbo].[tbld_SMSApplicationUsers]
    WHERE [fldv_PermissionLevel] = @pPermissionLevel
    ORDER BY [fldv_LastName], [fldv_FirstName];
END
GO

-- =============================================
-- PROCEDURE: pr_SMSApplicationUser_UpdateLoginInfo
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[pr_SMSApplicationUser_UpdateLoginInfo]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[pr_SMSApplicationUser_UpdateLoginInfo]
GO

CREATE PROCEDURE [dbo].[pr_SMSApplicationUser_UpdateLoginInfo]
    @pID VARCHAR(60),
    @pLoginDate DATETIME2(7),
    @pResetFailedAttempts BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate required parameters
    IF @pID IS NULL
    BEGIN
        RAISERROR('ID is required', 16, 1);
        RETURN;
    END
    
    -- Check if record exists
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbld_SMSApplicationUsers] WHERE [fldv_Code] = @pID)
    BEGIN
        RAISERROR('SMS Application User with ID %s was not found', 16, 1, @pID);
        RETURN;
    END
    
    IF @pResetFailedAttempts = 1
    BEGIN
        UPDATE [dbo].[tbld_SMSApplicationUsers]
        SET [fldd_LastLoginDate] = @pLoginDate,
            [fldi_LoginCount] = [fldi_LoginCount] + 1,
            [fldi_FailedLoginAttempts] = 0,
            [fldd_AccountLockedUntil] = NULL
        WHERE [fldv_Code] = @pID;
    END
    ELSE
    BEGIN
        UPDATE [dbo].[tbld_SMSApplicationUsers]
        SET [fldd_LastLoginDate] = @pLoginDate,
            [fldi_LoginCount] = [fldi_LoginCount] + 1
        WHERE [fldv_Code] = @pID;
    END
    
    -- Confirm update was successful
    IF @@ROWCOUNT = 0
    BEGIN
        RAISERROR('Failed to update login info for SMS Application User with ID %s', 16, 1, @pID);
        RETURN;
    END
END
GO

-- =============================================
-- PROCEDURE: pr_SMSApplicationUser_RecordFailedLogin
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[pr_SMSApplicationUser_RecordFailedLogin]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[pr_SMSApplicationUser_RecordFailedLogin]
GO

CREATE PROCEDURE [dbo].[pr_SMSApplicationUser_RecordFailedLogin]
    @pID VARCHAR(60),
    @pMaxFailedAttempts INT = 5,
    @pLockoutDurationMinutes INT = 30
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @CurrentFailedAttempts INT;
    DECLARE @LockoutUntil DATETIME2(7);
    
    -- Validate required parameters
    IF @pID IS NULL
    BEGIN
        RAISERROR('ID is required', 16, 1);
        RETURN;
    END
    
    -- Check if record exists
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbld_SMSApplicationUsers] WHERE [fldv_Code] = @pID)
    BEGIN
        RAISERROR('SMS Application User with ID %s was not found', 16, 1, @pID);
        RETURN;
    END
    
    -- Get current failed attempts
    SELECT @CurrentFailedAttempts = [fldi_FailedLoginAttempts]
    FROM [dbo].[tbld_SMSApplicationUsers]
    WHERE [fldv_Code] = @pID;
    
    SET @CurrentFailedAttempts = @CurrentFailedAttempts + 1;
    
    -- Check if account should be locked
    IF @CurrentFailedAttempts >= @pMaxFailedAttempts
    BEGIN
        SET @LockoutUntil = DATEADD(MINUTE, @pLockoutDurationMinutes, GETUTCDATE());
        
        UPDATE [dbo].[tbld_SMSApplicationUsers]
        SET [fldi_FailedLoginAttempts] = @CurrentFailedAttempts,
            [fldd_AccountLockedUntil] = @LockoutUntil
        WHERE [fldv_Code] = @pID;
    END
    ELSE
    BEGIN
        UPDATE [dbo].[tbld_SMSApplicationUsers]
        SET [fldi_FailedLoginAttempts] = @CurrentFailedAttempts
        WHERE [fldv_Code] = @pID;
    END
    
    -- Return lockout status
    SELECT 
        @CurrentFailedAttempts AS FailedAttempts,
        @LockoutUntil AS LockedUntil,
        CASE WHEN @LockoutUntil IS NOT NULL THEN 1 ELSE 0 END AS IsLocked;
END
GO

-- =============================================
-- PROCEDURE: pr_SMSApplicationUser_GetUsersRequiringPasswordChange
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[pr_SMSApplicationUser_GetUsersRequiringPasswordChange]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[pr_SMSApplicationUser_GetUsersRequiringPasswordChange]
GO

CREATE PROCEDURE [dbo].[pr_SMSApplicationUser_GetUsersRequiringPasswordChange]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [fldi_ID],
        [fldv_Code],
        [fldv_FirstName],
        [fldv_LastName], 
        [fldv_UserName],
        [fldv_ApplicationRole],
        [fldv_PermissionLevel],
        [fldb_IsActive],
        [fldd_LastLoginDate],
        [fldb_RequiresPasswordChange],
        [fldd_PasswordLastChanged],
        [fldv_CreatedBy], 
        [fldd_CreatedDate], 
        [fldv_UpdatedBy], 
        [fldd_UpdatedDate]
    FROM [dbo].[tbld_SMSApplicationUsers]
    WHERE [fldb_RequiresPasswordChange] = 1
       OR [fldd_PasswordLastChanged] IS NULL
       OR [fldd_PasswordLastChanged] < DATEADD(DAY, -90, GETUTCDATE()) -- 90 days old
    ORDER BY [fldd_PasswordLastChanged] ASC, [fldv_LastName], [fldv_FirstName];
END
GO

-- =============================================
-- ENHANCED SMS ORGANIZATIONAL USER PROCEDURES
-- =============================================

-- =============================================
-- PROCEDURE: pr_SMSOrganizationalUser_UpdateLoginInfo
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[pr_SMSOrganizationalUser_UpdateLoginInfo]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[pr_SMSOrganizationalUser_UpdateLoginInfo]
GO

CREATE PROCEDURE [dbo].[pr_SMSOrganizationalUser_UpdateLoginInfo]
    @pID VARCHAR(60),
    @pLoginDate DATETIME2(7),
    @pResetFailedAttempts BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate required parameters
    IF @pID IS NULL
    BEGIN
        RAISERROR('ID is required', 16, 1);
        RETURN;
    END
    
    -- Check if record exists
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbld_SMSOrganizationalUsers] WHERE [fldv_Code] = @pID)
    BEGIN
        RAISERROR('SMS Organizational User with ID %s was not found', 16, 1, @pID);
        RETURN;
    END
    
    IF @pResetFailedAttempts = 1
    BEGIN
        UPDATE [dbo].[tbld_SMSOrganizationalUsers]
        SET [fldd_LastLoginDate] = @pLoginDate,
            [fldi_LoginCount] = [fldi_LoginCount] + 1,
            [fldi_FailedLoginAttempts] = 0,
            [fldd_AccountLockedUntil] = NULL
        WHERE [fldv_Code] = @pID;
    END
    ELSE
    BEGIN
        UPDATE [dbo].[tbld_SMSOrganizationalUsers]
        SET [fldd_LastLoginDate] = @pLoginDate,
            [fldi_LoginCount] = [fldi_LoginCount] + 1
        WHERE [fldv_Code] = @pID;
    END
END
GO

-- =============================================
-- PROCEDURE: pr_SMSOrganizationalUser_GetByOrganizationLevel
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[pr_SMSOrganizationalUser_GetByOrganizationLevel]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[pr_SMSOrganizationalUser_GetByOrganizationLevel]
GO

CREATE PROCEDURE [dbo].[pr_SMSOrganizationalUser_GetByOrganizationLevel]
    @pOrganizationLevel NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [fldi_ID],
        [fldv_Code],
        [fldv_FirstName],
        [fldv_LastName], 
        [fldv_UserName],
        [fldv_Password],
        [fldv_Department],
        [fldv_Position],
        [fldv_OrganizationLevel],
        [fldb_IsActive],
        [fldd_LastLoginDate],
        [fldb_RequiresPasswordChange],
        [fldd_PasswordLastChanged],
        [fldi_FailedLoginAttempts],
        [fldd_AccountLockedUntil],
        [fldi_LoginCount],
        [fldv_CreatedBy], 
        [fldd_CreatedDate], 
        [fldv_UpdatedBy], 
        [fldd_UpdatedDate]
    FROM [dbo].[tbld_SMSOrganizationalUsers]
    WHERE [fldv_OrganizationLevel] = @pOrganizationLevel
    ORDER BY [fldv_Department], [fldv_LastName], [fldv_FirstName];
END
GO

-- =============================================
-- ENHANCED SMS STAKEHOLDER USER PROCEDURES
-- =============================================

-- =============================================
-- PROCEDURE: pr_SMSStakeholderUser_UpdateLoginInfo
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[pr_SMSStakeholderUser_UpdateLoginInfo]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[pr_SMSStakeholderUser_UpdateLoginInfo]
GO

CREATE PROCEDURE [dbo].[pr_SMSStakeholderUser_UpdateLoginInfo]
    @pID VARCHAR(60),
    @pLoginDate DATETIME2(7),
    @pResetFailedAttempts BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate required parameters
    IF @pID IS NULL
    BEGIN
        RAISERROR('ID is required', 16, 1);
        RETURN;
    END
    
    -- Check if record exists
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbld_SMSStakeholderUsers] WHERE [fldv_Code] = @pID)
    BEGIN
        RAISERROR('SMS Stakeholder User with ID %s was not found', 16, 1, @pID);
        RETURN;
    END
    
    IF @pResetFailedAttempts = 1
    BEGIN
        UPDATE [dbo].[tbld_SMSStakeholderUsers]
        SET [fldd_LastLoginDate] = @pLoginDate,
            [fldi_LoginCount] = [fldi_LoginCount] + 1,
            [fldi_FailedLoginAttempts] = 0,
            [fldd_AccountLockedUntil] = NULL
        WHERE [fldv_Code] = @pID;
    END
    ELSE
    BEGIN
        UPDATE [dbo].[tbld_SMSStakeholderUsers]
        SET [fldd_LastLoginDate] = @pLoginDate,
            [fldi_LoginCount] = [fldi_LoginCount] + 1
        WHERE [fldv_Code] = @pID;
    END
END
GO

-- =============================================
-- PROCEDURE: pr_SMSStakeholderUser_GetByAccessLevel
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[pr_SMSStakeholderUser_GetByAccessLevel]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[pr_SMSStakeholderUser_GetByAccessLevel]
GO

CREATE PROCEDURE [dbo].[pr_SMSStakeholderUser_GetByAccessLevel]
    @pAccessLevel NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [fldi_ID],
        [fldv_Code],
        [fldv_FirstName],
        [fldv_LastName], 
        [fldv_UserName],
        [fldv_Password],
        [fldv_StakeholderType],
        [fldv_Organization],
        [fldv_AccessLevel],
        [fldb_IsActive],
        [fldd_LastLoginDate],
        [fldb_RequiresPasswordChange],
        [fldd_PasswordLastChanged],
        [fldi_FailedLoginAttempts],
        [fldd_AccountLockedUntil],
        [fldi_LoginCount],
        [fldv_CreatedBy], 
        [fldd_CreatedDate], 
        [fldv_UpdatedBy], 
        [fldd_UpdatedDate]
    FROM [dbo].[tbld_SMSStakeholderUsers]
    WHERE [fldv_AccessLevel] = @pAccessLevel
    ORDER BY [fldv_Organization], [fldv_LastName], [fldv_FirstName];
END
GO

-- =============================================
-- CROSS-TABLE UTILITY PROCEDURES
-- =============================================

-- =============================================
-- PROCEDURE: pr_SMS_CheckUsernameAvailability
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[pr_SMS_CheckUsernameAvailability]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[pr_SMS_CheckUsernameAvailability]
GO

CREATE PROCEDURE [dbo].[pr_SMS_CheckUsernameAvailability]
    @pUserName NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @IsAvailable BIT = 1;
    DECLARE @ExistingUserType NVARCHAR(50) = NULL;
    DECLARE @ExistingUserID VARCHAR(60) = NULL;
    
    -- Check SMS Application Users
    IF EXISTS (SELECT 1 FROM [dbo].[tbld_SMSApplicationUsers] WHERE [fldv_UserName] = @pUserName)
    BEGIN
        SET @IsAvailable = 0;
        SET @ExistingUserType = 'SMSApplicationUser';
        SELECT @ExistingUserID = [fldv_Code] FROM [dbo].[tbld_SMSApplicationUsers] WHERE [fldv_UserName] = @pUserName;
    END
    
    -- Check SMS Organizational Users
    IF EXISTS (SELECT 1 FROM [dbo].[tbld_SMSOrganizationalUsers] WHERE [fldv_UserName] = @pUserName)
    BEGIN
        SET @IsAvailable = 0;
        SET @ExistingUserType = 'SMSOrganizationalUser';
        SELECT @ExistingUserID = [fldv_Code] FROM [dbo].[tbld_SMSOrganizationalUsers] WHERE [fldv_UserName] = @pUserName;
    END
    
    -- Check SMS Stakeholder Users
    IF EXISTS (SELECT 1 FROM [dbo].[tbld_SMSStakeholderUsers] WHERE [fldv_UserName] = @pUserName)
    BEGIN
        SET @IsAvailable = 0;
        SET @ExistingUserType = 'SMSStakeholderUser';
        SELECT @ExistingUserID = [fldv_Code] FROM [dbo].[tbld_SMSStakeholderUsers] WHERE [fldv_UserName] = @pUserName;
    END
    
    -- Return results
    SELECT 
        @pUserName AS UserName,
        @IsAvailable AS IsAvailable,
        @ExistingUserType AS ExistingUserType,
        @ExistingUserID AS ExistingUserID;
END
GO

-- =============================================
-- PROCEDURE: pr_SMS_GetUserStatisticsSummary
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[pr_SMS_GetUserStatisticsSummary]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[pr_SMS_GetUserStatisticsSummary]
GO

CREATE PROCEDURE [dbo].[pr_SMS_GetUserStatisticsSummary]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        'SMS Application Users' AS UserType,
        COUNT(*) AS TotalUsers,
        SUM(CASE WHEN [fldb_IsActive] = 1 THEN 1 ELSE 0 END) AS ActiveUsers,
        SUM(CASE WHEN [fldb_IsActive] = 0 THEN 1 ELSE 0 END) AS InactiveUsers,
        SUM(CASE WHEN [fldb_RequiresPasswordChange] = 1 THEN 1 ELSE 0 END) AS RequiresPasswordChange,
        SUM(CASE WHEN [fldd_AccountLockedUntil] IS NOT NULL AND [fldd_AccountLockedUntil] > GETUTCDATE() THEN 1 ELSE 0 END) AS LockedAccounts,
        MAX([fldd_LastLoginDate]) AS MostRecentLogin
    FROM [dbo].[tbld_SMSApplicationUsers]
    
    UNION ALL
    
    SELECT 
        'SMS Organizational Users' AS UserType,
        COUNT(*) AS TotalUsers,
        SUM(CASE WHEN [fldb_IsActive] = 1 THEN 1 ELSE 0 END) AS ActiveUsers,
        SUM(CASE WHEN [fldb_IsActive] = 0 THEN 1 ELSE 0 END) AS InactiveUsers,
        SUM(CASE WHEN [fldb_RequiresPasswordChange] = 1 THEN 1 ELSE 0 END) AS RequiresPasswordChange,
        SUM(CASE WHEN [fldd_AccountLockedUntil] IS NOT NULL AND [fldd_AccountLockedUntil] > GETUTCDATE() THEN 1 ELSE 0 END) AS LockedAccounts,
        MAX([fldd_LastLoginDate]) AS MostRecentLogin
    FROM [dbo].[tbld_SMSOrganizationalUsers]
    
    UNION ALL
    
    SELECT 
        'SMS Stakeholder Users' AS UserType,
        COUNT(*) AS TotalUsers,
        SUM(CASE WHEN [fldb_IsActive] = 1 THEN 1 ELSE 0 END) AS ActiveUsers,
        SUM(CASE WHEN [fldb_IsActive] = 0 THEN 1 ELSE 0 END) AS InactiveUsers,
        SUM(CASE WHEN [fldb_RequiresPasswordChange] = 1 THEN 1 ELSE 0 END) AS RequiresPasswordChange,
        SUM(CASE WHEN [fldd_AccountLockedUntil] IS NOT NULL AND [fldd_AccountLockedUntil] > GETUTCDATE() THEN 1 ELSE 0 END) AS LockedAccounts,
        MAX([fldd_LastLoginDate]) AS MostRecentLogin
    FROM [dbo].[tbld_SMSStakeholderUsers]
    
    ORDER BY UserType;
END
GO

PRINT '============================================='
PRINT 'Enhanced User Management Stored Procedures Created'
PRINT '============================================='
PRINT 'Created 8 additional procedures:'
PRINT '- pr_SMSApplicationUser_GetByPermissionLevel'
PRINT '- pr_SMSApplicationUser_UpdateLoginInfo'
PRINT '- pr_SMSApplicationUser_RecordFailedLogin'
PRINT '- pr_SMSApplicationUser_GetUsersRequiringPasswordChange'
PRINT '- pr_SMSOrganizationalUser_UpdateLoginInfo'
PRINT '- pr_SMSOrganizationalUser_GetByOrganizationLevel'
PRINT '- pr_SMSStakeholderUser_UpdateLoginInfo'
PRINT '- pr_SMSStakeholderUser_GetByAccessLevel'
PRINT '- pr_SMS_CheckUsernameAvailability'
PRINT '- pr_SMS_GetUserStatisticsSummary'
PRINT '============================================='