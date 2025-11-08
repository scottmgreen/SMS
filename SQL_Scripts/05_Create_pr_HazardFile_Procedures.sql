-- =============================================
-- SMS Hazard Files - Stored Procedures
-- Comprehensive CRUD operations for hazard file management
-- =============================================

-- =============================================
-- INSERT - Create new hazard file record
-- =============================================
CREATE OR ALTER PROCEDURE pr_HazardFile_Insert
    @pCode NVARCHAR(50),
    @pHazardCode NVARCHAR(50),
    @pReportCode NVARCHAR(50) = NULL,
    @pFileName NVARCHAR(255),
    @pFileType NVARCHAR(50),
    @pContentType NVARCHAR(100),
    @pFileSizeBytes BIGINT,
    @pFileHash NVARCHAR(128) = NULL,
    @pStorageType NVARCHAR(20) = 'FileSystem',
    @pFilePath NVARCHAR(500) = NULL,
    @pFileData VARBINARY(MAX) = NULL,
    @pDescription NVARCHAR(1000) = NULL,
    @pCategory NVARCHAR(50) = NULL,
    @pIsConfidential BIT = 0,
    @pUploadedBy NVARCHAR(50),
    @pTags NVARCHAR(500) = NULL,
    @pCreatedBy NVARCHAR(50) = 'SYSTEM',
    @pCreatedDate DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Set default created date if not provided
    IF @pCreatedDate IS NULL
        SET @pCreatedDate = GETUTCDATE();
    
    BEGIN TRY
        INSERT INTO tbld_HazardFiles (
            fldv_Code,
            fldv_HazardCode,
            fldv_ReportCode,
            fldv_FileName,
            fldv_FileType,
            fldv_ContentType,
            fldi_FileSizeBytes,
            fldv_FileHash,
            fldv_StorageType,
            fldv_FilePath,
            fldb_FileData,
            fldv_Description,
            fldv_Category,
            fldb_IsConfidential,
            fldv_UploadedBy,
            fldd_UploadedDate,
            fldv_Tags,
            fldv_CreatedBy,
            fldd_CreatedDate
        )
        VALUES (
            @pCode,
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
            @pCreatedBy,
            @pCreatedDate
        );
        
        -- Return the inserted record
        SELECT * FROM tbld_HazardFiles WHERE fldv_Code = @pCode;
        
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

-- =============================================
-- SELECT BY ID - Get hazard file by ID
-- =============================================
CREATE OR ALTER PROCEDURE pr_HazardFile_GetById
    @pID INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT * FROM tbld_HazardFiles 
    WHERE fldi_ID = @pID AND fldb_IsActive = 1;
END
GO

-- =============================================
-- SELECT BY CODE - Get hazard file by Code
-- =============================================
CREATE OR ALTER PROCEDURE pr_HazardFile_GetByCode
    @pCode NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT * FROM tbld_HazardFiles 
    WHERE fldv_Code = @pCode AND fldb_IsActive = 1;
END
GO

-- =============================================
-- SELECT BY HAZARD CODE - Get all files for a hazard
-- =============================================
CREATE OR ALTER PROCEDURE pr_HazardFile_GetByHazardCode
    @pHazardCode NVARCHAR(50),
    @pIncludeFileData BIT = 0,
    @pCategory NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @pIncludeFileData = 1
    BEGIN
        -- Include file data in results
        SELECT * FROM tbld_HazardFiles 
        WHERE fldv_HazardCode = @pHazardCode 
        AND fldb_IsActive = 1
        AND (@pCategory IS NULL OR fldv_Category = @pCategory)
        ORDER BY fldd_UploadedDate DESC;
    END
    ELSE
    BEGIN
        -- Exclude file data for performance
        SELECT 
            fldi_ID,
            fldv_Code,
            fldv_HazardCode,
            fldv_ReportCode,
            fldv_FileName,
            fldv_FileType,
            fldv_ContentType,
            fldi_FileSizeBytes,
            fldv_FileHash,
            fldv_StorageType,
            fldv_FilePath,
            fldv_Description,
            fldv_Category,
            fldb_IsConfidential,
            fldv_UploadedBy,
            fldd_UploadedDate,
            fldv_Tags,
            fldv_CreatedBy,
            fldd_CreatedDate,
            fldv_UpdatedBy,
            fldd_UpdatedDate,
            fldb_IsActive
        FROM tbld_HazardFiles 
        WHERE fldv_HazardCode = @pHazardCode 
        AND fldb_IsActive = 1
        AND (@pCategory IS NULL OR fldv_Category = @pCategory)
        ORDER BY fldd_UploadedDate DESC;
    END
END
GO

-- =============================================
-- SELECT BY REPORT CODE - Get all files for a report
-- =============================================
CREATE OR ALTER PROCEDURE pr_HazardFile_GetByReportCode
    @pReportCode NVARCHAR(50),
    @pIncludeFileData BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    
    IF @pIncludeFileData = 1
    BEGIN
        SELECT * FROM tbld_HazardFiles 
        WHERE fldv_ReportCode = @pReportCode 
        AND fldb_IsActive = 1
        ORDER BY fldd_UploadedDate DESC;
    END
    ELSE
    BEGIN
        SELECT 
            fldi_ID,
            fldv_Code,
            fldv_HazardCode,
            fldv_ReportCode,
            fldv_FileName,
            fldv_FileType,
            fldv_ContentType,
            fldi_FileSizeBytes,
            fldv_FileHash,
            fldv_StorageType,
            fldv_FilePath,
            fldv_Description,
            fldv_Category,
            fldb_IsConfidential,
            fldv_UploadedBy,
            fldd_UploadedDate,
            fldv_Tags,
            fldv_CreatedBy,
            fldd_CreatedDate,
            fldv_UpdatedBy,
            fldd_UpdatedDate,
            fldb_IsActive
        FROM tbld_HazardFiles 
        WHERE fldv_ReportCode = @pReportCode 
        AND fldb_IsActive = 1
        ORDER BY fldd_UploadedDate DESC;
    END
END
GO

-- =============================================
-- GET FILE DATA - Get only the binary file data
-- =============================================
CREATE OR ALTER PROCEDURE pr_HazardFile_GetFileData
    @pCode NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        fldv_Code,
        fldv_FileName,
        fldv_FileType,
        fldv_ContentType,
        fldi_FileSizeBytes,
        fldb_FileData
    FROM tbld_HazardFiles 
    WHERE fldv_Code = @pCode 
    AND fldb_IsActive = 1
    AND fldv_StorageType = 'Database';
END
GO

-- =============================================
-- UPDATE - Update hazard file metadata (not file data)
-- =============================================
CREATE OR ALTER PROCEDURE pr_HazardFile_Update
    @pID INT,
    @pFileName NVARCHAR(255) = NULL,
    @pDescription NVARCHAR(1000) = NULL,
    @pCategory NVARCHAR(50) = NULL,
    @pIsConfidential BIT = NULL,
    @pTags NVARCHAR(500) = NULL,
    @pUpdatedBy NVARCHAR(50) = 'SYSTEM',
    @pUpdatedDate DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Set default updated date if not provided
    IF @pUpdatedDate IS NULL
        SET @pUpdatedDate = GETUTCDATE();
    
    BEGIN TRY
        UPDATE tbld_HazardFiles 
        SET 
            fldv_FileName = ISNULL(@pFileName, fldv_FileName),
            fldv_Description = ISNULL(@pDescription, fldv_Description),
            fldv_Category = ISNULL(@pCategory, fldv_Category),
            fldb_IsConfidential = ISNULL(@pIsConfidential, fldb_IsConfidential),
            fldv_Tags = ISNULL(@pTags, fldv_Tags),
            fldv_UpdatedBy = @pUpdatedBy,
            fldd_UpdatedDate = @pUpdatedDate
        WHERE fldi_ID = @pID AND fldb_IsActive = 1;
        
        -- Return the updated record
        SELECT * FROM tbld_HazardFiles WHERE fldi_ID = @pID;
        
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

-- =============================================
-- SOFT DELETE - Deactivate hazard file
-- =============================================
CREATE OR ALTER PROCEDURE pr_HazardFile_Deactivate
    @pID INT,
    @pInactiveReason NVARCHAR(500),
    @pInactiveBy NVARCHAR(50) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        UPDATE tbld_HazardFiles 
        SET 
            fldb_IsActive = 0,
            fldv_InactiveReason = @pInactiveReason,
            fldd_InactiveDate = GETUTCDATE(),
            fldv_InactiveBy = @pInactiveBy,
            fldv_UpdatedBy = @pInactiveBy,
            fldd_UpdatedDate = GETUTCDATE()
        WHERE fldi_ID = @pID;
        
        SELECT @@ROWCOUNT as RowsAffected;
        
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

-- =============================================
-- REACTIVATE - Reactivate hazard file
-- =============================================
CREATE OR ALTER PROCEDURE pr_HazardFile_Reactivate
    @pID INT,
    @pUpdatedBy NVARCHAR(50) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRY
        UPDATE tbld_HazardFiles 
        SET 
            fldb_IsActive = 1,
            fldv_InactiveReason = NULL,
            fldd_InactiveDate = NULL,
            fldv_InactiveBy = NULL,
            fldv_UpdatedBy = @pUpdatedBy,
            fldd_UpdatedDate = GETUTCDATE()
        WHERE fldi_ID = @pID;
        
        SELECT @@ROWCOUNT as RowsAffected;
        
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        DECLARE @ErrorSeverity INT = ERROR_SEVERITY();
        DECLARE @ErrorState INT = ERROR_STATE();
        
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO

-- =============================================
-- SEARCH FILES - Search files by various criteria
-- =============================================
CREATE OR ALTER PROCEDURE pr_HazardFile_Search
    @pHazardCode NVARCHAR(50) = NULL,
    @pReportCode NVARCHAR(50) = NULL,
    @pFileType NVARCHAR(50) = NULL,
    @pCategory NVARCHAR(50) = NULL,
    @pSearchText NVARCHAR(255) = NULL,
    @pUploadedBy NVARCHAR(50) = NULL,
    @pDateFrom DATETIME = NULL,
    @pDateTo DATETIME = NULL,
    @pIncludeConfidential BIT = 0,
    @pMaxResults INT = 100
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT TOP (@pMaxResults)
        fldi_ID,
        fldv_Code,
        fldv_HazardCode,
        fldv_ReportCode,
        fldv_FileName,
        fldv_FileType,
        fldv_ContentType,
        fldi_FileSizeBytes,
        fldv_StorageType,
        fldv_Description,
        fldv_Category,
        fldb_IsConfidential,
        fldv_UploadedBy,
        fldd_UploadedDate,
        fldv_Tags
    FROM tbld_HazardFiles 
    WHERE fldb_IsActive = 1
    AND (@pHazardCode IS NULL OR fldv_HazardCode = @pHazardCode)
    AND (@pReportCode IS NULL OR fldv_ReportCode = @pReportCode)
    AND (@pFileType IS NULL OR fldv_FileType = @pFileType)
    AND (@pCategory IS NULL OR fldv_Category = @pCategory)
    AND (@pUploadedBy IS NULL OR fldv_UploadedBy = @pUploadedBy)
    AND (@pDateFrom IS NULL OR fldd_UploadedDate >= @pDateFrom)
    AND (@pDateTo IS NULL OR fldd_UploadedDate <= @pDateTo)
    AND (@pIncludeConfidential = 1 OR fldb_IsConfidential = 0)
    AND (
        @pSearchText IS NULL 
        OR fldv_FileName LIKE '%' + @pSearchText + '%'
        OR fldv_Description LIKE '%' + @pSearchText + '%'
        OR fldv_Tags LIKE '%' + @pSearchText + '%'
    )
    ORDER BY fldd_UploadedDate DESC;
END
GO

-- =============================================
-- GET FILE STATISTICS - Get file statistics for a hazard
-- =============================================
CREATE OR ALTER PROCEDURE pr_HazardFile_GetStatistics
    @pHazardCode NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        @pHazardCode as HazardCode,
        COUNT(*) as TotalFiles,
        SUM(fldi_FileSizeBytes) as TotalSizeBytes,
        COUNT(CASE WHEN fldv_Category = 'Photo' THEN 1 END) as PhotoCount,
        COUNT(CASE WHEN fldv_Category = 'Document' THEN 1 END) as DocumentCount,
        COUNT(CASE WHEN fldv_Category = 'Video' THEN 1 END) as VideoCount,
        COUNT(CASE WHEN fldb_IsConfidential = 1 THEN 1 END) as ConfidentialCount,
        MIN(fldd_UploadedDate) as FirstUploadDate,
        MAX(fldd_UploadedDate) as LastUploadDate
    FROM tbld_HazardFiles 
    WHERE fldv_HazardCode = @pHazardCode AND fldb_IsActive = 1;
END
GO

PRINT 'Hazard File stored procedures created successfully';