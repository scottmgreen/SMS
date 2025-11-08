-- =============================================
-- Enhanced Interview Stored Procedures
-- SMS Interview Management System
-- Comprehensive CRUD operations with workflow support
-- =============================================

USE [PDXSMS_V2]
GO

PRINT 'Creating Enhanced Interview Stored Procedures...'
GO

-- =============================================
-- DROP EXISTING PROCEDURES IF THEY EXIST
-- =============================================

-- Interview procedures
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'pr_Interview_Insert_Enhanced')
    DROP PROCEDURE [dbo].[pr_Interview_Insert_Enhanced]
GO

IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'pr_Interview_Update_Enhanced')
    DROP PROCEDURE [dbo].[pr_Interview_Update_Enhanced]
GO

IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'pr_Interview_GetByCode')
    DROP PROCEDURE [dbo].[pr_Interview_GetByCode]
GO

IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'pr_Interview_GetByInvestigation')
    DROP PROCEDURE [dbo].[pr_Interview_GetByInvestigation]
GO

IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'pr_Interview_GetByInvestigator')
    DROP PROCEDURE [dbo].[pr_Interview_GetByInvestigator]
GO

IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'pr_Interview_GetByStatus')
    DROP PROCEDURE [dbo].[pr_Interview_GetByStatus]
GO

IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'pr_Interview_UpdateStatus')
    DROP PROCEDURE [dbo].[pr_Interview_UpdateStatus]
GO

IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'pr_Interview_Schedule')
    DROP PROCEDURE [dbo].[pr_Interview_Schedule]
GO

IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'pr_Interview_Complete')
    DROP PROCEDURE [dbo].[pr_Interview_Complete]
GO

-- =============================================
-- Enhanced Interview Insert
-- =============================================
CREATE PROCEDURE [dbo].[pr_Interview_Insert_Enhanced]
    @pInterviewCode NVARCHAR(50) = NULL,
    @pInvestigationCode NVARCHAR(50) = NULL,
    @pSMSInvestigatorCode NVARCHAR(50) = NULL,
    @pPersonInterviewed NVARCHAR(200) = NULL,
    @pPersonInterviewedRole NVARCHAR(100) = NULL,
    @pPersonInterviewedDepartment NVARCHAR(100) = NULL,
    @pPersonInterviewedNotes NVARCHAR(MAX) = NULL,
    @pInvestigatorNotes NVARCHAR(MAX) = NULL,
    @pStatus NVARCHAR(50) = 'Planned',
    @pInterviewDate DATETIME2(7) = NULL,
    @pDurationMinutes INT = NULL,
    @pInterviewLocation NVARCHAR(200) = NULL,
    @pType NVARCHAR(50) = 'Witness',
    @pIsConfidential BIT = 0,
    @pPreparationNotes NVARCHAR(MAX) = NULL,
    @pQuestionsToAsk NVARCHAR(MAX) = NULL,
    @pBackgroundInformation NVARCHAR(MAX) = NULL,
    @pCreatedBy NVARCHAR(50) = 'SYSTEM',
    @pCreatedDate DATETIME2(7) = NULL,
    @pNewID INT OUTPUT,
    @pNewInterviewCode NVARCHAR(50) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage NVARCHAR(4000);
    DECLARE @FunctionName NVARCHAR(50) = 'pr_Interview_Insert_Enhanced';
    
    IF @pCreatedDate IS NULL SET @pCreatedDate = GETUTCDATE();
    
    BEGIN TRY
        -- Generate interview code if not provided
        IF @pInterviewCode IS NULL
        BEGIN
            EXEC [pr_GenerateFormattedCode] 
                @EntityName = 'Interview',
                @GeneratedCode = @pNewInterviewCode OUTPUT;
            SET @pInterviewCode = @pNewInterviewCode;
        END
        ELSE
        BEGIN
            SET @pNewInterviewCode = @pInterviewCode;
        END

        SET @AuditMessage = 'Starting enhanced Interview insert - Code: ' + COALESCE(@pInterviewCode, 'NULL');
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pCreatedBy, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Interviews', @pFunction = @FunctionName, @pDescription = @AuditMessage;

        INSERT INTO [dbo].[tbld_Interviews] (
            [fldv_InterviewCode], [fldv_InvestigationCode], [fldv_SMSInvestigatorCode], [fldv_PersonInterviewed],
            [fldv_PersonInterviewedRole], [fldv_PersonInterviewedDepartment], [fldv_PersonInterviewedNotes], [fldv_InvestigatorNotes],
            [fldv_Status], [fldd_InterviewDate], [fldi_DurationMinutes], [fldv_InterviewLocation], [fldv_Type], [fldb_IsConfidential],
            [fldv_PreparationNotes], [fldv_QuestionsToAsk], [fldv_BackgroundInformation],
            [fldv_CreatedBy], [fldd_CreatedDate]
        )
        VALUES (
            @pInterviewCode, @pInvestigationCode, @pSMSInvestigatorCode, @pPersonInterviewed,
            @pPersonInterviewedRole, @pPersonInterviewedDepartment, @pPersonInterviewedNotes, @pInvestigatorNotes,
            @pStatus, @pInterviewDate, @pDurationMinutes, @pInterviewLocation, @pType, @pIsConfidential,
            @pPreparationNotes, @pQuestionsToAsk, @pBackgroundInformation,
            @pCreatedBy, @pCreatedDate
        );
        
        SET @pNewID = SCOPE_IDENTITY();
        
        SET @AuditMessage = 'Successfully inserted Interview - ID: ' + CAST(@pNewID AS NVARCHAR(10)) + ', Code: ' + @pNewInterviewCode;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pCreatedBy, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Interviews', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error inserting Interview: ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pCreatedBy, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Interviews', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

-- =============================================
-- Get Interview By Code
-- =============================================
CREATE PROCEDURE [dbo].[pr_Interview_GetByCode]
    @pInterviewCode NVARCHAR(50),
    @pUserID NVARCHAR(50) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage NVARCHAR(4000);
    DECLARE @FunctionName NVARCHAR(50) = 'pr_Interview_GetByCode';
    
    BEGIN TRY
        SET @AuditMessage = 'Retrieving Interview by Code: ' + @pInterviewCode;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Interviews', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        SELECT 
            [fldi_ID], [fldv_InterviewCode], [fldv_InvestigationCode], [fldv_SMSInvestigatorCode], [fldv_PersonInterviewed],
            [fldv_PersonInterviewedRole], [fldv_PersonInterviewedDepartment], [fldv_PersonInterviewedNotes], [fldv_InvestigatorNotes],
            [fldv_Status], [fldd_InterviewDate], [fldi_DurationMinutes], [fldv_InterviewLocation], [fldv_Type], [fldb_IsConfidential],
            [fldv_PreparationNotes], [fldv_QuestionsToAsk], [fldv_BackgroundInformation],
            [fldv_KeyFindings], [fldv_FollowUpRequired], [fldv_AdditionalWitnesses], [fldd_CompletedDate],
            [fldv_CreatedBy], [fldd_CreatedDate], [fldv_UpdatedBy], [fldd_UpdatedDate]
        FROM [dbo].[tbld_Interviews] 
        WHERE [fldv_InterviewCode] = @pInterviewCode;
        
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error retrieving Interview by Code ' + @pInterviewCode + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Interviews', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

-- =============================================
-- Get Interviews By Investigation
-- =============================================
CREATE PROCEDURE [dbo].[pr_Interview_GetByInvestigation]
    @pInvestigationCode NVARCHAR(50),
    @pUserID NVARCHAR(50) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage NVARCHAR(4000);
    DECLARE @FunctionName NVARCHAR(50) = 'pr_Interview_GetByInvestigation', @RecordCount INT;
    
    BEGIN TRY
        SET @AuditMessage = 'Retrieving Interviews for Investigation Code: ' + @pInvestigationCode;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Interviews', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        SELECT 
            [fldi_ID], [fldv_InterviewCode], [fldv_InvestigationCode], [fldv_SMSInvestigatorCode], [fldv_PersonInterviewed],
            [fldv_PersonInterviewedRole], [fldv_PersonInterviewedDepartment], [fldv_PersonInterviewedNotes], [fldv_InvestigatorNotes],
            [fldv_Status], [fldd_InterviewDate], [fldi_DurationMinutes], [fldv_InterviewLocation], [fldv_Type], [fldb_IsConfidential],
            [fldv_PreparationNotes], [fldv_QuestionsToAsk], [fldv_BackgroundInformation],
            [fldv_KeyFindings], [fldv_FollowUpRequired], [fldv_AdditionalWitnesses], [fldd_CompletedDate],
            [fldv_CreatedBy], [fldd_CreatedDate], [fldv_UpdatedBy], [fldd_UpdatedDate]
        FROM [dbo].[tbld_Interviews] 
        WHERE [fldv_InvestigationCode] = @pInvestigationCode
        ORDER BY [fldv_Status], [fldd_CreatedDate];
        
        SET @RecordCount = @@ROWCOUNT;
        SET @AuditMessage = 'Retrieved ' + CAST(@RecordCount AS NVARCHAR(10)) + ' Interviews for Investigation: ' + @pInvestigationCode;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Interviews', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error retrieving Interviews for Investigation ' + @pInvestigationCode + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Interviews', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

-- =============================================
-- Get Interviews By Investigator
-- =============================================
CREATE PROCEDURE [dbo].[pr_Interview_GetByInvestigator]
    @pInvestigatorCode NVARCHAR(50),
    @pUserID NVARCHAR(50) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage NVARCHAR(4000);
    DECLARE @FunctionName NVARCHAR(50) = 'pr_Interview_GetByInvestigator', @RecordCount INT;
    
    BEGIN TRY
        SET @AuditMessage = 'Retrieving Interviews for Investigator: ' + @pInvestigatorCode;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Interviews', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        SELECT 
            [fldi_ID], [fldv_InterviewCode], [fldv_InvestigationCode], [fldv_SMSInvestigatorCode], [fldv_PersonInterviewed],
            [fldv_PersonInterviewedRole], [fldv_PersonInterviewedDepartment], [fldv_PersonInterviewedNotes], [fldv_InvestigatorNotes],
            [fldv_Status], [fldd_InterviewDate], [fldi_DurationMinutes], [fldv_InterviewLocation], [fldv_Type], [fldb_IsConfidential],
            [fldv_PreparationNotes], [fldv_QuestionsToAsk], [fldv_BackgroundInformation],
            [fldv_KeyFindings], [fldv_FollowUpRequired], [fldv_AdditionalWitnesses], [fldd_CompletedDate],
            [fldv_CreatedBy], [fldd_CreatedDate], [fldv_UpdatedBy], [fldd_UpdatedDate]
        FROM [dbo].[tbld_Interviews] 
        WHERE [fldv_SMSInvestigatorCode] = @pInvestigatorCode
        ORDER BY [fldv_Status], [fldd_InterviewDate] DESC;
        
        SET @RecordCount = @@ROWCOUNT;
        SET @AuditMessage = 'Retrieved ' + CAST(@RecordCount AS NVARCHAR(10)) + ' Interviews for Investigator: ' + @pInvestigatorCode;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Interviews', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error retrieving Interviews for Investigator ' + @pInvestigatorCode + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Interviews', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

-- =============================================
-- Get Interviews By Status
-- =============================================
CREATE PROCEDURE [dbo].[pr_Interview_GetByStatus]
    @pStatus NVARCHAR(50),
    @pUserID NVARCHAR(50) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage NVARCHAR(4000);
    DECLARE @FunctionName NVARCHAR(50) = 'pr_Interview_GetByStatus', @RecordCount INT;
    
    BEGIN TRY
        SET @AuditMessage = 'Retrieving Interviews with Status: ' + @pStatus;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Interviews', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        SELECT 
            [fldi_ID], [fldv_InterviewCode], [fldv_InvestigationCode], [fldv_SMSInvestigatorCode], [fldv_PersonInterviewed],
            [fldv_PersonInterviewedRole], [fldv_PersonInterviewedDepartment], [fldv_PersonInterviewedNotes], [fldv_InvestigatorNotes],
            [fldv_Status], [fldd_InterviewDate], [fldi_DurationMinutes], [fldv_InterviewLocation], [fldv_Type], [fldb_IsConfidential],
            [fldv_PreparationNotes], [fldv_QuestionsToAsk], [fldv_BackgroundInformation],
            [fldv_KeyFindings], [fldv_FollowUpRequired], [fldv_AdditionalWitnesses], [fldd_CompletedDate],
            [fldv_CreatedBy], [fldd_CreatedDate], [fldv_UpdatedBy], [fldd_UpdatedDate]
        FROM [dbo].[tbld_Interviews] 
        WHERE [fldv_Status] = @pStatus
        ORDER BY [fldd_InterviewDate] ASC, [fldd_CreatedDate] DESC;
        
        SET @RecordCount = @@ROWCOUNT;
        SET @AuditMessage = 'Retrieved ' + CAST(@RecordCount AS NVARCHAR(10)) + ' Interviews with Status: ' + @pStatus;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Interviews', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error retrieving Interviews by Status ' + @pStatus + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Interviews', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

-- =============================================
-- Enhanced Interview Update
-- =============================================
CREATE PROCEDURE [dbo].[pr_Interview_Update_Enhanced]
    @pInterviewCode NVARCHAR(50),
    @pInvestigationCode NVARCHAR(50) = NULL,
    @pSMSInvestigatorCode NVARCHAR(50) = NULL,
    @pPersonInterviewed NVARCHAR(200) = NULL,
    @pPersonInterviewedRole NVARCHAR(100) = NULL,
    @pPersonInterviewedDepartment NVARCHAR(100) = NULL,
    @pPersonInterviewedNotes NVARCHAR(MAX) = NULL,
    @pInvestigatorNotes NVARCHAR(MAX) = NULL,
    @pStatus NVARCHAR(50) = NULL,
    @pInterviewDate DATETIME2(7) = NULL,
    @pDurationMinutes INT = NULL,
    @pInterviewLocation NVARCHAR(200) = NULL,
    @pType NVARCHAR(50) = NULL,
    @pIsConfidential BIT = NULL,
    @pPreparationNotes NVARCHAR(MAX) = NULL,
    @pQuestionsToAsk NVARCHAR(MAX) = NULL,
    @pBackgroundInformation NVARCHAR(MAX) = NULL,
    @pKeyFindings NVARCHAR(MAX) = NULL,
    @pFollowUpRequired NVARCHAR(MAX) = NULL,
    @pAdditionalWitnesses NVARCHAR(MAX) = NULL,
    @pCompletedDate DATETIME2(7) = NULL,
    @pUpdatedBy NVARCHAR(50) = 'SYSTEM',
    @pUpdatedDate DATETIME2(7) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage NVARCHAR(4000);
    DECLARE @FunctionName NVARCHAR(50) = 'pr_Interview_Update_Enhanced', @RowsAffected INT;
    
    IF @pUpdatedDate IS NULL SET @pUpdatedDate = GETUTCDATE();
    
    BEGIN TRY
        SET @AuditMessage = 'Starting enhanced Interview update - Code: ' + @pInterviewCode;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUpdatedBy, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Interviews', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        UPDATE [dbo].[tbld_Interviews] 
        SET 
            [fldv_InvestigationCode] = COALESCE(@pInvestigationCode, [fldv_InvestigationCode]),
            [fldv_SMSInvestigatorCode] = COALESCE(@pSMSInvestigatorCode, [fldv_SMSInvestigatorCode]),
            [fldv_PersonInterviewed] = COALESCE(@pPersonInterviewed, [fldv_PersonInterviewed]),
            [fldv_PersonInterviewedRole] = @pPersonInterviewedRole,
            [fldv_PersonInterviewedDepartment] = @pPersonInterviewedDepartment,
            [fldv_PersonInterviewedNotes] = @pPersonInterviewedNotes,
            [fldv_InvestigatorNotes] = @pInvestigatorNotes,
            [fldv_Status] = COALESCE(@pStatus, [fldv_Status]),
            [fldd_InterviewDate] = @pInterviewDate,
            [fldi_DurationMinutes] = @pDurationMinutes,
            [fldv_InterviewLocation] = @pInterviewLocation,
            [fldv_Type] = COALESCE(@pType, [fldv_Type]),
            [fldb_IsConfidential] = COALESCE(@pIsConfidential, [fldb_IsConfidential]),
            [fldv_PreparationNotes] = @pPreparationNotes,
            [fldv_QuestionsToAsk] = @pQuestionsToAsk,
            [fldv_BackgroundInformation] = @pBackgroundInformation,
            [fldv_KeyFindings] = @pKeyFindings,
            [fldv_FollowUpRequired] = @pFollowUpRequired,
            [fldv_AdditionalWitnesses] = @pAdditionalWitnesses,
            [fldd_CompletedDate] = @pCompletedDate,
            [fldv_UpdatedBy] = @pUpdatedBy,
            [fldd_UpdatedDate] = @pUpdatedDate
        WHERE [fldv_InterviewCode] = @pInterviewCode;
        
        SET @RowsAffected = @@ROWCOUNT;
        
        SET @AuditMessage = 'Successfully updated Interview - Code: ' + @pInterviewCode + ', Rows affected: ' + CAST(@RowsAffected AS NVARCHAR(10));
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUpdatedBy, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Interviews', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error updating Interview ' + @pInterviewCode + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUpdatedBy, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Interviews', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

-- =============================================
-- Update Interview Status
-- =============================================
CREATE PROCEDURE [dbo].[pr_Interview_UpdateStatus]
    @pInterviewCode NVARCHAR(50),
    @pStatus NVARCHAR(50),
    @pUpdatedBy NVARCHAR(50) = 'SYSTEM',
    @pUpdatedDate DATETIME2(7) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage NVARCHAR(4000);
    DECLARE @FunctionName NVARCHAR(50) = 'pr_Interview_UpdateStatus', @RowsAffected INT;
    
    IF @pUpdatedDate IS NULL SET @pUpdatedDate = GETUTCDATE();
    
    BEGIN TRY
        SET @AuditMessage = 'Updating Interview Status - Code: ' + @pInterviewCode + ' to Status: ' + @pStatus;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUpdatedBy, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Interviews', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        UPDATE [dbo].[tbld_Interviews] 
        SET 
            [fldv_Status] = @pStatus,
            [fldv_UpdatedBy] = @pUpdatedBy,
            [fldd_UpdatedDate] = @pUpdatedDate
        WHERE [fldv_InterviewCode] = @pInterviewCode;
        
        SET @RowsAffected = @@ROWCOUNT;
        
        SET @AuditMessage = 'Successfully updated Interview Status - Code: ' + @pInterviewCode + ', Rows affected: ' + CAST(@RowsAffected AS NVARCHAR(10));
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUpdatedBy, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Interviews', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error updating Interview Status ' + @pInterviewCode + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUpdatedBy, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Interviews', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

