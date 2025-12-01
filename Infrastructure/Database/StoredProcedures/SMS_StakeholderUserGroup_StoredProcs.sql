-- =============================================
-- SMS Stakeholder User-Group Junction Table Stored Procedures
-- Tables: tblr_SMSStakeholderUserGroups (relationship table)
-- =============================================

USE [PDXSMS_V2]
GO

-- =============================================
-- Procedure: pr_SMSStakeholderUserGroup_Insert
-- Description: Create a new user-group association
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[pr_SMSStakeholderUserGroup_Insert]
    @pSMSStakeholderUserCode VARCHAR(60),
    @pSMSStakeholderGroupCode VARCHAR(60),
    @pAssignedBy VARCHAR(50) = 'SYSTEM',
    @pAssignedDate DATETIME = NULL,
    @pNewID INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(50) = COALESCE(@pAssignedBy, 'SYSTEM'), @FunctionName VARCHAR(50) = 'pr_SMSStakeholderUserGroup_Insert', @RowsAffected INT;
    IF @pAssignedDate IS NULL SET @pAssignedDate = GETDATE();
    
    BEGIN TRY
        SET @AuditMessage = 'Starting User-Group assignment: User=' + @pSMSStakeholderUserCode + ', Group=' + @pSMSStakeholderGroupCode;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        -- Check if association already exists
        IF EXISTS (
            SELECT 1 FROM [dbo].[tblr_SMSStakeholderUserGroups] 
            WHERE [fldv_UserCode] = @pSMSStakeholderUserCode 
              AND [fldv_GroupCode] = @pSMSStakeholderGroupCode
        )
        BEGIN
            RAISERROR('User is already assigned to this group: User=%s, Group=%s', 16, 1, @pSMSStakeholderUserCode, @pSMSStakeholderGroupCode);
        END
        
        INSERT INTO [dbo].[tblr_SMSStakeholderUserGroups] (
            [fldv_UserCode],
            [fldv_GroupCode],
            [fldd_AssignedDate],
            [fldv_AssignedBy],
            [fldv_CreatedBy],
            [fldd_CreatedDate]
        )
        VALUES (
            @pSMSStakeholderUserCode,
            @pSMSStakeholderGroupCode,
            @pAssignedDate,
            @UserID,
            @UserID,
            @pAssignedDate
        );
        
        SET @pNewID = SCOPE_IDENTITY();
        SET @RowsAffected = @@ROWCOUNT;
        
        SET @AuditMessage = 'Successfully assigned User=' + @pSMSStakeholderUserCode + ' to Group=' + @pSMSStakeholderGroupCode + ', ID: ' + CAST(@pNewID AS VARCHAR(10));
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error assigning User=' + @pSMSStakeholderUserCode + ' to Group=' + @pSMSStakeholderGroupCode + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

-- =============================================
-- Procedure: pr_SMSStakeholderUserGroup_Update
-- Description: Update user-group association (mainly for audit fields)
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[pr_SMSStakeholderUserGroup_Update]
    @pId INT,
    @pAssignedBy VARCHAR(50) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(50) = COALESCE(@pAssignedBy, 'SYSTEM'), @FunctionName VARCHAR(50) = 'pr_SMSStakeholderUserGroup_Update', @RowsAffected INT;
    
    BEGIN TRY
        SET @AuditMessage = 'Starting User-Group assignment update for ID: ' + CAST(@pId AS VARCHAR(10));
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        UPDATE [dbo].[tblr_SMSStakeholderUserGroups]
        SET 
            [fldv_AssignedBy] = @UserID,
            [fldd_AssignedDate] = GETDATE(),
            [fldv_UpdatedBy] = @UserID,
            [fldd_UpdatedDate] = GETDATE()
        WHERE [fldi_ID] = @pId;
        
        SET @RowsAffected = @@ROWCOUNT;
        
        IF @RowsAffected = 0
        BEGIN
            RAISERROR('User-group association not found: ID=%d', 16, 1, @pId);
        END
        
        SET @AuditMessage = 'Successfully updated User-Group assignment ID: ' + CAST(@pId AS VARCHAR(10)) + ', Rows affected: ' + CAST(@RowsAffected AS VARCHAR(10));
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error updating User-Group assignment ID ' + CAST(@pId AS VARCHAR(10)) + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

