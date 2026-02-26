//-----------------------------------------------------------------------
// <copyright file="AirportSharedDatasetID.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Strongly-typed identifier for airportshareddataset entities ensuring type safety.
//                  Domain entity implementing business rules and invariants
//                  following Domain-Driven Design principles.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

/// <summary>
/// Represents a unique identifier for an Airport Shared Dataset entity.
/// This critical SMS compliance entity contains all regulatory-required data elements
/// for SMS Risk validation and hazard processing.
/// </summary>
public class AirportSharedDatasetID : BaseID<string>
{
    /// <summary>
    /// Initializes a new instance of the AirportSharedDatasetID class.
    /// </summary>
    /// <param name="id">The string identifier value</param>
    public AirportSharedDatasetID(string id) : base(id)
    {
    }
}
