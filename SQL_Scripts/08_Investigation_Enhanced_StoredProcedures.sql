-- =============================================
-- Enhanced Investigation Stored Procedures
-- SMS Investigation Management System
-- Comprehensive CRUD operations with workflow support
-- =============================================

USE [PDXSMS_V2]
GO

PRINT 'Creating Enhanced Investigation Stored Procedures...'
GO

-- =============================================
-- DROP EXISTING PROCEDURES IF THEY EXIST
-- =============================================

-- Investigation procedures
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'pr_Investigation_Insert_Enhanced')
    DROP PROCEDURE [dbo].[pr_Investigation_Insert_Enhanced]
GO

IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'pr_Investigation_Update_Enhanced')
    DROP PROCEDURE [dbo].[pr_Investigation_Update_Enhanced]
GO

IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'pr_Investigation_GetByCode')
    DROP PROCEDURE [dbo].[pr_Investigation_GetByCode]
GO

IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'pr_Investigation_GetByHazardCode')
    DROP PROCEDURE [dbo].[pr_Investigation_GetByHazardCode]
GO

IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'pr_Investigation_GetByInvestigator')
    DROP PROCEDURE [dbo].[pr_Investigation_GetByInvestigator]
GO

IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'pr_Investigation_GetByStatus')
    DROP PROCEDURE [dbo].[pr_Investigation_GetByStatus]
GO

IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'pr_Investigation_UpdateStatus')
    DROP PROCEDURE [dbo].[pr_Investigation_UpdateStatus]
GO

IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'pr_Investigation_RecordDecision')
    DROP PROCEDURE [dbo].[pr_Investigation_RecordDecision]
GO

IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'pr_Investigation_Complete')
    DROP PROCEDURE [dbo].[pr_Investigation_Complete]
GO

-- =============================================
-- Enhanced Investigation Insert
-- =============================================
CREATE PROCEDURE [dbo].[pr_Investigation_Insert_Enhanced]
    @pInvestigationCode NVARCHAR(50) = NULL,
    @pReportCode NVARCHAR(50) = NULL,
    @pHazardCode NVARCHAR(50) = NULL,
    @pInvestigationNotes NVARCHAR(MAX) = NULL,
    @pAssignedInvestigatorId NVARCHAR(50) = NULL,
    @pStatus NVARCHAR(50) = 'Assigned',
    @pInvestigationPlan NVARCHAR(MAX) = NULL,
    @pInvestigationObjectives NVARCHAR(MAX) = NULL,
    @pCreatedBy NVARCHAR(50) = 'SYSTEM',
    @pCreatedDate DATETIME2(7) = NULL,
    @pNewID INT OUTPUT,
    @pNewInvestigationCode NVARCHAR(50) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage NVARCHAR(4000);
    DECLARE @FunctionName NVARCHAR(50) = 'pr_Investigation_Insert_Enhanced';
    
    IF @pCreatedDate IS NULL SET @pCreatedDate = GETUTCDATE();
    
    BEGIN TRY
        -- Generate investigation code if not provided
        IF @pInvestigationCode IS NULL
        BEGIN
            EXEC [pr_GenerateFormattedCode] 
                @EntityName = 'Investigation',
                @GeneratedCode = @pNewInvestigationCode OUTPUT;
            SET @pInvestigationCode = @pNewInvestigationCode;
        END
        ELSE
        BEGIN
            SET @pNewInvestigationCode = @pInvestigationCode;
        END

        SET @AuditMessage = 'Starting enhanced Investigation insert - Code: ' + COALESCE(@pInvestigationCode, 'NULL');
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pCreatedBy, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Investigations', @pFunction = @FunctionName, @pDescription = @AuditMessage;

        INSERT INTO [dbo].[tbld_Investigations] (
            [fldv_InvestigationCode], [fldv_ReportCode], [fldv_HazardCode], [fldv_InvestigationNotes],
            [fldv_AssignedInvestigatorId], [fldv_Status], [fldv_InvestigationPlan], [fldv_InvestigationObjectives],
            [fldv_CreatedBy], [fldd_CreatedDate]
        )
        VALUES (
            @pInvestigationCode, @pReportCode, @pHazardCode, @pInvestigationNotes,
            @pAssignedInvestigatorId, @pStatus, @pInvestigationPlan, @pInvestigationObjectives,
            @pCreatedBy, @pCreatedDate
        );
        
        SET @pNewID = SCOPE_IDENTITY();
        
        SET @AuditMessage = 'Successfully inserted Investigation - ID: ' + CAST(@pNewID AS NVARCHAR(10)) + ', Code: ' + @pNewInvestigationCode;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pCreatedBy, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Investigations', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error inserting Investigation: ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pCreatedBy, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Investigations', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

