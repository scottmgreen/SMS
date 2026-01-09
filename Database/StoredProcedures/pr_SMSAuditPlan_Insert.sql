USE [PDXSMS_V2]
GO

/****** Object:  StoredProcedure [dbo].[pr_SMSAuditPlan_Insert]    Script Date: 1/9/2026 12:34:52 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[pr_SMSAuditPlan_Insert]
    @pCode NVARCHAR(50) = NULL,
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
    @pStatus NVARCHAR(50) = 'Draft',
    @pPriority NVARCHAR(20) = 'Medium',
    @pRecurrencePattern NVARCHAR(60) = NULL,
    @pRequiresApproval BIT = 0,
    @pApprovedBy NVARCHAR(200) = NULL,
    @pApprovedDate DATETIME = NULL,
    @pExpectedDurationHours INT = NULL,
    @pNotes NVARCHAR(2000) = NULL,
    @pCreatedBy VARCHAR(50) = 'SYSTEM',
    @pCreatedDate DATETIME = NULL,
    @pNewID INT OUTPUT,
    @pNewAuditPlanCode NVARCHAR(50) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(10) = COALESCE(@pCreatedBy, 'SYSTEM'),
            @FunctionName VARCHAR(50) = 'pr_SMSAuditPlan_Insert';
    
    IF @pCreatedDate IS NULL SET @pCreatedDate = GETDATE();
    
    BEGIN TRY
        EXEC [dbo].[sp_AddAuditLogEntry] 
            @pUserID = @UserID, 
            @pMessageType = 'SMS_CRUD', 
            @pSeverity = 'Information', 
            @pModule = 'SMS_AuditManagement', 
            @pFunction = @FunctionName, 
            @pDescription = 'Starting SMS Audit Plan insert operation';

        -- Generate Audit Plan Code if not provided
        IF @pCode IS NULL OR @pCode = ''
        BEGIN
            EXEC [pr_GenerateFormattedCode] 
                @EntityName = 'SMSAuditPlan',           -- Uses registry: Table='tbld_SMSAuditPlans', Prefix='AP'
                @GeneratedCode = @pNewAuditPlanCode OUTPUT;
        END
        ELSE
        BEGIN
            SET @pNewAuditPlanCode = @pCode;
        END

        INSERT INTO [dbo].[tbld_SMSAuditPlans] (
            [fldv_Code], [fldv_Name], [fldv_Description], [fldv_AuditType], [fldv_Scope], 
            [fldv_Objectives], [fldd_PlannedStartDate], [fldd_PlannedEndDate], [fldv_LeadAuditor], 
            [fldv_AuditorTeam], [fldv_ResponsibleDepartment], [fldv_Status], [fldv_Priority], 
            [fldv_RecurrencePattern], [fldb_RequiresApproval], [fldv_ApprovedBy], [fldd_ApprovedDate],
            [fldi_ExpectedDurationHours], [fldv_Notes], [fldv_CreatedBy], [fldd_CreatedDate]
        )
        VALUES (
            @pNewAuditPlanCode, @pName, @pDescription, @pAuditType, @pScope, 
            @pObjectives, @pPlannedStartDate, @pPlannedEndDate, @pLeadAuditor, 
            @pAuditorTeam, @pResponsibleDepartment, @pStatus, @pPriority, 
            @pRecurrencePattern, @pRequiresApproval, @pApprovedBy, @pApprovedDate,
            @pExpectedDurationHours, @pNotes, @pCreatedBy, @pCreatedDate
        );

        SET @pNewID = SCOPE_IDENTITY();
        SET @AuditMessage = 'Successfully inserted SMS Audit Plan with ID: ' + CAST(@pNewID AS VARCHAR(10)) + ', Code: ' + COALESCE(@pNewAuditPlanCode, 'NULL');
        
        -- Log approval workflow if set during creation
        IF @pRequiresApproval = 1 AND @pApprovedBy IS NOT NULL
        BEGIN
            SET @AuditMessage = @AuditMessage + ' - Created with approval: Approved By: ' + COALESCE(@pApprovedBy, 'NULL') + 
                               ', Approved Date: ' + COALESCE(CONVERT(VARCHAR(20), @pApprovedDate, 120), 'NULL');
        END
        
        EXEC [dbo].[sp_AddAuditLogEntry] 
            @pUserID = @UserID, 
            @pMessageType = 'SMS_CRUD', 
            @pSeverity = 'Information', 
            @pModule = 'SMS_AuditManagement', 
            @pFunction = @FunctionName, 
            @pDescription = @AuditMessage;

        -- Log separate approval workflow entry if applicable
        IF @pRequiresApproval = 1 AND @pApprovedBy IS NOT NULL
        BEGIN
            SET @AuditMessage = 'SMS Audit Plan created with approval workflow - Code: ' + COALESCE(@pNewAuditPlanCode, 'NULL') + 
                               ', Approved By: ' + COALESCE(@pApprovedBy, 'NULL') + 
                               ', Approved Date: ' + COALESCE(CONVERT(VARCHAR(20), @pApprovedDate, 120), 'NULL');
            
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
        SET @AuditMessage = 'Error inserting SMS Audit Plan: ' + @ErrorMessage;
        
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