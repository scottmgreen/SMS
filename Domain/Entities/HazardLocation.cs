//-----------------------------------------------------------------------
// <copyright file="HazardLocation.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: SMS hazard entity representing hazardlocation for safety management processes.
//                  Domain entity implementing business rules and invariants
//                  following Domain-Driven Design principles.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

/// <summary>
/// HazardLocation Domain Entity - Mission Critical
/// Represents geospatial location data for hazards with map visualization support
/// Supports aviation-specific location references and GPS accuracy tracking
/// </summary>
public sealed class HazardLocation : BaseAuditableEntity
{
    // Public constructor for Entity Framework and Model Binding
    public HazardLocation() : base(new HazardLocationID(Guid.NewGuid().ToString()), "SYSTEM", DateTime.UtcNow) { }

    // Public constructor for domain usage
    public HazardLocation(HazardLocationID id) : base(id, "SYSTEM", DateTime.UtcNow) { }

    // Private constructor for creation with validation
    private HazardLocation(HazardLocationID id, string code, string hazardCode)
        : base(id, "SYSTEM", DateTime.UtcNow)
    {
        Code = code;
        HazardCode = hazardCode;

        DateSelected = DateTime.UtcNow;


    }

    #region Core Properties

    public string Code { get; set; } = string.Empty;
    public string HazardCode { get; set; } = string.Empty;

    public bool IsValid { get; set; }

    #endregion

    #region Geospatial Properties

    public decimal? Latitude { get; set; }          // Decimal degrees (e.g., 45.52345678)
    public decimal? Longitude { get; set; }         // Decimal degrees (e.g., -122.67890123)
    public string? Description { get; set; }        // Location description
    public DateTime DateSelected { get; set; } = DateTime.UtcNow;

    #endregion

    #region Map Visualization Properties



    #endregion

    #region Precision and Accuracy Properties


    #endregion

    #region Airport Reference Properties



    #endregion

    #region Status and Validation Properties



    #endregion

    #region Additional Properties

    // Comma-separated tags for categorization

    #endregion

    #region Factory Methods

    /// <summary>
    /// Create a new hazard location with basic information
    /// </summary>

    /// <summary>
    /// Create hazard location with GPS data
    /// </summary>


    #endregion

    #region Domain Behavior Methods

    /// <summary>
    /// Update location coordinates
    /// </summary>


    /// <summary>
    /// Update location description and area information
    /// </summary>





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


    /// <summary>
    /// Check if location needs validation
    /// </summary>


    /// <summary>
    /// Get location accuracy category
    /// </summary>


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

    #endregion

    #region Private Helper Methods





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