-- =============================================
-- Get Investigation By Code
-- =============================================
CREATE PROCEDURE [dbo].[pr_Investigation_GetByCode]
    @pInvestigationCode NVARCHAR(50),
    @pUserID NVARCHAR(50) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage NVARCHAR(4000);
    DECLARE @FunctionName NVARCHAR(50) = 'pr_Investigation_GetByCode';
    
    BEGIN TRY
        SET @AuditMessage = 'Retrieving Investigation by Code: ' + @pInvestigationCode;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Investigations', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        SELECT 
            [fldi_ID], [fldv_InvestigationCode], [fldv_ReportCode], [fldv_HazardCode], [fldv_InvestigationNotes],
            [fldv_AssignedInvestigatorId], [fldv_Status], [fldd_CompletedDate], [fldv_InvestigationPlan], [fldv_InvestigationObjectives],
            [fldv_DecisionType], [fldv_DecisionRationale], [fldv_DecisionMaker], [fldd_DecisionDate], [fldv_NextSteps], [fldv_ReferralDetails],
            [fldv_CreatedBy], [fldd_CreatedDate], [fldv_UpdatedBy], [fldd_UpdatedDate]
        FROM [dbo].[tbld_Investigations] 
        WHERE [fldv_InvestigationCode] = @pInvestigationCode;
        
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error retrieving Investigation by Code ' + @pInvestigationCode + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Investigations', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

-- =============================================
-- Get Investigations By Hazard Code
-- =============================================
CREATE PROCEDURE [dbo].[pr_Investigation_GetByHazardCode]
    @pHazardCode NVARCHAR(50),
    @pUserID NVARCHAR(50) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage NVARCHAR(4000);
    DECLARE @FunctionName NVARCHAR(50) = 'pr_Investigation_GetByHazardCode', @RecordCount INT;
    
    BEGIN TRY
        SET @AuditMessage = 'Retrieving Investigations for Hazard Code: ' + @pHazardCode;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Investigations', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        SELECT 
            [fldi_ID], [fldv_InvestigationCode], [fldv_ReportCode], [fldv_HazardCode], [fldv_InvestigationNotes],
            [fldv_AssignedInvestigatorId], [fldv_Status], [fldd_CompletedDate], [fldv_InvestigationPlan], [fldv_InvestigationObjectives],
            [fldv_DecisionType], [fldv_DecisionRationale], [fldv_DecisionMaker], [fldd_DecisionDate], [fldv_NextSteps], [fldv_ReferralDetails],
            [fldv_CreatedBy], [fldd_CreatedDate], [fldv_UpdatedBy], [fldd_UpdatedDate]
        FROM [dbo].[tbld_Investigations] 
        WHERE [fldv_HazardCode] = @pHazardCode
        ORDER BY [fldv_InvestigationCode];
        
        SET @RecordCount = @@ROWCOUNT;
        SET @AuditMessage = 'Retrieved ' + CAST(@RecordCount AS NVARCHAR(10)) + ' Investigations for Hazard: ' + @pHazardCode;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Investigations', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error retrieving Investigations for Hazard ' + @pHazardCode + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Investigations', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

