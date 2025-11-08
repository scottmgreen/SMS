-- =====================================================================================================================
-- SMS Stored Procedures: Hazard Locations CRUD Operations
-- Purpose: Complete CRUD operations for tbld_HazardLocations with SMS standard error handling
-- Pattern: Follows existing SMS stored procedure conventions with consistent error handling
-- =====================================================================================================================

-- =====================================================================================================================
-- STORED PROCEDURE: pr_HazardLocation_Insert
-- Purpose: Insert new hazard location with full validation and error handling
-- =====================================================================================================================

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[pr_HazardLocation_Insert]') AND type in (N'P'))
    DROP PROCEDURE [dbo].[pr_HazardLocation_Insert]
GO

CREATE PROCEDURE [dbo].[pr_HazardLocation_Insert]
(
    @pCode NVARCHAR(50),
    @pHazardCode NVARCHAR(50),
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
    @pSource NVARCHAR(50) = 'Manual',
    @pStatus NVARCHAR(20) = 'Active',
    @pIsValidated BIT = 0,
    @pValidatedDate DATETIME2(7) = NULL,
    @pValidatedBy NVARCHAR(50) = NULL,
    @pNotes NVARCHAR(1000) = NULL,
    @pTags NVARCHAR(500) = NULL,
    @pCreatedBy NVARCHAR(50),
    @pCreatedDate DATETIME2(7),
    @pNewID INT OUTPUT,
    @pNewHazardLocationCode NVARCHAR(50) OUTPUT
)
AS
BEGIN
    SET NOCOUNT ON
    
    DECLARE @ErrorMessage NVARCHAR(4000)
    DECLARE @ErrorSeverity INT
    DECLARE @ErrorState INT
    DECLARE @NewCode NVARCHAR(50)
    
    BEGIN TRY
        -- Input validation
        IF @pCode IS NULL OR LTRIM(RTRIM(@pCode)) = ''
        BEGIN
            RAISERROR('HazardLocation Code is required and cannot be empty.', 16, 1)
            RETURN
        END
        
        IF @pHazardCode IS NULL OR LTRIM(RTRIM(@pHazardCode)) = ''
        BEGIN
            RAISERROR('HazardCode is required and cannot be empty.', 16, 1)
            RETURN
        END
        
        IF @pCreatedBy IS NULL OR LTRIM(RTRIM(@pCreatedBy)) = ''
        BEGIN
            RAISERROR('CreatedBy is required and cannot be empty.', 16, 1)
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
        
        -- Validate status
        IF @pStatus NOT IN ('Active', 'Inactive', 'Pending')
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
        
        -- Check for duplicate codes
        IF EXISTS (SELECT 1 FROM [dbo].[tbld_HazardLocations] WHERE [fldv_Code] = @pCode)
        BEGIN
            RAISERROR('HazardLocation with code %s already exists.', 16, 1, @pCode)
            RETURN
        END
        
        -- Set defaults
        SET @pDateSelected = ISNULL(@pDateSelected, GETUTCDATE())
        SET @pCreatedDate = ISNULL(@pCreatedDate, GETUTCDATE())
        
        -- Generate new code if auto-generation is needed (when @pCode is placeholder)
        IF @pCode LIKE 'AUTO_%' OR @pCode = 'AUTO'
        BEGIN
            SET @NewCode = 'HL-' + FORMAT(GETUTCDATE(), 'yyyyMMdd') + '-' + RIGHT('00000000' + CAST(NEWID() AS NVARCHAR(36)), 8)
        END
        ELSE
        BEGIN
            SET @NewCode = @pCode
        END
        
        -- Begin transaction
        BEGIN TRANSACTION
        
        -- Insert the new hazard location
        INSERT INTO [dbo].[tbld_HazardLocations]
        (
            [fldv_Code], [fldv_HazardCode], [fldv_Latitude], [fldv_Longitude], [fldv_Description],
            [fldd_DateSelected], [fldv_LocationMapSVG], [fldv_LocationArea], [fldv_LocationSubArea],
            [fldv_LocationName], [fldv_AccuracyMeters], [fldv_ElevationFeet], [fldv_Source],
            [fldv_Status], [fldb_IsValidated], [fldd_ValidatedDate], [fldv_ValidatedBy],
            [fldv_Notes], [fldv_Tags], [fldv_CreatedBy], [fldd_CreatedDate]
        )
        VALUES
        (
            @NewCode, @pHazardCode, @pLatitude, @pLongitude, @pDescription,
            @pDateSelected, @pLocationMapSVG, @pLocationArea, @pLocationSubArea,
            @pLocationName, @pAccuracyMeters, @pElevationFeet, @pSource,
            @pStatus, @pIsValidated, @pValidatedDate, @pValidatedBy,
            @pNotes, @pTags, @pCreatedBy, @pCreatedDate
        )
        
        -- Get the new ID
        SET @pNewID = SCOPE_IDENTITY()
        SET @pNewHazardLocationCode = @NewCode
        
        -- Commit transaction
        COMMIT TRANSACTION
        
        -- Return success message
        PRINT 'HazardLocation created successfully with ID: ' + CAST(@pNewID AS NVARCHAR(10)) + ', Code: ' + @NewCode
        
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
        
        -- Log the error (this would integrate with your logging system)
        PRINT 'Error in pr_HazardLocation_Insert: ' + @ErrorMessage
        
        -- Re-raise the error with consistent SMS error handling
        RAISERROR('Failed to create HazardLocation. Error: %s', @ErrorSeverity, @ErrorState, @ErrorMessage)
    END CATCH
END
GO

-- =====================================================================================================================
-- STORED PROCEDURE: pr_HazardLocation_GetById
-- Purpose: Retrieve hazard location by ID with error handling
-- =====================================================================================================================

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[pr_HazardLocation_GetById]') AND type in (N'P'))
    DROP PROCEDURE [dbo].[pr_HazardLocation_GetById]
GO

CREATE PROCEDURE [dbo].[pr_HazardLocation_GetById]
(
    @pID NVARCHAR(50)  -- Can be either numeric ID or Code
)
AS
BEGIN
    SET NOCOUNT ON
    
    DECLARE @ErrorMessage NVARCHAR(4000)
    DECLARE @ErrorSeverity INT
    DECLARE @ErrorState INT
    
    BEGIN TRY
        -- Input validation
        IF @pID IS NULL OR LTRIM(RTRIM(@pID)) = ''
        BEGIN
            RAISERROR('ID parameter is required and cannot be empty.', 16, 1)
            RETURN
        END
        
        -- Try to find by Code first, then by numeric ID
        IF EXISTS (SELECT 1 FROM [dbo].[tbld_HazardLocations] WHERE [fldv_Code] = @pID)
        BEGIN
            -- Found by Code
            SELECT 
                [fldi_ID], [fldv_Code], [fldv_HazardCode], [fldv_Latitude], [fldv_Longitude],
                [fldv_Description], [fldd_DateSelected], [fldv_LocationMapSVG], [fldv_LocationArea],
                [fldv_LocationSubArea], [fldv_LocationName], [fldv_AccuracyMeters], [fldv_ElevationFeet],
                [fldv_Source], [fldv_Status], [fldb_IsValidated], [fldd_ValidatedDate], [fldv_ValidatedBy],
                [fldv_Notes], [fldv_Tags], [fldv_CreatedBy], [fldd_CreatedDate], [fldv_UpdatedBy], [fldd_UpdatedDate]
            FROM [dbo].[tbld_HazardLocations]
            WHERE [fldv_Code] = @pID
        END
        ELSE IF ISNUMERIC(@pID) = 1 AND EXISTS (SELECT 1 FROM [dbo].[tbld_HazardLocations] WHERE [fldi_ID] = CAST(@pID AS INT))
        BEGIN
            -- Found by numeric ID
            SELECT 
                [fldi_ID], [fldv_Code], [fldv_HazardCode], [fldv_Latitude], [fldv_Longitude],
                [fldv_Description], [fldd_DateSelected], [fldv_LocationMapSVG], [fldv_LocationArea],
                [fldv_LocationSubArea], [fldv_LocationName], [fldv_AccuracyMeters], [fldv_ElevationFeet],
                [fldv_Source], [fldv_Status], [fldb_IsValidated], [fldd_ValidatedDate], [fldv_ValidatedBy],
                [fldv_Notes], [fldv_Tags], [fldv_CreatedBy], [fldd_CreatedDate], [fldv_UpdatedBy], [fldd_UpdatedDate]
            FROM [dbo].[tbld_HazardLocations]
            WHERE [fldi_ID] = CAST(@pID AS INT)
        END
        ELSE
        BEGIN
            -- Not found
            RAISERROR('HazardLocation with ID %s not found.', 16, 1, @pID)
            RETURN
        END
        
    END TRY
    BEGIN CATCH
        -- Capture error information
        SELECT 
            @ErrorMessage = ERROR_MESSAGE(),
            @ErrorSeverity = ERROR_SEVERITY(),
            @ErrorState = ERROR_STATE()
        
        -- Log the error
        PRINT 'Error in pr_HazardLocation_GetById: ' + @ErrorMessage
        
        -- Re-raise the error
        RAISERROR('Failed to retrieve HazardLocation. Error: %s', @ErrorSeverity, @ErrorState, @ErrorMessage)
    END CATCH
END
GO

-- =====================================================================================================================
-- STORED PROCEDURE: pr_HazardLocation_GetAll
-- Purpose: Retrieve all hazard locations with optional filtering
-- =====================================================================================================================

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[pr_HazardLocation_GetAll]') AND type in (N'P'))
    DROP PROCEDURE [dbo].[pr_HazardLocation_GetAll]
GO

CREATE PROCEDURE [dbo].[pr_HazardLocation_GetAll]
(
    @pHazardCode NVARCHAR(50) = NULL,   -- Optional filter by hazard
    @pStatus NVARCHAR(20) = NULL,       -- Optional filter by status
    @pLocationArea NVARCHAR(100) = NULL, -- Optional filter by location area
    @pIncludeInactive BIT = 0           -- Whether to include inactive locations
)
AS
BEGIN
    SET NOCOUNT ON
    
    DECLARE @ErrorMessage NVARCHAR(4000)
    DECLARE @ErrorSeverity INT
    DECLARE @ErrorState INT
    
    BEGIN TRY
        SELECT 
            [fldi_ID], [fldv_Code], [fldv_HazardCode], [fldv_Latitude], [fldv_Longitude],
            [fldv_Description], [fldd_DateSelected], [fldv_LocationMapSVG], [fldv_LocationArea],
            [fldv_LocationSubArea], [fldv_LocationName], [fldv_AccuracyMeters], [fldv_ElevationFeet],
            [fldv_Source], [fldv_Status], [fldb_IsValidated], [fldd_ValidatedDate], [fldv_ValidatedBy],
            [fldv_Notes], [fldv_Tags], [fldv_CreatedBy], [fldd_CreatedDate], [fldv_UpdatedBy], [fldd_UpdatedDate]
        FROM [dbo].[tbld_HazardLocations]
        WHERE 
            (@pHazardCode IS NULL OR [fldv_HazardCode] = @pHazardCode)
            AND (@pStatus IS NULL OR [fldv_Status] = @pStatus)
            AND (@pLocationArea IS NULL OR [fldv_LocationArea] LIKE '%' + @pLocationArea + '%')
            AND (@pIncludeInactive = 1 OR [fldv_Status] = 'Active')
        ORDER BY [fldd_CreatedDate] DESC, [fldv_Code]
        
    END TRY
    BEGIN CATCH
        -- Capture error information
        SELECT 
            @ErrorMessage = ERROR_MESSAGE(),
            @ErrorSeverity = ERROR_SEVERITY(),
            @ErrorState = ERROR_STATE()
        
        -- Log the error
        PRINT 'Error in pr_HazardLocation_GetAll: ' + @ErrorMessage
        
        -- Re-raise the error
        RAISERROR('Failed to retrieve HazardLocations. Error: %s', @ErrorSeverity, @ErrorState, @ErrorMessage)
    END CATCH
END
GO

-- =====================================================================================================================
-- STORED PROCEDURE: pr_HazardLocation_GetByHazardCode
-- Purpose: Retrieve all locations for a specific hazard
-- =====================================================================================================================

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[pr_HazardLocation_GetByHazardCode]') AND type in (N'P'))
    DROP PROCEDURE [dbo].[pr_HazardLocation_GetByHazardCode]
GO

CREATE PROCEDURE [dbo].[pr_HazardLocation_GetByHazardCode]
(
    @pHazardCode NVARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON
    
    DECLARE @ErrorMessage NVARCHAR(4000)
    DECLARE @ErrorSeverity INT
    DECLARE @ErrorState INT
    
    BEGIN TRY
        -- Input validation
        IF @pHazardCode IS NULL OR LTRIM(RTRIM(@pHazardCode)) = ''
        BEGIN
            RAISERROR('HazardCode parameter is required and cannot be empty.', 16, 1)
            RETURN
        END
        
        SELECT 
            [fldi_ID], [fldv_Code], [fldv_HazardCode], [fldv_Latitude], [fldv_Longitude],
            [fldv_Description], [fldd_DateSelected], [fldv_LocationMapSVG], [fldv_LocationArea],
            [fldv_LocationSubArea], [fldv_LocationName], [fldv_AccuracyMeters], [fldv_ElevationFeet],
            [fldv_Source], [fldv_Status], [fldb_IsValidated], [fldd_ValidatedDate], [fldv_ValidatedBy],
            [fldv_Notes], [fldv_Tags], [fldv_CreatedBy], [fldd_CreatedDate], [fldv_UpdatedBy], [fldd_UpdatedDate]
        FROM [dbo].[tbld_HazardLocations]
        WHERE [fldv_HazardCode] = @pHazardCode
            AND [fldv_Status] = 'Active'
        ORDER BY [fldd_DateSelected] DESC
        
    END TRY
    BEGIN CATCH
        -- Capture error information
        SELECT 
            @ErrorMessage = ERROR_MESSAGE(),
            @ErrorSeverity = ERROR_SEVERITY(),
            @ErrorState = ERROR_STATE()
        
        -- Log the error
        PRINT 'Error in pr_HazardLocation_GetByHazardCode: ' + @ErrorMessage
        
        -- Re-raise the error
        RAISERROR('Failed to retrieve HazardLocations for hazard %s. Error: %s', @ErrorSeverity, @ErrorState, @pHazardCode, @ErrorMessage)
    END CATCH
END
GO

PRINT 'Hazard Location stored procedures created successfully!'
PRINT '- pr_HazardLocation_Insert'
PRINT '- pr_HazardLocation_GetById' 
PRINT '- pr_HazardLocation_GetAll'
PRINT '- pr_HazardLocation_GetByHazardCode'