USE [PDXSMS_V2]
GO

-- =============================================
-- SMS CRUD STORED PROCEDURES - HazardReportTracking
-- HazardReportTracking Operations for Tracking Code Management
-- =============================================

-- =============================================
-- HAZARD REPORT TRACKING CRUD OPERATIONS
-- =============================================

/****** Object:  StoredProcedure [dbo].[pr_HazardReportTracking_Insert]    Script Date: 1/22/2026 8:52:49 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		System Generated
-- Create date: 1/22/2026
-- Description:	Insert new HazardReportTracking record with automatic tracking code generation
-- =============================================
CREATE PROCEDURE [dbo].[pr_HazardReportTracking_Insert]
    @pHazardCode NCHAR(60),
    @pReportCode NCHAR(60) = NULL,
    @pTrackingCode NCHAR(60),
    @pCreatedBy VARCHAR(50) = 'SYSTEM',
    @pCreatedDate DATETIME = NULL,
    @pNewID INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(10) = COALESCE(@pCreatedBy, 'SYSTEM'), @FunctionName VARCHAR(50) = 'pr_HazardReportTracking_Insert';
    IF @pCreatedDate IS NULL SET @pCreatedDate = GETDATE();
    
    BEGIN TRY
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_HazardReportTracking', @pFunction = @FunctionName, @pDescription = 'Starting HazardReportTracking insert operation';
        
        -- Validate required parameters
        IF @pHazardCode IS NULL OR LTRIM(RTRIM(@pHazardCode)) = ''
        BEGIN
            RAISERROR('HazardCode is required and cannot be empty', 16, 1);
            RETURN;
        END;
        
        IF @pTrackingCode IS NULL OR LTRIM(RTRIM(@pTrackingCode)) = ''
        BEGIN
            RAISERROR('TrackingCode is required and cannot be empty', 16, 1);
            RETURN;
        END;
        
        -- Check for duplicate tracking code
        IF EXISTS (SELECT 1 FROM [dbo].[tbld_HazardReportTracking] WHERE [fldv_TrackingCode] = @pTrackingCode)
        BEGIN
            RAISERROR('Tracking code already exists: %s', 16, 1, @pTrackingCode);
            RETURN;
        END;

        INSERT INTO [dbo].[tbld_HazardReportTracking] (
            [fldv_HazardCode], 
            [fldv_ReportCode], 
            [fldv_TrackingCode],
            [fldv_CreatedBy],
            [fldd_CreatedDate]
        )
        VALUES (
            @pHazardCode, 
            @pReportCode, 
            @pTrackingCode,
            @pCreatedBy,
            @pCreatedDate
        );
        
        SET @pNewID = SCOPE_IDENTITY();
        SET @AuditMessage = 'Successfully inserted HazardReportTracking with ID: ' + CAST(@pNewID AS VARCHAR(10)) + ', TrackingCode: ' + COALESCE(@pTrackingCode, 'NULL');
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_HazardReportTracking', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error inserting HazardReportTracking: ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_HazardReportTracking', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

/****** Object:  StoredProcedure [dbo].[pr_HazardReportTracking_GetByTrackingCode]    Script Date: 1/22/2026 8:52:49 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		System Generated
-- Create date: 1/22/2026
-- Description:	Get HazardReportTracking record by tracking code
-- =============================================
CREATE PROCEDURE [dbo].[pr_HazardReportTracking_GetByTrackingCode]
    @pTrackingCode NCHAR(60)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(10) = 'SYSTEM', @FunctionName VARCHAR(50) = 'pr_HazardReportTracking_GetByTrackingCode';
    
    BEGIN TRY
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_HazardReportTracking', @pFunction = @FunctionName, @pDescription = 'Starting HazardReportTracking get by tracking code operation';
        
        -- Validate required parameters
        IF @pTrackingCode IS NULL OR LTRIM(RTRIM(@pTrackingCode)) = ''
        BEGIN
            RAISERROR('TrackingCode is required and cannot be empty', 16, 1);
            RETURN;
        END;

        SELECT 
            [fldi_ID],
            [fldv_HazardCode], 
            [fldv_ReportCode], 
            [fldv_TrackingCode],
            [fldv_CreatedBy],
            [fldd_CreatedDate],
            [fldv_UpdatedBy],
            [fldd_UpdatedDate]
        FROM [dbo].[tbld_HazardReportTracking] 
        WHERE [fldv_TrackingCode] = @pTrackingCode;
        
        SET @AuditMessage = 'Successfully retrieved HazardReportTracking for TrackingCode: ' + COALESCE(@pTrackingCode, 'NULL');
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_HazardReportTracking', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error retrieving HazardReportTracking by tracking code: ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_HazardReportTracking', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

/****** Object:  StoredProcedure [dbo].[pr_HazardReportTracking_GetAll]    Script Date: 1/22/2026 8:52:49 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		System Generated
-- Create date: 1/22/2026
-- Description:	Get all HazardReportTracking records
-- =============================================
CREATE PROCEDURE [dbo].[pr_HazardReportTracking_GetAll]
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(10) = 'SYSTEM', @FunctionName VARCHAR(50) = 'pr_HazardReportTracking_GetAll';
    
    BEGIN TRY
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_HazardReportTracking', @pFunction = @FunctionName, @pDescription = 'Starting HazardReportTracking get all operation';
        
        SELECT 
            [fldi_ID],
            [fldv_HazardCode], 
            [fldv_ReportCode], 
            [fldv_TrackingCode],
            [fldv_CreatedBy],
            [fldd_CreatedDate],
            [fldv_UpdatedBy],
            [fldd_UpdatedDate]
        FROM [dbo].[tbld_HazardReportTracking] 
        ORDER BY [fldd_CreatedDate] DESC;
        
        SET @AuditMessage = 'Successfully retrieved all HazardReportTracking records';
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_HazardReportTracking', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error retrieving all HazardReportTracking records: ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_HazardReportTracking', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

/****** Object:  StoredProcedure [dbo].[pr_HazardReportTracking_GetByHazardCode]    Script Date: 1/22/2026 8:52:49 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		System Generated
-- Create date: 1/22/2026
-- Description:	Get HazardReportTracking records by hazard code
-- =============================================
CREATE PROCEDURE [dbo].[pr_HazardReportTracking_GetByHazardCode]
    @pHazardCode NCHAR(60)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(10) = 'SYSTEM', @FunctionName VARCHAR(50) = 'pr_HazardReportTracking_GetByHazardCode';
    
    BEGIN TRY
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_HazardReportTracking', @pFunction = @FunctionName, @pDescription = 'Starting HazardReportTracking get by hazard code operation';
        
        -- Validate required parameters
        IF @pHazardCode IS NULL OR LTRIM(RTRIM(@pHazardCode)) = ''
        BEGIN
            RAISERROR('HazardCode is required and cannot be empty', 16, 1);
            RETURN;
        END;

        SELECT 
            [fldi_ID],
            [fldv_HazardCode], 
            [fldv_ReportCode], 
            [fldv_TrackingCode],
            [fldv_CreatedBy],
            [fldd_CreatedDate],
            [fldv_UpdatedBy],
            [fldd_UpdatedDate]
        FROM [dbo].[tbld_HazardReportTracking] 
        WHERE [fldv_HazardCode] = @pHazardCode
        ORDER BY [fldd_CreatedDate] DESC;
        
        SET @AuditMessage = 'Successfully retrieved HazardReportTracking records for HazardCode: ' + COALESCE(@pHazardCode, 'NULL');
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_HazardReportTracking', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error retrieving HazardReportTracking by hazard code: ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_HazardReportTracking', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

/****** Object:  StoredProcedure [dbo].[pr_HazardReportTracking_GetByReportCode]    Script Date: 1/22/2026 8:52:49 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		System Generated
-- Create date: 1/22/2026
-- Description:	Get HazardReportTracking records by report code
-- =============================================
CREATE PROCEDURE [dbo].[pr_HazardReportTracking_GetByReportCode]
    @pReportCode NCHAR(60)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(10) = 'SYSTEM', @FunctionName VARCHAR(50) = 'pr_HazardReportTracking_GetByReportCode';
    
    BEGIN TRY
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_HazardReportTracking', @pFunction = @FunctionName, @pDescription = 'Starting HazardReportTracking get by report code operation';
        
        -- Validate required parameters
        IF @pReportCode IS NULL OR LTRIM(RTRIM(@pReportCode)) = ''
        BEGIN
            RAISERROR('ReportCode is required and cannot be empty', 16, 1);
            RETURN;
        END;

        SELECT 
            [fldi_ID],
            [fldv_HazardCode], 
            [fldv_ReportCode], 
            [fldv_TrackingCode],
            [fldv_CreatedBy],
            [fldd_CreatedDate],
            [fldv_UpdatedBy],
            [fldd_UpdatedDate]
        FROM [dbo].[tbld_HazardReportTracking] 
        WHERE [fldv_ReportCode] = @pReportCode
        ORDER BY [fldd_CreatedDate] DESC;
        
        SET @AuditMessage = 'Successfully retrieved HazardReportTracking records for ReportCode: ' + COALESCE(@pReportCode, 'NULL');
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_HazardReportTracking', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error retrieving HazardReportTracking by report code: ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_HazardReportTracking', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

/****** Object:  StoredProcedure [dbo].[pr_HazardReportTracking_Update]    Script Date: 1/22/2026 8:52:49 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		System Generated
-- Create date: 1/22/2026
-- Description:	Update existing HazardReportTracking record
-- =============================================
CREATE PROCEDURE [dbo].[pr_HazardReportTracking_Update]
    @pTrackingCode NCHAR(60),
    @pHazardCode NCHAR(60),
    @pReportCode NCHAR(60) = NULL,
    @pUpdatedBy VARCHAR(50) = 'SYSTEM',
    @pUpdatedDate DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(10) = COALESCE(@pUpdatedBy, 'SYSTEM'), @FunctionName VARCHAR(50) = 'pr_HazardReportTracking_Update';
    IF @pUpdatedDate IS NULL SET @pUpdatedDate = GETDATE();
    
    BEGIN TRY
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_HazardReportTracking', @pFunction = @FunctionName, @pDescription = 'Starting HazardReportTracking update operation';
        
        -- Validate required parameters
        IF @pTrackingCode IS NULL OR LTRIM(RTRIM(@pTrackingCode)) = ''
        BEGIN
            RAISERROR('TrackingCode is required and cannot be empty', 16, 1);
            RETURN;
        END;
        
        IF @pHazardCode IS NULL OR LTRIM(RTRIM(@pHazardCode)) = ''
        BEGIN
            RAISERROR('HazardCode is required and cannot be empty', 16, 1);
            RETURN;
        END;
        
        -- Check if record exists
        IF NOT EXISTS (SELECT 1 FROM [dbo].[tbld_HazardReportTracking] WHERE [fldv_TrackingCode] = @pTrackingCode)
        BEGIN
            RAISERROR('HazardReportTracking record not found for TrackingCode: %s', 16, 1, @pTrackingCode);
            RETURN;
        END;

        UPDATE [dbo].[tbld_HazardReportTracking] 
        SET 
            [fldv_HazardCode] = @pHazardCode,
            [fldv_ReportCode] = @pReportCode,
            [fldv_UpdatedBy] = @pUpdatedBy,
            [fldd_UpdatedDate] = @pUpdatedDate
        WHERE [fldv_TrackingCode] = @pTrackingCode;
        
        SET @AuditMessage = 'Successfully updated HazardReportTracking for TrackingCode: ' + COALESCE(@pTrackingCode, 'NULL');
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_HazardReportTracking', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error updating HazardReportTracking: ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_HazardReportTracking', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

/****** Object:  StoredProcedure [dbo].[pr_HazardReportTracking_Delete]    Script Date: 1/22/2026 8:52:49 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		System Generated
-- Create date: 1/22/2026
-- Description:	Delete HazardReportTracking record by tracking code
-- =============================================
CREATE PROCEDURE [dbo].[pr_HazardReportTracking_Delete]
    @pTrackingCode NCHAR(60)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(10) = 'SYSTEM', @FunctionName VARCHAR(50) = 'pr_HazardReportTracking_Delete';
    
    BEGIN TRY
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_HazardReportTracking', @pFunction = @FunctionName, @pDescription = 'Starting HazardReportTracking delete operation';
        
        -- Validate required parameters
        IF @pTrackingCode IS NULL OR LTRIM(RTRIM(@pTrackingCode)) = ''
        BEGIN
            RAISERROR('TrackingCode is required and cannot be empty', 16, 1);
            RETURN;
        END;
        
        -- Check if record exists
        IF NOT EXISTS (SELECT 1 FROM [dbo].[tbld_HazardReportTracking] WHERE [fldv_TrackingCode] = @pTrackingCode)
        BEGIN
            RAISERROR('HazardReportTracking record not found for TrackingCode: %s', 16, 1, @pTrackingCode);
            RETURN;
        END;

        DELETE FROM [dbo].[tbld_HazardReportTracking] 
        WHERE [fldv_TrackingCode] = @pTrackingCode;
        
        SET @AuditMessage = 'Successfully deleted HazardReportTracking for TrackingCode: ' + COALESCE(@pTrackingCode, 'NULL');
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_HazardReportTracking', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error deleting HazardReportTracking: ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_HazardReportTracking', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO