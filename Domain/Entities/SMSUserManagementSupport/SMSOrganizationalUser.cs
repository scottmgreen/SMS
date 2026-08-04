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
    /// Numeric authority level (legacy persisted field). OrganizationLevel.AuthorityLevel is authoritative.
    /// </summary>
    public int? AuthorityLevel { get; set; }

    /// <summary>
    /// Legacy risk approval authority label retained for compatibility.
    /// </summary>
    public string? RiskApprovalAuthority { get; set; }

    /// <summary>
    /// Effective authority level derived from OrganizationLevel, with AuthorityLevel as fallback.
    /// </summary>
    public int EffectiveAuthorityLevel => OrganizationLevel?.AuthorityLevel ?? AuthorityLevel ?? 0;

    /// <summary>
    /// Synchronizes legacy authority fields from the OrganizationLevel source of truth.
    /// </summary>
    public void SyncAuthorityFromOrganizationLevel()
    {
        AuthorityLevel = OrganizationLevel?.AuthorityLevel ?? 0;
        RiskApprovalAuthority = OrganizationLevel?.Value;
    }
}
