USE [PDXSMS_V2]
GO

-- =============================================
-- Procedure: pr_SMSApplicationUserGroup_Assign
-- Description: Assign a user to an application group
-- =============================================
CREATE PROCEDURE [dbo].[pr_SMSApplicationUserGroup_Assign]
    @pUserCode VARCHAR(60),
    @pGroupCode VARCHAR(60),
    @pAssignedBy VARCHAR(50) = 'SYSTEM',
    @pAssignedDate DATETIME = NULL,
    @pNewID INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(50) = COALESCE(@pAssignedBy, 'SYSTEM'), @FunctionName VARCHAR(50) = 'pr_SMSApplicationUserGroup_Assign', @RowsAffected INT;
    IF @pAssignedDate IS NULL SET @pAssignedDate = GETDATE();
    
    BEGIN TRY
        SET @AuditMessage = 'Starting Application User-Group assignment for User: ' + @pUserCode + ', Group: ' + @pGroupCode;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        BEGIN TRANSACTION;
        
        -- Check if user exists
        IF NOT EXISTS (SELECT 1 FROM [dbo].[tbld_SMSApplicationUsers] WHERE [fldv_Code] = @pUserCode)
        BEGIN
            RAISERROR('Application user not found: %s', 16, 1, @pUserCode);
        END
        
        -- Check if group exists
        IF NOT EXISTS (SELECT 1 FROM [dbo].[tbld_SMSApplicationGroups] WHERE [fldv_Code] = @pGroupCode)
        BEGIN
            RAISERROR('Application group not found: %s', 16, 1, @pGroupCode);
        END
        
        -- Check if assignment already exists
        IF EXISTS (SELECT 1 FROM [dbo].[tblr_ApplicationUserGroups] WHERE [fldv_UserCode] = @pUserCode AND [fldv_GroupCode] = @pGroupCode)
        BEGIN
            RAISERROR('User %s is already assigned to group %s', 16, 1, @pUserCode, @pGroupCode);
        END
        
        INSERT INTO [dbo].[tblr_ApplicationUserGroups] (
            [fldv_UserCode],
            [fldv_GroupCode],
            [fldd_AssignedDate],
            [fldv_AssignedBy],
            [fldv_CreatedBy],
            [fldd_CreatedDate]
        )
        VALUES (
            @pUserCode,
            @pGroupCode,
            @pAssignedDate,
            @UserID,
            @UserID,
            @pAssignedDate
        );
        
        SET @pNewID = SCOPE_IDENTITY();
        SET @RowsAffected = @@ROWCOUNT;
        
        COMMIT TRANSACTION;
        
        SET @AuditMessage = 'Successfully assigned User: ' + @pUserCode + ' to Group: ' + @pGroupCode + ', ID: ' + CAST(@pNewID AS VARCHAR(10)) + ', Rows affected: ' + CAST(@RowsAffected AS VARCHAR(10));
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error assigning User ' + @pUserCode + ' to Group ' + @pGroupCode + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

-- =============================================
-- Procedure: pr_SMSApplicationUserGroup_Remove
-- Description: Remove a user from an application group
-- =============================================
CREATE PROCEDURE [dbo].[pr_SMSApplicationUserGroup_Remove]
    @pUserCode VARCHAR(60),
    @pGroupCode VARCHAR(60),
    @pRemovedBy VARCHAR(50) = 'SYSTEM',
    @pRowsAffected INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(50) = COALESCE(@pRemovedBy, 'SYSTEM'), @FunctionName VARCHAR(50) = 'pr_SMSApplicationUserGroup_Remove';
    
    BEGIN TRY
        SET @AuditMessage = 'Starting Application User-Group removal for User: ' + @pUserCode + ', Group: ' + @pGroupCode;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        BEGIN TRANSACTION;
        
        -- Check if assignment exists
        IF NOT EXISTS (SELECT 1 FROM [dbo].[tblr_ApplicationUserGroups] WHERE [fldv_UserCode] = @pUserCode AND [fldv_GroupCode] = @pGroupCode)
        BEGIN
            RAISERROR('User %s is not assigned to group %s', 16, 1, @pUserCode, @pGroupCode);
        END
        
        DELETE FROM [dbo].[tblr_ApplicationUserGroups]
        WHERE [fldv_UserCode] = @pUserCode AND [fldv_GroupCode] = @pGroupCode;
        
        SET @pRowsAffected = @@ROWCOUNT;
        
        COMMIT TRANSACTION;
        
        SET @AuditMessage = 'Successfully removed User: ' + @pUserCode + ' from Group: ' + @pGroupCode + ', Rows affected: ' + CAST(@pRowsAffected AS VARCHAR(10));
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error removing User ' + @pUserCode + ' from Group ' + @pGroupCode + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

-- =============================================
-- Procedure: pr_SMSApplicationUserGroup_GetUsersByGroup
-- Description: Get all users assigned to a specific application group
-- =============================================
CREATE PROCEDURE [dbo].[pr_SMSApplicationUserGroup_GetUsersByGroup]
    @pGroupCode VARCHAR(60),
    @pRequestedBy VARCHAR(50) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(50) = COALESCE(@pRequestedBy, 'SYSTEM'), @FunctionName VARCHAR(50) = 'pr_SMSApplicationUserGroup_GetUsersByGroup';
    DECLARE @RecordCount INT;
    
    BEGIN TRY
        SET @AuditMessage = 'Starting retrieval of users for Application Group: ' + @pGroupCode;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        SELECT 
            u.[fldv_Code] AS fldv_Code,
            u.[fldv_FirstName] AS fldv_FirstName,
            u.[fldv_LastName] AS fldv_LastName,
            u.[fldv_UserName] AS fldv_UserName,
            u.[fldv_Password] AS fldv_Password,
            u.[fldv_SMSUserTypeCode] AS fldv_SMSUserTypeCode,
            u.[fldv_SMSUserRoleCode] AS fldv_SMSUserRoleCode,
            u.[fldb_IsActive] AS fldb_IsActive,
            u.[fldd_LastLoginDate] AS fldd_LastLoginDate,
            u.[fldv_CreatedBy] AS fldv_CreatedBy,
            u.[fldd_CreatedDate] AS fldd_CreatedDate,
            u.[fldv_UpdatedBy] AS fldv_UpdatedBy,
            u.[fldd_UpdatedDate] AS fldd_UpdatedDate,
            ug.[fldv_GroupCode] AS GroupCode,
            ug.[fldd_AssignedDate] AS AssignedDate,
            ug.[fldv_AssignedBy] AS AssignedBy
        FROM [dbo].[tblr_ApplicationUserGroups] ug
        INNER JOIN [dbo].[tbld_SMSApplicationUsers] u ON ug.[fldv_UserCode] = u.[fldv_Code]
        WHERE ug.[fldv_GroupCode] = @pGroupCode
        ORDER BY u.[fldv_LastName], u.[fldv_FirstName];
        
        SET @RecordCount = @@ROWCOUNT;
        
        SET @AuditMessage = 'Successfully retrieved ' + CAST(@RecordCount AS VARCHAR(10)) + ' users for Application Group: ' + @pGroupCode;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE(), @ErrorSeverity INT = ERROR_SEVERITY(), @ErrorState INT = ERROR_STATE();
        SET @AuditMessage = 'Error retrieving users for Application Group ' + @pGroupCode + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

-- =============================================
-- Procedure: pr_SMSApplicationUserGroup_ClearUserGroups
-- Description: Clear all group memberships for a specific user
-- =============================================
CREATE PROCEDURE [dbo].[pr_SMSApplicationUserGroup_ClearUserGroups]
    @pUserCode VARCHAR(60),
    @pClearedBy VARCHAR(50) = 'SYSTEM',
    @pRowsAffected INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(50) = COALESCE(@pClearedBy, 'SYSTEM'), @FunctionName VARCHAR(50) = 'pr_SMSApplicationUserGroup_ClearUserGroups';
    DECLARE @GroupCount INT;
    
    BEGIN TRY
        SET @AuditMessage = 'Starting clearing all group memberships for User: ' + @pUserCode;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        BEGIN TRANSACTION;
        
        -- Check if user exists
        IF NOT EXISTS (SELECT 1 FROM [dbo].[tbld_SMSApplicationUsers] WHERE [fldv_Code] = @pUserCode)
        BEGIN
            RAISERROR('Application user not found: %s', 16, 1, @pUserCode);
        END
        
        -- Get count of current group memberships
        SELECT @GroupCount = COUNT(*)
        FROM [dbo].[tblr_ApplicationUserGroups]
        WHERE [fldv_UserCode] = @pUserCode;
        
        DELETE FROM [dbo].[tblr_ApplicationUserGroups]
        WHERE [fldv_UserCode] = @pUserCode;
        
        SET @pRowsAffected = @@ROWCOUNT;
        
        COMMIT TRANSACTION;
        
        SET @AuditMessage = 'Successfully cleared ' + CAST(@pRowsAffected AS VARCHAR(10)) + ' group memberships for User: ' + @pUserCode + ' (was in ' + CAST(@GroupCount AS VARCHAR(10)) + ' groups)';
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error clearing group memberships for User ' + @pUserCode + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

-- =============================================
-- Procedure: pr_SMSApplicationUserGroup_GetGroupsByUser  
-- Description: Get all groups that a specific user is assigned to (optional utility procedure)
-- =============================================
CREATE PROCEDURE [dbo].[pr_SMSApplicationUserGroup_GetGroupsByUser]
    @pUserCode VARCHAR(60),
    @pRequestedBy VARCHAR(50) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(50) = COALESCE(@pRequestedBy, 'SYSTEM'), @FunctionName VARCHAR(50) = 'pr_SMSApplicationUserGroup_GetGroupsByUser';
    DECLARE @RecordCount INT;
    
    BEGIN TRY
        SET @AuditMessage = 'Starting retrieval of groups for Application User: ' + @pUserCode;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        SELECT 
            g.[fldv_Code] AS Code,
            g.[fldv_GroupName] AS Name,
            g.[fldv_Description] AS Description,
            g.[fldb_IsActive] AS IsActive,
            g.[fldv_CreatedBy] AS CreatedBy,
            g.[fldd_CreatedDate] AS CreatedDate,
            g.[fldv_UpdatedBy] AS UpdatedBy,
            g.[fldd_UpdatedDate] AS UpdatedDate,
            ug.[fldd_AssignedDate] AS AssignedDate,
            ug.[fldv_AssignedBy] AS AssignedBy
        FROM [dbo].[tblr_ApplicationUserGroups] ug
        INNER JOIN [dbo].[tbld_SMSApplicationGroups] g ON ug.[fldv_GroupCode] = g.[fldv_Code]
        WHERE ug.[fldv_UserCode] = @pUserCode
        ORDER BY g.[fldv_GroupName];
        
        SET @RecordCount = @@ROWCOUNT;
        
        SET @AuditMessage = 'Successfully retrieved ' + CAST(@RecordCount AS VARCHAR(10)) + ' groups for Application User: ' + @pUserCode;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE(), @ErrorSeverity INT = ERROR_SEVERITY(), @ErrorState INT = ERROR_STATE();
        SET @AuditMessage = 'Error retrieving groups for Application User ' + @pUserCode + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO