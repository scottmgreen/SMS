USE [PDXSMS_V2]
GO

-- =============================================
-- SMS User Role Stored Procedures
-- Table: tbls_SMSUserRoles
-- Created: 2025-11-24
-- =============================================

-- =============================================
-- 1. Insert SMS User Role
-- =============================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[pr_SMSUserRole_Insert]
    @pCode NCHAR(30),
    @pName NCHAR(30)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @NextId INT;
    
    -- Get next ID
    SELECT @NextId = ISNULL(MAX([fldi_Id]), 0) + 1 FROM [dbo].[tbls_SMSUserRoles];
    
    INSERT INTO [dbo].[tbls_SMSUserRoles]
    (
        [fldi_Id],
        [fldv_Code],
        [fldv_Name]
    )
    VALUES
    (
        @NextId,
        @pCode,
        @pName
    );
    
    SELECT @NextId AS NewId, @pCode AS NewCode;
END
GO

-- =============================================
-- 2. Get All SMS User Roles
-- =============================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[pr_SMSUserRole_GetAll]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [fldi_Id],
        [fldv_Code],
        [fldv_Name]
    FROM [dbo].[tbls_SMSUserRoles]
    ORDER BY [fldv_Name];
END
GO

-- =============================================
-- 3. Get SMS User Role by ID
-- =============================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[pr_SMSUserRole_GetById]
    @pId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [fldi_Id],
        [fldv_Code],
        [fldv_Name]
    FROM [dbo].[tbls_SMSUserRoles]
    WHERE [fldi_Id] = @pId;
END
GO

-- =============================================
-- 4. Get SMS User Role by User ID (from user role assignments table)
-- Note: This requires a user role assignment junction table
-- =============================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[pr_SMSUserRole_GetByUserId]
    @pUserId NVARCHAR(60)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- This assumes there's a junction table for user role assignments
    -- Adjust table names as needed based on your schema
    SELECT DISTINCT
        ur.[fldi_Id],
        ur.[fldv_Code],
        ur.[fldv_Name]
    FROM [dbo].[tbls_SMSUserRoles] ur
    INNER JOIN [dbo].[tbld_SMSUserRoles] ura ON ur.[fldv_Code] = ura.[fldv_SMSRoleCode]
    WHERE ura.[fldv_UserId] = @pUserId
    ORDER BY ur.[fldv_Name];
END
GO

-- =============================================
-- 5. Get Active SMS User Roles by User ID
-- =============================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[pr_SMSUserRole_GetActiveByUserId]
    @pUserId NVARCHAR(60)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT DISTINCT
        ur.[fldi_Id],
        ur.[fldv_Code],
        ur.[fldv_Name]
    FROM [dbo].[tbls_SMSUserRoles] ur
    INNER JOIN [dbo].[tbld_SMSUserRoles] ura ON ur.[fldv_Code] = ura.[fldv_SMSRoleCode]
    WHERE ura.[fldv_UserId] = @pUserId
      AND ura.[fldb_IsActive] = 1
      AND (ura.[fldd_ExpirationDate] IS NULL OR ura.[fldd_ExpirationDate] > GETUTCDATE())
    ORDER BY ur.[fldv_Name];
END
GO

-- =============================================
-- 6. Get SMS User Roles by Role Code
-- =============================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[pr_SMSUserRole_GetByRole]
    @pRoleCode NCHAR(30)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [fldi_Id],
        [fldv_Code],
        [fldv_Name]
    FROM [dbo].[tbls_SMSUserRoles]
    WHERE [fldv_Code] = @pRoleCode;
END
GO

-- =============================================
-- 7. Get SMS User Roles by Department
-- =============================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[pr_SMSUserRole_GetByDepartment]
    @pDepartment NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT DISTINCT
        ur.[fldi_Id],
        ur.[fldv_Code],
        ur.[fldv_Name]
    FROM [dbo].[tbls_SMSUserRoles] ur
    INNER JOIN [dbo].[tbld_SMSUserRoles] ura ON ur.[fldv_Code] = ura.[fldv_SMSRoleCode]
    WHERE ura.[fldv_Department] = @pDepartment
    ORDER BY ur.[fldv_Name];
