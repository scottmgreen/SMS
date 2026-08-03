//-----------------------------------------------------------------------
// <copyright file="BaseUserGroup.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Base entity class for SMS user group variants with shared group metadata.
//                  Shared domain infrastructure providing common user group
//                  properties across Application, Stakeholder, and Organizational groups.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

/// <summary>
/// Base entity for user group types in SMS
/// </summary>
public abstract class BaseUserGroup : BaseAuditableEntity
{
    protected BaseUserGroup(BaseUserID id, string createdBy, DateTime createdDate)
        : base(id ?? throw new ArgumentNullException(nameof(id)), createdBy, createdDate)
    {
        Code = id.Value;
    }

    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public string? ContactEmail { get; set; }
}
