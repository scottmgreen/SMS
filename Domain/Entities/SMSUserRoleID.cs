//-----------------------------------------------------------------------
// <copyright file="SMSUserRoleID.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Strongly-typed identifier for smsuserrole entities ensuring type safety.
//                  Immutable value object encapsulating domain concepts with
//                  business logic and validation rules.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

//SMSUserRoles
public class SMSUserRoleID : BaseID<string>
{
    public SMSUserRoleID(string id) : base(id) { }
}

