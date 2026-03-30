//-----------------------------------------------------------------------
// <copyright file="ScoringPanelID.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Strongly-typed identifier for scoringpanel entities ensuring type safety.
//                  Domain entity implementing business rules and invariants
//                  following Domain-Driven Design principles.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

/// <summary>
/// Scoring Panel unique identifier following the established ID pattern
/// </summary>
public class ScoringPanelID : BaseID<string>
{
    public ScoringPanelID(string id) : base(id) { }
}
