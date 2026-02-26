//-----------------------------------------------------------------------
// <copyright file="SMSUserRolePermissionID.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Value object representing smsuserrolepermissionid with access control and authorization logic.
//                  Immutable value object encapsulating domain concepts with
//                  business logic and validation rules.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

public class SMSUserRolePermissionID : BaseID<string>
{
    public SMSUserRolePermissionID(string id) : base(id) { }
}
