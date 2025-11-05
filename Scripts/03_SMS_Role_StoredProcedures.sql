-- =============================================
-- SMS ROLE STORED PROCEDURES
-- Database: PDXSMS_V2
-- Description: Complete CRUD operations for SMS Roles
-- Author: System Generated
-- Date: Generated for SMS User Management System
-- =============================================

USE [PDXSMS_V2]
GO

-- =============================================
-- PROCEDURE: pr_SMSRole_GetAll
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[pr_SMSRole_GetAll]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[pr_SMSRole_GetAll]
GO

CREATE PROCEDURE [dbo].[pr_SMSRole_GetAll]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [fldi_ID] AS ID,
        [fldv_Code],
        [fldv_RoleName],
        [fldv_AuthorityLevel],
        [fldv_RoleCategory],
        [fldv_Description],
        [fldb_IsActive],
        [fldv_CreatedBy], 
        [fldd_CreatedDate], 
        [fldv_UpdatedBy], 
        [fldd_UpdatedDate]
    FROM [dbo].[tbld_SMSRoles]
    ORDER BY [fldv_RoleCategory], [fldv_AuthorityLevel], [fldv_RoleName];
END
GO

-- =============================================
-- PROCEDURE: pr_SMSRole_GetById
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[pr_SMSRole_GetById]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[pr_SMSRole_GetById]
GO

CREATE PROCEDURE [dbo].[pr_SMSRole_GetById]
    @pID VARCHAR(60) -- The Code 'SR-0001'
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [fldi_ID],
        [fldv_Code],
        [fldv_RoleName],
        [fldv_AuthorityLevel],
        [fldv_RoleCategory],
        [fldv_Description],
        [fldb_IsActive],
        [fldv_CreatedBy], 
        [fldd_CreatedDate], 
        [fldv_UpdatedBy], 
        [fldd_UpdatedDate]
    FROM [dbo].[tbld_SMSRoles]
    WHERE [fldv_Code] = @pID;
END
GO

-- =============================================
-- PROCEDURE: pr_SMSRole_GetByName
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[pr_SMSRole_GetByName]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[pr_SMSRole_GetByName]
GO

CREATE PROCEDURE [dbo].[pr_SMSRole_GetByName]
    @pRoleName NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [fldi_ID],
        [fldv_Code],
        [fldv_RoleName],
        [fldv_AuthorityLevel],
        [fldv_RoleCategory],
        [fldv_Description],
        [fldb_IsActive],
        [fldv_CreatedBy], 
        [fldd_CreatedDate], 
        [fldv_UpdatedBy], 
        [fldd_UpdatedDate]
    FROM [dbo].[tbld_SMSRoles]
    WHERE [fldv_RoleName] = @pRoleName;
END
GO

-- =============================================
-- PROCEDURE: pr_SMSRole_GetByAuthorityLevel
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[pr_SMSRole_GetByAuthorityLevel]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[pr_SMSRole_GetByAuthorityLevel]
GO

CREATE PROCEDURE [dbo].[pr_SMSRole_GetByAuthorityLevel]
    @pAuthorityLevel NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [fldi_ID],
        [fldv_Code],
        [fldv_RoleName],
        [fldv_AuthorityLevel],
        [fldv_RoleCategory],
        [fldv_Description],
        [fldb_IsActive],
        [fldv_CreatedBy], 
        [fldd_CreatedDate], 
        [fldv_UpdatedBy], 
        [fldd_UpdatedDate]
    FROM [dbo].[tbld_SMSRoles]
    WHERE [fldv_AuthorityLevel] = @pAuthorityLevel
    ORDER BY [fldv_RoleCategory], [fldv_RoleName];
END
GO

-- =============================================
-- PROCEDURE: pr_SMSRole_GetByCategory
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[pr_SMSRole_GetByCategory]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[pr_SMSRole_GetByCategory]
GO

CREATE PROCEDURE [dbo].[pr_SMSRole_GetByCategory]
    @pRoleCategory NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [fldi_ID],
        [fldv_Code],
        [fldv_RoleName],
        [fldv_AuthorityLevel],
        [fldv_RoleCategory],
        [fldv_Description],
        [fldb_IsActive],
        [fldv_CreatedBy], 
        [fldd_CreatedDate], 
        [fldv_UpdatedBy], 
        [fldd_UpdatedDate]
    FROM [dbo].[tbld_SMSRoles]
    WHERE [fldv_RoleCategory] = @pRoleCategory
    ORDER BY [fldv_AuthorityLevel], [fldv_RoleName];
