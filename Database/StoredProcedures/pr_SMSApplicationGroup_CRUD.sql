USE [PDXSMS_V2]
GO

-- =============================================
-- Procedure: pr_SMSApplicationGroup_Insert
-- Description: Create a new application group
-- =============================================
CREATE PROCEDURE [dbo].[pr_SMSApplicationGroup_Insert]
    @pCode VARCHAR(60),
    @pName VARCHAR(100),
    @pDescription VARCHAR(500) = NULL,
    @pCreatedBy VARCHAR(50) = 'SYSTEM',
    @pCreatedDate DATETIME = NULL,
    @pNewID INT OUTPUT,
    @pNewGroupCode VARCHAR(60) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(50) = COALESCE(@pCreatedBy, 'SYSTEM'), @FunctionName VARCHAR(50) = 'pr_SMSApplicationGroup_Insert', @RowsAffected INT;
    IF @pCreatedDate IS NULL SET @pCreatedDate = GETDATE();
    
    BEGIN TRY
        SET @AuditMessage = 'Starting Application Group creation for Code: ' + @pCode;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        BEGIN TRANSACTION;
        
        -- Check for duplicate code
        IF EXISTS (SELECT 1 FROM [dbo].[tbld_SMSApplicationGroups] WHERE [fldv_Code] = @pCode)
        BEGIN
            RAISERROR('Application group code already exists: %s', 16, 1, @pCode);
        END
        
        -- Check for duplicate group name
        IF EXISTS (SELECT 1 FROM [dbo].[tbld_SMSApplicationGroups] WHERE [fldv_GroupName] = @pName)
        BEGIN
            RAISERROR('Application group name already exists: %s', 16, 1, @pName);
        END
        
        INSERT INTO [dbo].[tbld_SMSApplicationGroups] (
            [fldv_Code],
            [fldv_GroupName], 
            [fldv_Description],
            [fldb_IsActive],
            [fldv_CreatedBy],
            [fldd_CreatedDate]
        )
        VALUES (
            @pCode,
            @pName,
            @pDescription,
            1, -- Active by default
            @UserID,
            @pCreatedDate
        );
        
        SET @pNewID = SCOPE_IDENTITY();
        SET @pNewGroupCode = @pCode;
        SET @RowsAffected = @@ROWCOUNT;
        
        COMMIT TRANSACTION;
        
        SET @AuditMessage = 'Successfully created Application Group Code: ' + @pCode + ', ID: ' + CAST(@pNewID AS VARCHAR(10)) + ', Rows affected: ' + CAST(@RowsAffected AS VARCHAR(10));
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error creating Application Group Code ' + @pCode + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

-- =============================================
-- Procedure: pr_SMSApplicationGroup_Update
-- Description: Update an existing application group
-- =============================================
CREATE PROCEDURE [dbo].[pr_SMSApplicationGroup_Update]
    @pCode VARCHAR(60),
    @pName VARCHAR(100),
    @pDescription VARCHAR(500) = NULL,
    @pIsActive BIT = 1,
    @pUpdatedBy VARCHAR(50) = 'SYSTEM',
    @pUpdatedDate DATETIME = NULL,
    @pRowsAffected INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(50) = COALESCE(@pUpdatedBy, 'SYSTEM'), @FunctionName VARCHAR(50) = 'pr_SMSApplicationGroup_Update';
    IF @pUpdatedDate IS NULL SET @pUpdatedDate = GETDATE();
    
    BEGIN TRY
        SET @AuditMessage = 'Starting Application Group update for Code: ' + @pCode;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        BEGIN TRANSACTION;
        
        -- Check if group exists
        IF NOT EXISTS (SELECT 1 FROM [dbo].[tbld_SMSApplicationGroups] WHERE [fldv_Code] = @pCode)
        BEGIN
            RAISERROR('Application group not found: %s', 16, 1, @pCode);
        END
        
        -- Check for duplicate group name (excluding current record)
        IF EXISTS (SELECT 1 FROM [dbo].[tbld_SMSApplicationGroups] WHERE [fldv_GroupName] = @pName AND [fldv_Code] <> @pCode)
        BEGIN
            RAISERROR('Application group name already exists: %s', 16, 1, @pName);
        END
        
        UPDATE [dbo].[tbld_SMSApplicationGroups] 
        SET 
            [fldv_GroupName] = @pName,
            [fldv_Description] = @pDescription,
            [fldb_IsActive] = @pIsActive,
            [fldv_UpdatedBy] = @UserID,
            [fldd_UpdatedDate] = @pUpdatedDate
        WHERE [fldv_Code] = @pCode;
        
        SET @pRowsAffected = @@ROWCOUNT;
        
        COMMIT TRANSACTION;
        
        SET @AuditMessage = 'Successfully updated Application Group Code: ' + @pCode + ', Rows affected: ' + CAST(@pRowsAffected AS VARCHAR(10));
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error updating Application Group Code ' + @pCode + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

