//-----------------------------------------------------------------------
// <copyright file="RiskAnalysisID.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Strongly-typed identifier for riskanalysis entities ensuring type safety.
//                  Domain entity implementing business rules and invariants
//                  following Domain-Driven Design principles.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

/// <summary>
/// Risk Analysis unique identifier following the established ID pattern
/// </summary>
public class RiskAnalysisID : BaseID<string>
{
    public RiskAnalysisID(string id) : base(id) { }
}

