//-----------------------------------------------------------------------
// <copyright file="SMSUserRoleID.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Strongly-typed identifier for smsuserrole entities ensuring type safety.
//                  Domain entity implementing business rules and invariants
//                  following Domain-Driven Design principles.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

/// <summary>
/// SMS User Role unique identifier following the established ID pattern
/// </summary>
public class SMSUserRoleID : BaseID<string>
{
    public SMSUserRoleID(string id) : base(id) { }
}

