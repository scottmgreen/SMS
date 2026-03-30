//-----------------------------------------------------------------------
// <copyright file="ReportID.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Strongly-typed identifier for report entities ensuring type safety.
//                  Domain entity implementing business rules and invariants
//                  following Domain-Driven Design principles.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

/// <summary>
/// Report unique identifier following the established ID pattern
/// </summary>
public sealed class ReportID : BaseID<string>
{
    public ReportID(string id) : base(id) { }
}

