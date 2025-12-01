USE [PDXSMS_V2]
GO

-- =============================================
-- SMS Stakeholder User Stored Procedures - Multi Dataset Approach
-- Updated to support 3-dataset pattern with User Roles and Permissions
-- Created: 2025-11-24
-- =============================================

-- =============================================
-- 1. Get All SMS Stakeholder Users with Roles and Permissions
-- =============================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[pr_SMSStakeholderUser_GetAll]
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Dataset 1: All SMSStakeholderUsers
    SELECT 
        [fldi_ID],
        [fldv_Code],
        [fldv_FirstName],
        [fldv_LastName], 
        [fldv_UserName],
        [fldv_Password],
        [fldv_StakeholderTypeCode],
        [fldv_Organization],
        [fldv_SMSUserRoleCode],
        [fldb_IsActive],
        [fldd_LastLoginDate],
        [fldv_CreatedBy], 
        [fldd_CreatedDate], 
        [fldv_UpdatedBy], 
        [fldd_UpdatedDate]
    FROM [dbo].[tbld_SMSStakeholderUsers]
    ORDER BY [fldv_LastName], [fldv_FirstName];

    -- Dataset 2: All SMSUserRoles used by stakeholder users
    SELECT DISTINCT
        ur.[fldi_Id],
        ur.[fldv_Code],
        ur.[fldv_Name]
    FROM [PDXSMS_V2].[dbo].[tbls_SMSUserRoles] ur
    INNER JOIN [dbo].[tbld_SMSStakeholderUsers] su ON ur.[fldv_Code] = su.[fldv_SMSUserRoleCode]
    ORDER BY ur.[fldv_Name];

    -- Dataset 3: All SMSUserRolePermissions for stakeholder user roles
    SELECT 
        urp.[fldi_Id],
        urp.[fldv_Code],
        urp.[fldv_SMSUserRoleCode],
        urp.[fldv_Module],
        urp.[fldb_Create],
        urp.[fldb_Read],
        urp.[fldb_Update],
        urp.[fldb_Delete]
    FROM [PDXSMS_V2].[dbo].[tbls_SMSUserRolePermissions] urp
    INNER JOIN [dbo].[tbld_SMSStakeholderUsers] su ON urp.[fldv_SMSUserRoleCode] = su.[fldv_SMSUserRoleCode]
    ORDER BY urp.[fldv_SMSUserRoleCode], urp.[fldv_Module];

END
GO

-- =============================================
-- 2. Get Active SMS Stakeholder Users with Roles and Permissions
-- =============================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[pr_SMSStakeholderUser_GetActiveUsers]
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Dataset 1: Active SMSStakeholderUsers only
    SELECT 
        [fldi_ID],
        [fldv_Code],
        [fldv_FirstName],
        [fldv_LastName], 
        [fldv_UserName],
        [fldv_Password],
        [fldv_StakeholderTypeCode],
        [fldv_Organization],
        [fldv_SMSUserRoleCode],
        [fldb_IsActive],
        [fldd_LastLoginDate],
        [fldv_CreatedBy], 
        [fldd_CreatedDate], 
        [fldv_UpdatedBy], 
        [fldd_UpdatedDate]
    FROM [dbo].[tbld_SMSStakeholderUsers]
    WHERE [fldb_IsActive] = 1
    ORDER BY [fldv_LastName], [fldv_FirstName];

    -- Dataset 2: SMSUserRoles for active stakeholder users
    SELECT DISTINCT
        ur.[fldi_Id],
        ur.[fldv_Code],
        ur.[fldv_Name]
    FROM [PDXSMS_V2].[dbo].[tbls_SMSUserRoles] ur
    INNER JOIN [dbo].[tbld_SMSStakeholderUsers] su ON ur.[fldv_Code] = su.[fldv_SMSUserRoleCode]
    WHERE su.[fldb_IsActive] = 1
    ORDER BY ur.[fldv_Name];

    -- Dataset 3: SMSUserRolePermissions for active stakeholder user roles
    SELECT 
        urp.[fldi_Id],
        urp.[fldv_Code],
        urp.[fldv_SMSUserRoleCode],
        urp.[fldv_Module],
        urp.[fldb_Create],
        urp.[fldb_Read],
        urp.[fldb_Update],
        urp.[fldb_Delete]
    FROM [PDXSMS_V2].[dbo].[tbls_SMSUserRolePermissions] urp
    INNER JOIN [dbo].[tbld_SMSStakeholderUsers] su ON urp.[fldv_SMSUserRoleCode] = su.[fldv_SMSUserRoleCode]
    WHERE su.[fldb_IsActive] = 1
    ORDER BY urp.[fldv_SMSUserRoleCode], urp.[fldv_Module];

