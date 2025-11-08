-- =====================================================================================================================
-- SMS Database Schema Update: Hazard Locations Support
-- Table: tbld_HazardLocations
-- Purpose: Store geospatial location data for hazards with map visualization support
-- =====================================================================================================================

-- =====================================================================================================================
-- TABLE CREATION: tbld_HazardLocations
-- =====================================================================================================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tbld_HazardLocations]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[tbld_HazardLocations]
    (
        -- Primary Key
        [fldi_ID] [int] IDENTITY(1,1) NOT NULL,
        
        -- Core Properties (following SMS naming convention)
        [fldv_Code] [nvarchar](50) NOT NULL,
        [fldv_HazardCode] [nvarchar](50) NOT NULL,
        
        -- Geospatial Properties
        [fldv_Latitude] [decimal](10,8) NULL,     -- Decimal degrees (e.g., 45.52345678)
        [fldv_Longitude] [decimal](11,8) NULL,    -- Decimal degrees (e.g., -122.67890123)
        [fldv_Description] [nvarchar](500) NULL,  -- Location description
        [fldd_DateSelected] [datetime2](7) NOT NULL DEFAULT GETUTCDATE(),
        
        -- Map Visualization Support
        [fldv_LocationMapSVG] [nvarchar](MAX) NULL, -- Thumbnail SVG or base64 encoded image
        [fldv_LocationArea] [nvarchar](100) NULL,   -- General area (e.g., "Terminal A", "Runway 10L")
        [fldv_LocationSubArea] [nvarchar](100) NULL, -- Sub-area (e.g., "Gate 12", "Taxiway Charlie")
        [fldv_LocationName] [nvarchar](200) NULL,   -- Friendly name for the location
        
        -- Precision and Accuracy Tracking
        [fldv_AccuracyMeters] [decimal](8,2) NULL, -- GPS accuracy in meters
        [fldv_ElevationFeet] [decimal](8,2) NULL,  -- Elevation in feet above MSL
        [fldv_Source] [nvarchar](50) NULL,         -- How location was obtained (GPS, Manual, Import, etc.)
        
        -- Airport Reference System (for aviation-specific coordinates)
        [fldv_AirportGrid] [nvarchar](50) NULL,    -- Airport grid reference if applicable
        [fldv_RunwayReference] [nvarchar](50) NULL, -- Runway reference point if applicable
        [fldv_TaxiwayReference] [nvarchar](50) NULL, -- Taxiway reference if applicable
        
        -- Status and Validation
        [fldv_Status] [nvarchar](20) NOT NULL DEFAULT 'Active', -- Active, Inactive, Pending
        [fldb_IsValidated] [bit] NOT NULL DEFAULT 0, -- Whether location has been field-validated
        [fldd_ValidatedDate] [datetime2](7) NULL,
        [fldv_ValidatedBy] [nvarchar](50) NULL,
        
        -- Additional Context
        [fldv_Notes] [nvarchar](1000) NULL,
        [fldv_Tags] [nvarchar](500) NULL,          -- Comma-separated tags for categorization
        
        -- Standard SMS Audit Fields
        [fldv_CreatedBy] [nvarchar](50) NOT NULL DEFAULT 'SYSTEM',
        [fldd_CreatedDate] [datetime2](7) NOT NULL DEFAULT GETUTCDATE(),
        [fldv_UpdatedBy] [nvarchar](50) NULL,
        [fldd_UpdatedDate] [datetime2](7) NULL,
        
        -- Constraints
        CONSTRAINT [PK_tbld_HazardLocations] PRIMARY KEY CLUSTERED ([fldi_ID] ASC),
        CONSTRAINT [UK_tbld_HazardLocations_Code] UNIQUE NONCLUSTERED ([fldv_Code] ASC),
        CONSTRAINT [CK_tbld_HazardLocations_Latitude] CHECK ([fldv_Latitude] >= -90 AND [fldv_Latitude] <= 90),
        CONSTRAINT [CK_tbld_HazardLocations_Longitude] CHECK ([fldv_Longitude] >= -180 AND [fldv_Longitude] <= 180),
        CONSTRAINT [CK_tbld_HazardLocations_Status] CHECK ([fldv_Status] IN ('Active', 'Inactive', 'Pending')),
        CONSTRAINT [CK_tbld_HazardLocations_Source] CHECK ([fldv_Source] IS NULL OR [fldv_Source] IN ('GPS', 'Manual', 'Import', 'Survey', 'Estimated'))
    )
    
    PRINT 'Table tbld_HazardLocations created successfully.'
END
ELSE
BEGIN
    PRINT 'Table tbld_HazardLocations already exists.'
END
GO

-- =====================================================================================================================
-- INDEXES for Performance
-- =====================================================================================================================

-- Index on HazardCode for quick lookups
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('tbld_HazardLocations') AND name = 'IX_tbld_HazardLocations_HazardCode')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_tbld_HazardLocations_HazardCode] 
    ON [dbo].[tbld_HazardLocations] ([fldv_HazardCode])
    PRINT 'Index IX_tbld_HazardLocations_HazardCode created.'
END
GO

-- Spatial index for geospatial queries (if needed for proximity searches)
-- Note: This would require converting to geography data type for advanced spatial queries

-- Index on Status for filtering active locations
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('tbld_HazardLocations') AND name = 'IX_tbld_HazardLocations_Status')
BEGIN
    CREATE NONCLUSTERED INDEX [IX_tbld_HazardLocations_Status] 
    ON [dbo].[tbld_HazardLocations] ([fldv_Status])
    WHERE [fldv_Status] = 'Active'
    PRINT 'Index IX_tbld_HazardLocations_Status created.'
END
GO

-- =====================================================================================================================
-- FOREIGN KEY CONSTRAINTS (uncomment if you want to enforce referential integrity)
-- =====================================================================================================================
/*
-- Foreign Key to tbld_Hazards (uncomment if hazards table exists and you want FK constraint)
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_tbld_HazardLocations_tbld_Hazards]'))
BEGIN
    ALTER TABLE [dbo].[tbld_HazardLocations]
    ADD CONSTRAINT [FK_tbld_HazardLocations_tbld_Hazards] 
    FOREIGN KEY([fldv_HazardCode]) REFERENCES [dbo].[tbld_Hazards] ([fldv_Code])
    ON DELETE CASCADE
    ON UPDATE CASCADE
    
    PRINT 'Foreign key FK_tbld_HazardLocations_tbld_Hazards created.'
END
*/

-- =====================================================================================================================
-- SAMPLE DATA (for testing purposes)
-- =====================================================================================================================
IF NOT EXISTS (SELECT * FROM tbld_HazardLocations WHERE fldv_Code = 'HL-SAMPLE-001')
BEGIN
    INSERT INTO [dbo].[tbld_HazardLocations]
    (
        [fldv_Code], [fldv_HazardCode], [fldv_Latitude], [fldv_Longitude], [fldv_Description],
        [fldd_DateSelected], [fldv_LocationArea], [fldv_LocationSubArea], [fldv_LocationName],
        [fldv_AccuracyMeters], [fldv_ElevationFeet], [fldv_Source], [fldv_Status], [fldb_IsValidated],
        [fldv_Notes], [fldv_Tags], [fldv_CreatedBy], [fldd_CreatedDate]
    )
    VALUES
    (
        'HL-SAMPLE-001', 'HZ-TEST-001', 45.58723000, -122.59349000, 'Sample hazard location at Terminal A Gate 12',
        GETUTCDATE(), 'Terminal A', 'Gate 12', 'Terminal A - Gate 12 Jetbridge',
        3.5, 31.2, 'GPS', 'Active', 1,
        'Sample hazard location for testing geospatial functionality', 'terminal,gate,jetbridge',
        'SYSTEM', GETUTCDATE()
    )
    
    PRINT 'Sample data inserted into tbld_HazardLocations.'
END
GO

PRINT 'tbld_HazardLocations table and indexes created successfully!'
PRINT '===================================================='