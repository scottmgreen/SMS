//-----------------------------------------------------------------------
// <copyright file="RiskAssessmentID.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Strongly-typed identifier for riskassessment entities ensuring type safety.
//                  Domain entity implementing business rules and invariants
//                  following Domain-Driven Design principles.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

/// <summary>
/// Risk Assessment ID Value Object - Mission Critical
/// </summary>
public sealed class RiskAssessmentID : BaseID<string>
{
    public RiskAssessmentID(string id) : base(id) { }
}