END
GO

-- =============================================
-- 3. Get SMS Stakeholder User by Username with Roles and Permissions
-- =============================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[pr_SMSStakeholderUser_GetByUserName]
    @pUserName NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @UserRoleCode NCHAR(10);
    
    -- Get the user role code for this stakeholder user
    SELECT @UserRoleCode = [fldv_SMSUserRoleCode] 
    FROM [dbo].[tbld_SMSStakeholderUsers] 
    WHERE [fldv_UserName] = @pUserName;
    
    -- Dataset 1: SMSStakeholderUser data
    SELECT 
        [fldi_ID],
        [fldv_Code],
        [fldv_FirstName],
        [fldv_LastName], 
        [fldv_UserName],
        [fldv_Password],
        [fldv_StakeholderTypeCode],
        [fldv_Organization],
        [fldv_SMSUserRoleCode],
        [fldb_IsActive],
        [fldd_LastLoginDate],
        [fldv_CreatedBy], 
        [fldd_CreatedDate], 
        [fldv_UpdatedBy], 
        [fldd_UpdatedDate]
    FROM [dbo].[tbld_SMSStakeholderUsers]
    WHERE [fldv_UserName] = @pUserName;

    -- Dataset 2: SMSUserRole data
    SELECT 
        [fldi_Id],
        [fldv_Code],
        [fldv_Name]
    FROM [PDXSMS_V2].[dbo].[tbls_SMSUserRoles] 
    WHERE [fldv_Code] = @UserRoleCode;

    -- Dataset 3: SMSUserRolePermissions data
    SELECT 
        [fldi_Id],
        [fldv_Code],
        [fldv_SMSUserRoleCode],
        [fldv_Module],
        [fldb_Create],
        [fldb_Read],
        [fldb_Update],
        [fldb_Delete]
    FROM [PDXSMS_V2].[dbo].[tbls_SMSUserRolePermissions] 
    WHERE [fldv_SMSUserRoleCode] = @UserRoleCode;

END
GO

-- =============================================
-- 4. Get SMS Stakeholder Users by Stakeholder Type with Roles and Permissions
-- =============================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[pr_SMSStakeholderUser_GetByStakeholderType]
    @pStakeholderType NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Dataset 1: SMSStakeholderUsers by type
    SELECT 
        [fldi_ID],
        [fldv_Code],
        [fldv_FirstName],
        [fldv_LastName], 
        [fldv_UserName],
        [fldv_Password],
        [fldv_StakeholderTypeCode],
        [fldv_Organization],
        [fldv_SMSUserRoleCode],
        [fldb_IsActive],
        [fldd_LastLoginDate],
        [fldv_CreatedBy], 
        [fldd_CreatedDate], 
        [fldv_UpdatedBy], 
        [fldd_UpdatedDate]
    FROM [dbo].[tbld_SMSStakeholderUsers]
    WHERE [fldv_StakeholderTypeCode] = @pStakeholderType
    ORDER BY [fldv_LastName], [fldv_FirstName];

    -- Dataset 2: SMSUserRoles for this stakeholder type
    SELECT DISTINCT
        ur.[fldi_Id],
        ur.[fldv_Code],
        ur.[fldv_Name]
    FROM [PDXSMS_V2].[dbo].[tbls_SMSUserRoles] ur
    INNER JOIN [dbo].[tbld_SMSStakeholderUsers] su ON ur.[fldv_Code] = su.[fldv_SMSUserRoleCode]
    WHERE su.[fldv_StakeholderTypeCode] = @pStakeholderType
    ORDER BY ur.[fldv_Name];

    -- Dataset 3: SMSUserRolePermissions for this stakeholder type
    SELECT 
        urp.[fldi_Id],
        urp.[fldv_Code],
        urp.[fldv_SMSUserRoleCode],
        urp.[fldv_Module],
        urp.[fldb_Create],
        urp.[fldb_Read],
        urp.[fldb_Update],
        urp.[fldb_Delete]
    FROM [PDXSMS_V2].[dbo].[tbls_SMSUserRolePermissions] urp
    INNER JOIN [dbo].[tbld_SMSStakeholderUsers] su ON urp.[fldv_SMSUserRoleCode] = su.[fldv_SMSUserRoleCode]
    WHERE su.[fldv_StakeholderTypeCode] = @pStakeholderType
    ORDER BY urp.[fldv_SMSUserRoleCode], urp.[fldv_Module];

END
GO

-- =============================================
-- 5. Get SMS Stakeholder Users by Organization with Roles and Permissions
-- =============================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[pr_SMSStakeholderUser_GetByOrganization]
    @pOrganization NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Dataset 1: SMSStakeholderUsers by organization
    SELECT 
        [fldi_ID],
        [fldv_Code],
        [fldv_FirstName],
        [fldv_LastName], 
        [fldv_UserName],
        [fldv_Password],
        [fldv_StakeholderTypeCode],
        [fldv_Organization],
        [fldv_SMSUserRoleCode],
        [fldb_IsActive],
        [fldd_LastLoginDate],
        [fldv_CreatedBy], 
        [fldd_CreatedDate], 
        [fldv_UpdatedBy], 
        [fldd_UpdatedDate]
    FROM [dbo].[tbld_SMSStakeholderUsers]
    WHERE [fldv_Organization] = @pOrganization
    ORDER BY [fldv_LastName], [fldv_FirstName];

    -- Dataset 2: SMSUserRoles for this organization
    SELECT DISTINCT
        ur.[fldi_Id],
        ur.[fldv_Code],
        ur.[fldv_Name]
    FROM [PDXSMS_V2].[dbo].[tbls_SMSUserRoles] ur
    INNER JOIN [dbo].[tbld_SMSStakeholderUsers] su ON ur.[fldv_Code] = su.[fldv_SMSUserRoleCode]
    WHERE su.[fldv_Organization] = @pOrganization
    ORDER BY ur.[fldv_Name];

    -- Dataset 3: SMSUserRolePermissions for this organization
    SELECT 
        urp.[fldi_Id],
        urp.[fldv_Code],
        urp.[fldv_SMSUserRoleCode],
        urp.[fldv_Module],
        urp.[fldb_Create],
        urp.[fldb_Read],
        urp.[fldb_Update],
        urp.[fldb_Delete]
    FROM [PDXSMS_V2].[dbo].[tbls_SMSUserRolePermissions] urp
    INNER JOIN [dbo].[tbld_SMSStakeholderUsers] su ON urp.[fldv_SMSUserRoleCode] = su.[fldv_SMSUserRoleCode]
    WHERE su.[fldv_Organization] = @pOrganization
    ORDER BY urp.[fldv_SMSUserRoleCode], urp.[fldv_Module];

END
GO

-- =============================================
-- 6. Get SMS Stakeholder Users by Group Code with Roles and Permissions
-- =============================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[pr_SMSStakeholderUser_GetByGroupCode]
    @pCode VARCHAR(60),
    @pUserID VARCHAR(10) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @FunctionName VARCHAR(50) = 'pr_SMSStakeholderUserGroup_GetUsersByGroup', @RecordCount INT;
    
    BEGIN TRY
        SET @AuditMessage = 'Retrieving users for group: ' + @pCode;
        
        -- Dataset 1: SMSStakeholderUsers in group
        SELECT 
            su.[fldi_ID],
            su.[fldv_Code],
            su.[fldv_FirstName],
            su.[fldv_LastName], 
            su.[fldv_UserName],
            su.[fldv_Password],
            su.[fldv_StakeholderTypeCode],
            su.[fldv_Organization],
            su.[fldv_SMSUserRoleCode],
            su.[fldb_IsActive],
            su.[fldd_LastLoginDate],
            su.[fldv_CreatedBy], 
            su.[fldd_CreatedDate], 
            su.[fldv_UpdatedBy], 
            su.[fldd_UpdatedDate]
        FROM [dbo].[tbld_SMSStakeholderUsers] su
        INNER JOIN [dbo].tblr_SMSStakeholderUserGroups ug ON su.[fldv_Code] = ug.[fldv_UserCode]
        WHERE ug.[fldv_GroupCode] = @pCode
          AND su.[fldb_IsActive] = 1
        ORDER BY su.[fldv_LastName], su.[fldv_FirstName];

        -- Dataset 2: SMSUserRoles for users in group
        SELECT DISTINCT
            ur.[fldi_Id],
            ur.[fldv_Code],
            ur.[fldv_Name]
        FROM [PDXSMS_V2].[dbo].[tbls_SMSUserRoles] ur
        INNER JOIN [dbo].[tbld_SMSStakeholderUsers] su ON ur.[fldv_Code] = su.[fldv_SMSUserRoleCode]
        INNER JOIN [dbo].tblr_SMSStakeholderUserGroups ug ON su.[fldv_Code] = ug.[fldv_UserCode]
        WHERE ug.[fldv_GroupCode] = @pCode
          AND su.[fldb_IsActive] = 1
        ORDER BY ur.[fldv_Name];

        -- Dataset 3: SMSUserRolePermissions for users in group
        SELECT 
            urp.[fldi_Id],
            urp.[fldv_Code],
            urp.[fldv_SMSUserRoleCode],
            urp.[fldv_Module],
            urp.[fldb_Create],
            urp.[fldb_Read],
            urp.[fldb_Update],
            urp.[fldb_Delete]
        FROM [PDXSMS_V2].[dbo].[tbls_SMSUserRolePermissions] urp
        INNER JOIN [dbo].[tbld_SMSStakeholderUsers] su ON urp.[fldv_SMSUserRoleCode] = su.[fldv_SMSUserRoleCode]
        INNER JOIN [dbo].tblr_SMSStakeholderUserGroups ug ON su.[fldv_Code] = ug.[fldv_UserCode]
        WHERE ug.[fldv_GroupCode] = @pCode
          AND su.[fldb_IsActive] = 1
        ORDER BY urp.[fldv_SMSUserRoleCode], urp.[fldv_Module];
        
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error retrieving users for group ' + @pCode + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_StakeholderUserGroups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

-- =============================================
-- 7. Get SMS Stakeholder User by ID with Roles and Permissions (TEMPLATE)
-- =============================================
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[pr_SMSStakeholderUser_GetById]
    @pID VARCHAR(60) -- The Code 'SU-0001'
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @StakeholderUserTypeCode NCHAR(10);
    DECLARE @UserRoleCode NCHAR(10);
    
    -- Get the user role code for this stakeholder user
    SELECT @UserRoleCode = [fldv_SMSUserRoleCode] 
    FROM [dbo].[tbld_SMSStakeholderUsers] 
    WHERE [fldv_Code] = @pID;
    
    -- Dataset 1: SMSStakeholderUser data
    SELECT 
        [fldi_ID],
        [fldv_Code],
        [fldv_FirstName],
        [fldv_LastName], 
        [fldv_UserName],
        [fldv_Password],
        [fldv_StakeholderTypeCode],
        [fldv_Organization],
        [fldv_SMSUserRoleCode],
        [fldb_IsActive],
        [fldd_LastLoginDate],
        [fldv_CreatedBy], 
        [fldd_CreatedDate], 
        [fldv_UpdatedBy], 
        [fldd_UpdatedDate]
    FROM [dbo].[tbld_SMSStakeholderUsers] 
    WHERE [fldv_Code] = @pID;

    -- Dataset 2: SMSUserRole data
    SELECT 
        [fldi_Id],
        [fldv_Code],
        [fldv_Name]
    FROM [PDXSMS_V2].[dbo].[tbls_SMSUserRoles] 
    WHERE [fldv_Code] = @UserRoleCode;

    -- Dataset 3: SMSUserRolePermissions data
    SELECT 
        [fldi_Id],
        [fldv_Code],
        [fldv_SMSUserRoleCode],
        [fldv_Module],
        [fldb_Create],
        [fldb_Read],
        [fldb_Update],
        [fldb_Delete]
    FROM [PDXSMS_V2].[dbo].[tbls_SMSUserRolePermissions] 
    WHERE [fldv_SMSUserRoleCode] = @UserRoleCode;

