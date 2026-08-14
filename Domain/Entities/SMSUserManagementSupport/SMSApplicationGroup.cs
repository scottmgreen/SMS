//-----------------------------------------------------------------------
// <copyright file="SMSApplicationGroup.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: SMS application group entity representing smsapplicationgroup for group-based access control.
//                  Domain entity implementing business rules and invariants
//                  following Domain-Driven Design principles.
// </copyright>
//-----------------------------------------------------------------------

using Domain.Common;

namespace SMS_Domain.Entities;

/// <summary>
/// SMS Application Group entity for managing application user groups and access control
/// </summary>
public sealed class SMSApplicationGroup : BaseUserGroup
{
    public SMSApplicationGroup(SMSApplicationGroupID id) : base(id, string.Empty, DateTime.UtcNow) { }

    public List<SMSApplicationUser> GroupMembers { get; set; }
}

