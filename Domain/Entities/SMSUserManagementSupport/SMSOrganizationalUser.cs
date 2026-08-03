//-----------------------------------------------------------------------
// <copyright file="SMSOrganizationalUser.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: SMS user entity representing smsorganizationaluser with authentication and authorization capabilities.
//                  Domain entity implementing business rules and invariants
//                  following Domain-Driven Design principles.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

/// <summary>
/// Represents an SMS organizational user entity for internal Port of Portland employees
/// </summary>
public sealed class SMSOrganizationalUser : BaseUser
{
    public SMSOrganizationalUser(SMSOrganizationalUserID id) : base(id, string.Empty, DateTime.UtcNow)
    {
        OrganizationalUserId = id;
    }
    public SMSOrganizationalUserID OrganizationalUserId { get; private set; }
    public SMSDepartment Department { get; set; } 
    public string Position { get; set; } = string.Empty;
    /// <summary>
    /// OrganizationLevel (AE, RE, RM, SMS Manager, SMS Coordinator, SMS Team)
    /// </summary>
    public SMSOrganizationalLevel OrganizationLevel { get; set; } 


    /// <summary>
    /// Authority level for risk approval (Strategic, Executive, Operational, Process, Support)
    /// </summary>
    public int? AuthorityLevel { get; set; }

    /// <summary>
    /// Risk levels this user can approve (Critical/High, High/Escalated, Medium/Low, etc.)
    /// </summary>
    public string? RiskApprovalAuthority { get; set; }
}