END
GO

-- =============================================
-- PROCEDURE: pr_SMSRole_GetActiveRoles
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[pr_SMSRole_GetActiveRoles]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[pr_SMSRole_GetActiveRoles]
GO

CREATE PROCEDURE [dbo].[pr_SMSRole_GetActiveRoles]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [fldi_ID],
        [fldv_Code],
        [fldv_RoleName],
        [fldv_AuthorityLevel],
        [fldv_RoleCategory],
        [fldv_Description],
        [fldb_IsActive],
        [fldv_CreatedBy], 
        [fldd_CreatedDate], 
        [fldv_UpdatedBy], 
        [fldd_UpdatedDate]
    FROM [dbo].[tbld_SMSRoles]
    WHERE [fldb_IsActive] = 1
    ORDER BY [fldv_RoleCategory], [fldv_AuthorityLevel], [fldv_RoleName];
END
GO

-- =============================================
-- PROCEDURE: pr_SMSRole_Insert
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[pr_SMSRole_Insert]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[pr_SMSRole_Insert]
GO

CREATE PROCEDURE [dbo].[pr_SMSRole_Insert]
    @pCode NVARCHAR(50),
    @pRoleName NVARCHAR(100),
    @pAuthorityLevel NVARCHAR(50),
    @pRoleCategory NVARCHAR(50),
    @pDescription NVARCHAR(500) = NULL,
    @pIsActive BIT = 1,
    @pCreatedBy NVARCHAR(50),
    @pCreatedDate DATETIME2(7),
    @pNewID INT OUTPUT,
    @pNewSMSRoleCode VARCHAR(60) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate required parameters
    IF @pRoleName IS NULL OR LTRIM(RTRIM(@pRoleName)) = ''
    BEGIN
        RAISERROR('RoleName is required', 16, 1);
        RETURN;
    END
    
    IF @pAuthorityLevel IS NULL OR LTRIM(RTRIM(@pAuthorityLevel)) = ''
    BEGIN
        RAISERROR('AuthorityLevel is required', 16, 1);
        RETURN;
    END
    
    IF @pRoleCategory IS NULL OR LTRIM(RTRIM(@pRoleCategory)) = ''
    BEGIN
        RAISERROR('RoleCategory is required', 16, 1);
        RETURN;
    END
    
    -- Check for duplicate role name
    IF EXISTS (SELECT 1 FROM [dbo].[tbld_SMSRoles] WHERE [fldv_RoleName] = @pRoleName)
    BEGIN
        RAISERROR('A role with name %s already exists', 16, 1, @pRoleName);
        RETURN;
    END
    
    -- Generate SMS Role Code using entity registry
    EXEC [pr_GenerateFormattedCode] 
        @EntityName = 'SMSRole',           -- Uses registry: Table='tbld_SMSRoles', Prefix='SR'
        @GeneratedCode = @pNewSMSRoleCode OUTPUT;

    INSERT INTO [dbo].[tbld_SMSRoles] (
        [fldv_Code], [fldv_RoleName], [fldv_AuthorityLevel], [fldv_RoleCategory], 
        [fldv_Description], [fldb_IsActive], [fldv_CreatedBy], [fldd_CreatedDate]
    )
    VALUES (
        @pNewSMSRoleCode, @pRoleName, @pAuthorityLevel, @pRoleCategory,
        @pDescription, @pIsActive, @pCreatedBy, @pCreatedDate
    );
    
    SET @pNewID = SCOPE_IDENTITY();
END
GO

-- =============================================
-- PROCEDURE: pr_SMSRole_Update
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[pr_SMSRole_Update]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[pr_SMSRole_Update]
GO

