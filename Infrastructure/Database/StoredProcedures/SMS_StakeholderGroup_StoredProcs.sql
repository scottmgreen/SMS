-- =============================================
-- SMS Stakeholder Group Management Stored Procedures
-- Tables: tbld_SMSStakeholderGroups, tblr_SMSStakeholderUserGroups
-- =============================================

USE [PDXSMS_V2]
GO

-- =============================================
-- Procedure: pr_SMSStakeholderGroup_Insert
-- Description: Create a new stakeholder group
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[pr_SMSStakeholderGroup_Insert]
    @pSMSStakeholderGroupCode VARCHAR(60),
    @pSMSStakeholderGroupName VARCHAR(100),
    @pSMSStakeholderGroupDescription VARCHAR(500) = NULL,
    @pCreatedBy VARCHAR(50) = 'SYSTEM',
    @pCreatedDate DATETIME = NULL,
    @pNewID INT OUTPUT,
    @pNewGroupCode VARCHAR(60) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(50) = COALESCE(@pCreatedBy, 'SYSTEM'), @FunctionName VARCHAR(50) = 'pr_SMSStakeholderGroup_Insert', @RowsAffected INT;
    IF @pCreatedDate IS NULL SET @pCreatedDate = GETDATE();
    
    BEGIN TRY
        SET @AuditMessage = 'Starting Stakeholder Group creation for Code: ' + @pSMSStakeholderGroupCode;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        BEGIN TRANSACTION;
        
        -- Check for duplicate code
        IF EXISTS (SELECT 1 FROM [dbo].[tbld_SMSStakeholderGroups] WHERE [fldv_Code] = @pSMSStakeholderGroupCode)
        BEGIN
            RAISERROR('Stakeholder group code already exists: %s', 16, 1, @pSMSStakeholderGroupCode);
        END
        
        -- Check for duplicate group name
        IF EXISTS (SELECT 1 FROM [dbo].[tbld_SMSStakeholderGroups] WHERE [fldv_GroupName] = @pSMSStakeholderGroupName)
        BEGIN
            RAISERROR('Stakeholder group name already exists: %s', 16, 1, @pSMSStakeholderGroupName);
        END
        
        INSERT INTO [dbo].[tbld_SMSStakeholderGroups] (
            [fldv_Code],
            [fldv_GroupName], 
            [fldv_Description],
            [fldb_IsActive],
            [fldv_CreatedBy],
            [fldd_CreatedDate]
        )
        VALUES (
            @pSMSStakeholderGroupCode,
            @pSMSStakeholderGroupName,
            @pSMSStakeholderGroupDescription,
            1, -- Active by default
            @UserID,
            @pCreatedDate
        );
        
        SET @pNewID = SCOPE_IDENTITY();
        SET @pNewGroupCode = @pSMSStakeholderGroupCode;
        SET @RowsAffected = @@ROWCOUNT;
        
        COMMIT TRANSACTION;
        
        SET @AuditMessage = 'Successfully created Stakeholder Group Code: ' + @pSMSStakeholderGroupCode + ', ID: ' + CAST(@pNewID AS VARCHAR(10)) + ', Rows affected: ' + CAST(@RowsAffected AS VARCHAR(10));
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error creating Stakeholder Group Code ' + @pSMSStakeholderGroupCode + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

-- =============================================
-- Procedure: pr_SMSStakeholderGroup_Update  
-- Description: Update an existing stakeholder group
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[pr_SMSStakeholderGroup_Update]
    @pId VARCHAR(60),
    @pSMSStakeholderGroupName VARCHAR(100),
    @pSMSStakeholderGroupDescription VARCHAR(500) = NULL,
    @pSMSStakeholderGroupIsActive BIT,
    @pUpdatedBy VARCHAR(50) = 'SYSTEM',
    @pUpdatedDate DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(50) = COALESCE(@pUpdatedBy, 'SYSTEM'), @FunctionName VARCHAR(50) = 'pr_SMSStakeholderGroup_Update', @RowsAffected INT;
    IF @pUpdatedDate IS NULL SET @pUpdatedDate = GETDATE();
    
    BEGIN TRY
        SET @AuditMessage = 'Starting Stakeholder Group update for Code: ' + @pId;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        -- Check for duplicate group name (excluding current record)
        IF EXISTS (SELECT 1 FROM [dbo].[tbld_SMSStakeholderGroups] WHERE [fldv_GroupName] = @pSMSStakeholderGroupName AND [fldv_Code] <> @pId)
        BEGIN
            RAISERROR('Stakeholder group name already exists: %s', 16, 1, @pSMSStakeholderGroupName);
        END
        
        UPDATE [dbo].[tbld_SMSStakeholderGroups]
        SET 
            [fldv_GroupName] = @pSMSStakeholderGroupName,
            [fldv_Description] = @pSMSStakeholderGroupDescription,
            [fldb_IsActive] = @pSMSStakeholderGroupIsActive,
            [fldv_UpdatedBy] = @UserID,
            [fldd_UpdatedDate] = @pUpdatedDate
        WHERE [fldv_Code] = @pId;
        
        SET @RowsAffected = @@ROWCOUNT;
        
        IF @RowsAffected = 0
        BEGIN
            RAISERROR('Stakeholder group not found: %s', 16, 1, @pId);
        END
        
        SET @AuditMessage = 'Successfully updated Stakeholder Group Code: ' + @pId + ', Rows affected: ' + CAST(@RowsAffected AS VARCHAR(10));
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error updating Stakeholder Group Code ' + @pId + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

-- =============================================
-- Procedure: pr_SMSStakeholderGroup_Delete
-- Description: Delete a stakeholder group (soft delete)
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[pr_SMSStakeholderGroup_Delete]
    @pId VARCHAR(60),
    @pUserId VARCHAR(50) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(50) = COALESCE(@pUserId, 'SYSTEM'), @FunctionName VARCHAR(50) = 'pr_SMSStakeholderGroup_Delete', @RowsAffected INT;
    
    BEGIN TRY
        SET @AuditMessage = 'Starting Stakeholder Group deletion for Code: ' + @pId;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        BEGIN TRANSACTION;
        
        -- Soft delete the group
        UPDATE [dbo].[tbld_SMSStakeholderGroups]
        SET 
            [fldb_IsActive] = 0,
            [fldv_UpdatedBy] = @UserID,
            [fldd_UpdatedDate] = GETDATE()
        WHERE [fldv_Code] = @pId;
        
        SET @RowsAffected = @@ROWCOUNT;
        
        IF @RowsAffected = 0
        BEGIN
            RAISERROR('Stakeholder group not found: %s', 16, 1, @pId);
        END
            
        -- Remove all user-group associations (hard delete since it's a relationship table)
        DELETE FROM [dbo].[tblr_SMSStakeholderUserGroups] 
        WHERE [fldv_GroupCode] = @pId;
        
        COMMIT TRANSACTION;
        
        SET @AuditMessage = 'Successfully deleted Stakeholder Group Code: ' + @pId + ', Rows affected: ' + CAST(@RowsAffected AS VARCHAR(10));
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error deleting Stakeholder Group Code ' + @pId + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

-- =============================================
-- Procedure: pr_SMSStakeholderGroup_GetAll
-- Description: Get all stakeholder groups
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[pr_SMSStakeholderGroup_GetAll]
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(50) = 'SYSTEM', @FunctionName VARCHAR(50) = 'pr_SMSStakeholderGroup_GetAll';
    
    BEGIN TRY
        SET @AuditMessage = 'Retrieving all Stakeholder Groups';
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        SELECT 
            [fldv_Code],
            [fldv_GroupName],
            [fldv_Description],
            [fldb_IsActive],
            [fldv_CreatedBy],
            [fldd_CreatedDate],
            [fldv_UpdatedBy],
            [fldd_UpdatedDate]
        FROM [dbo].[tbld_SMSStakeholderGroups]
        ORDER BY [fldv_GroupName];
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        SET @AuditMessage = 'Error retrieving Stakeholder Groups: ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        THROW;
    END CATCH
END;
GO

-- =============================================
-- Procedure: pr_SMSStakeholderGroup_GetByCode
-- Description: Get a stakeholder group by code
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[pr_SMSStakeholderGroup_GetByCode]
    @pCode VARCHAR(60),
    @pUserId VARCHAR(50) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(50) = COALESCE(@pUserId, 'SYSTEM'), @FunctionName VARCHAR(50) = 'pr_SMSStakeholderGroup_GetByCode';
    
    BEGIN TRY
        SET @AuditMessage = 'Retrieving Stakeholder Group by Code: ' + @pCode;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        SELECT 
            [fldv_Code],
            [fldv_GroupName],
            [fldv_Description],
            [fldb_IsActive],
            [fldv_CreatedBy],
            [fldd_CreatedDate],
            [fldv_UpdatedBy],
            [fldd_UpdatedDate]
        FROM [dbo].[tbld_SMSStakeholderGroups]
        WHERE [fldv_Code] = @pCode;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        SET @AuditMessage = 'Error retrieving Stakeholder Group by Code ' + @pCode + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        THROW;
    END CATCH
END;
GO

-- =============================================
-- Procedure: pr_SMSStakeholderGroup_GetByUserID
-- Description: Get stakeholder groups for a specific user
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[pr_SMSStakeholderGroup_GetByUserID]
    @pSMSStakeholderUserCode VARCHAR(60),
    @pUserId VARCHAR(50) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(50) = COALESCE(@pUserId, 'SYSTEM'), @FunctionName VARCHAR(50) = 'pr_SMSStakeholderGroup_GetByUserID';
    
    BEGIN TRY
        SET @AuditMessage = 'Retrieving Stakeholder Groups for User Code: ' + @pSMSStakeholderUserCode;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        SELECT 
            g.[fldv_Code],
            g.[fldv_GroupName],
            g.[fldv_Description],
            g.[fldb_IsActive],
            g.[fldv_CreatedBy],
            g.[fldd_CreatedDate],
            g.[fldv_UpdatedBy],
            g.[fldd_UpdatedDate]
        FROM [dbo].[tbld_SMSStakeholderGroups] g
        INNER JOIN [dbo].[tblr_SMSStakeholderUserGroups] ug ON g.[fldv_Code] = ug.[fldv_GroupCode]
        WHERE ug.[fldv_UserCode] = @pSMSStakeholderUserCode
          AND g.[fldb_IsActive] = 1
        ORDER BY g.[fldv_GroupName];
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        SET @AuditMessage = 'Error retrieving Stakeholder Groups for User Code ' + @pSMSStakeholderUserCode + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        THROW;
    END CATCH
END;
GO