-- =============================================
-- Get Investigations By Investigator
-- =============================================
CREATE PROCEDURE [dbo].[pr_Investigation_GetByInvestigator]
    @pInvestigatorId NVARCHAR(50),
    @pUserID NVARCHAR(50) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage NVARCHAR(4000);
    DECLARE @FunctionName NVARCHAR(50) = 'pr_Investigation_GetByInvestigator', @RecordCount INT;
    
    BEGIN TRY
        SET @AuditMessage = 'Retrieving Investigations for Investigator: ' + @pInvestigatorId;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Investigations', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        SELECT 
            [fldi_ID], [fldv_InvestigationCode], [fldv_ReportCode], [fldv_HazardCode], [fldv_InvestigationNotes],
            [fldv_AssignedInvestigatorId], [fldv_Status], [fldd_CompletedDate], [fldv_InvestigationPlan], [fldv_InvestigationObjectives],
            [fldv_DecisionType], [fldv_DecisionRationale], [fldv_DecisionMaker], [fldd_DecisionDate], [fldv_NextSteps], [fldv_ReferralDetails],
            [fldv_CreatedBy], [fldd_CreatedDate], [fldv_UpdatedBy], [fldd_UpdatedDate]
        FROM [dbo].[tbld_Investigations] 
        WHERE [fldv_AssignedInvestigatorId] = @pInvestigatorId
        ORDER BY [fldv_Status], [fldd_CreatedDate] DESC;
        
        SET @RecordCount = @@ROWCOUNT;
        SET @AuditMessage = 'Retrieved ' + CAST(@RecordCount AS NVARCHAR(10)) + ' Investigations for Investigator: ' + @pInvestigatorId;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Investigations', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error retrieving Investigations for Investigator ' + @pInvestigatorId + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Investigations', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

-- =============================================
-- Get Investigations By Status
-- =============================================
CREATE PROCEDURE [dbo].[pr_Investigation_GetByStatus]
    @pStatus NVARCHAR(50),
    @pUserID NVARCHAR(50) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage NVARCHAR(4000);
    DECLARE @FunctionName NVARCHAR(50) = 'pr_Investigation_GetByStatus', @RecordCount INT;
    
    BEGIN TRY
        SET @AuditMessage = 'Retrieving Investigations with Status: ' + @pStatus;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Investigations', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        SELECT 
            [fldi_ID], [fldv_InvestigationCode], [fldv_ReportCode], [fldv_HazardCode], [fldv_InvestigationNotes],
            [fldv_AssignedInvestigatorId], [fldv_Status], [fldd_CompletedDate], [fldv_InvestigationPlan], [fldv_InvestigationObjectives],
            [fldv_DecisionType], [fldv_DecisionRationale], [fldv_DecisionMaker], [fldd_DecisionDate], [fldv_NextSteps], [fldv_ReferralDetails],
            [fldv_CreatedBy], [fldd_CreatedDate], [fldv_UpdatedBy], [fldd_UpdatedDate]
        FROM [dbo].[tbld_Investigations] 
        WHERE [fldv_Status] = @pStatus
        ORDER BY [fldd_CreatedDate] DESC;
        
        SET @RecordCount = @@ROWCOUNT;
        SET @AuditMessage = 'Retrieved ' + CAST(@RecordCount AS NVARCHAR(10)) + ' Investigations with Status: ' + @pStatus;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Investigations', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error retrieving Investigations by Status ' + @pStatus + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Investigations', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

-- =============================================
-- Enhanced Investigation Update
-- =============================================
CREATE PROCEDURE [dbo].[pr_Investigation_Update_Enhanced]
    @pInvestigationCode NVARCHAR(50),
    @pReportCode NVARCHAR(50) = NULL,
    @pHazardCode NVARCHAR(50) = NULL,
    @pInvestigationNotes NVARCHAR(MAX) = NULL,
    @pAssignedInvestigatorId NVARCHAR(50) = NULL,
    @pStatus NVARCHAR(50) = NULL,
    @pCompletedDate DATETIME2(7) = NULL,
    @pInvestigationPlan NVARCHAR(MAX) = NULL,
    @pInvestigationObjectives NVARCHAR(MAX) = NULL,
    @pDecisionType NVARCHAR(100) = NULL,
    @pDecisionRationale NVARCHAR(MAX) = NULL,
    @pDecisionMaker NVARCHAR(50) = NULL,
    @pDecisionDate DATETIME2(7) = NULL,
    @pNextSteps NVARCHAR(MAX) = NULL,
    @pReferralDetails NVARCHAR(MAX) = NULL,
    @pUpdatedBy NVARCHAR(50) = 'SYSTEM',
    @pUpdatedDate DATETIME2(7) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage NVARCHAR(4000);
    DECLARE @FunctionName NVARCHAR(50) = 'pr_Investigation_Update_Enhanced', @RowsAffected INT;
    
    IF @pUpdatedDate IS NULL SET @pUpdatedDate = GETUTCDATE();
    
    BEGIN TRY
        SET @AuditMessage = 'Starting enhanced Investigation update - Code: ' + @pInvestigationCode;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUpdatedBy, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Investigations', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        UPDATE [dbo].[tbld_Investigations] 
        SET 
            [fldv_ReportCode] = COALESCE(@pReportCode, [fldv_ReportCode]),
            [fldv_HazardCode] = COALESCE(@pHazardCode, [fldv_HazardCode]),
            [fldv_InvestigationNotes] = COALESCE(@pInvestigationNotes, [fldv_InvestigationNotes]),
            [fldv_AssignedInvestigatorId] = COALESCE(@pAssignedInvestigatorId, [fldv_AssignedInvestigatorId]),
            [fldv_Status] = COALESCE(@pStatus, [fldv_Status]),
            [fldd_CompletedDate] = @pCompletedDate,
            [fldv_InvestigationPlan] = COALESCE(@pInvestigationPlan, [fldv_InvestigationPlan]),
            [fldv_InvestigationObjectives] = COALESCE(@pInvestigationObjectives, [fldv_InvestigationObjectives]),
            [fldv_DecisionType] = @pDecisionType,
            [fldv_DecisionRationale] = @pDecisionRationale,
            [fldv_DecisionMaker] = @pDecisionMaker,
            [fldd_DecisionDate] = @pDecisionDate,
            [fldv_NextSteps] = @pNextSteps,
            [fldv_ReferralDetails] = @pReferralDetails,
            [fldv_UpdatedBy] = @pUpdatedBy,
            [fldd_UpdatedDate] = @pUpdatedDate
        WHERE [fldv_InvestigationCode] = @pInvestigationCode;
        
        SET @RowsAffected = @@ROWCOUNT;
        
        SET @AuditMessage = 'Successfully updated Investigation - Code: ' + @pInvestigationCode + ', Rows affected: ' + CAST(@RowsAffected AS NVARCHAR(10));
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUpdatedBy, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Investigations', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error updating Investigation ' + @pInvestigationCode + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUpdatedBy, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Investigations', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

-- =============================================
-- Update Investigation Status
-- =============================================
CREATE PROCEDURE [dbo].[pr_Investigation_UpdateStatus]
    @pInvestigationCode NVARCHAR(50),
    @pStatus NVARCHAR(50),
    @pUpdatedBy NVARCHAR(50) = 'SYSTEM',
    @pUpdatedDate DATETIME2(7) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage NVARCHAR(4000);
    DECLARE @FunctionName NVARCHAR(50) = 'pr_Investigation_UpdateStatus', @RowsAffected INT;
    
    IF @pUpdatedDate IS NULL SET @pUpdatedDate = GETUTCDATE();
    
    BEGIN TRY
        SET @AuditMessage = 'Updating Investigation Status - Code: ' + @pInvestigationCode + ' to Status: ' + @pStatus;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUpdatedBy, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Investigations', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        UPDATE [dbo].[tbld_Investigations] 
        SET 
            [fldv_Status] = @pStatus,
            [fldv_UpdatedBy] = @pUpdatedBy,
            [fldd_UpdatedDate] = @pUpdatedDate
        WHERE [fldv_InvestigationCode] = @pInvestigationCode;
        
        SET @RowsAffected = @@ROWCOUNT;
        
        SET @AuditMessage = 'Successfully updated Investigation Status - Code: ' + @pInvestigationCode + ', Rows affected: ' + CAST(@RowsAffected AS NVARCHAR(10));
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUpdatedBy, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Investigations', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error updating Investigation Status ' + @pInvestigationCode + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUpdatedBy, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Investigations', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

-- =============================================
-- Record Investigation Decision
-- =============================================
CREATE PROCEDURE [dbo].[pr_Investigation_RecordDecision]
    @pInvestigationCode NVARCHAR(50),
    @pDecisionType NVARCHAR(100),
    @pDecisionRationale NVARCHAR(MAX),
    @pDecisionMaker NVARCHAR(50),
    @pNextSteps NVARCHAR(MAX) = NULL,
    @pReferralDetails NVARCHAR(MAX) = NULL,
    @pUpdatedBy NVARCHAR(50) = 'SYSTEM',
    @pUpdatedDate DATETIME2(7) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage NVARCHAR(4000);
    DECLARE @FunctionName NVARCHAR(50) = 'pr_Investigation_RecordDecision', @RowsAffected INT;
    
    IF @pUpdatedDate IS NULL SET @pUpdatedDate = GETUTCDATE();
    
    BEGIN TRY
        SET @AuditMessage = 'Recording Investigation Decision - Code: ' + @pInvestigationCode + ', Decision: ' + @pDecisionType;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUpdatedBy, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Investigations', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        UPDATE [dbo].[tbld_Investigations] 
        SET 
            [fldv_DecisionType] = @pDecisionType,
            [fldv_DecisionRationale] = @pDecisionRationale,
            [fldv_DecisionMaker] = @pDecisionMaker,
            [fldd_DecisionDate] = @pUpdatedDate,
            [fldv_NextSteps] = @pNextSteps,
            [fldv_ReferralDetails] = @pReferralDetails,
            [fldv_UpdatedBy] = @pUpdatedBy,
            [fldd_UpdatedDate] = @pUpdatedDate
        WHERE [fldv_InvestigationCode] = @pInvestigationCode;
        
        SET @RowsAffected = @@ROWCOUNT;
        
        SET @AuditMessage = 'Successfully recorded Investigation Decision - Code: ' + @pInvestigationCode + ', Rows affected: ' + CAST(@RowsAffected AS NVARCHAR(10));
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUpdatedBy, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Investigations', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error recording Investigation Decision ' + @pInvestigationCode + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUpdatedBy, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Investigations', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

-- =============================================
-- Complete Investigation
-- =============================================
CREATE PROCEDURE [dbo].[pr_Investigation_Complete]
    @pInvestigationCode NVARCHAR(50),
    @pCompletedBy NVARCHAR(50),
    @pCompletionNotes NVARCHAR(MAX) = NULL,
    @pUpdatedBy NVARCHAR(50) = 'SYSTEM',
    @pUpdatedDate DATETIME2(7) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage NVARCHAR(4000);
    DECLARE @FunctionName NVARCHAR(50) = 'pr_Investigation_Complete', @RowsAffected INT;
    
    IF @pUpdatedDate IS NULL SET @pUpdatedDate = GETUTCDATE();
    
    BEGIN TRY
        SET @AuditMessage = 'Completing Investigation - Code: ' + @pInvestigationCode + ' by: ' + @pCompletedBy;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUpdatedBy, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Investigations', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        UPDATE [dbo].[tbld_Investigations] 
        SET 
            [fldv_Status] = 'Completed',
            [fldd_CompletedDate] = @pUpdatedDate,
            [fldv_InvestigationNotes] = COALESCE([fldv_InvestigationNotes] + CHAR(13) + CHAR(10) + 'COMPLETION NOTES: ' + @pCompletionNotes, @pCompletionNotes, [fldv_InvestigationNotes]),
            [fldv_UpdatedBy] = @pUpdatedBy,
            [fldd_UpdatedDate] = @pUpdatedDate
        WHERE [fldv_InvestigationCode] = @pInvestigationCode;
        
        SET @RowsAffected = @@ROWCOUNT;
        
        SET @AuditMessage = 'Successfully completed Investigation - Code: ' + @pInvestigationCode + ', Rows affected: ' + CAST(@RowsAffected AS NVARCHAR(10));
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUpdatedBy, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Investigations', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error completing Investigation ' + @pInvestigationCode + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUpdatedBy, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Investigations', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

PRINT 'Enhanced Investigation Stored Procedures created successfully!'
GO