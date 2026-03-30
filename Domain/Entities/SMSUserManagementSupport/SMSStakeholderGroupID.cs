//-----------------------------------------------------------------------
// <copyright file="SMSStakeholderGroupID.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Strongly-typed identifier for smsstakeholdergroup entities ensuring type safety.
//                  Domain entity implementing business rules and invariants
//                  following Domain-Driven Design principles.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

/// <summary>
/// Represents a unique identifier for SMS Stakeholder Group entities
/// </summary>
public sealed class SMSStakeholderGroupID : BaseUserID
{
    public SMSStakeholderGroupID(string id) : base(id) { }


}
