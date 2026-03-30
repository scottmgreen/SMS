//-----------------------------------------------------------------------
// <copyright file="SMSStakeholderGroup.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: SMS stakeholder group entity representing smsstakeholdergroup for external partner and stakeholder organization.
//                  Domain entity implementing business rules and invariants
//                  following Domain-Driven Design principles.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

/// <summary>
/// SMS Stakeholder Group entity for managing external stakeholder groups
/// </summary>
public class SMSStakeholderGroup : BaseAuditableEntity
{
    public SMSStakeholderGroup(SMSStakeholderGroupID id) : base(id, "SYSTEM", DateTime.UtcNow) { }

    public string Code { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }

    public List<SMSStakeholderUser> GroupMembers { get; set; }
}

