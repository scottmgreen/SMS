-- =============================================
-- SMS USER ROLE ASSIGNMENT STORED PROCEDURES
-- Database: PDXSMS_V2
-- Description: Complete CRUD operations for SMS User Role Assignments
-- Author: System Generated
-- Date: Generated for SMS User Management System
-- =============================================

USE [PDXSMS_V2]
GO

-- =============================================
-- PROCEDURE: pr_SMSUserRole_GetAll
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[pr_SMSUserRole_GetAll]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[pr_SMSUserRole_GetAll]
GO

CREATE PROCEDURE [dbo].[pr_SMSUserRole_GetAll]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ur.[fldi_ID] AS ID,
        ur.[fldv_Code],
        ur.[fldv_UserID],
        ur.[fldv_UserType],
        ur.[fldv_SMSRoleCode],
        r.[fldv_RoleName],
        r.[fldv_AuthorityLevel],
        r.[fldv_RoleCategory],
        ur.[fldv_Department],
        ur.[fldd_EffectiveDate],
        ur.[fldd_ExpirationDate],
        ur.[fldb_IsActive],
        ur.[fldv_AssignedBy],
        ur.[fldd_AssignedDate],
        ur.[fldv_DeactivatedBy],
        ur.[fldd_DeactivatedDate],
        ur.[fldv_CreatedBy], 
        ur.[fldd_CreatedDate], 
        ur.[fldv_UpdatedBy], 
        ur.[fldd_UpdatedDate]
    FROM [dbo].[tbld_SMSUserRoles] ur
    INNER JOIN [dbo].[tbld_SMSRoles] r ON ur.[fldv_SMSRoleCode] = r.[fldv_Code]
    ORDER BY ur.[fldv_UserType], ur.[fldv_UserID], ur.[fldd_EffectiveDate] DESC;
END
GO

-- =============================================
-- PROCEDURE: pr_SMSUserRole_GetById
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[pr_SMSUserRole_GetById]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[pr_SMSUserRole_GetById]
GO

CREATE PROCEDURE [dbo].[pr_SMSUserRole_GetById]
    @pID VARCHAR(60) -- The Code 'UR-0001'
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ur.[fldi_ID],
        ur.[fldv_Code],
        ur.[fldv_UserID],
        ur.[fldv_UserType],
        ur.[fldv_SMSRoleCode],
        r.[fldv_RoleName],
        r.[fldv_AuthorityLevel],
        r.[fldv_RoleCategory],
        ur.[fldv_Department],
        ur.[fldd_EffectiveDate],
        ur.[fldd_ExpirationDate],
        ur.[fldb_IsActive],
        ur.[fldv_AssignedBy],
        ur.[fldd_AssignedDate],
        ur.[fldv_DeactivatedBy],
        ur.[fldd_DeactivatedDate],
        ur.[fldv_CreatedBy], 
        ur.[fldd_CreatedDate], 
        ur.[fldv_UpdatedBy], 
        ur.[fldd_UpdatedDate]
    FROM [dbo].[tbld_SMSUserRoles] ur
    INNER JOIN [dbo].[tbld_SMSRoles] r ON ur.[fldv_SMSRoleCode] = r.[fldv_Code]
    WHERE ur.[fldv_Code] = @pID;
END
GO

-- =============================================
-- PROCEDURE: pr_SMSUserRole_GetByUserId
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[pr_SMSUserRole_GetByUserId]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[pr_SMSUserRole_GetByUserId]
GO

CREATE PROCEDURE [dbo].[pr_SMSUserRole_GetByUserId]
    @pUserID VARCHAR(60)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ur.[fldi_ID],
        ur.[fldv_Code],
        ur.[fldv_UserID],
        ur.[fldv_UserType],
        ur.[fldv_SMSRoleCode],
        r.[fldv_RoleName],
        r.[fldv_AuthorityLevel],
        r.[fldv_RoleCategory],
        ur.[fldv_Department],
        ur.[fldd_EffectiveDate],
        ur.[fldd_ExpirationDate],
        ur.[fldb_IsActive],
        ur.[fldv_AssignedBy],
        ur.[fldd_AssignedDate],
        ur.[fldv_DeactivatedBy],
        ur.[fldd_DeactivatedDate],
        ur.[fldv_CreatedBy], 
        ur.[fldd_CreatedDate], 
        ur.[fldv_UpdatedBy], 
        ur.[fldd_UpdatedDate]
    FROM [dbo].[tbld_SMSUserRoles] ur
    INNER JOIN [dbo].[tbld_SMSRoles] r ON ur.[fldv_SMSRoleCode] = r.[fldv_Code]
    WHERE ur.[fldv_UserID] = @pUserID
    ORDER BY ur.[fldd_EffectiveDate] DESC;
