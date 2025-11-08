-- =====================================================================================================================
-- SMS Stored Procedures: Hazard Locations CRUD Operations (Part 2)
-- Purpose: Update and Delete operations with SMS standard error handling
-- =====================================================================================================================

-- =====================================================================================================================
-- STORED PROCEDURE: pr_HazardLocation_Update
-- Purpose: Update existing hazard location with full validation and error handling
-- =====================================================================================================================

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[pr_HazardLocation_Update]') AND type in (N'P'))
    DROP PROCEDURE [dbo].[pr_HazardLocation_Update]
GO

CREATE PROCEDURE [dbo].[pr_HazardLocation_Update]
(
    @pID NVARCHAR(50),                 -- Can be numeric ID or Code
    @pCode NVARCHAR(50) = NULL,        -- Allow code updates
    @pHazardCode NVARCHAR(50) = NULL,
    @pLatitude DECIMAL(10,8) = NULL,
    @pLongitude DECIMAL(11,8) = NULL,
    @pDescription NVARCHAR(500) = NULL,
    @pDateSelected DATETIME2(7) = NULL,
    @pLocationMapSVG NVARCHAR(MAX) = NULL,
    @pLocationArea NVARCHAR(100) = NULL,
    @pLocationSubArea NVARCHAR(100) = NULL,
    @pLocationName NVARCHAR(200) = NULL,
    @pAccuracyMeters DECIMAL(8,2) = NULL,
    @pElevationFeet DECIMAL(8,2) = NULL,
    @pSource NVARCHAR(50) = NULL,
    @pStatus NVARCHAR(20) = NULL,
    @pIsValidated BIT = NULL,
    @pValidatedDate DATETIME2(7) = NULL,
    @pValidatedBy NVARCHAR(50) = NULL,
    @pNotes NVARCHAR(1000) = NULL,
    @pTags NVARCHAR(500) = NULL,
    @pUpdatedBy NVARCHAR(50),
    @pUpdatedDate DATETIME2(7) = NULL
)
AS
BEGIN
    SET NOCOUNT ON
    
    DECLARE @ErrorMessage NVARCHAR(4000)
    DECLARE @ErrorSeverity INT
    DECLARE @ErrorState INT
    DECLARE @CurrentID INT
    DECLARE @CurrentCode NVARCHAR(50)
    
    BEGIN TRY
        -- Input validation
        IF @pID IS NULL OR LTRIM(RTRIM(@pID)) = ''
        BEGIN
            RAISERROR('ID parameter is required and cannot be empty.', 16, 1)
            RETURN
        END
        
        IF @pUpdatedBy IS NULL OR LTRIM(RTRIM(@pUpdatedBy)) = ''
        BEGIN
            RAISERROR('UpdatedBy is required and cannot be empty.', 16, 1)
            RETURN
        END
        
        -- Find the record to update
        IF EXISTS (SELECT 1 FROM [dbo].[tbld_HazardLocations] WHERE [fldv_Code] = @pID)
        BEGIN
            SELECT @CurrentID = [fldi_ID], @CurrentCode = [fldv_Code]
            FROM [dbo].[tbld_HazardLocations] 
            WHERE [fldv_Code] = @pID
        END
        ELSE IF ISNUMERIC(@pID) = 1 AND EXISTS (SELECT 1 FROM [dbo].[tbld_HazardLocations] WHERE [fldi_ID] = CAST(@pID AS INT))
        BEGIN
            SELECT @CurrentID = [fldi_ID], @CurrentCode = [fldv_Code]
            FROM [dbo].[tbld_HazardLocations] 
            WHERE [fldi_ID] = CAST(@pID AS INT)
        END
        ELSE
        BEGIN
            RAISERROR('HazardLocation with ID %s not found.', 16, 1, @pID)
            RETURN
        END
        
        -- Validate coordinates if provided
        IF (@pLatitude IS NOT NULL AND (@pLatitude < -90 OR @pLatitude > 90))
        BEGIN
            RAISERROR('Latitude must be between -90 and 90 degrees.', 16, 1)
            RETURN
        END
        
        IF (@pLongitude IS NOT NULL AND (@pLongitude < -180 OR @pLongitude > 180))
        BEGIN
            RAISERROR('Longitude must be between -180 and 180 degrees.', 16, 1)
            RETURN
        END
        
        -- Validate status if provided
        IF @pStatus IS NOT NULL AND @pStatus NOT IN ('Active', 'Inactive', 'Pending')
        BEGIN
            RAISERROR('Status must be Active, Inactive, or Pending.', 16, 1)
            RETURN
        END
        
        -- Validate source if provided
        IF @pSource IS NOT NULL AND @pSource NOT IN ('GPS', 'Manual', 'Import', 'Survey', 'Estimated')
        BEGIN
            RAISERROR('Source must be GPS, Manual, Import, Survey, or Estimated.', 16, 1)
            RETURN
        END
        
        -- Check for duplicate codes if code is being changed
        IF @pCode IS NOT NULL AND @pCode <> @CurrentCode
        BEGIN
            IF EXISTS (SELECT 1 FROM [dbo].[tbld_HazardLocations] WHERE [fldv_Code] = @pCode AND [fldi_ID] <> @CurrentID)
            BEGIN
                RAISERROR('HazardLocation with code %s already exists.', 16, 1, @pCode)
                RETURN
            END
        END
        
        -- Set defaults
        SET @pUpdatedDate = ISNULL(@pUpdatedDate, GETUTCDATE())
        
        -- Begin transaction
        BEGIN TRANSACTION
        
        -- Build dynamic UPDATE statement to only update provided fields
        DECLARE @SQL NVARCHAR(MAX) = 'UPDATE [dbo].[tbld_HazardLocations] SET '
        DECLARE @UpdateFields NVARCHAR(MAX) = ''
        DECLARE @Params NVARCHAR(MAX) = ''
        
        -- Add fields to update if they are provided (not NULL)
        IF @pCode IS NOT NULL
        BEGIN
            SET @UpdateFields = @UpdateFields + '[fldv_Code] = @pCode, '
        END
        
        IF @pHazardCode IS NOT NULL
        BEGIN
            SET @UpdateFields = @UpdateFields + '[fldv_HazardCode] = @pHazardCode, '
        END
        
        IF @pLatitude IS NOT NULL
        BEGIN
            SET @UpdateFields = @UpdateFields + '[fldv_Latitude] = @pLatitude, '
        END
        
        IF @pLongitude IS NOT NULL
        BEGIN
            SET @UpdateFields = @UpdateFields + '[fldv_Longitude] = @pLongitude, '
        END
        
        IF @pDescription IS NOT NULL
        BEGIN
            SET @UpdateFields = @UpdateFields + '[fldv_Description] = @pDescription, '
        END
        
        IF @pDateSelected IS NOT NULL
        BEGIN
            SET @UpdateFields = @UpdateFields + '[fldd_DateSelected] = @pDateSelected, '
        END
        
        IF @pLocationMapSVG IS NOT NULL
        BEGIN
            SET @UpdateFields = @UpdateFields + '[fldv_LocationMapSVG] = @pLocationMapSVG, '
        END
        
        IF @pLocationArea IS NOT NULL
        BEGIN
            SET @UpdateFields = @UpdateFields + '[fldv_LocationArea] = @pLocationArea, '
        END
        
        IF @pLocationSubArea IS NOT NULL
        BEGIN
            SET @UpdateFields = @UpdateFields + '[fldv_LocationSubArea] = @pLocationSubArea, '
        END
        
        IF @pLocationName IS NOT NULL
        BEGIN
            SET @UpdateFields = @UpdateFields + '[fldv_LocationName] = @pLocationName, '
        END
        
        IF @pAccuracyMeters IS NOT NULL
        BEGIN
            SET @UpdateFields = @UpdateFields + '[fldv_AccuracyMeters] = @pAccuracyMeters, '
        END
        
        IF @pElevationFeet IS NOT NULL
        BEGIN
            SET @UpdateFields = @UpdateFields + '[fldv_ElevationFeet] = @pElevationFeet, '
        END
        
        IF @pSource IS NOT NULL
        BEGIN
            SET @UpdateFields = @UpdateFields + '[fldv_Source] = @pSource, '
        END
        
        IF @pStatus IS NOT NULL
        BEGIN
            SET @UpdateFields = @UpdateFields + '[fldv_Status] = @pStatus, '
        END
        
        IF @pIsValidated IS NOT NULL
        BEGIN
            SET @UpdateFields = @UpdateFields + '[fldb_IsValidated] = @pIsValidated, '
        END
        
        IF @pValidatedDate IS NOT NULL
        BEGIN
            SET @UpdateFields = @UpdateFields + '[fldd_ValidatedDate] = @pValidatedDate, '
        END
        
        IF @pValidatedBy IS NOT NULL
        BEGIN
            SET @UpdateFields = @UpdateFields + '[fldv_ValidatedBy] = @pValidatedBy, '
        END
        
        IF @pNotes IS NOT NULL
        BEGIN
            SET @UpdateFields = @UpdateFields + '[fldv_Notes] = @pNotes, '
        END
        
        IF @pTags IS NOT NULL
        BEGIN
            SET @UpdateFields = @UpdateFields + '[fldv_Tags] = @pTags, '
        END
        
        -- Always update the audit fields
        SET @UpdateFields = @UpdateFields + '[fldv_UpdatedBy] = @pUpdatedBy, [fldd_UpdatedDate] = @pUpdatedDate '
        
        -- Check if any fields to update
        IF LEN(@UpdateFields) = 0 OR @UpdateFields = '[fldv_UpdatedBy] = @pUpdatedBy, [fldd_UpdatedDate] = @pUpdatedDate '
        BEGIN
            RAISERROR('No fields provided for update.', 16, 1)
            RETURN
        END
        
        -- Simple UPDATE approach for better maintainability
        UPDATE [dbo].[tbld_HazardLocations] SET
            [fldv_Code] = ISNULL(@pCode, [fldv_Code]),
            [fldv_HazardCode] = ISNULL(@pHazardCode, [fldv_HazardCode]),
            [fldv_Latitude] = ISNULL(@pLatitude, [fldv_Latitude]),
            [fldv_Longitude] = ISNULL(@pLongitude, [fldv_Longitude]),
            [fldv_Description] = ISNULL(@pDescription, [fldv_Description]),
            [fldd_DateSelected] = ISNULL(@pDateSelected, [fldd_DateSelected]),
            [fldv_LocationMapSVG] = ISNULL(@pLocationMapSVG, [fldv_LocationMapSVG]),
            [fldv_LocationArea] = ISNULL(@pLocationArea, [fldv_LocationArea]),
            [fldv_LocationSubArea] = ISNULL(@pLocationSubArea, [fldv_LocationSubArea]),
            [fldv_LocationName] = ISNULL(@pLocationName, [fldv_LocationName]),
            [fldv_AccuracyMeters] = ISNULL(@pAccuracyMeters, [fldv_AccuracyMeters]),
            [fldv_ElevationFeet] = ISNULL(@pElevationFeet, [fldv_ElevationFeet]),
            [fldv_Source] = ISNULL(@pSource, [fldv_Source]),
            [fldv_Status] = ISNULL(@pStatus, [fldv_Status]),
            [fldb_IsValidated] = ISNULL(@pIsValidated, [fldb_IsValidated]),
            [fldd_ValidatedDate] = CASE WHEN @pValidatedDate IS NOT NULL THEN @pValidatedDate ELSE [fldd_ValidatedDate] END,
            [fldv_ValidatedBy] = CASE WHEN @pValidatedBy IS NOT NULL THEN @pValidatedBy ELSE [fldv_ValidatedBy] END,
            [fldv_Notes] = CASE WHEN @pNotes IS NOT NULL THEN @pNotes ELSE [fldv_Notes] END,
            [fldv_Tags] = CASE WHEN @pTags IS NOT NULL THEN @pTags ELSE [fldv_Tags] END,
            [fldv_UpdatedBy] = @pUpdatedBy,
            [fldd_UpdatedDate] = @pUpdatedDate
        WHERE [fldi_ID] = @CurrentID
        
        -- Check if any rows were affected
        IF @@ROWCOUNT = 0
        BEGIN
            RAISERROR('No rows were updated. HazardLocation with ID %s may not exist.', 16, 1, @pID)
            ROLLBACK TRANSACTION
            RETURN
        END
        
        -- Commit transaction
        COMMIT TRANSACTION
        
        -- Return success message
        PRINT 'HazardLocation updated successfully with ID: ' + CAST(@CurrentID AS NVARCHAR(10))
        
    END TRY
    BEGIN CATCH
        -- Rollback transaction if it exists
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION
        
        -- Capture error information
        SELECT 
            @ErrorMessage = ERROR_MESSAGE(),
            @ErrorSeverity = ERROR_SEVERITY(),
            @ErrorState = ERROR_STATE()
        
        -- Log the error
        PRINT 'Error in pr_HazardLocation_Update: ' + @ErrorMessage
        
        -- Re-raise the error with consistent SMS error handling
        RAISERROR('Failed to update HazardLocation. Error: %s', @ErrorSeverity, @ErrorState, @ErrorMessage)
    END CATCH
END
GO

-- =====================================================================================================================
-- STORED PROCEDURE: pr_HazardLocation_Delete
-- Purpose: Delete hazard location with validation and error handling
-- =====================================================================================================================

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[pr_HazardLocation_Delete]') AND type in (N'P'))
    DROP PROCEDURE [dbo].[pr_HazardLocation_Delete]
GO

CREATE PROCEDURE [dbo].[pr_HazardLocation_Delete]
(
    @pID NVARCHAR(50),          -- Can be numeric ID or Code
    @pDeletedBy NVARCHAR(50) = 'SYSTEM',
    @pSoftDelete BIT = 1        -- 1 = Soft delete (set status to Inactive), 0 = Hard delete
)
AS
BEGIN
    SET NOCOUNT ON
    
    DECLARE @ErrorMessage NVARCHAR(4000)
    DECLARE @ErrorSeverity INT
    DECLARE @ErrorState INT
    DECLARE @CurrentID INT
    DECLARE @CurrentCode NVARCHAR(50)
    DECLARE @CurrentStatus NVARCHAR(20)
    
    BEGIN TRY
        -- Input validation
        IF @pID IS NULL OR LTRIM(RTRIM(@pID)) = ''
        BEGIN
            RAISERROR('ID parameter is required and cannot be empty.', 16, 1)
            RETURN
        END
        
        -- Find the record to delete
        IF EXISTS (SELECT 1 FROM [dbo].[tbld_HazardLocations] WHERE [fldv_Code] = @pID)
        BEGIN
            SELECT @CurrentID = [fldi_ID], @CurrentCode = [fldv_Code], @CurrentStatus = [fldv_Status]
            FROM [dbo].[tbld_HazardLocations] 
            WHERE [fldv_Code] = @pID
        END
        ELSE IF ISNUMERIC(@pID) = 1 AND EXISTS (SELECT 1 FROM [dbo].[tbld_HazardLocations] WHERE [fldi_ID] = CAST(@pID AS INT))
        BEGIN
            SELECT @CurrentID = [fldi_ID], @CurrentCode = [fldv_Code], @CurrentStatus = [fldv_Status]
            FROM [dbo].[tbld_HazardLocations] 
            WHERE [fldi_ID] = CAST(@pID AS INT)
        END
        ELSE
        BEGIN
            RAISERROR('HazardLocation with ID %s not found.', 16, 1, @pID)
            RETURN
        END
        
        -- Check if already inactive (for soft delete)
        IF @pSoftDelete = 1 AND @CurrentStatus = 'Inactive'
        BEGIN
            PRINT 'HazardLocation ' + @CurrentCode + ' is already inactive.'
            RETURN
        END
        
        -- Begin transaction
        BEGIN TRANSACTION
        
        IF @pSoftDelete = 1
        BEGIN
            -- Soft delete: Update status to Inactive
            UPDATE [dbo].[tbld_HazardLocations] SET
                [fldv_Status] = 'Inactive',
                [fldv_UpdatedBy] = @pDeletedBy,
                [fldd_UpdatedDate] = GETUTCDATE()
            WHERE [fldi_ID] = @CurrentID
            
            PRINT 'HazardLocation ' + @CurrentCode + ' soft deleted (status set to Inactive).'
        END
        ELSE
        BEGIN
            -- Hard delete: Remove from database
            -- Note: Consider adding checks for foreign key constraints here
            
            DELETE FROM [dbo].[tbld_HazardLocations]
            WHERE [fldi_ID] = @CurrentID
            
            PRINT 'HazardLocation ' + @CurrentCode + ' permanently deleted from database.'
        END
        
        -- Check if any rows were affected
        IF @@ROWCOUNT = 0
        BEGIN
            RAISERROR('No rows were affected. HazardLocation with ID %s may not exist.', 16, 1, @pID)
            ROLLBACK TRANSACTION
            RETURN
        END
        
        -- Commit transaction
        COMMIT TRANSACTION
        
    END TRY
    BEGIN CATCH
        -- Rollback transaction if it exists
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION
        
        -- Capture error information
        SELECT 
            @ErrorMessage = ERROR_MESSAGE(),
            @ErrorSeverity = ERROR_SEVERITY(),
            @ErrorState = ERROR_STATE()
        
        -- Log the error
        PRINT 'Error in pr_HazardLocation_Delete: ' + @ErrorMessage
        
        -- Re-raise the error with consistent SMS error handling
        RAISERROR('Failed to delete HazardLocation. Error: %s', @ErrorSeverity, @ErrorState, @ErrorMessage)
    END CATCH
END
GO

-- =====================================================================================================================
-- STORED PROCEDURE: pr_HazardLocation_GetNearby
-- Purpose: Find hazard locations within a specified radius (proximity search)
-- =====================================================================================================================

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[pr_HazardLocation_GetNearby]') AND type in (N'P'))
    DROP PROCEDURE [dbo].[pr_HazardLocation_GetNearby]
GO

CREATE PROCEDURE [dbo].[pr_HazardLocation_GetNearby]
(
    @pLatitude DECIMAL(10,8),
    @pLongitude DECIMAL(11,8), 
    @pRadiusMeters DECIMAL(10,2) = 1000,  -- Default 1km radius
    @pMaxResults INT = 50                   -- Limit results
)
AS
BEGIN
    SET NOCOUNT ON
    
    DECLARE @ErrorMessage NVARCHAR(4000)
    DECLARE @ErrorSeverity INT
    DECLARE @ErrorState INT
    
    BEGIN TRY
        -- Input validation
        IF @pLatitude IS NULL OR @pLatitude < -90 OR @pLatitude > 90
        BEGIN
            RAISERROR('Latitude must be between -90 and 90 degrees.', 16, 1)
            RETURN
        END
        
        IF @pLongitude IS NULL OR @pLongitude < -180 OR @pLongitude > 180
        BEGIN
            RAISERROR('Longitude must be between -180 and 180 degrees.', 16, 1)
            RETURN
        END
        
        IF @pRadiusMeters <= 0 OR @pRadiusMeters > 100000  -- Max 100km
        BEGIN
            RAISERROR('Radius must be between 0 and 100000 meters.', 16, 1)
            RETURN
        END
        
        -- Calculate distances using Haversine formula approximation
        -- Note: This is an approximation. For production use, consider geography data types
        SELECT TOP (@pMaxResults)
            [fldi_ID], [fldv_Code], [fldv_HazardCode], [fldv_Latitude], [fldv_Longitude],
            [fldv_Description], [fldd_DateSelected], [fldv_LocationArea], [fldv_LocationSubArea],
            [fldv_LocationName], [fldv_Status], [fldb_IsValidated], [fldv_Notes], [fldv_Tags],
            [fldv_CreatedBy], [fldd_CreatedDate], [fldv_UpdatedBy], [fldd_UpdatedDate],
            -- Calculate approximate distance in meters
            (
                6371000 * ACOS(
                    COS(RADIANS(@pLatitude)) * 
                    COS(RADIANS([fldv_Latitude])) * 
                    COS(RADIANS([fldv_Longitude]) - RADIANS(@pLongitude)) + 
                    SIN(RADIANS(@pLatitude)) * 
                    SIN(RADIANS([fldv_Latitude]))
                )
            ) AS DistanceMeters
        FROM [dbo].[tbld_HazardLocations]
        WHERE 
            [fldv_Status] = 'Active'
            AND [fldv_Latitude] IS NOT NULL 
            AND [fldv_Longitude] IS NOT NULL
            -- Pre-filter using bounding box for performance
            AND [fldv_Latitude] BETWEEN (@pLatitude - 0.01) AND (@pLatitude + 0.01)
            AND [fldv_Longitude] BETWEEN (@pLongitude - 0.01) AND (@pLongitude + 0.01)
            -- Calculate distance and filter by radius
            AND (
                6371000 * ACOS(
                    COS(RADIANS(@pLatitude)) * 
                    COS(RADIANS([fldv_Latitude])) * 
                    COS(RADIANS([fldv_Longitude]) - RADIANS(@pLongitude)) + 
                    SIN(RADIANS(@pLatitude)) * 
                    SIN(RADIANS([fldv_Latitude]))
                )
            ) <= @pRadiusMeters
        ORDER BY DistanceMeters ASC
        
    END TRY
    BEGIN CATCH
        -- Capture error information
        SELECT 
            @ErrorMessage = ERROR_MESSAGE(),
            @ErrorSeverity = ERROR_SEVERITY(),
            @ErrorState = ERROR_STATE()
        
        -- Log the error
        PRINT 'Error in pr_HazardLocation_GetNearby: ' + @ErrorMessage
        
        -- Re-raise the error
        RAISERROR('Failed to find nearby HazardLocations. Error: %s', @ErrorSeverity, @ErrorState, @ErrorMessage)
    END CATCH
END
GO

PRINT 'Additional Hazard Location stored procedures created successfully!'
PRINT '- pr_HazardLocation_Update'
PRINT '- pr_HazardLocation_Delete'
PRINT '- pr_HazardLocation_GetNearby'
PRINT '===================================================='
PRINT 'All Hazard Location stored procedures are now ready!'