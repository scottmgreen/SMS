-- =============================================
-- SMS Hazard File Support - Table and Stored Procedures
-- Supports multiple file types with both database and file system storage options
-- =============================================

-- Create the Hazard Files table
CREATE TABLE tbld_HazardFiles (
    fldi_ID INT IDENTITY(1,1) NOT NULL,
    fldv_Code NVARCHAR(50) NOT NULL,                    -- HF-YYYYMMDD-XXXXXXXX
    fldv_HazardCode NVARCHAR(50) NOT NULL,              -- FK to tbld_Hazards
    fldv_ReportCode NVARCHAR(50) NULL,                  -- FK to tbld_Reports (optional)
    fldv_FileName NVARCHAR(255) NOT NULL,               -- Original file name
    fldv_FileType NVARCHAR(50) NOT NULL,                -- doc, pdf, jpg, png, mp4, etc.
    fldv_ContentType NVARCHAR(100) NOT NULL,            -- MIME type (application/pdf, image/jpeg, etc.)
    fldi_FileSizeBytes BIGINT NOT NULL,                 -- File size in bytes
    fldv_FileHash NVARCHAR(128) NULL,                   -- SHA-256 hash for integrity verification
    fldv_StorageType NVARCHAR(20) NOT NULL DEFAULT 'FileSystem', -- 'Database', 'FileSystem', 'Cloud'
    fldv_FilePath NVARCHAR(500) NULL,                   -- Path for file system storage
    fldb_FileData VARBINARY(MAX) NULL,                  -- Binary data for database storage
    fldv_Description NVARCHAR(1000) NULL,               -- File description
    fldv_Category NVARCHAR(50) NULL,                    -- Photo, Document, Video, Audio, Evidence, etc.
    fldb_IsConfidential BIT NOT NULL DEFAULT 0,         -- Confidential file flag
    fldv_UploadedBy NVARCHAR(50) NOT NULL,              -- Who uploaded the file
    fldd_UploadedDate DATETIME NOT NULL DEFAULT GETUTCDATE(),
    fldv_Tags NVARCHAR(500) NULL,                       -- Searchable tags (comma-separated)
    fldb_IsActive BIT NOT NULL DEFAULT 1,               -- Soft delete flag
    fldv_InactiveReason NVARCHAR(500) NULL,             -- Reason for deactivation
    fldd_InactiveDate DATETIME NULL,                    -- When deactivated
    fldv_InactiveBy NVARCHAR(50) NULL,                  -- Who deactivated
    
    -- Audit fields
    fldv_CreatedBy NVARCHAR(50) NOT NULL DEFAULT 'SYSTEM',
    fldd_CreatedDate DATETIME NOT NULL DEFAULT GETUTCDATE(),
    fldv_UpdatedBy NVARCHAR(50) NULL,
    fldd_UpdatedDate DATETIME NULL,
    
    -- Primary Key
    CONSTRAINT PK_tbld_HazardFiles PRIMARY KEY (fldi_ID),
    
    -- Unique constraint on code
    CONSTRAINT UK_tbld_HazardFiles_Code UNIQUE (fldv_Code),
    
    -- Foreign key constraints (uncomment when tables exist)
    -- CONSTRAINT FK_tbld_HazardFiles_Hazard FOREIGN KEY (fldv_HazardCode) REFERENCES tbld_Hazards(fldv_Code),
    -- CONSTRAINT FK_tbld_HazardFiles_Report FOREIGN KEY (fldv_ReportCode) REFERENCES tbld_Reports(fldv_Code),
    
    -- Check constraints
    CONSTRAINT CK_tbld_HazardFiles_FileSizeBytes CHECK (fldi_FileSizeBytes >= 0),
    CONSTRAINT CK_tbld_HazardFiles_StorageType CHECK (fldv_StorageType IN ('Database', 'FileSystem', 'Cloud')),
    CONSTRAINT CK_tbld_HazardFiles_FileType CHECK (LEN(fldv_FileType) > 0)
);

-- Create indexes for performance
CREATE NONCLUSTERED INDEX IX_tbld_HazardFiles_HazardCode ON tbld_HazardFiles (fldv_HazardCode) INCLUDE (fldv_Code, fldv_FileName, fldb_IsActive);
CREATE NONCLUSTERED INDEX IX_tbld_HazardFiles_ReportCode ON tbld_HazardFiles (fldv_ReportCode) WHERE fldv_ReportCode IS NOT NULL;
CREATE NONCLUSTERED INDEX IX_tbld_HazardFiles_FileType ON tbld_HazardFiles (fldv_FileType) INCLUDE (fldv_FileName);
CREATE NONCLUSTERED INDEX IX_tbld_HazardFiles_Category ON tbld_HazardFiles (fldv_Category) WHERE fldv_Category IS NOT NULL;
CREATE NONCLUSTERED INDEX IX_tbld_HazardFiles_UploadedDate ON tbld_HazardFiles (fldd_UploadedDate DESC) INCLUDE (fldv_HazardCode, fldv_FileName);
CREATE NONCLUSTERED INDEX IX_tbld_HazardFiles_Active ON tbld_HazardFiles (fldb_IsActive) INCLUDE (fldv_Code, fldv_HazardCode);

PRINT 'tbld_HazardFiles table created successfully with indexes';