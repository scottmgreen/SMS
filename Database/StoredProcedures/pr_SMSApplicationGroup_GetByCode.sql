USE [PDXSMS_V2]
GO

-- =============================================
-- Procedure: pr_SMSApplicationGroup_GetByCode
-- Description: Get application group by code with full member details
-- =============================================
CREATE PROCEDURE [dbo].[pr_SMSApplicationGroup_GetByCode]
    @pCode VARCHAR(60),
    @pRequestedBy VARCHAR(50) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(50) = COALESCE(@pRequestedBy, 'SYSTEM'), @FunctionName VARCHAR(50) = 'pr_SMSApplicationGroup_GetByCode';
    DECLARE @RecordCount INT, @MemberCount INT;
    
    BEGIN TRY
        SET @AuditMessage = 'Starting Application Group retrieval by code: ' + @pCode;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        -- First Dataset: Application Group Details
        SELECT 
            g.[fldv_Code] AS Code,
            g.[fldv_GroupName] AS Name,
            g.[fldv_Description] AS Description,
            g.[fldb_IsActive] AS IsActive,
            g.[fldv_CreatedBy] AS CreatedBy,
            g.[fldd_CreatedDate] AS CreatedDate,
            g.[fldv_UpdatedBy] AS UpdatedBy,
            g.[fldd_UpdatedDate] AS UpdatedDate
        FROM [dbo].[tbld_SMSApplicationGroups] g
        WHERE g.[fldv_Code] = @pCode;
        
        SET @RecordCount = @@ROWCOUNT;
        
        -- Second Dataset: Group Members with COMPLETE User Details for MapToSMSApplicationUser
        SELECT 
            u.[fldv_Code] AS fldv_Code,
            u.[fldv_FirstName] AS fldv_FirstName,
            u.[fldv_LastName] AS fldv_LastName,
            u.[fldv_UserName] AS fldv_UserName,
            u.[fldv_Password] AS fldv_Password,
            u.[fldv_ApplicationUserTypeCode] AS fldv_SMSUserTypeCode,  -- Map to expected field name
            u.[fldv_SMSUserRoleCode] AS fldv_SMSUserRoleCode,
            u.[fldb_IsActive] AS fldb_IsActive,
            u.[fldd_LastLoginDate] AS fldd_LastLoginDate,
            u.[fldv_CreatedBy] AS fldv_CreatedBy,
            u.[fldd_CreatedDate] AS fldd_CreatedDate,
            u.[fldv_UpdatedBy] AS fldv_UpdatedBy,
            u.[fldd_UpdatedDate] AS fldd_UpdatedDate
        FROM [dbo].[tblr_ApplicationUserGroups] ug
        INNER JOIN [dbo].[tbld_SMSApplicationUsers] u ON ug.[fldv_UserCode] = u.[fldv_Code]
        INNER JOIN [dbo].[tbld_SMSApplicationGroups] g ON ug.[fldv_GroupCode] = g.[fldv_Code]
        WHERE ug.[fldv_GroupCode] = @pCode
        ORDER BY u.[fldv_LastName], u.[fldv_FirstName];
        
        SET @MemberCount = @@ROWCOUNT;
        
        SET @AuditMessage = 'Successfully retrieved Application Group: ' + @pCode + ' with ' + CAST(@MemberCount AS VARCHAR(10)) + ' members';
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE(), @ErrorSeverity INT = ERROR_SEVERITY(), @ErrorState INT = ERROR_STATE();
        SET @AuditMessage = 'Error retrieving Application Group ' + @pCode + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO