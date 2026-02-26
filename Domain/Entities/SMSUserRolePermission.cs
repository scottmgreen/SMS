//-----------------------------------------------------------------------
// <copyright file="SMSUserRolePermission.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: SMS user entity representing smsuserrolepermission with authentication and authorization capabilities.
//                  Domain entity implementing business rules and invariants
//                  following Domain-Driven Design principles.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

public sealed class SMSUserRolePermission : BaseAuditableEntity
{
    public SMSUserRolePermission(SMSUserRolePermissionID id) : base(id, "SYSTEM", DateTime.UtcNow) { }

    public string? Code { get; set; }
    public string? SMSUserRoleCode { get; set; }
    public string? SMSModule { get; set; }
    public bool Create { get; set; }
    public bool Read { get; set; }
    public bool Update { get; set; }
    public bool Delete { get; set; }
}

