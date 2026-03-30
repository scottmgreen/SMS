//-----------------------------------------------------------------------
// <copyright file="SMSOrganizationalGroupID.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Strongly-typed identifier for smsorganizationalgroup entities ensuring type safety.
//                  Domain entity implementing business rules and invariants
//                  following Domain-Driven Design principles.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

/// <summary>
/// Value object representing a unique identifier for SMS Organizational Groups
/// Follows the same pattern as other group IDs in the system
/// </summary>
public sealed class SMSOrganizationalGroupID : BaseUserID
{
    public SMSOrganizationalGroupID(string id) : base(id) { }

    // Implicit conversion from string for convenience
    public static implicit operator SMSOrganizationalGroupID(string value) => new(value);
}