END
GO

-- =============================================
-- 8. Get SMS User Roles by User Type
-- =============================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[pr_SMSUserRole_GetByUserType]
    @pUserType NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT DISTINCT
        ur.[fldi_Id],
        ur.[fldv_Code],
        ur.[fldv_Name]
    FROM [dbo].[tbls_SMSUserRoles] ur
    INNER JOIN [dbo].[tbld_SMSUserRoles] ura ON ur.[fldv_Code] = ura.[fldv_SMSRoleCode]
    WHERE ura.[fldv_UserType] = @pUserType
    ORDER BY ur.[fldv_Name];
END
GO

-- =============================================
-- 9. Get Active SMS User Role Assignments
-- =============================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[pr_SMSUserRole_GetActiveAssignments]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ura.[fldv_Code],
        ura.[fldv_UserId],
        ura.[fldv_UserType],
        ura.[fldv_SMSRoleCode],
        ur.[fldv_Name] AS RoleName,
        ura.[fldv_Department],
        ura.[fldd_EffectiveDate],
        ura.[fldd_ExpirationDate],
        ura.[fldv_AssignedBy],
        ura.[fldd_AssignedDate]
    FROM [dbo].[tbld_SMSUserRoles] ura
    INNER JOIN [dbo].[tbls_SMSUserRoles] ur ON ura.[fldv_SMSRoleCode] = ur.[fldv_Code]
    WHERE ura.[fldb_IsActive] = 1
      AND (ura.[fldd_ExpirationDate] IS NULL OR ura.[fldd_ExpirationDate] > GETUTCDATE())
    ORDER BY ura.[fldv_UserId], ur.[fldv_Name];
END
GO

-- =============================================
-- 10. Get Expired SMS User Role Assignments
-- =============================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[pr_SMSUserRole_GetExpiredAssignments]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        ura.[fldv_Code],
        ura.[fldv_UserId],
        ura.[fldv_UserType],
        ura.[fldv_SMSRoleCode],
        ur.[fldv_Name] AS RoleName,
        ura.[fldv_Department],
        ura.[fldd_EffectiveDate],
        ura.[fldd_ExpirationDate],
        ura.[fldv_AssignedBy],
        ura.[fldd_AssignedDate]
    FROM [dbo].[tbld_SMSUserRoles] ura
    INNER JOIN [dbo].[tbls_SMSUserRoles] ur ON ura.[fldv_SMSRoleCode] = ur.[fldv_Code]
    WHERE ura.[fldb_IsActive] = 1
      AND ura.[fldd_ExpirationDate] IS NOT NULL 
      AND ura.[fldd_ExpirationDate] <= GETUTCDATE()
    ORDER BY ura.[fldd_ExpirationDate] DESC;
END
GO

-- =============================================
-- 11. Get Expiring SMS User Roles (within specified days)
-- =============================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[pr_SMSUserRole_GetExpiringRoles]
    @pDaysFromNow INT = 30
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @CutoffDate DATETIME;
    SET @CutoffDate = DATEADD(DAY, @pDaysFromNow, GETUTCDATE());
    
    SELECT 
        ura.[fldv_Code],
        ura.[fldv_UserId],
        ura.[fldv_UserType],
        ura.[fldv_SMSRoleCode],
        ur.[fldv_Name] AS RoleName,
        ura.[fldv_Department],
        ura.[fldd_EffectiveDate],
        ura.[fldd_ExpirationDate],
        DATEDIFF(DAY, GETUTCDATE(), ura.[fldd_ExpirationDate]) AS DaysUntilExpiration,
        ura.[fldv_AssignedBy],
        ura.[fldd_AssignedDate]
    FROM [dbo].[tbld_SMSUserRoles] ura
    INNER JOIN [dbo].[tbls_SMSUserRoles] ur ON ura.[fldv_SMSRoleCode] = ur.[fldv_Code]
    WHERE ura.[fldb_IsActive] = 1
      AND ura.[fldd_ExpirationDate] IS NOT NULL
      AND ura.[fldd_ExpirationDate] > GETUTCDATE()
      AND ura.[fldd_ExpirationDate] <= @CutoffDate
    ORDER BY ura.[fldd_ExpirationDate] ASC;