-- =============================================
-- Procedure: pr_SMSStakeholderUserGroup_Delete
-- Description: Remove user-group association
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[pr_SMSStakeholderUserGroup_Delete]
    @pId INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(50) = 'SYSTEM', @FunctionName VARCHAR(50) = 'pr_SMSStakeholderUserGroup_Delete', @RowsAffected INT;
    
    BEGIN TRY
        SET @AuditMessage = 'Starting User-Group assignment deletion for ID: ' + CAST(@pId AS VARCHAR(10));
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        DELETE FROM [dbo].[tblr_SMSStakeholderUserGroups]
        WHERE [fldi_ID] = @pId;
        
        SET @RowsAffected = @@ROWCOUNT;
        
        IF @RowsAffected = 0
        BEGIN
            RAISERROR('User-group association not found: ID=%d', 16, 1, @pId);
        END
        
        SET @AuditMessage = 'Successfully deleted User-Group assignment ID: ' + CAST(@pId AS VARCHAR(10)) + ', Rows affected: ' + CAST(@RowsAffected AS VARCHAR(10));
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error deleting User-Group assignment ID ' + CAST(@pId AS VARCHAR(10)) + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

-- =============================================
-- Procedure: pr_SMSStakeholderUserGroup_GetAll
-- Description: Get all user-group associations
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[pr_SMSStakeholderUserGroup_GetAll]
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(50) = 'SYSTEM', @FunctionName VARCHAR(50) = 'pr_SMSStakeholderUserGroup_GetAll';
    
    BEGIN TRY
        SET @AuditMessage = 'Retrieving all User-Group associations';
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        SELECT 
            ug.[fldi_ID],
            ug.[fldv_UserCode],
            ug.[fldv_GroupCode],
            ug.[fldd_AssignedDate],
            ug.[fldv_AssignedBy],
            ug.[fldv_CreatedBy],
            ug.[fldd_CreatedDate],
            ug.[fldv_UpdatedBy],
            ug.[fldd_UpdatedDate],
            u.[fldv_FirstName],
            u.[fldv_LastName],
            u.[fldv_Organization],
            g.[fldv_GroupName]
        FROM [dbo].[tblr_SMSStakeholderUserGroups] ug
        INNER JOIN [dbo].[tbld_SMSStakeholderUsers] u ON ug.[fldv_UserCode] = u.[fldv_Code]
        INNER JOIN [dbo].[tbld_SMSStakeholderGroups] g ON ug.[fldv_GroupCode] = g.[fldv_Code]
        WHERE g.[fldb_IsActive] = 1 
          AND u.[fldb_IsActive] = 1
        ORDER BY g.[fldv_GroupName], u.[fldv_LastName], u.[fldv_FirstName];
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        SET @AuditMessage = 'Error retrieving User-Group associations: ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        THROW;
    END CATCH
END;
GO

-- =============================================
-- Procedure: pr_SMSStakeholderUserGroup_GetByUserID
-- Description: Get all group associations for a specific user
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[pr_SMSStakeholderUserGroup_GetByUserID]
    @pSMSStakeholderUserCode VARCHAR(60)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(50) = 'SYSTEM', @FunctionName VARCHAR(50) = 'pr_SMSStakeholderUserGroup_GetByUserID';
    
    BEGIN TRY
        SET @AuditMessage = 'Retrieving Group associations for User Code: ' + @pSMSStakeholderUserCode;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        SELECT 
            ug.[fldi_ID],
            ug.[fldv_UserCode],
            ug.[fldv_GroupCode],
            ug.[fldd_AssignedDate],
            ug.[fldv_AssignedBy],
            ug.[fldv_CreatedBy],
            ug.[fldd_CreatedDate],
            ug.[fldv_UpdatedBy],
            ug.[fldd_UpdatedDate],
            g.[fldv_GroupName],
            g.[fldv_Description]
        FROM [dbo].[tblr_SMSStakeholderUserGroups] ug
        INNER JOIN [dbo].[tbld_SMSStakeholderGroups] g ON ug.[fldv_GroupCode] = g.[fldv_Code]
        WHERE ug.[fldv_UserCode] = @pSMSStakeholderUserCode
          AND g.[fldb_IsActive] = 1
        ORDER BY g.[fldv_GroupName];
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        SET @AuditMessage = 'Error retrieving Group associations for User Code ' + @pSMSStakeholderUserCode + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        THROW;
    END CATCH
END;
GO

