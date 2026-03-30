//-----------------------------------------------------------------------
// <copyright file="SMSUserRolePermissionID.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Strongly-typed identifier for smsuserolepermission entities ensuring type safety.
//                  Domain entity implementing business rules and invariants
//                  following Domain-Driven Design principles.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

/// <summary>
/// SMS User Role Permission unique identifier following the established ID pattern
/// </summary>
public class SMSUserRolePermissionID : BaseID<string>
{
    public SMSUserRolePermissionID(string id) : base(id) { }
}
