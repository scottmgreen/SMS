//-----------------------------------------------------------------------
// <copyright file="SMSApplicationGroup.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: SMS domain entity representing smsapplicationgroup with business rules and lifecycle management.
//                  Domain entity implementing business rules and invariants
//                  following Domain-Driven Design principles.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

public sealed class SMSApplicationGroup : BaseAuditableEntity
{
    public SMSApplicationGroup(SMSApplicationGroupID id) : base(id, "SYSTEM", DateTime.UtcNow) { }

    public string Code { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }

    public List<SMSApplicationUser> GroupMembers { get; set; }
}