END
GO

-- =============================================
-- 8. Get SMS Stakeholder Users by Access Level (LEGACY - REMOVED AccessLevel field)
-- Note: This procedure may need to be updated or removed as AccessLevel 
-- has been replaced with SMSUserRoleCode in the new architecture
-- =============================================
/*
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[pr_SMSStakeholderUser_GetByAccessLevel]
    @pAccessLevel NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- NOTE: AccessLevel field has been replaced with SMSUserRoleCode
    -- This procedure may need to be updated to work with role-based access
    -- or deprecated in favor of role-based queries
    
    SELECT 
        [fldi_ID],
        [fldv_Code],
        [fldv_FirstName],
        [fldv_LastName], 
        [fldv_UserName],
        [fldv_Password],
        [fldv_StakeholderTypeCode],
        [fldv_Organization],
        [fldv_SMSUserRoleCode], -- Replaced AccessLevel
        [fldb_IsActive],
        [fldd_LastLoginDate],
        [fldv_CreatedBy], 
        [fldd_CreatedDate], 
        [fldv_UpdatedBy], 
        [fldd_UpdatedDate]
    FROM [dbo].[tbld_SMSStakeholderUsers]
    -- WHERE [fldv_AccessLevel] = @pAccessLevel -- Field no longer exists
    WHERE [fldv_SMSUserRoleCode] = @pAccessLevel -- Use role code instead
    ORDER BY [fldv_Organization], [fldv_LastName], [fldv_FirstName];
END
GO
*/

-- =============================================
-- SUMMARY
-- =============================================
/*
All stored procedures have been updated to support the new multi-dataset approach:

1. Dataset 1: Main SMSStakeholderUser entities
2. Dataset 2: Related SMSUserRole entities  
3. Dataset 3: Related SMSUserRolePermissions collections

Key Changes Made:
- Updated field names to match new schema (fldv_StakeholderTypeCode, fldv_SMSUserRoleCode)
- Added multi-dataset support following the template pattern
- Optimized queries to only return relevant roles and permissions
- Maintained ordering and filtering logic from original procedures
- Added error handling where appropriate

Usage in C# Repository:
```csharp
// Dataset 1: Main entity
if (await reader.ReadAsync().ConfigureAwait(false))
{
    user = Mappers.MapToSMSStakeholderUser(reader);
}

// Dataset 2: Related entity
if (user != null && await reader.NextResultAsync().ConfigureAwait(false) && await reader.ReadAsync().ConfigureAwait(false))
{
    user.UserRole = Mappers.MapToSMSUserRole(reader);
}

// Dataset 3: Collection of related entities
if (user?.UserRole != null && await reader.NextResultAsync().ConfigureAwait(false))
{
    var permissions = new List<SMSUserRolePermission>();
    while (await reader.ReadAsync().ConfigureAwait(false))
    {
        permissions.Add(Mappers.MapToSMSUserRolePermission(reader));
    }
    user.UserRole.Permissions = permissions;
}
```
*/