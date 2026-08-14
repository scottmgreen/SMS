//-----------------------------------------------------------------------
// <copyright file="SMSStakeholderUser.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: SMS user entity representing smsstakeholderuser with authentication and authorization capabilities.
//                  Domain entity implementing business rules and invariants
//                  following Domain-Driven Design principles.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

/// <summary>
/// Represents an SMS stakeholder user entity for external partners, airlines, contractors, etc.
/// </summary>
public sealed class SMSStakeholderUser : BaseUser
{
    // Simple constructors

    public SMSStakeholderUser(SMSStakeholderUserID id) : base(id, string.Empty, DateTime.UtcNow)
    {
        StakeholderUserId = id;
    }

    // Simple properties with public setters
    public SMSStakeholderUserID StakeholderUserId { get; set; }
    public string StakeholderType
    {
        get => Title;
        set => Title = value?.Trim() ?? string.Empty;
    }

    public bool IsPOPEmployee { get; set; }
    


}
