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
    public HazardLocation() : base(new HazardLocationID(Guid.NewGuid().ToString()), string.Empty, DateTime.UtcNow) { }

    // Public constructor for domain usage
    public HazardLocation(HazardLocationID id) : base(id, string.Empty, DateTime.UtcNow) { }

    
    #region Core Properties

    public string Code { get; set; } = string.Empty;
    public string HazardCode { get; set; } = string.Empty;

    public bool IsValid { get; set; } = true;              // Indicates if location data is valid and usable    
    #endregion

    #region Geospatial Properties

    public decimal? Latitude { get; set; }          // Decimal degrees (e.g., 45.52345678)
    public decimal? Longitude { get; set; }         // Decimal degrees (e.g., -122.67890123)
    public string? Description { get; set; }        // Location description


    public DateTime DateSelected { get; set; } = DateTime.UtcNow;

    #endregion

    #region Status and Validation Properties

    public bool IsValidated { get; set; }                  // Indicates if location has been validated  

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
