USE [PDXSMS_V2]
GO

-- =============================================
-- Procedure: pr_SMSApplicationGroup_GetAll
-- Description: Retrieve all application groups with member information
-- =============================================
ALTER PROCEDURE [dbo].[pr_SMSApplicationGroup_GetAll]
    @pActiveOnly BIT = 0,
    @pRequestedBy VARCHAR(50) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(50) = COALESCE(@pRequestedBy, 'SYSTEM'), @FunctionName VARCHAR(50) = 'pr_SMSApplicationGroup_GetAll';
    DECLARE @RecordCount INT, @MemberCount INT;
    
    BEGIN TRY
        SET @AuditMessage = 'Starting Application Group retrieval, ActiveOnly: ' + CAST(@pActiveOnly AS VARCHAR(1));
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Groups', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        -- First Dataset: Application Groups with member counts
        SELECT 
            g.[fldv_Code] AS Code,
            g.[fldv_GroupName] AS Name,
            g.[fldv_Description] AS Description,
            g.[fldb_IsActive] AS IsActive,
            g.[fldv_CreatedBy] AS CreatedBy,
            g.[fldd_CreatedDate] AS CreatedDate,
            g.[fldv_UpdatedBy] AS UpdatedBy,
            g.[fldd_UpdatedDate] AS UpdatedDate,
            ISNULL(mc.MemberCount, 0) AS MemberCount
        FROM [dbo].[tbld_SMSApplicationGroups] g
        LEFT JOIN (
            SELECT 
                [fldv_GroupCode],
                COUNT(*) AS MemberCount
            FROM [dbo].[tblr_ApplicationUserGroups]
            GROUP BY [fldv_GroupCode]
        ) mc ON g.[fldv_Code] = mc.[fldv_GroupCode]
        WHERE (@pActiveOnly = 0 OR g.[fldb_IsActive] = 1)
        ORDER BY g.[fldv_GroupName];
        
        SET @RecordCount = @@ROWCOUNT;
        
        -- Second Dataset: Group Members using existing SMSApplicationUser structure
        SELECT 
            ug.[fldv_GroupCode] AS GroupCode,
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
            u.[fldd_UpdatedDate] AS fldd_UpdatedDate
        FROM [dbo].[tblr_ApplicationUserGroups] ug
        INNER JOIN [dbo].[tbld_SMSApplicationUsers] u ON ug.[fldv_UserCode] = u.[fldv_Code]
        INNER JOIN [dbo].[tbld_SMSApplicationGroups] g ON ug.[fldv_GroupCode] = g.[fldv_Code]
        WHERE (@pActiveOnly = 0 OR g.[fldb_IsActive] = 1)
        ORDER BY ug.[fldv_GroupCode], u.[fldv_LastName], u.[fldv_FirstName];
        
        SET @MemberCount = @@ROWCOUNT;
        
        SET @AuditMessage = 'Successfully retrieved ' + CAST(@RecordCount AS VARCHAR(10)) + ' Application Groups with ' + CAST(@MemberCount AS VARCHAR(10)) + ' total member assignments';
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