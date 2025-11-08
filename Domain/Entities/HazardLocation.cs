using SMS_Shared.Common;

namespace SMS_Domain.Entities;

/// <summary>
/// HazardLocation Domain Entity - Mission Critical
/// Represents geospatial location data for hazards with map visualization support
/// Supports aviation-specific location references and GPS accuracy tracking
/// </summary>
public sealed class HazardLocation : BaseAuditableEntity
{
    // Private constructor for Entity Framework
    private HazardLocation() : base(new HazardLocationID(Guid.NewGuid().ToString()), "SYSTEM", DateTime.UtcNow) { }

    // Public constructor for domain usage
    public HazardLocation(HazardLocationID id) : base(id, "SYSTEM", DateTime.UtcNow) { }

    // Private constructor for creation with validation
    private HazardLocation(HazardLocationID id, string code, string hazardCode) 
        : base(id, "SYSTEM", DateTime.UtcNow)
    {
        Code = code;
        HazardCode = hazardCode;
        Status = HazardLocationStatus.Active;
        DateSelected = DateTime.UtcNow;
        IsValidated = false;
        Source = "Manual";
    }

    #region Core Properties

    public string Code { get; set; } = string.Empty;
    public string HazardCode { get; set; } = string.Empty;

    #endregion

    #region Geospatial Properties

    public decimal? Latitude { get; set; }          // Decimal degrees (e.g., 45.52345678)
    public decimal? Longitude { get; set; }         // Decimal degrees (e.g., -122.67890123)
    public string? Description { get; set; }        // Location description
    public DateTime DateSelected { get; set; } = DateTime.UtcNow;

    #endregion

    #region Map Visualization Properties

    public string? LocationMapSVG { get; set; }     // Thumbnail SVG or base64 encoded image
    public string? LocationArea { get; set; }       // General area (e.g., "Terminal A", "Runway 10L")
    public string? LocationSubArea { get; set; }    // Sub-area (e.g., "Gate 12", "Taxiway Charlie")
    public string? LocationName { get; set; }       // Friendly name for the location

    #endregion

    #region Precision and Accuracy Properties

    public decimal? AccuracyMeters { get; set; }    // GPS accuracy in meters
    public decimal? ElevationFeet { get; set; }     // Elevation above MSL
    public string Source { get; set; } = "Manual";  // How location was obtained (GPS, Manual, Import, Survey, Estimated)

    #endregion

    #region Airport Reference Properties

    public string? AirportGrid { get; set; }        // Airport grid reference if applicable
    public string? RunwayReference { get; set; }    // Runway reference point if applicable
    public string? TaxiwayReference { get; set; }   // Taxiway reference if applicable

    #endregion

    #region Status and Validation Properties

    public HazardLocationStatus Status { get; set; } = HazardLocationStatus.Active;
    public bool IsValidated { get; set; } = false;  // Whether location has been field-validated
    public DateTime? ValidatedDate { get; set; }
    public string? ValidatedBy { get; set; }

    #endregion

    #region Additional Properties

    public string? Notes { get; set; }
    public string? Tags { get; set; }               // Comma-separated tags for categorization

    #endregion

    #region Factory Methods

    /// <summary>
    /// Create a new hazard location with basic information
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

    /// <summary>
    /// Create hazard location with GPS data
    /// </summary>
    public static Result<HazardLocation> CreateWithGPS(string code, string hazardCode, decimal latitude, decimal longitude, 
        decimal accuracyMeters, decimal? elevationFeet = null)
    {
        var createResult = Create(code, hazardCode, latitude, longitude);
        if (createResult.IsFailure)
        {
            return createResult;
        }

        var location = createResult.Value;
        location.AccuracyMeters = accuracyMeters;
        location.ElevationFeet = elevationFeet;
        location.Source = "GPS";

        return Result<HazardLocation>.Success(location);
    }

    #endregion

    #region Domain Behavior Methods

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

        if (!IsValidSource(source))
        {
            return Result<bool>.Failure<bool>(DomainErrors.HazardLocationError.InvalidSource);
        }

        Latitude = latitude;
        Longitude = longitude;
        AccuracyMeters = accuracyMeters;
        Source = source;
        DateSelected = DateTime.UtcNow;
        UpdatedDate = DateTime.UtcNow;