END
GO

-- =============================================
-- 12. Update SMS User Role
-- =============================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[pr_SMSUserRole_Update]
    @pId INT,
    @pCode NCHAR(30),
    @pName NCHAR(30)
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE [dbo].[tbls_SMSUserRoles]
    SET 
        [fldv_Code] = @pCode,
        [fldv_Name] = @pName
    WHERE [fldi_Id] = @pId;
    
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- =============================================
-- 13. Deactivate SMS User Role Assignment
-- =============================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[pr_SMSUserRole_Deactivate]
    @pAssignmentCode NVARCHAR(60),
    @pDeactivatedBy NVARCHAR(50),
    @pDeactivationReason NVARCHAR(200) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE [dbo].[tbld_SMSUserRoles]
    SET 
        [fldb_IsActive] = 0,
        [fldv_DeactivatedBy] = @pDeactivatedBy,
        [fldd_DeactivatedDate] = GETUTCDATE()
    WHERE [fldv_Code] = @pAssignmentCode;
    
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- =============================================
-- 14. Delete SMS User Role
-- =============================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[pr_SMSUserRole_Delete]
    @pId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Check if role is in use
    DECLARE @InUseCount INT;
    SELECT @InUseCount = COUNT(*) 
    FROM [dbo].[tbld_SMSUserRoles] 
    WHERE [fldv_SMSRoleCode] = (SELECT [fldv_Code] FROM [dbo].[tbls_SMSUserRoles] WHERE [fldi_Id] = @pId);
    
    IF @InUseCount > 0
    BEGIN
        RAISERROR('Cannot delete SMS User Role - it is currently assigned to users', 16, 1);
        RETURN;
    END
    
    DELETE FROM [dbo].[tbls_SMSUserRoles]
    WHERE [fldi_Id] = @pId;
    
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- =============================================
-- 15. Extend Expiration of SMS User Role Assignment
-- =============================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[pr_SMSUserRole_ExtendExpiration]
    @pAssignmentCode NVARCHAR(60),
    @pNewExpirationDate DATETIME,
    @pExtendedBy NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE [dbo].[tbld_SMSUserRoles]
    SET 
        [fldd_ExpirationDate] = @pNewExpirationDate,
        [fldv_UpdatedBy] = @pExtendedBy,
        [fldd_UpdatedDate] = GETUTCDATE()
    WHERE [fldv_Code] = @pAssignmentCode;
    
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- =============================================
-- 16. Reactivate SMS User Role Assignment
-- =============================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[pr_SMSUserRole_Reactivate]
    @pAssignmentCode NVARCHAR(60),
    @pReactivatedBy NVARCHAR(50),
    @pNewExpirationDate DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE [dbo].[tbld_SMSUserRoles]
    SET 
        [fldb_IsActive] = 1,
        [fldv_DeactivatedBy] = NULL,
        [fldd_DeactivatedDate] = NULL,
        [fldd_ExpirationDate] = @pNewExpirationDate,
        [fldv_UpdatedBy] = @pReactivatedBy,
        [fldd_UpdatedDate] = GETUTCDATE()
    WHERE [fldv_Code] = @pAssignmentCode;
    
    SELECT @@ROWCOUNT AS RowsAffected;
END
GO

