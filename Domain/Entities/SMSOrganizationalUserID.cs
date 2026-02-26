//-----------------------------------------------------------------------
// <copyright file="SMSOrganizationalUserID.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Strongly-typed identifier for smsorganizationaluser entities ensuring type safety.
//                  Domain entity implementing business rules and invariants
//                  following Domain-Driven Design principles.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

/// <summary>
/// Represents a unique identifier for SMS Organizational User entities
/// </summary>
public sealed class SMSOrganizationalUserID : BaseUserID
{
    public SMSOrganizationalUserID(string id) : base(id) { }

    // Implicit conversion from string for convenience
    public static implicit operator SMSOrganizationalUserID(string value) => new(value);
}