        // Reset validation if coordinates change significantly
        if (IsValidated && HasSignificantLocationChange())
        {
            IsValidated = false;
            ValidatedDate = null;
            ValidatedBy = null;
        }

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Validate the location in the field
    /// </summary>
    public Result<bool> ValidateLocation(string validatedBy, string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(validatedBy))
        {
            return Result<bool>.Failure<bool>(DomainErrors.HazardLocationError.ValidatedByRequired);
        }

        if (!HasCoordinates())
        {
            return Result<bool>.Failure<bool>(DomainErrors.HazardLocationError.CoordinatesRequired);
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
    /// Update location description and area information
    /// </summary>
    public Result<bool> UpdateLocationInfo(string? locationArea, string? locationSubArea, string? locationName, string? description)
    {
        LocationArea = locationArea;
        LocationSubArea = locationSubArea;
        LocationName = locationName;
        Description = description;
        UpdatedDate = DateTime.UtcNow;

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Update airport-specific references
    /// </summary>
    public Result<bool> UpdateAirportReferences(string? airportGrid, string? runwayReference, string? taxiwayReference)
    {
        AirportGrid = airportGrid;
        RunwayReference = runwayReference;
        TaxiwayReference = taxiwayReference;
        UpdatedDate = DateTime.UtcNow;

        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Deactivate the location
    /// </summary>
    public Result<bool> Deactivate(string reason = "")
    {
        Status = HazardLocationStatus.Inactive;
        
        if (!string.IsNullOrWhiteSpace(reason))
        {
            Notes = string.IsNullOrWhiteSpace(Notes) ? $"Deactivated: {reason}" : $"{Notes}; Deactivated: {reason}";
        }

        UpdatedDate = DateTime.UtcNow;
        return Result<bool>.Success(true);
    }

    /// <summary>
    /// Reactivate the location
    /// </summary>
    public Result<bool> Reactivate()
    {
        Status = HazardLocationStatus.Active;
        UpdatedDate = DateTime.UtcNow;
        return Result<bool>.Success(true);
    }

    #endregion

    #region Query Methods

    /// <summary>
    /// Check if location has coordinates
    /// </summary>
    public bool HasCoordinates()
    {
        return Latitude.HasValue && Longitude.HasValue;
    }

    /// <summary>
    /// Check if location is GPS sourced
    /// </summary>
    public bool IsGPSLocation()
    {
        return Source == "GPS" && HasCoordinates();
    }

    /// <summary>
    /// Check if location needs validation
    /// </summary>
    public bool NeedsValidation()
    {
        return HasCoordinates() && !IsValidated;
    }

    /// <summary>
    /// Get location accuracy category
    /// </summary>
    public string GetAccuracyCategory()
    {
        if (!AccuracyMeters.HasValue) return "Unknown";
        
        return AccuracyMeters.Value switch
        {
            <= 1 => "Survey Grade",
            <= 3 => "High Precision GPS",
            <= 5 => "GPS",
            <= 10 => "Consumer GPS",
            <= 50 => "Approximate",
            _ => "Low Accuracy"
        };
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

    /// <summary>
    /// Get formatted coordinate string
    /// </summary>
    public string GetFormattedCoordinates()
    {
        if (!HasCoordinates()) return "No coordinates";
        
        return $"{Latitude:F6}°, {Longitude:F6}°";
    }

    /// <summary>
    /// Get full location display name
    /// </summary>
    public string GetDisplayName()
    {
        if (!string.IsNullOrWhiteSpace(LocationName))
            return LocationName;
            
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(LocationArea)) parts.Add(LocationArea);
        if (!string.IsNullOrWhiteSpace(LocationSubArea)) parts.Add(LocationSubArea);
        
        return parts.Any() ? string.Join(" - ", parts) : Description ?? "Unnamed Location";
    }

    #endregion

    #region Private Helper Methods

    private bool IsValidSource(string source)
    {
        var validSources = new[] { "GPS", "Manual", "Import", "Survey", "Estimated" };
        return validSources.Contains(source);
    }

    private bool HasSignificantLocationChange()
    {
        // Define what constitutes a significant change (e.g., > 10 meters)
        // This is a simplified check - in practice you'd compare with previous coordinates
        return AccuracyMeters > 10;
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