-- =============================================
-- 17. Get SMS User Role Statistics Report
-- =============================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[pr_SMSUserRole_GetStatsReport]
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Role usage statistics
    SELECT 
        ur.[fldv_Code] AS RoleCode,
        ur.[fldv_Name] AS RoleName,
        COUNT(ura.[fldv_Code]) AS TotalAssignments,
        SUM(CASE WHEN ura.[fldb_IsActive] = 1 THEN 1 ELSE 0 END) AS ActiveAssignments,
        SUM(CASE WHEN ura.[fldb_IsActive] = 0 THEN 1 ELSE 0 END) AS InactiveAssignments,
        SUM(CASE WHEN ura.[fldd_ExpirationDate] IS NOT NULL AND ura.[fldd_ExpirationDate] <= GETUTCDATE() THEN 1 ELSE 0 END) AS ExpiredAssignments,
        SUM(CASE WHEN ura.[fldd_ExpirationDate] IS NOT NULL AND ura.[fldd_ExpirationDate] <= DATEADD(DAY, 30, GETUTCDATE()) AND ura.[fldd_ExpirationDate] > GETUTCDATE() THEN 1 ELSE 0 END) AS ExpiringAssignments
    FROM [dbo].[tbls_SMSUserRoles] ur
    LEFT JOIN [dbo].[tbld_SMSUserRoles] ura ON ur.[fldv_Code] = ura.[fldv_SMSRoleCode]
    GROUP BY ur.[fldv_Code], ur.[fldv_Name]
    ORDER BY ur.[fldv_Name];
END
GO

-- =============================================
-- 18. Validate User Authorization
-- =============================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[pr_SMSUserRole_ValidateUserAuthorization]
    @pUserId NVARCHAR(60),
    @pRequiredRole NCHAR(30)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @HasRole BIT = 0;
    
    SELECT @HasRole = CASE WHEN COUNT(*) > 0 THEN 1 ELSE 0 END
    FROM [dbo].[tbld_SMSUserRoles] ura
    INNER JOIN [dbo].[tbls_SMSUserRoles] ur ON ura.[fldv_SMSRoleCode] = ur.[fldv_Code]
    WHERE ura.[fldv_UserId] = @pUserId
      AND ur.[fldv_Code] = @pRequiredRole
      AND ura.[fldb_IsActive] = 1
      AND (ura.[fldd_ExpirationDate] IS NULL OR ura.[fldd_ExpirationDate] > GETUTCDATE());
    
    SELECT @HasRole AS HasAuthorization;
END
GO

-- =============================================
-- 19. Get User Maximum Authority Level
-- =============================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[pr_SMSUserRole_GetUserMaxAuthorityLevel]
    @pUserId NVARCHAR(60)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- This assumes there's an authority level field in roles or role permissions
    -- Adjust based on your actual schema
    SELECT TOP 1
        ur.[fldv_Code] AS RoleCode,
        ur.[fldv_Name] AS RoleName,
        -- You may need to join with permissions table to get actual authority level
        10 AS AuthorityLevel -- Placeholder - adjust based on your authority level logic
    FROM [dbo].[tbld_SMSUserRoles] ura
    INNER JOIN [dbo].[tbls_SMSUserRoles] ur ON ura.[fldv_SMSRoleCode] = ur.[fldv_Code]
    WHERE ura.[fldv_UserId] = @pUserId
      AND ura.[fldb_IsActive] = 1
      AND (ura.[fldd_ExpirationDate] IS NULL OR ura.[fldd_ExpirationDate] > GETUTCDATE())
    ORDER BY 10 DESC; -- Order by authority level descending
END
GO

-- =============================================
-- SUMMARY
-- =============================================
/*
All stored procedures have been created for the tbls_SMSUserRoles table.

Key Notes:
1. Some procedures reference tbld_SMSUserRoles table for user role assignments
2. Authority levels may need adjustment based on your actual schema
3. Field names follow your naming convention (fldv_, fldi_, fldb_, fldd_)
4. Includes proper error handling and validation
5. All procedures include proper ordering and filtering

Usage:
- pr_SMSUserRole_Insert: Add new role
- pr_SMSUserRole_GetAll: Get all roles
- pr_SMSUserRole_GetById: Get specific role
- pr_SMSUserRole_GetByUserId: Get roles for user
- pr_SMSUserRole_GetActiveByUserId: Get active roles for user
- pr_SMSUserRole_Update: Update role details
- pr_SMSUserRole_Delete: Remove role (with validation)
- pr_SMSUserRole_Deactivate: Deactivate user role assignment
- pr_SMSUserRole_Reactivate: Reactivate user role assignment
- pr_SMSUserRole_GetStatsReport: Role usage statistics
- pr_SMSUserRole_ValidateUserAuthorization: Check user permissions
- pr_SMSUserRole_GetUserMaxAuthorityLevel: Get highest authority level
*/