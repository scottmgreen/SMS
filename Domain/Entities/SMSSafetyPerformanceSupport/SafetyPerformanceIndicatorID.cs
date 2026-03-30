//-----------------------------------------------------------------------
// <copyright file="SafetyPerformanceIndicatorID.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Strongly-typed identifier for safetyperformanceindicator entities ensuring type safety.
//                  Domain entity implementing business rules and invariants
//                  following Domain-Driven Design principles.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

/// <summary>
/// Safety Performance Indicator unique identifier following the established ID pattern
/// </summary>
public class SafetyPerformanceIndicatorID : BaseID<string>
{
    public SafetyPerformanceIndicatorID(string value) : base(value)
    {
    }

}
