//-----------------------------------------------------------------------
// <copyright file="SMSApplicationGroup.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: SMS application group entity representing smsapplicationgroup for group-based access control.
//                  Domain entity implementing business rules and invariants
//                  following Domain-Driven Design principles.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

/// <summary>
/// SMS Application Group entity for managing application user groups and access control
/// </summary>
public class SMSApplicationGroup : BaseAuditableEntity
{
    public SMSApplicationGroup(SMSApplicationGroupID id) : base(id, "SYSTEM", DateTime.UtcNow) { }

    public string Code { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }

    public List<SMSApplicationUser> GroupMembers { get; set; }
}

