USE [PDXSMS_V2]
GO

-- =============================================
-- SMS STAKEHOLDER GROUPS CRUD OPERATIONS
-- Following the Investigation stored procedure pattern
-- =============================================

/****** Object:  StoredProcedure [dbo].[pr_SMSStakeholderGroup_Insert] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[pr_SMSStakeholderGroup_Insert]
    @pGroupCode VARCHAR(60) = NULL,
    @pGroupName VARCHAR(100),
    @pDescription VARCHAR(500) = NULL,
    @pCreatedBy VARCHAR(50) = 'SYSTEM',
    @pCreatedDate DATETIME = NULL,
    @pNewID INT OUTPUT,
    @pNewGroupCode VARCHAR(60) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(10) = COALESCE(@pCreatedBy, 'SYSTEM'), @FunctionName VARCHAR(50) = 'pr_SMSStakeholderGroup_Insert';
    IF @pCreatedDate IS NULL SET @pCreatedDate = GETDATE();
    
    BEGIN TRY
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_StakeholderGroups', @pFunction = @FunctionName, @pDescription = 'Starting stakeholder group insert operation';
        
        -- Generate code if not provided
        IF @pGroupCode IS NULL
        BEGIN
            EXEC [pr_GenerateFormattedCode] 
                @EntityName = 'StakeholderGroup',
                @GeneratedCode = @pNewGroupCode OUTPUT;
        END
        ELSE
        BEGIN
            SET @pNewGroupCode = @pGroupCode;
        END

        INSERT INTO [dbo].[tbld_SMSStakeholderGroups] 
        ([fldv_Code], [fldv_GroupName], [fldv_Description], [fldb_IsActive], [fldv_CreatedBy], [fldd_CreatedDate])
        VALUES (@pNewGroupCode, @pGroupName, @pDescription, 1, @pCreatedBy, @pCreatedDate);
        
        SET @pNewID = SCOPE_IDENTITY();
        SET @AuditMessage = 'Successfully inserted stakeholder group with ID: ' + CAST(@pNewID AS VARCHAR(10)) + ', Code: ' + COALESCE(@pNewGroupCode, 'NULL') + ', Name: ' + COALESCE(@pGroupName, 'NULL');
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_StakeholderGroups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error inserting stakeholder group: ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_StakeholderGroups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

/****** Object:  StoredProcedure [dbo].[pr_SMSStakeholderGroup_Update] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[pr_SMSStakeholderGroup_Update]
    @pID VARCHAR(60),
    @pGroupName VARCHAR(100),
    @pDescription VARCHAR(500) = NULL,
    @pIsActive BIT = 1,
    @pUpdatedBy VARCHAR(50) = 'SYSTEM',
    @pUpdatedDate DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(10) = COALESCE(@pUpdatedBy, 'SYSTEM'), @FunctionName VARCHAR(50) = 'pr_SMSStakeholderGroup_Update', @RowsAffected INT;
    IF @pUpdatedDate IS NULL SET @pUpdatedDate = GETDATE();
    
    BEGIN TRY
        SET @AuditMessage = 'Starting stakeholder group update for ID: ' + CAST(@pID AS VARCHAR(60));
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_StakeholderGroups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        UPDATE [dbo].[tbld_SMSStakeholderGroups] 
        SET [fldv_GroupName] = @pGroupName, 
            [fldv_Description] = @pDescription,
            [fldb_IsActive] = @pIsActive,
            [fldv_UpdatedBy] = @pUpdatedBy,
            [fldd_UpdatedDate] = @pUpdatedDate
        WHERE [fldv_Code] = @pID;
        
        SET @RowsAffected = @@ROWCOUNT;
        SET @AuditMessage = 'Successfully updated stakeholder group ID: ' + CAST(@pID AS VARCHAR(60)) + ', Rows affected: ' + CAST(@RowsAffected AS VARCHAR(10));
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_StakeholderGroups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error updating stakeholder group ID ' + CAST(@pID AS VARCHAR(60)) + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_StakeholderGroups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

/****** Object:  StoredProcedure [dbo].[pr_SMSStakeholderGroup_Delete] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[pr_SMSStakeholderGroup_Delete]
    @pID VARCHAR(60),
    @pUserID VARCHAR(10) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @FunctionName VARCHAR(50) = 'pr_SMSStakeholderGroup_Delete', @RowsAffected INT, @GroupCode VARCHAR(60), @GroupName VARCHAR(100);
    
    BEGIN TRY
        SELECT @GroupCode = [fldv_Code], @GroupName = [fldv_GroupName] 
        FROM [dbo].[tbld_SMSStakeholderGroups] 
        WHERE [fldv_Code] = @pID;
        
        SET @AuditMessage = 'Starting stakeholder group deletion for ID: ' + CAST(@pID AS VARCHAR(60)) + ', Code: ' + COALESCE(@GroupCode, 'NULL') + ', Name: ' + COALESCE(@GroupName, 'NULL');
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Warning', @pModule = 'SMS_StakeholderGroups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        -- First delete user-group memberships
        DELETE FROM [dbo].[tbld_SMSStakeholderUserGroups] WHERE [fldv_GroupCode] = @pID;
        
        -- Then delete the group
        DELETE FROM [dbo].[tbld_SMSStakeholderGroups] WHERE [fldv_Code] = @pID;
        SET @RowsAffected = @@ROWCOUNT;
        
        SET @AuditMessage = 'Successfully deleted stakeholder group ID: ' + CAST(@pID AS VARCHAR(60)) + ', Code: ' + COALESCE(@GroupCode, 'NULL') + ', Rows affected: ' + CAST(@RowsAffected AS VARCHAR(10));
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Warning', @pModule = 'SMS_StakeholderGroups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error deleting stakeholder group ID ' + CAST(@pID AS VARCHAR(60)) + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_StakeholderGroups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

/****** Object:  StoredProcedure [dbo].[pr_SMSStakeholderGroup_GetAll] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[pr_SMSStakeholderGroup_GetAll]
    @pUserID VARCHAR(10) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @FunctionName VARCHAR(50) = 'pr_SMSStakeholderGroup_GetAll', @RecordCount INT;
    
    BEGIN TRY
        SELECT [fldi_ID], [fldv_Code], [fldv_GroupName], [fldv_Description], [fldb_IsActive],
               [fldv_CreatedBy], [fldd_CreatedDate], [fldv_UpdatedBy], [fldd_UpdatedDate]
        FROM [dbo].[tbld_SMSStakeholderGroups] 
        ORDER BY [fldv_GroupName];
        
        SET @RecordCount = @@ROWCOUNT;
        SET @AuditMessage = 'Successfully retrieved ' + CAST(@RecordCount AS VARCHAR(10)) + ' stakeholder groups';
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_StakeholderGroups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error retrieving all stakeholder groups: ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_StakeholderGroups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

/****** Object:  StoredProcedure [dbo].[pr_SMSStakeholderGroup_GetByUserID] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[pr_SMSStakeholderGroup_GetByUserID]
    @pUserCode VARCHAR(60),
    @pUserID VARCHAR(10) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @FunctionName VARCHAR(50) = 'pr_SMSStakeholderGroup_GetByUserID', @RecordCount INT;
    
    BEGIN TRY
        SET @AuditMessage = 'Retrieving stakeholder groups for user: ' + CAST(@pUserCode AS VARCHAR(60));
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_StakeholderGroups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        SELECT sg.[fldi_ID], sg.[fldv_Code], sg.[fldv_GroupName], sg.[fldv_Description], sg.[fldb_IsActive],
               sg.[fldv_CreatedBy], sg.[fldd_CreatedDate], sg.[fldv_UpdatedBy], sg.[fldd_UpdatedDate],
               ug.[fldd_AssignedDate], ug.[fldv_AssignedBy]
        FROM [dbo].[tbld_SMSStakeholderGroups] sg
        INNER JOIN [dbo].[tbld_SMSStakeholderUserGroups] ug ON sg.[fldv_Code] = ug.[fldv_GroupCode]
        WHERE ug.[fldv_UserCode] = @pUserCode
          AND sg.[fldb_IsActive] = 1
        ORDER BY sg.[fldv_GroupName];
        
        SET @RecordCount = @@ROWCOUNT;
        SET @AuditMessage = 'Successfully retrieved ' + CAST(@RecordCount AS VARCHAR(10)) + ' stakeholder groups for user: ' + CAST(@pUserCode AS VARCHAR(60));
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_StakeholderGroups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error retrieving stakeholder groups for user ' + CAST(@pUserCode AS VARCHAR(60)) + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_StakeholderGroups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

/****** Object:  StoredProcedure [dbo].[pr_SMSStakeholderGroup_GetByCode] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[pr_SMSStakeholderGroup_GetByCode]
    @pCode VARCHAR(60),
    @pUserID VARCHAR(10) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @FunctionName VARCHAR(50) = 'pr_SMSStakeholderGroup_GetByCode', @RecordCount INT;
    
    BEGIN TRY
        SET @AuditMessage = 'Retrieving stakeholder group by code: ' + CAST(@pCode AS VARCHAR(60));
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_StakeholderGroups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        SELECT [fldi_ID], [fldv_Code], [fldv_GroupName], [fldv_Description], [fldb_IsActive],
               [fldv_CreatedBy], [fldd_CreatedDate], [fldv_UpdatedBy], [fldd_UpdatedDate]
        FROM [dbo].[tbld_SMSStakeholderGroups] 
        WHERE [fldv_Code] = @pCode;
        
        SET @RecordCount = @@ROWCOUNT;
        SET @AuditMessage = 'Successfully retrieved ' + CAST(@RecordCount AS VARCHAR(10)) + ' stakeholder group for code: ' + CAST(@pCode AS VARCHAR(60));
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_StakeholderGroups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error retrieving stakeholder group for code ' + CAST(@pCode AS VARCHAR(60)) + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_StakeholderGroups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

PRINT 'SMS Stakeholder Group stored procedures created successfully!'