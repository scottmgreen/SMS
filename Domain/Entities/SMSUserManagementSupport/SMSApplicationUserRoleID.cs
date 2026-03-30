//-----------------------------------------------------------------------
// <copyright file="SMSApplicationUserRoleID.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Strongly-typed identifier for smsapplicationuserrole entities ensuring type safety.
//                  Immutable value object encapsulating domain concepts with
//                  business logic and validation rules.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

/// <summary>
/// SMS Application User Role unique identifier
/// </summary>
public sealed class SMSApplicationUserRoleID : BaseUserID
{
    public SMSApplicationUserRoleID(string value) : base(value) { }

    public static implicit operator string(SMSApplicationUserRoleID id) => id.Value;
    public static implicit operator SMSApplicationUserRoleID(string value) => new(value);
}
