//-----------------------------------------------------------------------
// <copyright file="HazardFileID.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Strongly-typed identifier for hazardfile entities ensuring type safety.
//                  Domain entity implementing business rules and invariants
//                  following Domain-Driven Design principles.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

/// <summary>
/// HazardFile ID Value Object
/// </summary>
public sealed class HazardFileID : BaseID<string>
{
    public HazardFileID(string id) : base(id) { }
}
