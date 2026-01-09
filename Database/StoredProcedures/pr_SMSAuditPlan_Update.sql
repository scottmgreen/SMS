USE [PDXSMS_V2]
GO

/****** Object:  StoredProcedure [dbo].[pr_SMSAuditPlan_Update]    Script Date: 1/9/2026 12:39:01 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[pr_SMSAuditPlan_Update]
    @pCode NVARCHAR(50),
    @pName NVARCHAR(200),
    @pDescription NVARCHAR(800) = NULL,
    @pAuditType NVARCHAR(60),
    @pScope NVARCHAR(800),
    @pObjectives NVARCHAR(800) = NULL,
    @pPlannedStartDate DATETIME,
    @pPlannedEndDate DATETIME,
    @pLeadAuditor NVARCHAR(200),
    @pAuditorTeam NVARCHAR(800) = NULL,
    @pResponsibleDepartment NVARCHAR(200),
    @pStatus NVARCHAR(50),
    @pPriority NVARCHAR(20),
    @pRecurrencePattern NVARCHAR(60) = NULL,
    @pRequiresApproval BIT,
    @pApprovedBy NVARCHAR(200) = NULL,
    @pApprovedDate DATETIME = NULL,
    @pExpectedDurationHours INT = NULL,
    @pNotes NVARCHAR(2000) = NULL,
    @pUpdatedBy VARCHAR(50),
    @pUpdatedDate DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(10) = COALESCE(@pUpdatedBy, 'SYSTEM'),
            @FunctionName VARCHAR(50) = 'pr_SMSAuditPlan_Update';
    DECLARE @RowsAffected INT = 0;
    DECLARE @ExistingRecord INT = 0;
    DECLARE @PreviousApprovedBy NVARCHAR(200) = NULL;
    DECLARE @PreviousApprovedDate DATETIME = NULL;
    
    IF @pUpdatedDate IS NULL SET @pUpdatedDate = GETDATE();
    
    BEGIN TRY
        -- Log start of operation
        EXEC [dbo].[sp_AddAuditLogEntry] 
            @pUserID = @UserID, 
            @pMessageType = 'SMS_CRUD', 
            @pSeverity = 'Information', 
            @pModule = 'SMS_AuditManagement', 
            @pFunction = @FunctionName, 
            @pDescription = 'Starting SMS Audit Plan update operation';

        -- Verify record exists and capture previous approval data
        SELECT @ExistingRecord = COUNT(*),
               @PreviousApprovedBy = [fldv_ApprovedBy],
               @PreviousApprovedDate = [fldd_ApprovedDate]
        FROM [dbo].[tbld_SMSAuditPlans]
        WHERE [fldv_Code] = @pCode;
        
        IF @ExistingRecord = 0
        BEGIN
            SET @AuditMessage = 'SMS Audit Plan not found for update with Code: ' + COALESCE(@pCode, 'NULL');
            
            EXEC [dbo].[sp_AddAuditLogEntry] 
                @pUserID = @UserID, 
                @pMessageType = 'SMS_CRUD', 
                @pSeverity = 'Warning', 
                @pModule = 'SMS_AuditManagement', 
                @pFunction = @FunctionName, 
                @pDescription = @AuditMessage;
                
            RAISERROR('SMS Audit Plan with Code ''%s'' not found for update.', 16, 1, @pCode);
            RETURN;
        END

        -- Perform the update with ALL fields (except fldi_ID)
        UPDATE [dbo].[tbld_SMSAuditPlans]
        SET 
            [fldv_Name] = @pName,
            [fldv_Description] = @pDescription,
            [fldv_AuditType] = @pAuditType,
            [fldv_Scope] = @pScope,
            [fldv_Objectives] = @pObjectives,
            [fldd_PlannedStartDate] = @pPlannedStartDate,
            [fldd_PlannedEndDate] = @pPlannedEndDate,
            [fldv_LeadAuditor] = @pLeadAuditor,
            [fldv_AuditorTeam] = @pAuditorTeam,
            [fldv_ResponsibleDepartment] = @pResponsibleDepartment,
            [fldv_Status] = @pStatus,
            [fldv_Priority] = @pPriority,
            [fldv_RecurrencePattern] = @pRecurrencePattern,
            [fldb_RequiresApproval] = @pRequiresApproval,
            [fldv_ApprovedBy] = @pApprovedBy,
            [fldd_ApprovedDate] = @pApprovedDate,
            [fldi_ExpectedDurationHours] = @pExpectedDurationHours,
            [fldv_Notes] = @pNotes,
            [fldv_UpdatedBy] = @pUpdatedBy,
            [fldd_UpdatedDate] = @pUpdatedDate
        WHERE [fldv_Code] = @pCode;

        SET @RowsAffected = @@ROWCOUNT;
        
        -- Log successful update
        SET @AuditMessage = 'Successfully updated SMS Audit Plan with Code: ' + COALESCE(@pCode, 'NULL') + 
                           ', Rows Affected: ' + CAST(@RowsAffected AS VARCHAR(10)) + 
                           ', Status: ' + COALESCE(@pStatus, 'NULL');
        
        EXEC [dbo].[sp_AddAuditLogEntry] 
            @pUserID = @UserID, 
            @pMessageType = 'SMS_CRUD', 
            @pSeverity = 'Information', 
            @pModule = 'SMS_AuditManagement', 
            @pFunction = @FunctionName, 
            @pDescription = @AuditMessage;

        -- Log approval workflow changes if applicable
        IF @pRequiresApproval = 1 AND 
           ((@pApprovedBy IS NOT NULL AND (@PreviousApprovedBy IS NULL OR @PreviousApprovedBy != @pApprovedBy)) OR
            (@pApprovedDate IS NOT NULL AND (@PreviousApprovedDate IS NULL OR @PreviousApprovedDate != @pApprovedDate)))
        BEGIN
            SET @AuditMessage = 'SMS Audit Plan approval workflow updated - Code: ' + COALESCE(@pCode, 'NULL') + 
                               ', Previous Approved By: ' + COALESCE(@PreviousApprovedBy, 'NULL') + 
                               ', New Approved By: ' + COALESCE(@pApprovedBy, 'NULL') + 
                               ', Previous Approved Date: ' + COALESCE(CONVERT(VARCHAR(20), @PreviousApprovedDate, 120), 'NULL') +
                               ', New Approved Date: ' + COALESCE(CONVERT(VARCHAR(20), @pApprovedDate, 120), 'NULL');
            
            EXEC [dbo].[sp_AddAuditLogEntry] 
                @pUserID = @UserID, 
                @pMessageType = 'SMS_APPROVAL', 
                @pSeverity = 'Information', 
                @pModule = 'SMS_AuditManagement', 
                @pFunction = @FunctionName, 
                @pDescription = @AuditMessage;
        END
            
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error updating SMS Audit Plan with Code: ' + COALESCE(@pCode, 'NULL') + 
                           ' - Error: ' + @ErrorMessage;
        
        EXEC [dbo].[sp_AddAuditLogEntry] 
            @pUserID = @UserID, 
            @pMessageType = 'SMS_CRUD', 
            @pSeverity = 'Error', 
            @pModule = 'SMS_AuditManagement', 
            @pFunction = @FunctionName, 
            @pDescription = @AuditMessage;
            
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO