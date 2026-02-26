//-----------------------------------------------------------------------
// <copyright file="SMSApplicationUserRoleID.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Strongly-typed identifier for smsapplicationuserrole entities ensuring type safety.
//                  Domain entity implementing business rules and invariants
//                  following Domain-Driven Design principles.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

/// <summary>
/// Represents a unique identifier for SMS User Role assignments
/// </summary>
public sealed class SMSApplicationUserRoleID : BaseID<string>
{
    public SMSApplicationUserRoleID(string value) : base(value) { }

    public static implicit operator string(SMSApplicationUserRoleID id) => id.Value;
    public static implicit operator SMSApplicationUserRoleID(string value) => new(value);
}
