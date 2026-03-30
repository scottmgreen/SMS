//-----------------------------------------------------------------------
// <copyright file="HazardReportTrackingID.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Strongly-typed identifier for hazardreporttracking entities ensuring type safety.
//                  Domain entity implementing business rules and invariants
//                  following Domain-Driven Design principles.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

/// <summary>
/// Hazard Report Tracking unique identifier following the established ID pattern
/// </summary>
public class HazardReportTrackingID : BaseID<string>
{
    public HazardReportTrackingID(string id) : base(id) { }
}
