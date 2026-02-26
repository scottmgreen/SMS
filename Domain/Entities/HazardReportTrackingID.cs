//-----------------------------------------------------------------------
// <copyright file="HazardReportTrackingID.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Strongly-typed identifier for hazardreporttracking entities ensuring type safety.
//                  Immutable value object encapsulating domain concepts with
//                  business logic and validation rules.
// </copyright>
//-----------------------------------------------------------------------

public class HazardReportTrackingID : BaseID<string>
{
    public HazardReportTrackingID(string id) : base(id) { }
}
