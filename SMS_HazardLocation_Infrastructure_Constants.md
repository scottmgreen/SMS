-- =====================================================================================================================
-- SMS Infrastructure Constants Update: Hazard Locations Support  
-- Purpose: Add field names, parameter names, and stored procedure constants for tbld_HazardLocations
-- Instructions: Add these constants to your existing Infrastructure/Common files
-- =====================================================================================================================

-- =====================================================================================================================
-- ADD TO: Infrastructure/Common/FieldNames.cs
-- Section: Add these field name constants for tbld_HazardLocations
-- =====================================================================================================================

/*
    /// <summary>
    /// Hazard Locations table (tbld_HazardLocations)
    /// </summary>
    private static readonly Lazy<string> _fHazardLocationCode = new Lazy<string>(() => "fldv_Code");
    public static string fHazardLocationCode => _fHazardLocationCode.Value;

    private static readonly Lazy<string> _fHazardLocationHazardCode = new Lazy<string>(() => "fldv_HazardCode");
    public static string fHazardLocationHazardCode => _fHazardLocationHazardCode.Value;

    private static readonly Lazy<string> _fHazardLocationLatitude = new Lazy<string>(() => "fldv_Latitude");
    public static string fHazardLocationLatitude => _fHazardLocationLatitude.Value;

    private static readonly Lazy<string> _fHazardLocationLongitude = new Lazy<string>(() => "fldv_Longitude");
    public static string fHazardLocationLongitude => _fHazardLocationLongitude.Value;

    private static readonly Lazy<string> _fHazardLocationDescription = new Lazy<string>(() => "fldv_Description");
    public static string fHazardLocationDescription => _fHazardLocationDescription.Value;

    private static readonly Lazy<string> _fHazardLocationDateSelected = new Lazy<string>(() => "fldd_DateSelected");
    public static string fHazardLocationDateSelected => _fHazardLocationDateSelected.Value;

    private static readonly Lazy<string> _fHazardLocationMapSVG = new Lazy<string>(() => "fldv_LocationMapSVG");
    public static string fHazardLocationMapSVG => _fHazardLocationMapSVG.Value;

    private static readonly Lazy<string> _fHazardLocationArea = new Lazy<string>(() => "fldv_LocationArea");
    public static string fHazardLocationArea => _fHazardLocationArea.Value;

    private static readonly Lazy<string> _fHazardLocationSubArea = new Lazy<string>(() => "fldv_LocationSubArea");
    public static string fHazardLocationSubArea => _fHazardLocationSubArea.Value;

    private static readonly Lazy<string> _fHazardLocationName = new Lazy<string>(() => "fldv_LocationName");
    public static string fHazardLocationName => _fHazardLocationName.Value;

    private static readonly Lazy<string> _fHazardLocationAccuracyMeters = new Lazy<string>(() => "fldv_AccuracyMeters");
    public static string fHazardLocationAccuracyMeters => _fHazardLocationAccuracyMeters.Value;

    private static readonly Lazy<string> _fHazardLocationElevationFeet = new Lazy<string>(() => "fldv_ElevationFeet");
    public static string fHazardLocationElevationFeet => _fHazardLocationElevationFeet.Value;

    private static readonly Lazy<string> _fHazardLocationSource = new Lazy<string>(() => "fldv_Source");
    public static string fHazardLocationSource => _fHazardLocationSource.Value;

    private static readonly Lazy<string> _fHazardLocationStatus = new Lazy<string>(() => "fldv_Status");
    public static string fHazardLocationStatus => _fHazardLocationStatus.Value;

    private static readonly Lazy<string> _fHazardLocationIsValidated = new Lazy<string>(() => "fldb_IsValidated");
    public static string fHazardLocationIsValidated => _fHazardLocationIsValidated.Value;

    private static readonly Lazy<string> _fHazardLocationValidatedDate = new Lazy<string>(() => "fldd_ValidatedDate");
    public static string fHazardLocationValidatedDate => _fHazardLocationValidatedDate.Value;

    private static readonly Lazy<string> _fHazardLocationValidatedBy = new Lazy<string>(() => "fldv_ValidatedBy");
    public static string fHazardLocationValidatedBy => _fHazardLocationValidatedBy.Value;

    private static readonly Lazy<string> _fHazardLocationNotes = new Lazy<string>(() => "fldv_Notes");
    public static string fHazardLocationNotes => _fHazardLocationNotes.Value;

    private static readonly Lazy<string> _fHazardLocationTags = new Lazy<string>(() => "fldv_Tags");
    public static string fHazardLocationTags => _fHazardLocationTags.Value;

    private static readonly Lazy<string> _fHazardLocationAirportGrid = new Lazy<string>(() => "fldv_AirportGrid");
    public static string fHazardLocationAirportGrid => _fHazardLocationAirportGrid.Value;

    private static readonly Lazy<string> _fHazardLocationRunwayReference = new Lazy<string>(() => "fldv_RunwayReference");
    public static string fHazardLocationRunwayReference => _fHazardLocationRunwayReference.Value;

    private static readonly Lazy<string> _fHazardLocationTaxiwayReference = new Lazy<string>(() => "fldv_TaxiwayReference");
    public static string fHazardLocationTaxiwayReference => _fHazardLocationTaxiwayReference.Value;
*/

-- =====================================================================================================================
-- ADD TO: Infrastructure/Common/ParameterNames.cs
-- Section: Add these parameter name constants for stored procedure parameters
-- =====================================================================================================================

/*
    /// <summary>
    /// Hazard Location parameters (for pr_HazardLocation_* stored procedures)
    /// </summary>
    private static readonly Lazy<string> _pmHazardLocationId = new Lazy<string>(() => "@pHazardLocationID");
    public static string pmHazardLocationId => _pmHazardLocationId.Value;

    private static readonly Lazy<string> _pmHazardLocationCode = new Lazy<string>(() => "@pCode");
    public static string pmHazardLocationCode => _pmHazardLocationCode.Value;

    private static readonly Lazy<string> _pmHazardLocationHazardCode = new Lazy<string>(() => "@pHazardCode");
    public static string pmHazardLocationHazardCode => _pmHazardLocationHazardCode.Value;

    private static readonly Lazy<string> _pmHazardLocationLatitude = new Lazy<string>(() => "@pLatitude");
    public static string pmHazardLocationLatitude => _pmHazardLocationLatitude.Value;

    private static readonly Lazy<string> _pmHazardLocationLongitude = new Lazy<string>(() => "@pLongitude");
    public static string pmHazardLocationLongitude => _pmHazardLocationLongitude.Value;

    private static readonly Lazy<string> _pmHazardLocationDescription = new Lazy<string>(() => "@pDescription");
    public static string pmHazardLocationDescription => _pmHazardLocationDescription.Value;

    private static readonly Lazy<string> _pmHazardLocationDateSelected = new Lazy<string>(() => "@pDateSelected");
    public static string pmHazardLocationDateSelected => _pmHazardLocationDateSelected.Value;

    private static readonly Lazy<string> _pmHazardLocationMapSVG = new Lazy<string>(() => "@pLocationMapSVG");
    public static string pmHazardLocationMapSVG => _pmHazardLocationMapSVG.Value;

    private static readonly Lazy<string> _pmHazardLocationArea = new Lazy<string>(() => "@pLocationArea");
    public static string pmHazardLocationArea => _pmHazardLocationArea.Value;

    private static readonly Lazy<string> _pmHazardLocationSubArea = new Lazy<string>(() => "@pLocationSubArea");
    public static string pmHazardLocationSubArea => _pmHazardLocationSubArea.Value;

    private static readonly Lazy<string> _pmHazardLocationName = new Lazy<string>(() => "@pLocationName");
    public static string pmHazardLocationName => _pmHazardLocationName.Value;

    private static readonly Lazy<string> _pmHazardLocationAccuracyMeters = new Lazy<string>(() => "@pAccuracyMeters");
    public static string pmHazardLocationAccuracyMeters => _pmHazardLocationAccuracyMeters.Value;

    private static readonly Lazy<string> _pmHazardLocationElevationFeet = new Lazy<string>(() => "@pElevationFeet");
    public static string pmHazardLocationElevationFeet => _pmHazardLocationElevationFeet.Value;

    private static readonly Lazy<string> _pmHazardLocationSource = new Lazy<string>(() => "@pSource");
    public static string pmHazardLocationSource => _pmHazardLocationSource.Value;

    private static readonly Lazy<string> _pmHazardLocationStatus = new Lazy<string>(() => "@pStatus");
    public static string pmHazardLocationStatus => _pmHazardLocationStatus.Value;

    private static readonly Lazy<string> _pmHazardLocationIsValidated = new Lazy<string>(() => "@pIsValidated");
    public static string pmHazardLocationIsValidated => _pmHazardLocationIsValidated.Value;

    private static readonly Lazy<string> _pmHazardLocationValidatedDate = new Lazy<string>(() => "@pValidatedDate");
    public static string pmHazardLocationValidatedDate => _pmHazardLocationValidatedDate.Value;

    private static readonly Lazy<string> _pmHazardLocationValidatedBy = new Lazy<string>(() => "@pValidatedBy");
    public static string pmHazardLocationValidatedBy => _pmHazardLocationValidatedBy.Value;

    private static readonly Lazy<string> _pmHazardLocationNotes = new Lazy<string>(() => "@pNotes");
    public static string pmHazardLocationNotes => _pmHazardLocationNotes.Value;

    private static readonly Lazy<string> _pmHazardLocationTags = new Lazy<string>(() => "@pTags");
    public static string pmHazardLocationTags => _pmHazardLocationTags.Value;

    private static readonly Lazy<string> _pmHazardLocationAirportGrid = new Lazy<string>(() => "@pAirportGrid");
    public static string pmHazardLocationAirportGrid => _pmHazardLocationAirportGrid.Value;

    private static readonly Lazy<string> _pmHazardLocationRunwayReference = new Lazy<string>(() => "@pRunwayReference");
    public static string pmHazardLocationRunwayReference => _pmHazardLocationRunwayReference.Value;

    private static readonly Lazy<string> _pmHazardLocationTaxiwayReference = new Lazy<string>(() => "@pTaxiwayReference");
    public static string pmHazardLocationTaxiwayReference => _pmHazardLocationTaxiwayReference.Value;

    // Proximity search parameters
    private static readonly Lazy<string> _pmHazardLocationRadiusMeters = new Lazy<string>(() => "@pRadiusMeters");
    public static string pmHazardLocationRadiusMeters => _pmHazardLocationRadiusMeters.Value;

    private static readonly Lazy<string> _pmHazardLocationMaxResults = new Lazy<string>(() => "@pMaxResults");
    public static string pmHazardLocationMaxResults => _pmHazardLocationMaxResults.Value;

    // Common parameters
    private static readonly Lazy<string> _pmHazardLocationIncludeInactive = new Lazy<string>(() => "@pIncludeInactive");
    public static string pmHazardLocationIncludeInactive => _pmHazardLocationIncludeInactive.Value;

    private static readonly Lazy<string> _pmHazardLocationSoftDelete = new Lazy<string>(() => "@pSoftDelete");
    public static string pmHazardLocationSoftDelete => _pmHazardLocationSoftDelete.Value;

    private static readonly Lazy<string> _pmHazardLocationDeletedBy = new Lazy<string>(() => "@pDeletedBy");
    public static string pmHazardLocationDeletedBy => _pmHazardLocationDeletedBy.Value;
*/

-- =====================================================================================================================
-- ADD TO: Infrastructure/Common/StoredProcs.cs
-- Section: Add these stored procedure name constants
-- =====================================================================================================================

/*
    /// <summary>
    /// Hazard Location stored procedures (tbld_HazardLocations)
    /// </summary>
    private static readonly Lazy<string> _pr_HazardLocation_Insert = new Lazy<string>(() => "pr_HazardLocation_Insert");
    public static string pr_HazardLocation_Insert => _pr_HazardLocation_Insert.Value;

    private static readonly Lazy<string> _pr_HazardLocation_GetById = new Lazy<string>(() => "pr_HazardLocation_GetById");
    public static string pr_HazardLocation_GetById => _pr_HazardLocation_GetById.Value;

    private static readonly Lazy<string> _pr_HazardLocation_GetAll = new Lazy<string>(() => "pr_HazardLocation_GetAll");
    public static string pr_HazardLocation_GetAll => _pr_HazardLocation_GetAll.Value;

    private static readonly Lazy<string> _pr_HazardLocation_GetByHazardCode = new Lazy<string>(() => "pr_HazardLocation_GetByHazardCode");
    public static string pr_HazardLocation_GetByHazardCode => _pr_HazardLocation_GetByHazardCode.Value;

    private static readonly Lazy<string> _pr_HazardLocation_Update = new Lazy<string>(() => "pr_HazardLocation_Update");
    public static string pr_HazardLocation_Update => _pr_HazardLocation_Update.Value;

    private static readonly Lazy<string> _pr_HazardLocation_Delete = new Lazy<string>(() => "pr_HazardLocation_Delete");
    public static string pr_HazardLocation_Delete => _pr_HazardLocation_Delete.Value;

    private static readonly Lazy<string> _pr_HazardLocation_GetNearby = new Lazy<string>(() => "pr_HazardLocation_GetNearby");
    public static string pr_HazardLocation_GetNearby => _pr_HazardLocation_GetNearby.Value;
*/

-- =====================================================================================================================
-- DOMAIN ENTITY: HazardLocation Domain Entity
-- Instructions: Create Domain/Entities/HazardLocation.cs
-- =====================================================================================================================

/*
using SMS_Shared.Common;

namespace SMS_Domain.Entities;

/// <summary>
/// HazardLocation Domain Entity
/// Represents geospatial location data for hazards with map visualization support
/// </summary>
public sealed class HazardLocation : BaseAuditableEntity
{
    private HazardLocation() : base(new HazardLocationID(Guid.NewGuid().ToString()), "SYSTEM", DateTime.UtcNow) { }

    public HazardLocation(HazardLocationID id) : base(id, "SYSTEM", DateTime.UtcNow) { }

    private HazardLocation(HazardLocationID id, string code, string hazardCode) 
        : base(id, "SYSTEM", DateTime.UtcNow)
    {
        Code = code;
        HazardCode = hazardCode;
        Status = HazardLocationStatus.Active;
        DateSelected = DateTime.UtcNow;
        IsValidated = false;
    }

    #region Core Properties

    public string Code { get; set; } = string.Empty;
    public string HazardCode { get; set; } = string.Empty;

    #endregion

    #region Geospatial Properties

    public decimal? Latitude { get; set; }          // Decimal degrees
    public decimal? Longitude { get; set; }         // Decimal degrees
    public string? Description { get; set; }
    public DateTime DateSelected { get; set; } = DateTime.UtcNow;

    #endregion

    #region Map Visualization Properties

    public string? LocationMapSVG { get; set; }     // Thumbnail SVG or base64 encoded image
    public string? LocationArea { get; set; }       // General area
    public string? LocationSubArea { get; set; }    // Sub-area
    public string? LocationName { get; set; }       // Friendly name

    #endregion

    #region Precision and Accuracy Properties

    public decimal? AccuracyMeters { get; set; }    // GPS accuracy in meters
    public decimal? ElevationFeet { get; set; }     // Elevation above MSL
    public string? Source { get; set; }             // How location was obtained

    #endregion

    #region Airport Reference Properties

    public string? AirportGrid { get; set; }        // Airport grid reference
    public string? RunwayReference { get; set; }    // Runway reference point
    public string? TaxiwayReference { get; set; }   // Taxiway reference

    #endregion

    #region Status and Validation Properties

    public HazardLocationStatus Status { get; set; } = HazardLocationStatus.Active;
    public bool IsValidated { get; set; } = false;
    public DateTime? ValidatedDate { get; set; }
    public string? ValidatedBy { get; set; }

    #endregion

    #region Additional Properties

    public string? Notes { get; set; }
    public string? Tags { get; set; }               // Comma-separated tags

    #endregion

    #region Factory Methods

    /// <summary>
    /// Create a new hazard location
    /// </summary>
    public static Result<HazardLocation> Create(string code, string hazardCode, decimal? latitude = null, decimal? longitude = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Result<HazardLocation>.Failure<HazardLocation>(DomainErrors.HazardLocationError.CodeRequired);
        }

        if (string.IsNullOrWhiteSpace(hazardCode))
        {
            return Result<HazardLocation>.Failure<HazardLocation>(DomainErrors.HazardLocationError.HazardCodeRequired);
        }

        // Validate coordinates if provided
        if (latitude.HasValue && (latitude < -90 || latitude > 90))
        {
            return Result<HazardLocation>.Failure<HazardLocation>(DomainErrors.HazardLocationError.InvalidLatitude);
        }

        if (longitude.HasValue && (longitude < -180 || longitude > 180))
        {
            return Result<HazardLocation>.Failure<HazardLocation>(DomainErrors.HazardLocationError.InvalidLongitude);
        }

        var id = new HazardLocationID(code);
        var location = new HazardLocation(id, code, hazardCode)
        {
            Latitude = latitude,
            Longitude = longitude
        };

        return Result<HazardLocation>.Success(location);
    }

    #endregion

    #region Domain Methods

    /// <summary>
    /// Update location coordinates
    /// </summary>
    public Result<bool> UpdateCoordinates(decimal latitude, decimal longitude, decimal? accuracyMeters = null, string source = "Manual")
    {
        if (latitude < -90 || latitude > 90)
        {
            return Result<bool>.Failure<bool>(DomainErrors.HazardLocationError.InvalidLatitude);
        }

        if (longitude < -180 || longitude > 180)
        {
            return Result<bool>.Failure<bool>(DomainErrors.HazardLocationError.InvalidLongitude);
        }

        Latitude = latitude;
        Longitude = longitude;
        AccuracyMeters = accuracyMeters;
        Source = source;
        DateSelected = DateTime.UtcNow;
        UpdatedDate = DateTime.UtcNow;

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Validate the location
    /// </summary>
    public Result<bool> ValidateLocation(string validatedBy, string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(validatedBy))
        {
            return Result<bool>.Failure<bool>(DomainErrors.HazardLocationError.ValidatedByRequired);
        }

        IsValidated = true;
        ValidatedDate = DateTime.UtcNow;
        ValidatedBy = validatedBy;
        
        if (!string.IsNullOrWhiteSpace(notes))
        {
            Notes = string.IsNullOrWhiteSpace(Notes) ? notes : $"{Notes}; {notes}";
        }

        UpdatedDate = DateTime.UtcNow;
        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Check if location has coordinates
    /// </summary>
    public bool HasCoordinates()
    {
        return Latitude.HasValue && Longitude.HasValue;
    }

    /// <summary>
    /// Calculate approximate distance to another location in meters
    /// </summary>
    public double? CalculateDistanceTo(decimal otherLat, decimal otherLon)
    {
        if (!HasCoordinates()) return null;

        // Haversine formula for great circle distance
        const double earthRadius = 6371000; // meters
        
        var lat1Rad = (double)(Latitude!.Value * (decimal)Math.PI / 180);
        var lat2Rad = (double)(otherLat * (decimal)Math.PI / 180);
        var deltaLatRad = (double)((otherLat - Latitude.Value) * (decimal)Math.PI / 180);
        var deltaLonRad = (double)((otherLon - Longitude!.Value) * (decimal)Math.PI / 180);

        var a = Math.Sin(deltaLatRad / 2) * Math.Sin(deltaLatRad / 2) +
                Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                Math.Sin(deltaLonRad / 2) * Math.Sin(deltaLonRad / 2);
                
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return earthRadius * c;
    }

    #endregion
}

/// <summary>
/// HazardLocation ID Value Object
/// </summary>
public sealed class HazardLocationID : BaseID<string>
{
    public HazardLocationID(string id) : base(id) { }
}

/// <summary>
/// HazardLocation Status Enumeration
/// </summary>
public enum HazardLocationStatus
{
    Active,
    Inactive,
    Pending
}
*/

-- =====================================================================================================================
-- DOMAIN ERRORS: Add to Domain/Errors/DomainErrors.cs
-- =====================================================================================================================

/*
    /// <summary>
    /// Contains hazard location-related errors.
    /// </summary>
    public static class HazardLocationError
    {
        public static Error NullOrEmpty => new Error("HazardLocation.NullOrEmpty", "The Hazard Location is required.");
        public static Error CodeRequired => new Error("HazardLocation.CodeRequired", "The Hazard Location Code is required.");
        public static Error HazardCodeRequired => new Error("HazardLocation.HazardCodeRequired", "The Hazard Code is required.");
        public static Error InvalidCode => new Error("HazardLocation.InvalidCode", "The Hazard Location Code is invalid.");
        public static Error InvalidLatitude => new Error("HazardLocation.InvalidLatitude", "Latitude must be between -90 and 90 degrees.");
        public static Error InvalidLongitude => new Error("HazardLocation.InvalidLongitude", "Longitude must be between -180 and 180 degrees.");
        public static Error ValidatedByRequired => new Error("HazardLocation.ValidatedByRequired", "ValidatedBy is required when validating location.");
        public static Error NotFound => new Error("HazardLocation.NotFound", "The Hazard Location was not found.");
        public static Error CreateFailed => new Error("HazardLocation.CreateFailed", "Failed to create the Hazard Location.");
        public static Error UpdateFailed => new Error("HazardLocation.UpdateFailed", "Failed to update the Hazard Location.");
        public static Error DeleteFailed => new Error("HazardLocation.DeleteFailed", "Failed to delete the Hazard Location.");
    }
*/

PRINT 'Infrastructure constants documentation created successfully!'
PRINT 'Next Steps:'
PRINT '1. Add the FieldNames constants to Infrastructure/Common/FieldNames.cs'
PRINT '2. Add the ParameterNames constants to Infrastructure/Common/ParameterNames.cs'  
PRINT '3. Add the StoredProcs constants to Infrastructure/Common/StoredProcs.cs'
PRINT '4. Create the HazardLocation domain entity in Domain/Entities/HazardLocation.cs'
PRINT '5. Add the HazardLocationError constants to Domain/Errors/DomainErrors.cs'
PRINT '6. Create HazardLocationRepository in Infrastructure/Persistence/'
PRINT '7. Create HazardLocationDataService in Infrastructure/Services/'
PRINT '8. Add CQRS Commands and Queries for HazardLocation'