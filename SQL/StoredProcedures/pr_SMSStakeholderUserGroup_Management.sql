USE [PDXSMS_V2]
GO

-- =============================================
-- SMS STAKEHOLDER USER GROUP MEMBERSHIP OPERATIONS
-- Junction table management for user-group relationships
-- =============================================

/****** Object:  StoredProcedure [dbo].[pr_SMSStakeholderUserGroup_Assign] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[pr_SMSStakeholderUserGroup_Assign]
    @pUserCode VARCHAR(60),
    @pGroupCode VARCHAR(60),
    @pAssignedBy VARCHAR(50) = 'SYSTEM',
    @pAssignedDate DATETIME = NULL,
    @pNewID INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(10) = COALESCE(@pAssignedBy, 'SYSTEM'), @FunctionName VARCHAR(50) = 'pr_SMSStakeholderUserGroup_Assign';
    IF @pAssignedDate IS NULL SET @pAssignedDate = GETDATE();
    
    BEGIN TRY
        SET @AuditMessage = 'Starting user-group assignment: User=' + @pUserCode + ', Group=' + @pGroupCode;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_StakeholderUserGroups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        -- Check if assignment already exists
        IF EXISTS (SELECT 1 FROM [dbo].[tbld_SMSStakeholderUserGroups] WHERE [fldv_UserCode] = @pUserCode AND [fldv_GroupCode] = @pGroupCode)
        BEGIN
            SET @AuditMessage = 'User-group assignment already exists: User=' + @pUserCode + ', Group=' + @pGroupCode;
            EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Warning', @pModule = 'SMS_StakeholderUserGroups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
            
            SELECT @pNewID = [fldi_ID] FROM [dbo].[tbld_SMSStakeholderUserGroups] WHERE [fldv_UserCode] = @pUserCode AND [fldv_GroupCode] = @pGroupCode;
            RETURN;
        END
        
        INSERT INTO [dbo].[tbld_SMSStakeholderUserGroups] 
        ([fldv_UserCode], [fldv_GroupCode], [fldd_AssignedDate], [fldv_AssignedBy], [fldv_CreatedBy], [fldd_CreatedDate])
        VALUES (@pUserCode, @pGroupCode, @pAssignedDate, @pAssignedBy, @pAssignedBy, @pAssignedDate);
        
        SET @pNewID = SCOPE_IDENTITY();
        SET @AuditMessage = 'Successfully assigned user to group with ID: ' + CAST(@pNewID AS VARCHAR(10)) + ', User=' + @pUserCode + ', Group=' + @pGroupCode;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_StakeholderUserGroups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error assigning user to group: User=' + @pUserCode + ', Group=' + @pGroupCode + ', Error: ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_StakeholderUserGroups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

/****** Object:  StoredProcedure [dbo].[pr_SMSStakeholderUserGroup_Remove] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[pr_SMSStakeholderUserGroup_Remove]
    @pUserCode VARCHAR(60),
    @pGroupCode VARCHAR(60),
    @pUserID VARCHAR(10) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @FunctionName VARCHAR(50) = 'pr_SMSStakeholderUserGroup_Remove', @RowsAffected INT;
    
    BEGIN TRY
        SET @AuditMessage = 'Starting user-group removal: User=' + @pUserCode + ', Group=' + @pGroupCode;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Warning', @pModule = 'SMS_StakeholderUserGroups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        DELETE FROM [dbo].[tbld_SMSStakeholderUserGroups] 
        WHERE [fldv_UserCode] = @pUserCode AND [fldv_GroupCode] = @pGroupCode;
        
        SET @RowsAffected = @@ROWCOUNT;
        SET @AuditMessage = 'Successfully removed user from group: User=' + @pUserCode + ', Group=' + @pGroupCode + ', Rows affected: ' + CAST(@RowsAffected AS VARCHAR(10));
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Warning', @pModule = 'SMS_StakeholderUserGroups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error removing user from group: User=' + @pUserCode + ', Group=' + @pGroupCode + ', Error: ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_StakeholderUserGroups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

/****** Object:  StoredProcedure [dbo].[pr_SMSStakeholderUserGroup_GetUsersByGroup] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[pr_SMSStakeholderUserGroup_GetUsersByGroup]
    @pGroupCode VARCHAR(60),
    @pUserID VARCHAR(10) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @FunctionName VARCHAR(50) = 'pr_SMSStakeholderUserGroup_GetUsersByGroup', @RecordCount INT;
    
    BEGIN TRY
        SET @AuditMessage = 'Retrieving users for group: ' + @pGroupCode;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_StakeholderUserGroups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        SELECT su.[fldi_ID], su.[fldv_Code], su.[fldv_FirstName], su.[fldv_LastName], su.[fldv_UserName],
               su.[fldv_StakeholderType], su.[fldv_Organization], su.[fldv_AccessLevel], su.[fldb_IsActive],
               ug.[fldd_AssignedDate], ug.[fldv_AssignedBy]
        FROM [dbo].[tbld_SMSStakeholderUsers] su
        INNER JOIN [dbo].[tbld_SMSStakeholderUserGroups] ug ON su.[fldv_Code] = ug.[fldv_UserCode]
        WHERE ug.[fldv_GroupCode] = @pGroupCode
          AND su.[fldb_IsActive] = 1
        ORDER BY su.[fldv_LastName], su.[fldv_FirstName];
        
        SET @RecordCount = @@ROWCOUNT;
        SET @AuditMessage = 'Successfully retrieved ' + CAST(@RecordCount AS VARCHAR(10)) + ' users for group: ' + @pGroupCode;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_StakeholderUserGroups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error retrieving users for group ' + @pGroupCode + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_StakeholderUserGroups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

/****** Object:  StoredProcedure [dbo].[pr_SMSStakeholderUserGroup_ClearUserGroups] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[pr_SMSStakeholderUserGroup_ClearUserGroups]
    @pUserCode VARCHAR(60),
    @pUserID VARCHAR(10) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @FunctionName VARCHAR(50) = 'pr_SMSStakeholderUserGroup_ClearUserGroups', @RowsAffected INT;
    
    BEGIN TRY
        SET @AuditMessage = 'Starting to clear all group memberships for user: ' + @pUserCode;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Warning', @pModule = 'SMS_StakeholderUserGroups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        DELETE FROM [dbo].[tbld_SMSStakeholderUserGroups] WHERE [fldv_UserCode] = @pUserCode;
        
        SET @RowsAffected = @@ROWCOUNT;
        SET @AuditMessage = 'Successfully cleared all group memberships for user: ' + @pUserCode + ', Rows affected: ' + CAST(@RowsAffected AS VARCHAR(10));
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Warning', @pModule = 'SMS_StakeholderUserGroups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error clearing group memberships for user ' + @pUserCode + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_StakeholderUserGroups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

PRINT 'SMS Stakeholder User Group membership stored procedures created successfully!'