END
GO

-- =============================================
-- PROCEDURE: pr_SMSUserRole_GetByUserType
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[pr_SMSUserRole_GetByUserType]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[pr_SMSUserRole_GetByUserType]
GO

CREATE PROCEDURE [dbo].[pr_SMSUserRole_GetByUserType]
    @pUserType NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ur.[fldi_ID],
        ur.[fldv_Code],
        ur.[fldv_UserID],
        ur.[fldv_UserType],
        ur.[fldv_SMSRoleCode],
        r.[fldv_RoleName],
        r.[fldv_AuthorityLevel],
        r.[fldv_RoleCategory],
        ur.[fldv_Department],
        ur.[fldd_EffectiveDate],
        ur.[fldd_ExpirationDate],
        ur.[fldb_IsActive],
        ur.[fldv_AssignedBy],
        ur.[fldd_AssignedDate],
        ur.[fldv_DeactivatedBy],
        ur.[fldd_DeactivatedDate],
        ur.[fldv_CreatedBy], 
        ur.[fldd_CreatedDate], 
        ur.[fldv_UpdatedBy], 
        ur.[fldd_UpdatedDate]
    FROM [dbo].[tbld_SMSUserRoles] ur
    INNER JOIN [dbo].[tbld_SMSRoles] r ON ur.[fldv_SMSRoleCode] = r.[fldv_Code]
    WHERE ur.[fldv_UserType] = @pUserType
    ORDER BY ur.[fldv_UserID], ur.[fldd_EffectiveDate] DESC;
END
GO

-- =============================================
-- PROCEDURE: pr_SMSUserRole_GetByDepartment
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[pr_SMSUserRole_GetByDepartment]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[pr_SMSUserRole_GetByDepartment]
GO

CREATE PROCEDURE [dbo].[pr_SMSUserRole_GetByDepartment]
    @pDepartment NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ur.[fldi_ID],
        ur.[fldv_Code],
        ur.[fldv_UserID],
        ur.[fldv_UserType],
        ur.[fldv_SMSRoleCode],
        r.[fldv_RoleName],
        r.[fldv_AuthorityLevel],
        r.[fldv_RoleCategory],
        ur.[fldv_Department],
        ur.[fldd_EffectiveDate],
        ur.[fldd_ExpirationDate],
        ur.[fldb_IsActive],
        ur.[fldv_AssignedBy],
        ur.[fldd_AssignedDate],
        ur.[fldv_DeactivatedBy],
        ur.[fldd_DeactivatedDate],
        ur.[fldv_CreatedBy], 
        ur.[fldd_CreatedDate], 
        ur.[fldv_UpdatedBy], 
        ur.[fldd_UpdatedDate]
    FROM [dbo].[tbld_SMSUserRoles] ur
    INNER JOIN [dbo].[tbld_SMSRoles] r ON ur.[fldv_SMSRoleCode] = r.[fldv_Code]
    WHERE ur.[fldv_Department] = @pDepartment
    ORDER BY r.[fldv_AuthorityLevel], ur.[fldv_UserID];
END
GO

-- =============================================
-- PROCEDURE: pr_SMSUserRole_GetByRole
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[pr_SMSUserRole_GetByRole]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[pr_SMSUserRole_GetByRole]
GO

CREATE PROCEDURE [dbo].[pr_SMSUserRole_GetByRole]
    @pSMSRoleCode VARCHAR(60)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ur.[fldi_ID],
        ur.[fldv_Code],
        ur.[fldv_UserID],
        ur.[fldv_UserType],
        ur.[fldv_SMSRoleCode],
        r.[fldv_RoleName],
        r.[fldv_AuthorityLevel],
        r.[fldv_RoleCategory],
        ur.[fldv_Department],
        ur.[fldd_EffectiveDate],
        ur.[fldd_ExpirationDate],
        ur.[fldb_IsActive],
        ur.[fldv_AssignedBy],
        ur.[fldd_AssignedDate],
        ur.[fldv_DeactivatedBy],
        ur.[fldd_DeactivatedDate],
        ur.[fldv_CreatedBy], 
        ur.[fldd_CreatedDate], 
        ur.[fldv_UpdatedBy], 
        ur.[fldd_UpdatedDate]
    FROM [dbo].[tbld_SMSUserRoles] ur
    INNER JOIN [dbo].[tbld_SMSRoles] r ON ur.[fldv_SMSRoleCode] = r.[fldv_Code]
    WHERE ur.[fldv_SMSRoleCode] = @pSMSRoleCode
    ORDER BY ur.[fldv_UserType], ur.[fldv_UserID];
END
GO

-- =============================================
-- PROCEDURE: pr_SMSUserRole_GetActiveAssignments
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[pr_SMSUserRole_GetActiveAssignments]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[pr_SMSUserRole_GetActiveAssignments]
GO

CREATE PROCEDURE [dbo].[pr_SMSUserRole_GetActiveAssignments]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ur.[fldi_ID],
        ur.[fldv_Code],
        ur.[fldv_UserID],
        ur.[fldv_UserType],
        ur.[fldv_SMSRoleCode],
        r.[fldv_RoleName],
        r.[fldv_AuthorityLevel],
        r.[fldv_RoleCategory],
        ur.[fldv_Department],
        ur.[fldd_EffectiveDate],
        ur.[fldd_ExpirationDate],
        ur.[fldb_IsActive],
        ur.[fldv_AssignedBy],
        ur.[fldd_AssignedDate],
        ur.[fldv_DeactivatedBy],
        ur.[fldd_DeactivatedDate],
        ur.[fldv_CreatedBy], 
        ur.[fldd_CreatedDate], 
        ur.[fldv_UpdatedBy], 
        ur.[fldd_UpdatedDate]
    FROM [dbo].[tbld_SMSUserRoles] ur
    INNER JOIN [dbo].[tbld_SMSRoles] r ON ur.[fldv_SMSRoleCode] = r.[fldv_Code]
    WHERE ur.[fldb_IsActive] = 1
      AND ur.[fldd_EffectiveDate] <= GETUTCDATE()
      AND (ur.[fldd_ExpirationDate] IS NULL OR ur.[fldd_ExpirationDate] > GETUTCDATE())
    ORDER BY ur.[fldv_UserType], ur.[fldv_UserID], r.[fldv_AuthorityLevel];
END
GO

-- =============================================
-- PROCEDURE: pr_SMSUserRole_GetExpiredAssignments
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[pr_SMSUserRole_GetExpiredAssignments]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[pr_SMSUserRole_GetExpiredAssignments]
GO

CREATE PROCEDURE [dbo].[pr_SMSUserRole_GetExpiredAssignments]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ur.[fldi_ID],
        ur.[fldv_Code],
        ur.[fldv_UserID],
        ur.[fldv_UserType],
        ur.[fldv_SMSRoleCode],
        r.[fldv_RoleName],
        r.[fldv_AuthorityLevel],
        r.[fldv_RoleCategory],
        ur.[fldv_Department],
        ur.[fldd_EffectiveDate],
        ur.[fldd_ExpirationDate],
        ur.[fldb_IsActive],
        ur.[fldv_AssignedBy],
        ur.[fldd_AssignedDate],
        ur.[fldv_DeactivatedBy],
        ur.[fldd_DeactivatedDate],
        ur.[fldv_CreatedBy], 
        ur.[fldd_CreatedDate], 
        ur.[fldv_UpdatedBy], 
        ur.[fldd_UpdatedDate]
    FROM [dbo].[tbld_SMSUserRoles] ur
    INNER JOIN [dbo].[tbld_SMSRoles] r ON ur.[fldv_SMSRoleCode] = r.[fldv_Code]
    WHERE ur.[fldd_ExpirationDate] IS NOT NULL 
      AND ur.[fldd_ExpirationDate] <= GETUTCDATE()
      AND ur.[fldb_IsActive] = 1
    ORDER BY ur.[fldd_ExpirationDate] DESC;
END
GO

-- =============================================
-- PROCEDURE: pr_SMSUserRole_Insert
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[pr_SMSUserRole_Insert]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[pr_SMSUserRole_Insert]
GO

CREATE PROCEDURE [dbo].[pr_SMSUserRole_Insert]
    @pCode NVARCHAR(50),
    @pUserID VARCHAR(60),
    @pUserType NVARCHAR(50),
    @pSMSRoleCode VARCHAR(60),
    @pDepartment NVARCHAR(100),
    @pEffectiveDate DATETIME2(7),
    @pExpirationDate DATETIME2(7) = NULL,
    @pIsActive BIT = 1,
    @pAssignedBy NVARCHAR(50),
    @pAssignedDate DATETIME2(7),
    @pCreatedBy NVARCHAR(50),
    @pCreatedDate DATETIME2(7),
    @pNewID INT OUTPUT,
    @pNewSMSUserRoleCode VARCHAR(60) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Validate required parameters
    IF @pUserID IS NULL OR LTRIM(RTRIM(@pUserID)) = ''
    BEGIN
        RAISERROR('UserID is required', 16, 1);
        RETURN;
    END
    
    IF @pUserType IS NULL OR LTRIM(RTRIM(@pUserType)) = ''
    BEGIN
        RAISERROR('UserType is required', 16, 1);
        RETURN;
    END
    
    IF @pSMSRoleCode IS NULL OR LTRIM(RTRIM(@pSMSRoleCode)) = ''
    BEGIN
        RAISERROR('SMSRoleCode is required', 16, 1);
        RETURN;
    END
    
    IF @pDepartment IS NULL OR LTRIM(RTRIM(@pDepartment)) = ''
    BEGIN
        RAISERROR('Department is required', 16, 1);
        RETURN;
    END
    
    -- Validate that SMS Role exists
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbld_SMSRoles] WHERE [fldv_Code] = @pSMSRoleCode AND [fldb_IsActive] = 1)
    BEGIN
        RAISERROR('SMS Role with code %s does not exist or is inactive', 16, 1, @pSMSRoleCode);
        RETURN;
    END
    
    -- Validate UserType
    IF @pUserType NOT IN ('SMSApplicationUser', 'SMSOrganizationalUser', 'SMSStakeholderUser')
    BEGIN
        RAISERROR('Invalid UserType: %s', 16, 1, @pUserType);
        RETURN;
    END
    
    -- Check for overlapping active assignments for same user/role combination
    IF EXISTS (
        SELECT 1 FROM [dbo].[tbld_SMSUserRoles] 
        WHERE [fldv_UserID] = @pUserID 
          AND [fldv_SMSRoleCode] = @pSMSRoleCode
          AND [fldb_IsActive] = 1
          AND [fldd_EffectiveDate] <= @pEffectiveDate
          AND ([fldd_ExpirationDate] IS NULL OR [fldd_ExpirationDate] > @pEffectiveDate)
    )
    BEGIN
        RAISERROR('User %s already has an active assignment for role %s with overlapping dates', 16, 1, @pUserID, @pSMSRoleCode);
        RETURN;
    END
    
    -- Generate SMS User Role Code using entity registry
    EXEC [pr_GenerateFormattedCode] 
        @EntityName = 'SMSUserRole',           -- Uses registry: Table='tbld_SMSUserRoles', Prefix='UR'
        @GeneratedCode = @pNewSMSUserRoleCode OUTPUT;

    INSERT INTO [dbo].[tbld_SMSUserRoles] (
        [fldv_Code], [fldv_UserID], [fldv_UserType], [fldv_SMSRoleCode], 
        [fldv_Department], [fldd_EffectiveDate], [fldd_ExpirationDate], [fldb_IsActive],
        [fldv_AssignedBy], [fldd_AssignedDate], [fldv_CreatedBy], [fldd_CreatedDate]
    )
    VALUES (
        @pNewSMSUserRoleCode, @pUserID, @pUserType, @pSMSRoleCode,
        @pDepartment, @pEffectiveDate, @pExpirationDate, @pIsActive,
        @pAssignedBy, @pAssignedDate, @pCreatedBy, @pCreatedDate
    );
    
    SET @pNewID = SCOPE_IDENTITY();
END
GO

-- =============================================
-- PROCEDURE: pr_SMSUserRole_Update
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[pr_SMSUserRole_Update]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[pr_SMSUserRole_Update]
GO

CREATE PROCEDURE [dbo].[pr_SMSUserRole_Update]
    @pID VARCHAR(60),
    @pCode NVARCHAR(50),
    @pUserID VARCHAR(60),
    @pUserType NVARCHAR(50),
    @pSMSRoleCode VARCHAR(60),
    @pDepartment NVARCHAR(100),
    @pEffectiveDate DATETIME2(7),
    @pExpirationDate DATETIME2(7) = NULL,
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
    
    IF @pUserID IS NULL OR LTRIM(RTRIM(@pUserID)) = ''
    BEGIN
        RAISERROR('UserID is required', 16, 1);
        RETURN;
    END
    
    IF @pUserType IS NULL OR LTRIM(RTRIM(@pUserType)) = ''
    BEGIN
        RAISERROR('UserType is required', 16, 1);
        RETURN;
    END
    
    IF @pSMSRoleCode IS NULL OR LTRIM(RTRIM(@pSMSRoleCode)) = ''
    BEGIN
        RAISERROR('SMSRoleCode is required', 16, 1);
        RETURN;
    END
    
    IF @pDepartment IS NULL OR LTRIM(RTRIM(@pDepartment)) = ''
    BEGIN
        RAISERROR('Department is required', 16, 1);
        RETURN;
    END
    
    -- Check if record exists
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbld_SMSUserRoles] WHERE [fldv_Code] = @pID)
    BEGIN
        RAISERROR('SMS User Role with ID %s was not found', 16, 1, @pID);
        RETURN;
    END
    
    -- Validate that SMS Role exists
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbld_SMSRoles] WHERE [fldv_Code] = @pSMSRoleCode)
    BEGIN
        RAISERROR('SMS Role with code %s does not exist', 16, 1, @pSMSRoleCode);
        RETURN;
    END
    
    UPDATE [dbo].[tbld_SMSUserRoles]
    SET 
        [fldv_Code] = @pCode,
        [fldv_UserID] = @pUserID,
        [fldv_UserType] = @pUserType,
        [fldv_SMSRoleCode] = @pSMSRoleCode,
        [fldv_Department] = @pDepartment,
        [fldd_EffectiveDate] = @pEffectiveDate,
        [fldd_ExpirationDate] = @pExpirationDate,
        [fldb_IsActive] = @pIsActive,
        [fldv_UpdatedBy] = @pUpdatedBy,
        [fldd_UpdatedDate] = @pUpdatedDate
    WHERE [fldv_Code] = @pID;
    
    -- Confirm update was successful
    IF @@ROWCOUNT = 0
    BEGIN
        RAISERROR('Failed to update SMS User Role with ID %s', 16, 1, @pID);
        RETURN;
    END
END
GO

-- =============================================
-- PROCEDURE: pr_SMSUserRole_Deactivate
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[pr_SMSUserRole_Deactivate]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[pr_SMSUserRole_Deactivate]
GO

CREATE PROCEDURE [dbo].[pr_SMSUserRole_Deactivate]
    @pID VARCHAR(60),
    @pDeactivatedBy NVARCHAR(50),
    @pDeactivatedDate DATETIME2(7),
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
    
    -- Check if record exists
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbld_SMSUserRoles] WHERE [fldv_Code] = @pID)
    BEGIN
        RAISERROR('SMS User Role with ID %s was not found', 16, 1, @pID);
        RETURN;
    END
    
    -- Check if already deactivated
    IF EXISTS (SELECT 1 FROM [dbo].[tbld_SMSUserRoles] WHERE [fldv_Code] = @pID AND [fldb_IsActive] = 0)
    BEGIN
        RAISERROR('SMS User Role with ID %s is already deactivated', 16, 1, @pID);
        RETURN;
    END
    
    UPDATE [dbo].[tbld_SMSUserRoles]
    SET 
        [fldb_IsActive] = 0,
        [fldv_DeactivatedBy] = @pDeactivatedBy,
        [fldd_DeactivatedDate] = @pDeactivatedDate,
        [fldv_UpdatedBy] = @pUpdatedBy,
        [fldd_UpdatedDate] = @pUpdatedDate
    WHERE [fldv_Code] = @pID;
    
    -- Confirm update was successful
    IF @@ROWCOUNT = 0
    BEGIN
        RAISERROR('Failed to deactivate SMS User Role with ID %s', 16, 1, @pID);
        RETURN;
    END
END
GO

-- =============================================
-- PROCEDURE: pr_SMSUserRole_Reactivate
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[pr_SMSUserRole_Reactivate]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[pr_SMSUserRole_Reactivate]
GO

CREATE PROCEDURE [dbo].[pr_SMSUserRole_Reactivate]
    @pID VARCHAR(60),
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
    
    -- Check if record exists
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbld_SMSUserRoles] WHERE [fldv_Code] = @pID)
    BEGIN
        RAISERROR('SMS User Role with ID %s was not found', 16, 1, @pID);
        RETURN;
    END
    
    -- Check if already active
    IF EXISTS (SELECT 1 FROM [dbo].[tbld_SMSUserRoles] WHERE [fldv_Code] = @pID AND [fldb_IsActive] = 1)
    BEGIN
        RAISERROR('SMS User Role with ID %s is already active', 16, 1, @pID);
        RETURN;
    END
    
    UPDATE [dbo].[tbld_SMSUserRoles]
    SET 
        [fldb_IsActive] = 1,
        [fldv_DeactivatedBy] = NULL,
        [fldd_DeactivatedDate] = NULL,
        [fldv_UpdatedBy] = @pUpdatedBy,
        [fldd_UpdatedDate] = @pUpdatedDate
    WHERE [fldv_Code] = @pID;
    
    -- Confirm update was successful
    IF @@ROWCOUNT = 0
    BEGIN
        RAISERROR('Failed to reactivate SMS User Role with ID %s', 16, 1, @pID);
        RETURN;
    END
END
GO

-- =============================================
-- PROCEDURE: pr_SMSUserRole_ExtendExpiration
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[pr_SMSUserRole_ExtendExpiration]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[pr_SMSUserRole_ExtendExpiration]
GO

CREATE PROCEDURE [dbo].[pr_SMSUserRole_ExtendExpiration]
    @pID VARCHAR(60),
    @pNewExpirationDate DATETIME2(7),
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
    
    -- Check if record exists
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbld_SMSUserRoles] WHERE [fldv_Code] = @pID)
    BEGIN
        RAISERROR('SMS User Role with ID %s was not found', 16, 1, @pID);
        RETURN;
    END
    
    -- Validate that new expiration date is in the future
    IF @pNewExpirationDate IS NOT NULL AND @pNewExpirationDate <= GETUTCDATE()
    BEGIN
        RAISERROR('New expiration date must be in the future', 16, 1);
        RETURN;
    END
    
    UPDATE [dbo].[tbld_SMSUserRoles]
    SET 
        [fldd_ExpirationDate] = @pNewExpirationDate,
        [fldv_UpdatedBy] = @pUpdatedBy,
        [fldd_UpdatedDate] = @pUpdatedDate
    WHERE [fldv_Code] = @pID;
    
    -- Confirm update was successful
    IF @@ROWCOUNT = 0
    BEGIN
        RAISERROR('Failed to extend expiration for SMS User Role with ID %s', 16, 1, @pID);
        RETURN;
    END
END
GO

-- =============================================
-- PROCEDURE: pr_SMSUserRole_Delete
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[pr_SMSUserRole_Delete]') AND type in (N'P', N'PC'))
    DROP PROCEDURE [dbo].[pr_SMSUserRole_Delete]
GO

CREATE PROCEDURE [dbo].[pr_SMSUserRole_Delete]
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
    IF NOT EXISTS (SELECT 1 FROM [dbo].[tbld_SMSUserRoles] WHERE [fldv_Code] = @pID)
    BEGIN
        RAISERROR('SMS User Role with ID %s was not found', 16, 1, @pID);
        RETURN;
    END
    
    DELETE FROM [dbo].[tbld_SMSUserRoles]
    WHERE [fldv_Code] = @pID;
    
    -- Confirm deletion was successful
    IF @@ROWCOUNT = 0
    BEGIN
        RAISERROR('Failed to delete SMS User Role with ID %s', 16, 1, @pID);
        RETURN;
    END
END
GO

PRINT '============================================='
PRINT 'SMS User Role Assignment Stored Procedures Created'
PRINT '============================================='
PRINT 'Created 16 SMS User Role procedures:'
PRINT '- pr_SMSUserRole_GetAll'
PRINT '- pr_SMSUserRole_GetById'
PRINT '- pr_SMSUserRole_GetByUserId'
PRINT '- pr_SMSUserRole_GetByUserType'
PRINT '- pr_SMSUserRole_GetByDepartment'
PRINT '- pr_SMSUserRole_GetByRole'
PRINT '- pr_SMSUserRole_GetActiveAssignments'
PRINT '- pr_SMSUserRole_GetExpiredAssignments'
PRINT '- pr_SMSUserRole_Insert'
PRINT '- pr_SMSUserRole_Update'
PRINT '- pr_SMSUserRole_Deactivate'
PRINT '- pr_SMSUserRole_Reactivate'
PRINT '- pr_SMSUserRole_ExtendExpiration'
PRINT '- pr_SMSUserRole_Delete'
PRINT '============================================='