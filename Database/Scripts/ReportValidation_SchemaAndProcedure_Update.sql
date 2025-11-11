-- =============================================
-- ReportValidation Table Schema and Stored Procedure Updates
-- PDXSMS_V2 Database Update Script
-- Date: $(date)
-- Purpose: Fix ReportValidation table schema and update all stored procedures
-- =============================================

USE [PDXSMS_V2]
GO

-- =============================================
-- STEP 1: BACKUP EXISTING DATA (if any exists)
-- =============================================
PRINT 'Step 1: Creating backup of existing ReportValidation data...'

-- Create temporary backup table
IF OBJECT_ID('tempdb..#ReportValidation_Backup') IS NOT NULL
    DROP TABLE #ReportValidation_Backup

SELECT * INTO #ReportValidation_Backup 
FROM [dbo].[tbld_ReportValidations]

DECLARE @BackupCount INT = @@ROWCOUNT
PRINT 'Backed up ' + CAST(@BackupCount AS VARCHAR(10)) + ' existing records'

-- =============================================
-- STEP 2: DROP EXISTING STORED PROCEDURES
-- =============================================
PRINT 'Step 2: Dropping existing stored procedures...'

IF OBJECT_ID('[dbo].[pr_ReportValidation_Delete]', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[pr_ReportValidation_Delete]

IF OBJECT_ID('[dbo].[pr_ReportValidation_GetAll]', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[pr_ReportValidation_GetAll]

IF OBJECT_ID('[dbo].[pr_ReportValidation_GetById]', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[pr_ReportValidation_GetById]

IF OBJECT_ID('[dbo].[pr_ReportValidation_Insert]', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[pr_ReportValidation_Insert]

IF OBJECT_ID('[dbo].[pr_ReportValidation_Update]', 'P') IS NOT NULL
    DROP PROCEDURE [dbo].[pr_ReportValidation_Update]

PRINT 'Existing stored procedures dropped'

-- =============================================
-- STEP 3: UPDATE TABLE SCHEMA
-- =============================================
PRINT 'Step 3: Updating table schema...'

-- Drop existing table (data is backed up in temp table)
IF OBJECT_ID('[dbo].[tbld_ReportValidations]', 'U') IS NOT NULL
    DROP TABLE [dbo].[tbld_ReportValidations]

-- Create new table with correct schema
CREATE TABLE [dbo].[tbld_ReportValidations](
    [fldi_ID] [int] IDENTITY(1,1) NOT NULL,
    [fldv_Code] [nvarchar](50) NOT NULL,
    [fldv_ReportCode] [nvarchar](50) NOT NULL,
    [fldv_ValidationDecision] [nvarchar](100) NULL,
    [fldv_Status] [nvarchar](50) NULL,
    [fldv_Stage] [nvarchar](50) NULL,
    [fldv_ValidatedBy] [nvarchar](50) NULL,
    [fldd_ValidatedDate] [datetime] NULL,
    [fldv_ValidationComments] [ntext] NULL,
    [fldv_ValidationType] [nvarchar](50) NULL,
    [fldv_CreatedBy] [nvarchar](50) NOT NULL,
    [fldd_CreatedDate] [datetime] NOT NULL,
    [fldv_UpdatedBy] [nvarchar](50) NULL,
    [fldd_UpdatedDate] [datetime] NULL,
    CONSTRAINT [PK_tbld_ReportValidations] PRIMARY KEY CLUSTERED ([fldi_ID] ASC)
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

PRINT 'Table schema updated successfully'

-- =============================================
-- STEP 4: RESTORE DATA WITH SCHEMA MAPPING
-- =============================================
PRINT 'Step 4: Restoring data with proper schema mapping...'

-- Restore data from backup with proper field mapping and defaults
INSERT INTO [dbo].[tbld_ReportValidations] (
    [fldv_Code],
    [fldv_ReportCode], 
    [fldv_ValidationDecision],
    [fldv_Status],
    [fldv_Stage],
    [fldv_ValidatedBy],
    [fldd_ValidatedDate],
    [fldv_ValidationComments],
    [fldv_ValidationType],
    [fldv_CreatedBy],
    [fldd_CreatedDate],
    [fldv_UpdatedBy],
    [fldd_UpdatedDate]
)
SELECT 
    RTRIM([fldv_Code]),                    -- Trim nchar padding
    RTRIM([fldv_ReportCode]),              -- Trim nchar padding  
    RTRIM([fldv_ValidationDecision]),      -- Trim nchar padding
    RTRIM([fldv_Status]),                  -- Trim nchar padding
    RTRIM([fldv_Stage]),                   -- Trim nchar padding
    'SYSTEM',                              -- Default ValidatedBy
    GETDATE(),                             -- Default ValidatedDate
    'Migrated from legacy schema',         -- Default ValidationComments
    'Standard',                            -- Default ValidationType
    'SYSTEM',                              -- Default CreatedBy
    GETDATE(),                             -- Default CreatedDate
    NULL,                                  -- UpdatedBy
    NULL                                   -- UpdatedDate
FROM #ReportValidation_Backup

DECLARE @RestoreCount INT = @@ROWCOUNT
PRINT 'Restored ' + CAST(@RestoreCount AS VARCHAR(10)) + ' records with new schema'

-- =============================================
-- STEP 5: CREATE UPDATED STORED PROCEDURES
-- =============================================
PRINT 'Step 5: Creating updated stored procedures...'

-- =============================================
-- INSERT Procedure
-- =============================================
CREATE PROCEDURE [dbo].[pr_ReportValidation_Insert]
    @pReportValidationCode NVARCHAR(50) = NULL,
    @pReportValidationReportCode NVARCHAR(50) = NULL,
    @pReportValidationDecision NVARCHAR(100) = NULL,
    @pReportValidationStatus NVARCHAR(50) = NULL,
    @pReportValidationStage NVARCHAR(50) = NULL,
    @pValidatedBy NVARCHAR(50) = NULL,
    @pValidatedDate DATETIME = NULL,
    @pValidationComments NTEXT = NULL,
    @pValidationType NVARCHAR(50) = 'Standard',
    @pCreatedBy NVARCHAR(50) = 'SYSTEM',
    @pCreatedDate DATETIME = NULL,
    @pNewID INT OUTPUT,
    @pNewReportValidationCode NVARCHAR(50) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage NVARCHAR(4000);
    DECLARE @UserID NVARCHAR(50) = COALESCE(@pCreatedBy, 'SYSTEM'), @FunctionName VARCHAR(50) = 'pr_ReportValidation_Insert';
    IF @pCreatedDate IS NULL SET @pCreatedDate = GETUTCDATE();
    
    BEGIN TRY
        -- Audit log entry
        EXEC [dbo].[sp_AddAuditLogEntry] 
            @pUserID = @UserID, 
            @pMessageType = 'SMS_CRUD', 
            @pSeverity = 'Information', 
            @pModule = 'SMS_ReportValidations', 
            @pFunction = @FunctionName, 
            @pDescription = 'Starting ReportValidation insert operation';
        
        -- Generate Code if not provided
        IF @pNewReportValidationCode IS NULL
        BEGIN
            EXEC [pr_GenerateFormattedCode] 
                @EntityName = 'ReportValidation',
                @GeneratedCode = @pNewReportValidationCode OUTPUT;
        END

        -- Insert record
        INSERT INTO [dbo].[tbld_ReportValidations] (
            [fldv_Code], 
            [fldv_ReportCode], 
            [fldv_ValidationDecision], 
            [fldv_Status], 
            [fldv_Stage],
            [fldv_ValidatedBy],
            [fldd_ValidatedDate],
            [fldv_ValidationComments],
            [fldv_ValidationType],
            [fldv_CreatedBy],
            [fldd_CreatedDate]
        )
        VALUES (
            @pNewReportValidationCode, 
            @pReportValidationReportCode, 
            @pReportValidationDecision, 
            @pReportValidationStatus, 
            @pReportValidationStage,
            @pValidatedBy,
            @pValidatedDate,
            @pValidationComments,
            @pValidationType,
            @UserID,
            @pCreatedDate
        );
        
        SET @pNewID = SCOPE_IDENTITY();
        
        SET @AuditMessage = 'Successfully inserted ReportValidation with ID: ' + CAST(@pNewID AS VARCHAR(10)) + ', Code: ' + COALESCE(@pNewReportValidationCode, 'NULL');
        EXEC [dbo].[sp_AddAuditLogEntry] 
            @pUserID = @UserID, 
            @pMessageType = 'SMS_CRUD', 
            @pSeverity = 'Information', 
            @pModule = 'SMS_ReportValidations', 
            @pFunction = @FunctionName, 
            @pDescription = @AuditMessage;
            
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error inserting ReportValidation: ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] 
            @pUserID = @UserID, 
            @pMessageType = 'SMS_CRUD', 
            @pSeverity = 'Error', 
            @pModule = 'SMS_ReportValidations', 
            @pFunction = @FunctionName, 
            @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

-- =============================================
-- UPDATE Procedure
-- =============================================
CREATE PROCEDURE [dbo].[pr_ReportValidation_Update]
    @pID NVARCHAR(50),
    @pReportValidationCode NVARCHAR(50) = NULL,
    @pReportValidationReportCode NVARCHAR(50) = NULL,
    @pReportValidationDecision NVARCHAR(100) = NULL,
    @pReportValidationStatus NVARCHAR(50) = NULL,
    @pReportValidationStage NVARCHAR(50) = NULL,
    @pValidatedBy NVARCHAR(50) = NULL,
    @pValidatedDate DATETIME = NULL,
    @pValidationComments NTEXT = NULL,
    @pValidationType NVARCHAR(50) = NULL,
    @pUpdatedBy NVARCHAR(50) = 'SYSTEM',
    @pUpdatedDate DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage NVARCHAR(4000);
    DECLARE @UserID NVARCHAR(50) = COALESCE(@pUpdatedBy, 'SYSTEM'), @FunctionName VARCHAR(50) = 'pr_ReportValidation_Update', @RowsAffected INT;
    IF @pUpdatedDate IS NULL SET @pUpdatedDate = GETUTCDATE();
    
    BEGIN TRY
        SET @AuditMessage = 'Starting ReportValidation update for ID: ' + @pID;
        EXEC [dbo].[sp_AddAuditLogEntry] 
            @pUserID = @UserID, 
            @pMessageType = 'SMS_CRUD', 
            @pSeverity = 'Information', 
            @pModule = 'SMS_ReportValidations', 
            @pFunction = @FunctionName, 
            @pDescription = @AuditMessage;
        
        UPDATE [dbo].[tbld_ReportValidations] 
        SET 
            [fldv_ReportCode] = COALESCE(@pReportValidationReportCode, [fldv_ReportCode]),
            [fldv_ValidationDecision] = COALESCE(@pReportValidationDecision, [fldv_ValidationDecision]),
            [fldv_Status] = COALESCE(@pReportValidationStatus, [fldv_Status]),
            [fldv_Stage] = COALESCE(@pReportValidationStage, [fldv_Stage]),
            [fldv_ValidatedBy] = COALESCE(@pValidatedBy, [fldv_ValidatedBy]),
            [fldd_ValidatedDate] = COALESCE(@pValidatedDate, [fldd_ValidatedDate]),
            [fldv_ValidationComments] = COALESCE(@pValidationComments, [fldv_ValidationComments]),
            [fldv_ValidationType] = COALESCE(@pValidationType, [fldv_ValidationType]),
            [fldv_UpdatedBy] = @UserID,
            [fldd_UpdatedDate] = @pUpdatedDate
        WHERE [fldv_Code] = @pID;
        
        SET @RowsAffected = @@ROWCOUNT;
        
        IF @RowsAffected = 0
        BEGIN
            SET @AuditMessage = 'No ReportValidation found with Code: ' + @pID;
            EXEC [dbo].[sp_AddAuditLogEntry] 
                @pUserID = @UserID, 
                @pMessageType = 'SMS_CRUD', 
                @pSeverity = 'Warning', 
                @pModule = 'SMS_ReportValidations', 
                @pFunction = @FunctionName, 
                @pDescription = @AuditMessage;
        END
        ELSE
        BEGIN
            SET @AuditMessage = 'Successfully updated ReportValidation Code: ' + @pID + ', Rows affected: ' + CAST(@RowsAffected AS VARCHAR(10));
            EXEC [dbo].[sp_AddAuditLogEntry] 
                @pUserID = @UserID, 
                @pMessageType = 'SMS_CRUD', 
                @pSeverity = 'Information', 
                @pModule = 'SMS_ReportValidations', 
                @pFunction = @FunctionName, 
                @pDescription = @AuditMessage;
        END
        
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error updating ReportValidation Code ' + @pID + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] 
            @pUserID = @UserID, 
            @pMessageType = 'SMS_CRUD', 
            @pSeverity = 'Error', 
            @pModule = 'SMS_ReportValidations', 
            @pFunction = @FunctionName, 
            @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

-- =============================================
-- GET BY ID Procedure
-- =============================================
CREATE PROCEDURE [dbo].[pr_ReportValidation_GetById]
    @pID NVARCHAR(50),
    @pUserID NVARCHAR(50) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage NVARCHAR(4000);
    DECLARE @FunctionName VARCHAR(50) = 'pr_ReportValidation_GetById';
    
    BEGIN TRY
        SET @AuditMessage = 'Retrieving ReportValidation with Code: ' + @pID;
        -- Uncomment for detailed auditing if needed
        --EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_ReportValidations', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
        SELECT 
            [fldi_ID], 
            [fldv_Code], 
            [fldv_ReportCode], 
            [fldv_ValidationDecision], 
            [fldv_Status], 
            [fldv_Stage],
            [fldv_ValidatedBy],
            [fldd_ValidatedDate],
            [fldv_ValidationComments],
            [fldv_ValidationType],
            [fldv_CreatedBy],
            [fldd_CreatedDate],
            [fldv_UpdatedBy],
            [fldd_UpdatedDate]
        FROM [dbo].[tbld_ReportValidations] 
        WHERE [fldv_Code] = @pID;
        
        -- Uncomment for detailed auditing if needed
        --SET @AuditMessage = 'Successfully retrieved ReportValidation with Code: ' + @pID;
        --EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_ReportValidations', @pFunction = @FunctionName, @pDescription = @AuditMessage;
        
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error retrieving ReportValidation Code ' + @pID + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] 
            @pUserID = @pUserID, 
            @pMessageType = 'SMS_CRUD', 
            @pSeverity = 'Error', 
            @pModule = 'SMS_ReportValidations', 
            @pFunction = @FunctionName, 
            @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

-- =============================================
-- GET ALL Procedure
-- =============================================
CREATE PROCEDURE [dbo].[pr_ReportValidation_GetAll]
    @pUserID NVARCHAR(50) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage NVARCHAR(4000);
    DECLARE @FunctionName VARCHAR(50) = 'pr_ReportValidation_GetAll', @RecordCount INT;
    
    BEGIN TRY
        -- Uncomment for detailed auditing if needed
        --EXEC [dbo].[sp_AddAuditLogEntry] @pUserID = @pUserID, @pMessageType = 'SMS_CRUD', @pSeverity = 'Information', @pModule = 'SMS_ReportValidations', @pFunction = @FunctionName, @pDescription = 'Retrieving all ReportValidations';
        
        SELECT 
            [fldi_ID], 
            [fldv_Code], 
            [fldv_ReportCode], 
            [fldv_ValidationDecision], 
            [fldv_Status], 
            [fldv_Stage],
            [fldv_ValidatedBy],
            [fldd_ValidatedDate],
            [fldv_ValidationComments],
            [fldv_ValidationType],
            [fldv_CreatedBy],
            [fldd_CreatedDate],
            [fldv_UpdatedBy],
            [fldd_UpdatedDate]
        FROM [dbo].[tbld_ReportValidations] 
        ORDER BY [fldv_Code];
        
        SET @RecordCount = @@ROWCOUNT;
        SET @AuditMessage = 'Successfully retrieved ' + CAST(@RecordCount AS VARCHAR(10)) + ' ReportValidations';
        EXEC [dbo].[sp_AddAuditLogEntry] 
            @pUserID = @pUserID, 
            @pMessageType = 'SMS_CRUD', 
            @pSeverity = 'Information', 
            @pModule = 'SMS_ReportValidations', 
            @pFunction = @FunctionName, 
            @pDescription = @AuditMessage;
            
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error retrieving all ReportValidations: ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] 
            @pUserID = @pUserID, 
            @pMessageType = 'SMS_CRUD', 
            @pSeverity = 'Error', 
            @pModule = 'SMS_ReportValidations', 
            @pFunction = @FunctionName, 
            @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

-- =============================================
-- DELETE Procedure
-- =============================================
CREATE PROCEDURE [dbo].[pr_ReportValidation_Delete]
    @pID NVARCHAR(50),
    @pUserID NVARCHAR(50) = 'SYSTEM'
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT, @AuditMessage NVARCHAR(4000);
    DECLARE @FunctionName VARCHAR(50) = 'pr_ReportValidation_Delete', @RowsAffected INT, @ReportValidationCode NVARCHAR(50);
    
    BEGIN TRY
        -- Get the code before deletion for audit purposes
        SELECT @ReportValidationCode = [fldv_Code] 
        FROM [dbo].[tbld_ReportValidations] 
        WHERE [fldv_Code] = @pID;
        
        SET @AuditMessage = 'Starting ReportValidation deletion for Code: ' + @pID + ', Found Code: ' + COALESCE(@ReportValidationCode, 'NULL');
        EXEC [dbo].[sp_AddAuditLogEntry] 
            @pUserID = @pUserID, 
            @pMessageType = 'SMS_CRUD', 
            @pSeverity = 'Warning', 
            @pModule = 'SMS_ReportValidations', 
            @pFunction = @FunctionName, 
            @pDescription = @AuditMessage;
        
        DELETE FROM [dbo].[tbld_ReportValidations] 
        WHERE [fldv_Code] = @pID;
        
        SET @RowsAffected = @@ROWCOUNT;
        
        IF @RowsAffected = 0
        BEGIN
            SET @AuditMessage = 'No ReportValidation found with Code: ' + @pID;
            EXEC [dbo].[sp_AddAuditLogEntry] 
                @pUserID = @pUserID, 
                @pMessageType = 'SMS_CRUD', 
                @pSeverity = 'Warning', 
                @pModule = 'SMS_ReportValidations', 
                @pFunction = @FunctionName, 
                @pDescription = @AuditMessage;
        END
        ELSE
        BEGIN
            SET @AuditMessage = 'Successfully deleted ReportValidation Code: ' + @pID + ', Rows affected: ' + CAST(@RowsAffected AS VARCHAR(10));
            EXEC [dbo].[sp_AddAuditLogEntry] 
                @pUserID = @pUserID, 
                @pMessageType = 'SMS_CRUD', 
                @pSeverity = 'Warning', 
                @pModule = 'SMS_ReportValidations', 
                @pFunction = @FunctionName, 
                @pDescription = @AuditMessage;
        END
        
    END TRY
    BEGIN CATCH
        SELECT @ErrorMessage = ERROR_MESSAGE(), @ErrorSeverity = ERROR_SEVERITY(), @ErrorState = ERROR_STATE();
        SET @AuditMessage = 'Error deleting ReportValidation Code ' + @pID + ': ' + @ErrorMessage;
        EXEC [dbo].[sp_AddAuditLogEntry] 
            @pUserID = @pUserID, 
            @pMessageType = 'SMS_CRUD', 
            @pSeverity = 'Error', 
            @pModule = 'SMS_ReportValidations', 
            @pFunction = @FunctionName, 
            @pDescription = @AuditMessage;
        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
GO

-- =============================================
-- STEP 6: ADD INDEXES FOR PERFORMANCE
-- =============================================
PRINT 'Step 6: Creating indexes for performance...'

-- Index on Code (most common lookup)
CREATE UNIQUE NONCLUSTERED INDEX [IX_ReportValidations_Code] 
ON [dbo].[tbld_ReportValidations]([fldv_Code] ASC)

-- Index on ReportCode (foreign key lookup)
CREATE NONCLUSTERED INDEX [IX_ReportValidations_ReportCode] 
ON [dbo].[tbld_ReportValidations]([fldv_ReportCode] ASC)

-- Index on Status for filtering
CREATE NONCLUSTERED INDEX [IX_ReportValidations_Status] 
ON [dbo].[tbld_ReportValidations]([fldv_Status] ASC)

-- Index on CreatedDate for sorting/filtering
CREATE NONCLUSTERED INDEX [IX_ReportValidations_CreatedDate] 
ON [dbo].[tbld_ReportValidations]([fldd_CreatedDate] ASC)

PRINT 'Indexes created successfully'

-- =============================================
-- STEP 7: UPDATE ENTITY REGISTRY (if it exists)
-- =============================================
PRINT 'Step 7: Updating entity registry...'

-- Update or insert ReportValidation in entity registry
IF EXISTS (SELECT 1 FROM [dbo].[tbld_EntityRegistry] WHERE [EntityName] = 'ReportValidation')
BEGIN
    UPDATE [dbo].[tbld_EntityRegistry] 
    SET 
        [TableName] = 'tbld_ReportValidations',
        [Prefix] = 'RV',
        [UpdatedBy] = 'SYSTEM',
        [UpdatedDate] = GETUTCDATE()
    WHERE [EntityName] = 'ReportValidation'
    
    PRINT 'Entity registry updated for ReportValidation'
END
ELSE IF OBJECT_ID('[dbo].[tbld_EntityRegistry]', 'U') IS NOT NULL
BEGIN
    INSERT INTO [dbo].[tbld_EntityRegistry] (
        [EntityName], 
        [TableName], 
        [Prefix], 
        [CreatedBy], 
        [CreatedDate]
    )
    VALUES (
        'ReportValidation', 
        'tbld_ReportValidations', 
        'RV', 
        'SYSTEM', 
        GETUTCDATE()
    )
    
    PRINT 'Entity registry entry created for ReportValidation'
END
ELSE
BEGIN
    PRINT 'Entity registry table not found - skipping registry update'
END

-- =============================================
-- STEP 8: VERIFICATION AND CLEANUP
-- =============================================
PRINT 'Step 8: Verification and cleanup...'

-- Verify table structure
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'tbld_ReportValidations'
ORDER BY ORDINAL_POSITION

-- Verify stored procedures exist
SELECT 
    ROUTINE_NAME,
    ROUTINE_TYPE,
    CREATED,
    LAST_ALTERED
FROM INFORMATION_SCHEMA.ROUTINES 
WHERE ROUTINE_NAME LIKE 'pr_ReportValidation_%'
ORDER BY ROUTINE_NAME

-- Show final record count
SELECT COUNT(*) as 'Total Records in tbld_ReportValidations' 
FROM [dbo].[tbld_ReportValidations]

-- Clean up temp table
IF OBJECT_ID('tempdb..#ReportValidation_Backup') IS NOT NULL
    DROP TABLE #ReportValidation_Backup

PRINT '============================================='
PRINT 'ReportValidation table and procedures update COMPLETED successfully!'
PRINT 'Schema fixes applied:'
PRINT '- Fixed data types (nchar to nvarchar)'
PRINT '- Added audit fields (CreatedBy, CreatedDate, UpdatedBy, UpdatedDate)'
PRINT '- Added validation fields (ValidatedBy, ValidatedDate, etc.)'
PRINT '- Updated all stored procedures with correct parameter types'
PRINT '- Added performance indexes'
PRINT '- Updated entity registry'
PRINT '============================================='