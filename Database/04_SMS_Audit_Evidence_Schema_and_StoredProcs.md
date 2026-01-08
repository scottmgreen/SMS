# SMS Audit Evidence - Table Schema and Stored Procedures

## Table Definition: tbld_SMSAuditEvidence

```sql
-- ============================================= 
-- SMS AUDIT EVIDENCE TABLE SCHEMA
-- ============================================= 

USE [PDXSMS_V2]
GO

CREATE TABLE [dbo].[tbld_SMSAuditEvidence]
(
    -- Primary Key
    [fldi_ID] INT IDENTITY(1,1) NOT NULL,
    
    -- Business Key (Code)
    [fldv_Code] NVARCHAR(50) NOT NULL,
    
    -- Foreign Keys
    [fldv_AuditCode] NVARCHAR(50) NOT NULL, -- FK to tbld_SMSAudits
    [fldv_FindingCode] NVARCHAR(50) NULL,   -- FK to tbld_SMSAuditFindings (optional - evidence can exist without findings)
    
    -- Core Fields
    [fldv_Title] NVARCHAR(200) NOT NULL,
    [fldv_Description] NVARCHAR(2000) NOT NULL,
    [fldv_EvidenceType] NVARCHAR(60) NOT NULL, -- Document, Photo, Video, Interview, Observation, etc.
    [fldv_Source] NVARCHAR(200) NOT NULL,
    
    -- Collection Fields
    [fldv_CollectedBy] NVARCHAR(200) NOT NULL,
    [fldd_CollectionDate] DATETIME NOT NULL,
    
    -- File Management Fields
    [fldv_FilePath] NVARCHAR(500) NULL,
    [fldi_FileSize] BIGINT NULL,
    [fldv_ContentType] NVARCHAR(100) NULL,
    [fldv_StorageLocation] NVARCHAR(200) NULL, -- Local, Cloud, Archive, etc.
    
    -- Security and Compliance Fields
    [fldv_ConfidentialityLevel] NVARCHAR(20) NOT NULL DEFAULT 'Internal', -- Public, Internal, Confidential, Restricted
    [fldi_RetentionPeriodMonths] INT NOT NULL DEFAULT 84, -- 7 years default
    [fldv_RetentionReason] NVARCHAR(200) NULL,
    
    -- Verification Fields
    [fldb_IsVerified] BIT NOT NULL DEFAULT 0,
    [fldv_VerifiedBy] NVARCHAR(200) NULL,
    [fldd_VerificationDate] DATETIME NULL,
    
    -- Archival Fields
    [fldb_IsArchived] BIT NOT NULL DEFAULT 0,
    [fldd_ArchivedDate] DATETIME NULL,
    
    -- Additional Fields
    [fldv_Notes] NVARCHAR(2000) NULL,
    
    -- Audit fields (inherited from BaseAuditableEntity)
    [fldv_CreatedBy] VARCHAR(50) NOT NULL DEFAULT 'SYSTEM',
    [fldd_CreatedDate] DATETIME NOT NULL DEFAULT GETDATE(),
    [fldv_UpdatedBy] VARCHAR(50) NULL,
    [fldd_UpdatedDate] DATETIME NULL,
    
    -- Primary Key Constraint
    CONSTRAINT [PK_tbld_SMSAuditEvidence] PRIMARY KEY CLUSTERED ([fldi_ID] ASC),
    
    -- Unique Constraint on Code
    CONSTRAINT [UC_tbld_SMSAuditEvidence_Code] UNIQUE NONCLUSTERED ([fldv_Code] ASC),
    
    -- Foreign Key Constraints
    CONSTRAINT [FK_tbld_SMSAuditEvidence_Audit] FOREIGN KEY ([fldv_AuditCode]) 
        REFERENCES [dbo].[tbld_SMSAudits] ([fldv_Code]),
    CONSTRAINT [FK_tbld_SMSAuditEvidence_Finding] FOREIGN KEY ([fldv_FindingCode]) 
        REFERENCES [dbo].[tbld_SMSAuditFindings] ([fldv_Code])
)
ON [PRIMARY]
GO

-- Indexes for performance
CREATE INDEX [IX_tbld_SMSAuditEvidence_AuditCode] ON [dbo].[tbld_SMSAuditEvidence] ([fldv_AuditCode])
CREATE INDEX [IX_tbld_SMSAuditEvidence_FindingCode] ON [dbo].[tbld_SMSAuditEvidence] ([fldv_FindingCode])
CREATE INDEX [IX_tbld_SMSAuditEvidence_EvidenceType] ON [dbo].[tbld_SMSAuditEvidence] ([fldv_EvidenceType])
CREATE INDEX [IX_tbld_SMSAuditEvidence_CollectedBy] ON [dbo].[tbld_SMSAuditEvidence] ([fldv_CollectedBy])
CREATE INDEX [IX_tbld_SMSAuditEvidence_CollectionDate] ON [dbo].[tbld_SMSAuditEvidence] ([fldd_CollectionDate])
CREATE INDEX [IX_tbld_SMSAuditEvidence_ConfidentialityLevel] ON [dbo].[tbld_SMSAuditEvidence] ([fldv_ConfidentialityLevel])
CREATE INDEX [IX_tbld_SMSAuditEvidence_IsArchived] ON [dbo].[tbld_SMSAuditEvidence] ([fldb_IsArchived])
GO
```

## Stored Procedures

### 1. SMS Audit Evidence Insert

```sql
-- ============================================= 
-- SMS AUDIT EVIDENCE CRUD STORED PROCEDURES
-- ============================================= 

USE [PDXSMS_V2]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- ============================================= 
-- SMS AUDIT EVIDENCE INSERT OPERATION
-- ============================================= 
CREATE OR ALTER PROCEDURE [dbo].[pr_SMSAuditEvidence_Insert]
    @pCode NVARCHAR(50) = NULL,
    @pAuditCode NVARCHAR(50),
    @pFindingCode NVARCHAR(50) = NULL,
    @pTitle NVARCHAR(200),
    @pDescription NVARCHAR(2000),
    @pEvidenceType NVARCHAR(60),
    @pSource NVARCHAR(200),
    @pCollectedBy NVARCHAR(200),
    @pCollectionDate DATETIME,
    @pFilePath NVARCHAR(500) = NULL,
    @pFileSize BIGINT = NULL,
    @pContentType NVARCHAR(100) = NULL,
    @pStorageLocation NVARCHAR(200) = 'Local',
    @pConfidentialityLevel NVARCHAR(20) = 'Internal',
    @pRetentionPeriodMonths INT = 84,
    @pRetentionReason NVARCHAR(200) = NULL,
    @pIsVerified BIT = 0,
    @pIsArchived BIT = 0,
    @pNotes NVARCHAR(2000) = NULL,
    @pCreatedBy VARCHAR(50) = 'SYSTEM',
    @pCreatedDate DATETIME = NULL,
    @pNewID INT OUTPUT,
    @pNewAuditEvidenceCode NVARCHAR(50) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(10) = COALESCE(@pCreatedBy, 'SYSTEM'),
            @FunctionName VARCHAR(50) = 'pr_SMSAuditEvidence_Insert';
    
    IF @pCreatedDate IS NULL SET @pCreatedDate = GETDATE();
    
    BEGIN TRY
        EXEC [dbo].[sp_AddAuditLogEntry] 
            @pUserID = @UserID, 
            @pMessageType = 'SMS_CRUD', 
            @pSeverity = 'Information', 
            @pModule = 'SMS_AuditManagement', 
            @pFunction = @FunctionName, 
            @pDescription = 'Starting SMS Audit Evidence insert operation';

        -- Generate Audit Evidence Code
        EXEC [pr_GenerateFormattedCode] 
            @EntityName = 'SMSAuditEvidence',           -- Uses registry: Table='tbld_SMSAuditEvidence', Prefix='AE'
            @GeneratedCode = @pNewAuditEvidenceCode OUTPUT;

        INSERT INTO [dbo].[tbld_SMSAuditEvidence] (
            [fldv_Code], [fldv_AuditCode], [fldv_FindingCode], [fldv_Title], [fldv_Description], 
            [fldv_EvidenceType], [fldv_Source], [fldv_CollectedBy], [fldd_CollectionDate], 
            [fldv_FilePath], [fldi_FileSize], [fldv_ContentType], [fldv_StorageLocation], 
            [fldv_ConfidentialityLevel], [fldi_RetentionPeriodMonths], [fldv_RetentionReason], 
            [fldb_IsVerified], [fldb_IsArchived], [fldv_Notes], [fldv_CreatedBy], [fldd_CreatedDate]
        )
        VALUES (
            @pNewAuditEvidenceCode, @pAuditCode, @pFindingCode, @pTitle, @pDescription, 
            @pEvidenceType, @pSource, @pCollectedBy, @pCollectionDate, 
            @pFilePath, @pFileSize, @pContentType, @pStorageLocation, 
            @pConfidentialityLevel, @pRetentionPeriodMonths, @pRetentionReason, 
            @pIsVerified, @pIsArchived, @pNotes, @pCreatedBy, @pCreatedDate
        );

        SET @pNewID = SCOPE_IDENTITY();
        SET @AuditMessage = 'Successfully inserted SMS Audit Evidence with ID: ' + CAST(@pNewID AS VARCHAR(10)) + ', Code: ' + COALESCE(@pNewAuditEvidenceCode, 'NULL');
        
        EXEC [dbo].[sp_AddAuditLogEntry] 
            @pUserID = @UserID, 
            @pMessageType = 'SMS_CRUD', 
            @pSeverity = 'Information', 
            @pModule = 'SMS_AuditManagement', 
            @pFunction = @FunctionName, 
            @pDescription = @AuditMessage;
            
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error inserting SMS Audit Evidence: ' + @ErrorMessage;
        
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
```

### 2. SMS Audit Evidence Get By Code

```sql
-- ============================================= 
-- SMS AUDIT EVIDENCE GET BY CODE OPERATION
-- ============================================= 
CREATE OR ALTER PROCEDURE [dbo].[pr_SMSAuditEvidence_GetByCode]
    @pCode NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [fldi_ID],
        [fldv_Code],
        [fldv_AuditCode],
        [fldv_FindingCode],
        [fldv_Title],
        [fldv_Description],
        [fldv_EvidenceType],
        [fldv_Source],
        [fldv_CollectedBy],
        [fldd_CollectionDate],
        [fldv_FilePath],
        [fldi_FileSize],
        [fldv_ContentType],
        [fldv_StorageLocation],
        [fldv_ConfidentialityLevel],
        [fldi_RetentionPeriodMonths],
        [fldv_RetentionReason],
        [fldb_IsVerified],
        [fldv_VerifiedBy],
        [fldd_VerificationDate],
        [fldb_IsArchived],
        [fldd_ArchivedDate],
        [fldv_Notes],
        [fldv_CreatedBy],
        [fldd_CreatedDate],
        [fldv_UpdatedBy],
        [fldd_UpdatedDate]
    FROM [dbo].[tbld_SMSAuditEvidence]
    WHERE [fldv_Code] = @pCode;
END;
GO
```

### 3. SMS Audit Evidence Get All

```sql
-- ============================================= 
-- SMS AUDIT EVIDENCE GET ALL OPERATION
-- ============================================= 
CREATE OR ALTER PROCEDURE [dbo].[pr_SMSAuditEvidence_GetAll]
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [fldi_ID],
        [fldv_Code],
        [fldv_AuditCode],
        [fldv_FindingCode],
        [fldv_Title],
        [fldv_Description],
        [fldv_EvidenceType],
        [fldv_Source],
        [fldv_CollectedBy],
        [fldd_CollectionDate],
        [fldv_FilePath],
        [fldi_FileSize],
        [fldv_ContentType],
        [fldv_StorageLocation],
        [fldv_ConfidentialityLevel],
        [fldi_RetentionPeriodMonths],
        [fldv_RetentionReason],
        [fldb_IsVerified],
        [fldv_VerifiedBy],
        [fldd_VerificationDate],
        [fldb_IsArchived],
        [fldd_ArchivedDate],
        [fldv_Notes],
        [fldv_CreatedBy],
        [fldd_CreatedDate],
        [fldv_UpdatedBy],
        [fldd_UpdatedDate]
    FROM [dbo].[tbld_SMSAuditEvidence]
    WHERE [fldb_IsArchived] = 0
    ORDER BY [fldd_CreatedDate] DESC;
END;
GO
```

### 4. SMS Audit Evidence Get By Audit

```sql
-- ============================================= 
-- SMS AUDIT EVIDENCE GET BY AUDIT OPERATION
-- ============================================= 
CREATE OR ALTER PROCEDURE [dbo].[pr_SMSAuditEvidence_GetByAudit]
    @pAuditCode NVARCHAR(50),
    @pIncludeArchived BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [fldi_ID],
        [fldv_Code],
        [fldv_AuditCode],
        [fldv_FindingCode],
        [fldv_Title],
        [fldv_Description],
        [fldv_EvidenceType],
        [fldv_Source],
        [fldv_CollectedBy],
        [fldd_CollectionDate],
        [fldv_FilePath],
        [fldi_FileSize],
        [fldv_ContentType],
        [fldv_StorageLocation],
        [fldv_ConfidentialityLevel],
        [fldi_RetentionPeriodMonths],
        [fldv_RetentionReason],
        [fldb_IsVerified],
        [fldv_VerifiedBy],
        [fldd_VerificationDate],
        [fldb_IsArchived],
        [fldd_ArchivedDate],
        [fldv_Notes],
        [fldv_CreatedBy],
        [fldd_CreatedDate],
        [fldv_UpdatedBy],
        [fldd_UpdatedDate]
    FROM [dbo].[tbld_SMSAuditEvidence]
    WHERE [fldv_AuditCode] = @pAuditCode
      AND ([fldb_IsArchived] = 0 OR @pIncludeArchived = 1)
    ORDER BY [fldd_CollectionDate] DESC;
END;
GO
```

### 5. SMS Audit Evidence Get By Finding

```sql
-- ============================================= 
-- SMS AUDIT EVIDENCE GET BY FINDING OPERATION
-- ============================================= 
CREATE OR ALTER PROCEDURE [dbo].[pr_SMSAuditEvidence_GetByFinding]
    @pFindingCode NVARCHAR(50),
    @pIncludeArchived BIT = 0
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [fldi_ID],
        [fldv_Code],
        [fldv_AuditCode],
        [fldv_FindingCode],
        [fldv_Title],
        [fldv_Description],
        [fldv_EvidenceType],
        [fldv_Source],
        [fldv_CollectedBy],
        [fldd_CollectionDate],
        [fldv_FilePath],
        [fldi_FileSize],
        [fldv_ContentType],
        [fldv_StorageLocation],
        [fldv_ConfidentialityLevel],
        [fldi_RetentionPeriodMonths],
        [fldv_RetentionReason],
        [fldb_IsVerified],
        [fldv_VerifiedBy],
        [fldd_VerificationDate],
        [fldb_IsArchived],
        [fldd_ArchivedDate],
        [fldv_Notes],
        [fldv_CreatedBy],
        [fldd_CreatedDate],
        [fldv_UpdatedBy],
        [fldd_UpdatedDate]
    FROM [dbo].[tbld_SMSAuditEvidence]
    WHERE [fldv_FindingCode] = @pFindingCode
      AND ([fldb_IsArchived] = 0 OR @pIncludeArchived = 1)
    ORDER BY [fldd_CollectionDate] DESC;
END;
GO
```

### 6. SMS Audit Evidence Update