-- =============================================
-- Procedure: pr_SMSApplicationGroup_Delete
-- Description: Delete an application group
-- =============================================
CREATE PROCEDURE [dbo].[pr_SMSApplicationGroup_Delete]
    @pCode VARCHAR(60),
    @pDeletedBy VARCHAR(50) = 'SYSTEM',
    @pRowsAffected INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(50) = COALESCE(@pDeletedBy, 'SYSTEM'), @FunctionName VARCHAR(50) = 'pr_SMSApplicationGroup_Delete';
    DECLARE @GroupName VARCHAR(100);
    
    BEGIN TRY
        SET @AuditMessage = 'Starting Application Group deletion for Code: ' + @pCode;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        BEGIN TRANSACTION;
        
        -- Check if group exists and get name for audit
        SELECT @GroupName = [fldv_GroupName] 
        FROM [dbo].[tbld_SMSApplicationGroups] 
        WHERE [fldv_Code] = @pCode;
        
        IF @GroupName IS NULL
        BEGIN
            RAISERROR('Application group not found: %s', 16, 1, @pCode);
        END
        
        -- Check for dependencies (users assigned to this group)
        IF EXISTS (SELECT 1 FROM [dbo].[tbld_SMSApplicationUsers] WHERE [fldv_GroupCode] = @pCode)
        BEGIN
            RAISERROR('Cannot delete application group %s: Users are still assigned to this group', 16, 1, @pCode);
        END
        
        DELETE FROM [dbo].[tbld_SMSApplicationGroups] 
        WHERE [fldv_Code] = @pCode;
        
        SET @pRowsAffected = @@ROWCOUNT;
        
        COMMIT TRANSACTION;
        
        SET @AuditMessage = 'Successfully deleted Application Group Code: ' + @pCode + ' (' + ISNULL(@GroupName, 'Unknown') + '), Rows affected: ' + CAST(@pRowsAffected AS VARCHAR(10));
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error deleting Application Group Code ' + @pCode + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

-- =============================================
-- Procedure: pr_SMSApplicationGroup_GetAll
-- Description: Retrieve all application groups
-- =============================================
CREATE PROCEDURE [dbo].[pr_SMSApplicationGroup_GetAll]
    @pActiveOnly BIT = 0,
    @pRequestedBy VARCHAR(50) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(50) = COALESCE(@pRequestedBy, 'SYSTEM'), @FunctionName VARCHAR(50) = 'pr_SMSApplicationGroup_GetAll';
    DECLARE @RecordCount INT;
    
    BEGIN TRY
        SET @AuditMessage = 'Starting Application Group retrieval, ActiveOnly: ' + CAST(@pActiveOnly AS VARCHAR(1));
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        SELECT 
            [fldi_ID] AS ID,
            [fldv_Code] AS Code,
            [fldv_GroupName] AS Name,
            [fldv_Description] AS Description,
            [fldb_IsActive] AS IsActive,
            [fldv_CreatedBy] AS CreatedBy,
            [fldd_CreatedDate] AS CreatedDate,
            [fldv_UpdatedBy] AS UpdatedBy,
            [fldd_UpdatedDate] AS UpdatedDate
        FROM [dbo].[tbld_SMSApplicationGroups]
        WHERE (@pActiveOnly = 0 OR [fldb_IsActive] = 1)
        ORDER BY [fldv_GroupName];
        
        SET @RecordCount = @@ROWCOUNT;
        
        SET @AuditMessage = 'Successfully retrieved ' + CAST(@RecordCount AS VARCHAR(10)) + ' Application Groups';
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE(), @ErrorSeverity INT = ERROR_SEVERITY(), @ErrorState INT = ERROR_STATE();
        SET @AuditMessage = 'Error retrieving Application Groups: ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

-- =============================================
-- Procedure: pr_SMSApplicationGroup_GetByCode
-- Description: Retrieve a specific application group by code
-- =============================================
CREATE PROCEDURE [dbo].[pr_SMSApplicationGroup_GetByCode]
    @pCode VARCHAR(60),
    @pRequestedBy VARCHAR(50) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(50) = COALESCE(@pRequestedBy, 'SYSTEM'), @FunctionName VARCHAR(50) = 'pr_SMSApplicationGroup_GetByCode';
    DECLARE @RecordCount INT;
    
    BEGIN TRY
        SET @AuditMessage = 'Starting Application Group retrieval for Code: ' + @pCode;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        SELECT 
            [fldi_ID] AS ID,
            [fldv_Code] AS Code,
            [fldv_GroupName] AS Name,
            [fldv_Description] AS Description,
            [fldb_IsActive] AS IsActive,
            [fldv_CreatedBy] AS CreatedBy,
            [fldd_CreatedDate] AS CreatedDate,
            [fldv_UpdatedBy] AS UpdatedBy,
            [fldd_UpdatedDate] AS UpdatedDate
        FROM [dbo].[tbld_SMSApplicationGroups]
        WHERE [fldv_Code] = @pCode;
        
        SET @RecordCount = @@ROWCOUNT;
        
        SET @AuditMessage = 'Retrieved ' + CAST(@RecordCount AS VARCHAR(10)) + ' Application Group for Code: ' + @pCode;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE(), @ErrorSeverity INT = ERROR_SEVERITY(), @ErrorState INT = ERROR_STATE();
        SET @AuditMessage = 'Error retrieving Application Group Code ' + @pCode + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO