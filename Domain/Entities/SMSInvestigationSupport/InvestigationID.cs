//-----------------------------------------------------------------------
// <copyright file="InvestigationID.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Strongly-typed identifier for investigation entities ensuring type safety.
//                  Domain entity implementing business rules and invariants
//                  following Domain-Driven Design principles.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

/// <summary>
/// Investigation unique identifier following the established ID pattern
/// </summary>
public class InvestigationID : BaseID<string>
{
    public InvestigationID(string id) : base(id) { }
}