CREATE PROCEDURE [dbo].[pr_SMSRole_Update]
    @pID VARCHAR(60),
    @pCode NVARCHAR(50),
    @pRoleName NVARCHAR(100),
    @pAuthorityLevel NVARCHAR(50),
    @pRoleCategory NVARCHAR(50),
    @pDescription NVARCHAR(500) = NULL,
    @pIsActive BIT,
    @pUpdatedBy NVARCHAR(50),
    @pUpdatedDate DATETIME2(7)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate required parameters
    IF @pID IS NULL
    BEGIN
        RAISERROR('ID is required', 16, 1);
        RETURN;
    END
    
    IF @pRoleName IS NULL OR LTRIM(RTRIM(@pRoleName)) = ''
    BEGIN
        RAISERROR('RoleName is required', 16, 1);
        RETURN;
    END
    
    IF @pAuthorityLevel IS NULL OR LTRIM(RTRIM(@pAuthorityLevel)) = ''
    BEGIN
        RAISERROR('AuthorityLevel is required', 16, 1);
        RETURN;
    END
    
    IF @pRoleCategory IS NULL OR LTRIM(RTRIM(@pRoleCategory)) = ''
    BEGIN
        RAISERROR('RoleCategory is required', 16, 1);
        RETURN;
    END
    
    -- Check if record exists
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbld_SMSRoles] WHERE [fldv_Code] = @pID)
    BEGIN
        RAISERROR('SMS Role with ID %s was not found', 16, 1, @pID);
        RETURN;
    END
    
    -- Check for duplicate role name (excluding current record)
    IF EXISTS (SELECT 1 FROM [dbo].[tbld_SMSRoles] WHERE [fldv_RoleName] = @pRoleName AND [fldv_Code] != @pID)
    BEGIN
        RAISERROR('A role with name %s already exists', 16, 1, @pRoleName);
        RETURN;
    END
    
    UPDATE [dbo].[tbld_SMSRoles]
    SET 
        [fldv_Code] = @pCode,
        [fldv_RoleName] = @pRoleName,
        [fldv_AuthorityLevel] = @pAuthorityLevel,
        [fldv_RoleCategory] = @pRoleCategory,
        [fldv_Description] = @pDescription,
        [fldb_IsActive] = @pIsActive,
        [fldv_UpdatedBy] = @pUpdatedBy,
        [fldd_UpdatedDate] = @pUpdatedDate
    WHERE [fldv_Code] = @pID;
    
    -- Confirm update was successful
    IF @@ROWCOUNT = 0
    BEGIN
        RAISERROR('Failed to update SMS Role with ID %s', 16, 1, @pID);
        RETURN;
    END
END
GO

-- =============================================
-- PROCEDURE: pr_SMSRole_Delete
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[pr_SMSRole_Delete]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[pr_SMSRole_Delete]
GO

CREATE PROCEDURE [dbo].[pr_SMSRole_Delete]
    @pID VARCHAR(60)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate required ID
    IF @pID IS NULL
    BEGIN
        RAISERROR('ID is required', 16, 1);
        RETURN;
    END
    
    -- Check if record exists
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbld_SMSRoles] WHERE [fldv_Code] = @pID)
    BEGIN
        RAISERROR('SMS Role with ID %s was not found', 16, 1, @pID);
        RETURN;
    END
    
    -- Check if role is in use (has active assignments)
    IF EXISTS (SELECT 1 FROM [dbo].[tbld_SMSUserRoles] WHERE [fldv_SMSRoleCode] = @pID AND [fldb_IsActive] = 1)
    BEGIN
        RAISERROR('Cannot delete SMS Role %s - it has active user assignments', 16, 1, @pID);
        RETURN;
    END
    
    DELETE FROM [dbo].[tbld_SMSRoles]
    WHERE [fldv_Code] = @pID;
    
    -- Confirm deletion was successful
    IF @@ROWCOUNT = 0
    BEGIN
        RAISERROR('Failed to delete SMS Role with ID %s', 16, 1, @pID);
        RETURN;
    END
END
GO

PRINT '============================================='
PRINT 'SMS Role Stored Procedures Created'
PRINT '============================================='
PRINT 'Created 9 SMS Role procedures:'
PRINT '- pr_SMSRole_GetAll'
PRINT '- pr_SMSRole_GetById'
PRINT '- pr_SMSRole_GetByName'
PRINT '- pr_SMSRole_GetByAuthorityLevel'
PRINT '- pr_SMSRole_GetByCategory'
PRINT '- pr_SMSRole_GetActiveRoles'
PRINT '- pr_SMSRole_Insert'
PRINT '- pr_SMSRole_Update'
PRINT '- pr_SMSRole_Delete'
PRINT '============================================='