-- =============================================
-- Schedule Interview
-- =============================================
CREATE PROCEDURE [dbo].[pr_Interview_Schedule]
    @pInterviewCode NVARCHAR(50),
    @pInterviewDate DATETIME2(7),
    @pInterviewLocation NVARCHAR(200),
    @pDurationMinutes INT = NULL,
    @pStatus NVARCHAR(50) = 'Scheduled',
    @pUpdatedBy NVARCHAR(50) = 'SYSTEM',
    @pUpdatedDate DATETIME2(7) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage NVARCHAR(4000);
    DECLARE @FunctionName NVARCHAR(50) = 'pr_Interview_Schedule', @RowsAffected INT;
    
    IF @pUpdatedDate IS NULL SET @pUpdatedDate = GETUTCDATE();
    
    BEGIN TRY
        SET @AuditMessage = 'Scheduling Interview - Code: ' + @pInterviewCode + ' for: ' + CAST(@pInterviewDate AS NVARCHAR(50));
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUpdatedBy, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Interviews', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        UPDATE [dbo].[tbld_Interviews] 
        SET 
            [fldv_Status] = @pStatus,
            [fldd_InterviewDate] = @pInterviewDate,
            [fldv_InterviewLocation] = @pInterviewLocation,
            [fldi_DurationMinutes] = @pDurationMinutes,
            [fldv_UpdatedBy] = @pUpdatedBy,
            [fldd_UpdatedDate] = @pUpdatedDate
        WHERE [fldv_InterviewCode] = @pInterviewCode;
        
        SET @RowsAffected = @@ROWCOUNT;
        
        SET @AuditMessage = 'Successfully scheduled Interview - Code: ' + @pInterviewCode + ', Rows affected: ' + CAST(@RowsAffected AS NVARCHAR(10));
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUpdatedBy, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Interviews', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error scheduling Interview ' + @pInterviewCode + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUpdatedBy, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Interviews', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

-- =============================================
-- Complete Interview
-- =============================================
CREATE PROCEDURE [dbo].[pr_Interview_Complete]
    @pInterviewCode NVARCHAR(50),
    @pPersonInterviewedNotes NVARCHAR(MAX) = NULL,
    @pInvestigatorNotes NVARCHAR(MAX) = NULL,
    @pKeyFindings NVARCHAR(MAX) = NULL,
    @pStatus NVARCHAR(50) = 'Completed',
    @pCompletedDate DATETIME2(7) = NULL,
    @pUpdatedBy NVARCHAR(50) = 'SYSTEM',
    @pUpdatedDate DATETIME2(7) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage NVARCHAR(4000);
    DECLARE @FunctionName NVARCHAR(50) = 'pr_Interview_Complete', @RowsAffected INT;
    
    IF @pUpdatedDate IS NULL SET @pUpdatedDate = GETUTCDATE();
    IF @pCompletedDate IS NULL SET @pCompletedDate = @pUpdatedDate;
    
    BEGIN TRY
        SET @AuditMessage = 'Completing Interview - Code: ' + @pInterviewCode;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUpdatedBy, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Interviews', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        UPDATE [dbo].[tbld_Interviews] 
        SET 
            [fldv_Status] = @pStatus,
            [fldv_PersonInterviewedNotes] = COALESCE(@pPersonInterviewedNotes, [fldv_PersonInterviewedNotes]),
            [fldv_InvestigatorNotes] = COALESCE(@pInvestigatorNotes, [fldv_InvestigatorNotes]),
            [fldv_KeyFindings] = @pKeyFindings,
            [fldd_CompletedDate] = @pCompletedDate,
            [fldv_UpdatedBy] = @pUpdatedBy,
            [fldd_UpdatedDate] = @pUpdatedDate
        WHERE [fldv_InterviewCode] = @pInterviewCode;
        
        SET @RowsAffected = @@ROWCOUNT;
        
        SET @AuditMessage = 'Successfully completed Interview - Code: ' + @pInterviewCode + ', Rows affected: ' + CAST(@RowsAffected AS NVARCHAR(10));
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUpdatedBy, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_Interviews', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error completing Interview ' + @pInterviewCode + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUpdatedBy, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_Interviews', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

PRINT 'Enhanced Interview Stored Procedures created successfully!'
GO