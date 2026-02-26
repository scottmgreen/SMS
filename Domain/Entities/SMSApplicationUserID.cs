//-----------------------------------------------------------------------
// <copyright file="SMSApplicationUserID.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Strongly-typed identifier for smsapplicationuser entities ensuring type safety.
//                  Domain entity implementing business rules and invariants
//                  following Domain-Driven Design principles.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

/// <summary>
/// Represents a unique identifier for SMS Application User entities
/// </summary>
public sealed class SMSApplicationUserID : BaseUserID
{
    public SMSApplicationUserID(string id) : base(id) { }

    // Implicit conversion from string for convenience
    public static implicit operator SMSApplicationUserID(string value) => new(value);
}
