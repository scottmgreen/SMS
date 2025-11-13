USE [PDXSMS_V2]
GO
/****** Object:  StoredProcedure [dbo].[pr_HazardFile_Insert]    Script Date: 11/12/2025 4:34:40 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[pr_HazardFile_Insert]
    @pCode NVARCHAR(50) = NULL,
    @pHazardCode NVARCHAR(50) = NULL,
    @pReportCode NVARCHAR(50) = NULL,
    @pFileName NVARCHAR(255) = NULL,
    @pFileType NVARCHAR(50) = NULL,
    @pContentType NVARCHAR(100) = NULL,
    @pFileSizeBytes BIGINT = NULL,
    @pFileHash NVARCHAR(128) = NULL,
    @pStorageType NVARCHAR(20) = 'Database',
    @pFilePath NVARCHAR(500) = NULL,
    @pFileData VARBINARY(MAX) = NULL,
    @pDescription NVARCHAR(1000) = NULL,
    @pCategory NVARCHAR(50) = NULL,
    @pIsConfidential BIT = 0,
    @pUploadedBy NVARCHAR(50) = NULL,
    @pTags NVARCHAR(500) = NULL,
    @pCreatedBy NVARCHAR(50) = 'SYSTEM',
    @pCreatedDate DATETIME2(7) = NULL,
    @pNewID INT OUTPUT,
    @pNewHazardFileCode NVARCHAR(50) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(10) = COALESCE(@pCreatedBy, 'SYSTEM'), @FunctionName VARCHAR(50) = 'pr_HazardFile_Insert';
    IF @pCreatedDate IS NULL SET @pCreatedDate = GETUTCDATE();
    
    BEGIN TRY
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_HazardFiles', @pFunction = @FunctionName, @pDescription = 'Starting HazardFile insert operation';
        
        -- Generate HazardFile Code using entity registry
        EXEC [pr_GenerateFormattedCode] 
            @EntityName = 'HazardFile',           -- Uses registry: Table='tbld_HazardFiles', Prefix='HF'
            @GeneratedCode = @pNewHazardFileCode OUTPUT;

        INSERT INTO [dbo].[tbld_HazardFiles] (
            [fldv_Code],
            [fldv_HazardCode],
            [fldv_ReportCode],
            [fldv_FileName],
            [fldv_FileType],
            [fldv_ContentType],
            [fldi_FileSizeBytes],
            [fldv_FileHash],
            [fldv_StorageType],
            [fldv_FilePath],
            [fldb_FileData],
            [fldv_Description],
            [fldv_Category],
            [fldb_IsConfidential],
            [fldv_UploadedBy],
            [fldd_UploadedDate],
            [fldv_Tags],
            [fldb_IsActive],
            [fldv_CreatedBy],
            [fldd_CreatedDate]
        )
        VALUES (
            @pNewHazardFileCode,
            @pHazardCode,
            @pReportCode,
            @pFileName,
            @pFileType,
            @pContentType,
            @pFileSizeBytes,
            @pFileHash,
            @pStorageType,
            @pFilePath,
            @pFileData,
            @pDescription,
            @pCategory,
            @pIsConfidential,
            @pUploadedBy,
            @pCreatedDate,
            @pTags,
            1, -- fldb_IsActive = TRUE by default
            @pCreatedBy,
            @pCreatedDate
        );
        
        SET @pNewID = SCOPE_IDENTITY();
        SET @AuditMessage = 'Successfully inserted HazardFile with ID: ' + CAST(@pNewID AS VARCHAR(10)) + ', Code: ' + COALESCE(@pNewHazardFileCode, 'NULL') + ', FileName: ' + COALESCE(@pFileName, 'NULL');
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_HazardFiles', @pFunction = @FunctionName, @pDescription = @AuditMessage;
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error inserting HazardFile: ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @UserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Error', @pModule = 'SMS_HazardFiles', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;