```sql
-- ============================================= 
-- SMS AUDIT EVIDENCE UPDATE OPERATION
-- ============================================= 
CREATE OR ALTER PROCEDURE [dbo].[pr_SMSAuditEvidence_Update]
    @pID INT,
    @pCode NVARCHAR(50),
    @pAuditCode NVARCHAR(50),
    @pFindingCode NVARCHAR(50) = NULL,
    @pTitle NVARCHAR(200),
    @pDescription NVARCHAR(2000),
    @pEvidenceType NVARCHAR(60),
    @pSource NVARCHAR(200),
    @pCollectedBy NVARCHAR(200),
    @pCollectionDate DATETIME,
    @pFilePath NVARCHAR(500) = NULL,
    @pFileSize BIGINT = NULL,
    @pContentType NVARCHAR(100) = NULL,
    @pStorageLocation NVARCHAR(200),
    @pConfidentialityLevel NVARCHAR(20),
    @pRetentionPeriodMonths INT,
    @pRetentionReason NVARCHAR(200) = NULL,
    @pIsVerified BIT = 0,
    @pVerifiedBy NVARCHAR(200) = NULL,
    @pVerificationDate DATETIME = NULL,
    @pIsArchived BIT = 0,
    @pArchivedDate DATETIME = NULL,
    @pNotes NVARCHAR(2000) = NULL,
    @pUpdatedBy VARCHAR(50),
    @pUpdatedDate DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(10) = COALESCE(@pUpdatedBy, 'SYSTEM'),
            @FunctionName VARCHAR(50) = 'pr_SMSAuditEvidence_Update';
    
    IF @pUpdatedDate IS NULL SET @pUpdatedDate = GETDATE();
    
    BEGIN TRY
        EXEC [dbo].[sp_AddAuditLogEntry] 
            @pUserID = @UserID, 
            @pMessageType = 'SMS_CRUD', 
            @pSeverity = 'Information', 
            @pModule = 'SMS_AuditManagement', 
            @pFunction = @FunctionName, 
            @pDescription = 'Starting SMS Audit Evidence update operation';

        UPDATE [dbo].[tbld_SMSAuditEvidence]
        SET 
            [fldv_AuditCode] = @pAuditCode,
            [fldv_FindingCode] = @pFindingCode,
            [fldv_Title] = @pTitle,
            [fldv_Description] = @pDescription,
            [fldv_EvidenceType] = @pEvidenceType,
            [fldv_Source] = @pSource,
            [fldv_CollectedBy] = @pCollectedBy,
            [fldd_CollectionDate] = @pCollectionDate,
            [fldv_FilePath] = @pFilePath,
            [fldi_FileSize] = @pFileSize,
            [fldv_ContentType] = @pContentType,
            [fldv_StorageLocation] = @pStorageLocation,
            [fldv_ConfidentialityLevel] = @pConfidentialityLevel,
            [fldi_RetentionPeriodMonths] = @pRetentionPeriodMonths,
            [fldv_RetentionReason] = @pRetentionReason,
            [fldb_IsVerified] = @pIsVerified,
            [fldv_VerifiedBy] = @pVerifiedBy,
            [fldd_VerificationDate] = @pVerificationDate,
            [fldb_IsArchived] = @pIsArchived,
            [fldd_ArchivedDate] = @pArchivedDate,
            [fldv_Notes] = @pNotes,
            [fldv_UpdatedBy] = @pUpdatedBy,
            [fldd_UpdatedDate] = @pUpdatedDate
        WHERE [fldi_ID] = @pID;

        SET @AuditMessage = 'Successfully updated SMS Audit Evidence with ID: ' + CAST(@pID AS VARCHAR(10)) + ', Code: ' + COALESCE(@pCode, 'NULL');
        
        EXEC [dbo].[sp_AddAuditLogEntry] 
            @pUserID = @UserID, 
            @pMessageType = 'SMS_CRUD', 
            @pSeverity = 'Information', 
            @pModule = 'SMS_AuditManagement', 
            @pFunction = @FunctionName, 
            @pDescription = @AuditMessage;
            
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error updating SMS Audit Evidence: ' + @ErrorMessage;
        
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
```

### 7. SMS Audit Evidence Delete

```sql
-- ============================================= 
-- SMS AUDIT EVIDENCE DELETE OPERATION
-- ============================================= 
CREATE OR ALTER PROCEDURE [dbo].[pr_SMSAuditEvidence_Delete]
    @pCode NVARCHAR(50),
    @pDeletedBy VARCHAR(50) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage VARCHAR(4000);
    DECLARE @UserID VARCHAR(10) = COALESCE(@pDeletedBy, 'SYSTEM'),
            @FunctionName VARCHAR(50) = 'pr_SMSAuditEvidence_Delete';
    
    BEGIN TRY
        EXEC [dbo].[sp_AddAuditLogEntry] 
            @pUserID = @UserID, 
            @pMessageType = 'SMS_CRUD', 
            @pSeverity = 'Information', 
            @pModule = 'SMS_AuditManagement', 
            @pFunction = @FunctionName, 
            @pDescription = 'Starting SMS Audit Evidence delete operation';

        DELETE FROM [dbo].[tbld_SMSAuditEvidence]
        WHERE [fldv_Code] = @pCode;

        SET @AuditMessage = 'Successfully deleted SMS Audit Evidence with Code: ' + COALESCE(@pCode, 'NULL');
        
        EXEC [dbo].[sp_AddAuditLogEntry] 
            @pUserID = @UserID, 
            @pMessageType = 'SMS_CRUD', 
            @pSeverity = 'Information', 
            @pModule = 'SMS_AuditManagement', 
            @pFunction = @FunctionName, 
            @pDescription = @AuditMessage;
            
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error deleting SMS Audit Evidence: ' + @ErrorMessage;
        
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
```

## Utility Procedures

### Archive Evidence

```sql
-- ============================================= 
-- UTILITY: ARCHIVE EVIDENCE
-- ============================================= 
CREATE OR ALTER PROCEDURE [dbo].[pr_SMSAuditEvidence_Archive]
    @pCode NVARCHAR(50),
    @pArchivedBy VARCHAR(50) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE [dbo].[tbld_SMSAuditEvidence]
    SET 
        [fldb_IsArchived] = 1,
        [fldd_ArchivedDate] = GETDATE(),
        [fldv_UpdatedBy] = @pArchivedBy,
        [fldd_UpdatedDate] = GETDATE()
    WHERE [fldv_Code] = @pCode;
END;
GO
```

### Get Evidence Due for Retention Review

```sql
-- ============================================= 
-- UTILITY: GET EVIDENCE DUE FOR RETENTION REVIEW
-- ============================================= 
CREATE OR ALTER PROCEDURE [dbo].[pr_SMSAuditEvidence_GetRetentionReview]
    @pMonthsFromNow INT = 6
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        [fldi_ID],
        [fldv_Code],
        [fldv_AuditCode],
        [fldv_Title],
        [fldv_EvidenceType],
        [fldv_CollectedBy],
        [fldd_CollectionDate],
        [fldi_RetentionPeriodMonths],
        [fldv_RetentionReason],
        DATEADD(MONTH, [fldi_RetentionPeriodMonths], [fldd_CollectionDate]) AS [RetentionExpiryDate],
        DATEDIFF(MONTH, GETDATE(), DATEADD(MONTH, [fldi_RetentionPeriodMonths], [fldd_CollectionDate])) AS [MonthsUntilExpiry]
    FROM [dbo].[tbld_SMSAuditEvidence]
    WHERE [fldb_IsArchived] = 0
      AND DATEADD(MONTH, [fldi_RetentionPeriodMonths], [fldd_CollectionDate]) <= DATEADD(MONTH, @pMonthsFromNow, GETDATE())
    ORDER BY DATEADD(MONTH, [fldi_RetentionPeriodMonths], [fldd_CollectionDate]) ASC;
END;
GO
```

## Code Generation Registry Entry

```sql
-- Registry entry for SMS Audit Evidence code generation
INSERT INTO [dbo].[tbld_CodeGenerationRegistry] ([fldv_EntityName], [fldv_TableName], [fldv_Prefix], [fldi_StartingNumber], [fldi_CurrentNumber])
VALUES ('SMSAuditEvidence', 'tbld_SMSAuditEvidence', 'AE', 1000, 1000);
GO
```

## Sample Data (Optional)

```sql
-- Sample SMS Audit Evidence data
INSERT INTO [dbo].[tbld_SMSAuditEvidence] (
    [fldv_Code], [fldv_AuditCode], [fldv_FindingCode], [fldv_Title], [fldv_Description], 
    [fldv_EvidenceType], [fldv_Source], [fldv_CollectedBy], [fldd_CollectionDate], 
    [fldv_ConfidentialityLevel], [fldv_CreatedBy]
)
VALUES 
    ('AE1001', 'AU1001', 'AF1001', 'Missing Training Documentation Photo', 'Photograph of training office showing incomplete documentation filing system', 'Photo', 'Training Department Office', 'John Smith', '2025-01-16', 'Internal', 'SYSTEM'),
    ('AE1002', 'AU1001', 'AF1002', 'Training Records Spreadsheet', 'Excel file showing incomplete training records for safety-critical staff', 'Document', 'HR Training Database', 'Jane Doe', '2025-01-17', 'Confidential', 'SYSTEM'),
    ('AE1003', 'AU1002', 'AF1003', 'Equipment Inspection Interview', 'Interview notes with maintenance supervisor regarding overdue inspections', 'Interview', 'Maintenance Supervisor', 'Bob Wilson', '2025-02-02', 'Internal', 'SYSTEM');
GO
```