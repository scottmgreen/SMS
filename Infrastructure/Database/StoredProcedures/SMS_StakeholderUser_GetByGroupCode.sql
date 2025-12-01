USE [PDXSMS_V2]
GO

ALTER PROCEDURE [dbo].[pr_SMSStakeholderUser_GetByGroupCode]
    @pCode VARCHAR(60)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(50) = 'SYSTEM', @FunctionName VARCHAR(50) = 'pr_SMSStakeholderUser_GetByGroupCode', @RecordCount INT;
    
    BEGIN TRY
        SET @AuditMessage = 'Retrieving stakeholder users for group: ' + @pCode;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_StakeholderUsers', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        -- Dataset 1: SMSStakeholderUsers in group with their role information
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
            su.[fldd_UpdatedDate],
            -- Include role information directly in the main query
            ur.[fldv_Code] AS [UserRoleCode],
            ur.[fldv_Name] AS [UserRoleName],
            ur.[fldv_Description] AS [UserRoleDescription],
            ur.[fldb_IsActive] AS [UserRoleIsActive]
        FROM [dbo].[tbld_SMSStakeholderUsers] su
        INNER JOIN [dbo].[tblr_SMSStakeholderUserGroups] ug ON su.[fldv_Code] = ug.[fldv_UserCode]
        LEFT JOIN [dbo].[tbls_SMSUserRoles] ur ON su.[fldv_SMSUserRoleCode] = ur.[fldv_Code]
        WHERE ug.[fldv_GroupCode] = @pCode
          AND su.[fldb_IsActive] = 1
        ORDER BY su.[fldv_LastName], su.[fldv_FirstName];

        -- Dataset 2: Unique SMSUserRoles for users in this group
        SELECT DISTINCT
            ur.[fldi_Id],
            ur.[fldv_Code],
            ur.[fldv_Name],
            ur.[fldv_Description],
            ur.[fldb_IsActive],
            ur.[fldv_CreatedBy],
            ur.[fldd_CreatedDate],
            ur.[fldv_UpdatedBy],
            ur.[fldd_UpdatedDate]
        FROM [dbo].[tbls_SMSUserRoles] ur
        INNER JOIN [dbo].[tbld_SMSStakeholderUsers] su ON ur.[fldv_Code] = su.[fldv_SMSUserRoleCode]
        INNER JOIN [dbo].[tblr_SMSStakeholderUserGroups] ug ON su.[fldv_Code] = ug.[fldv_UserCode]
        WHERE ug.[fldv_GroupCode] = @pCode
          AND su.[fldb_IsActive] = 1
        ORDER BY ur.[fldv_Name];

        -- Dataset 3: SMSUserRolePermissions for all roles used by users in this group
        SELECT DISTINCT
            urp.[fldi_Id],
            urp.[fldv_Code],
            urp.[fldv_SMSUserRoleCode],
            urp.[fldv_Module],
            urp.[fldb_Create],
            urp.[fldb_Read],
            urp.[fldb_Update],
            urp.[fldb_Delete],
            urp.[fldv_CreatedBy],
            urp.[fldd_CreatedDate],
            urp.[fldv_UpdatedBy],
            urp.[fldd_UpdatedDate]
        FROM [dbo].[tbls_SMSUserRolePermissions] urp
        INNER JOIN [dbo].[tbld_SMSStakeholderUsers] su ON urp.[fldv_SMSUserRoleCode] = su.[fldv_SMSUserRoleCode]
        INNER JOIN [dbo].[tblr_SMSStakeholderUserGroups] ug ON su.[fldv_Code] = ug.[fldv_UserCode]
        WHERE ug.[fldv_GroupCode] = @pCode
          AND su.[fldb_IsActive] = 1
        ORDER BY urp.[fldv_SMSUserRoleCode], urp.[fldv_Module];

        SET @RecordCount = @@ROWCOUNT;
        SET @AuditMessage = 'Successfully retrieved stakeholder users for group: ' + @pCode + ', Records: ' + CAST(@RecordCount AS VARCHAR(10));
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_StakeholderUsers', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error retrieving stakeholder users for group ' + @pCode + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_StakeholderUsers', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;