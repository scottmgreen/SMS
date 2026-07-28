//-----------------------------------------------------------------------
// <copyright file="SMSUserRole.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: SMS user entity representing smsuserrole with authentication and authorization capabilities.
//                  Domain entity implementing business rules and invariants
//                  following Domain-Driven Design principles.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

public sealed class SMSUserRole : BaseAuditableEntity
{

    public SMSUserRole(SMSUserRoleID id) : base(id, string.Empty, DateTime.UtcNow)
    {
        Code = id.Value;
        Permissions = new List<SMSUserRolePermission>();
    }

    public string? Code { get; set; }
    public string? Name { get; set; }
    public List<SMSUserRolePermission> Permissions { get; set; }
}
