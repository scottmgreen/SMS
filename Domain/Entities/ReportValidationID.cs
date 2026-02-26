//-----------------------------------------------------------------------
// <copyright file="ReportValidationID.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Strongly-typed identifier for reportvalidation entities ensuring type safety.
//                  Domain entity implementing business rules and invariants
//                  following Domain-Driven Design principles.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

/// <summary>
/// Unique identifier for Report Validation entities
/// </summary>
public sealed class ReportValidationID : BaseID<string>
{
    public ReportValidationID(string id) : base(id) { }

    //public static implicit operator ReportValidationID(string value) => new(value);
    //public static implicit operator string(ReportValidationID id) => id.Value;
}

