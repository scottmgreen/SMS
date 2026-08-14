//-----------------------------------------------------------------------
// <copyright file="SMSStakeholderGroup.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: SMS stakeholder group entity representing smsstakeholdergroup for external partner and stakeholder organization.
//                  Domain entity implementing business rules and invariants
//                  following Domain-Driven Design principles.
// </copyright>
//-----------------------------------------------------------------------

using Domain.Common;

namespace SMS_Domain.Entities;

/// <summary>
/// SMS Stakeholder Group entity for managing external stakeholder groups
/// </summary>
public sealed class SMSStakeholderGroup : BaseUserGroup
{
    public SMSStakeholderGroup(SMSStakeholderGroupID id) : base(id, string.Empty, DateTime.UtcNow) { }

    public List<SMSStakeholderUser> GroupMembers { get; set; }
}