-- =============================================
-- Procedure: pr_SMSStakeholderUserGroup_GetByGroupCode  
-- Description: Get all user associations for a specific group
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[pr_SMSStakeholderUserGroup_GetByGroupCode]
    @pSMSStakeholderGroupCode VARCHAR(60)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(50) = 'SYSTEM', @FunctionName VARCHAR(50) = 'pr_SMSStakeholderUserGroup_GetByGroupCode';
    
    BEGIN TRY
        SET @AuditMessage = 'Retrieving User associations for Group Code: ' + @pSMSStakeholderGroupCode;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        SELECT 
            ug.[fldi_ID],
            ug.[fldv_UserCode],
            ug.[fldv_GroupCode],
            ug.[fldd_AssignedDate],
            ug.[fldv_AssignedBy],
            ug.[fldv_CreatedBy],
            ug.[fldd_CreatedDate],
            ug.[fldv_UpdatedBy],
            ug.[fldd_UpdatedDate],
            u.[fldv_FirstName],
            u.[fldv_LastName],
            u.[fldv_UserName],
            u.[fldv_Organization],
            u.[fldv_StakeholderTypeCode]
        FROM [dbo].[tblr_SMSStakeholderUserGroups] ug
        INNER JOIN [dbo].[tbld_SMSStakeholderUsers] u ON ug.[fldv_UserCode] = u.[fldv_Code]
        WHERE ug.[fldv_GroupCode] = @pSMSStakeholderGroupCode
          AND u.[fldb_IsActive] = 1
        ORDER BY u.[fldv_LastName], u.[fldv_FirstName];
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        SET @AuditMessage = 'Error retrieving User associations for Group Code ' + @pSMSStakeholderGroupCode + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        THROW;
    END CATCH
END;
GO

-- =============================================
-- Procedure: pr_SMSStakeholderUserGroup_Assign
-- Description: Assign a user to a group (with duplicate check)
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[pr_SMSStakeholderUserGroup_Assign]
    @pSMSStakeholderUserCode VARCHAR(60),
    @pSMSStakeholderGroupCode VARCHAR(60),
    @pAssignedBy VARCHAR(50) = 'SYSTEM',
    @pAssignedDate DATETIME = NULL,
    @pNewID INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(50) = COALESCE(@pAssignedBy, 'SYSTEM'), @FunctionName VARCHAR(50) = 'pr_SMSStakeholderUserGroup_Assign', @RowsAffected INT;
    IF @pAssignedDate IS NULL SET @pAssignedDate = GETDATE();
    
    BEGIN TRY
        SET @AuditMessage = 'Starting User assignment: User=' + @pSMSStakeholderUserCode + ', Group=' + @pSMSStakeholderGroupCode;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        -- Validate user exists and is active
        IF NOT EXISTS (
            SELECT 1 FROM [dbo].[tbld_SMSStakeholderUsers] 
            WHERE [fldv_Code] = @pSMSStakeholderUserCode AND [fldb_IsActive] = 1
        )
        BEGIN
            RAISERROR('User not found or inactive: %s', 16, 1, @pSMSStakeholderUserCode);
        END
        
        -- Validate group exists and is active
        IF NOT EXISTS (
            SELECT 1 FROM [dbo].[tbld_SMSStakeholderGroups] 
            WHERE [fldv_Code] = @pSMSStakeholderGroupCode AND [fldb_IsActive] = 1
        )
        BEGIN
            RAISERROR('Group not found or inactive: %s', 16, 1, @pSMSStakeholderGroupCode);
        END
        
        -- Check if association already exists
        IF EXISTS (
            SELECT 1 FROM [dbo].[tblr_SMSStakeholderUserGroups] 
            WHERE [fldv_UserCode] = @pSMSStakeholderUserCode 
              AND [fldv_GroupCode] = @pSMSStakeholderGroupCode
        )
        BEGIN
            -- User already assigned, just return the existing ID
            SELECT @pNewID = [fldi_ID] 
            FROM [dbo].[tblr_SMSStakeholderUserGroups] 
            WHERE [fldv_UserCode] = @pSMSStakeholderUserCode 
              AND [fldv_GroupCode] = @pSMSStakeholderGroupCode;
            
            SET @AuditMessage = 'User already assigned to group: User=' + @pSMSStakeholderUserCode + ', Group=' + @pSMSStakeholderGroupCode + ', ID: ' + CAST(@pNewID AS VARCHAR(10));
            EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
            RETURN;
        END
        
        -- Create new association
        INSERT INTO [dbo].[tblr_SMSStakeholderUserGroups] (
            [fldv_UserCode],
            [fldv_GroupCode],
            [fldd_AssignedDate],
            [fldv_AssignedBy],
            [fldv_CreatedBy],
            [fldd_CreatedDate]
        )
        VALUES (
            @pSMSStakeholderUserCode,
            @pSMSStakeholderGroupCode,
            @pAssignedDate,
            @UserID,
            @UserID,
            @pAssignedDate
        );
        
        SET @pNewID = SCOPE_IDENTITY();
        SET @RowsAffected = @@ROWCOUNT;
        
        SET @AuditMessage = 'Successfully assigned User=' + @pSMSStakeholderUserCode + ' to Group=' + @pSMSStakeholderGroupCode + ', ID: ' + CAST(@pNewID AS VARCHAR(10));
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error assigning User=' + @pSMSStakeholderUserCode + ' to Group=' + @pSMSStakeholderGroupCode + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

-- =============================================
-- Procedure: pr_SMSStakeholderUserGroup_Remove
-- Description: Remove a user from a group
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[pr_SMSStakeholderUserGroup_Remove]
    @pSMSStakeholderUserCode VARCHAR(60),
    @pSMSStakeholderGroupCode VARCHAR(60),
    @pUserId VARCHAR(50) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(50) = COALESCE(@pUserId, 'SYSTEM'), @FunctionName VARCHAR(50) = 'pr_SMSStakeholderUserGroup_Remove', @RowsAffected INT;
    
    BEGIN TRY
        SET @AuditMessage = 'Starting User removal from Group: User=' + @pSMSStakeholderUserCode + ', Group=' + @pSMSStakeholderGroupCode;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        DELETE FROM [dbo].[tblr_SMSStakeholderUserGroups]
        WHERE [fldv_UserCode] = @pSMSStakeholderUserCode 
          AND [fldv_GroupCode] = @pSMSStakeholderGroupCode;
        
        SET @RowsAffected = @@ROWCOUNT;
        
        IF @RowsAffected = 0
        BEGIN
            RAISERROR('User-group association not found: User=%s, Group=%s', 16, 1, @pSMSStakeholderUserCode, @pSMSStakeholderGroupCode);
        END
        
        SET @AuditMessage = 'Successfully removed User=' + @pSMSStakeholderUserCode + ' from Group=' + @pSMSStakeholderGroupCode + ', Rows affected: ' + CAST(@RowsAffected AS VARCHAR(10));
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error removing User=' + @pSMSStakeholderUserCode + ' from Group=' + @pSMSStakeholderGroupCode + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

-- =============================================
-- Procedure: pr_SMSStakeholderUserGroup_GetUsersByGroup
-- Description: Get all users in a specific group (alias for GetByGroupCode)
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[pr_SMSStakeholderUserGroup_GetUsersByGroup]
    @pSMSStakeholderGroupCode VARCHAR(60)
AS
BEGIN
    SET NOCOUNT ON;
    
    EXEC [dbo].[pr_SMSStakeholderUserGroup_GetByGroupCode] @pSMSStakeholderGroupCode;
END;
GO

-- =============================================
-- Procedure: pr_SMSStakeholderUserGroup_ClearUserGroups
-- Description: Remove user from all groups
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[pr_SMSStakeholderUserGroup_ClearUserGroups]
    @pSMSStakeholderUserCode VARCHAR(60),
    @pUserId VARCHAR(50) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(50) = COALESCE(@pUserId, 'SYSTEM'), @FunctionName VARCHAR(50) = 'pr_SMSStakeholderUserGroup_ClearUserGroups', @RowsAffected INT;
    
    BEGIN TRY
        SET @AuditMessage = 'Starting to clear all Group assignments for User Code: ' + @pSMSStakeholderUserCode;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        DELETE FROM [dbo].[tblr_SMSStakeholderUserGroups]
        WHERE [fldv_UserCode] = @pSMSStakeholderUserCode;
        
        SET @RowsAffected = @@ROWCOUNT;
        
        SET @AuditMessage = 'Successfully cleared all Group assignments for User Code: ' + @pSMSStakeholderUserCode + ', Rows affected: ' + CAST(@RowsAffected AS VARCHAR(10));
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error clearing Group assignments for User Code ' + @pSMSStakeholderUserCode + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO