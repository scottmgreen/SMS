//-----------------------------------------------------------------------
// <copyright file="SMSOrganizationalGroup.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: SMS organizational group entity representing smsorganizationalgroup for internal team and department organization.
//                  Domain entity implementing business rules and invariants
//                  following Domain-Driven Design principles.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Entities;

/// <summary>
/// SMS Organizational Group entity for managing internal organizational groups
/// </summary>
public sealed class SMSOrganizationalGroup : BaseUserGroup
{
    private const string DefaultActor = "FLYPDX_API";

    /// <summary>
    /// Unique identifier for the organizational group
    /// </summary>
    public SMSOrganizationalGroupID Id { get; private set; }

    /// <summary>
    /// Unique code for the organizational group (e.g., "OG-20240101-XXXXXXXX")
    /// </summary>
    /// Type of organizational group (e.g., "Department", "Division", "Team", "Committee")
    public string GroupType { get; set; } = "Department";

    /// <summary>
    /// Authority level of this group for decision-making and approvals
    /// </summary>
    public string AuthorityLevel { get; set; } = "Standard";

    /// <summary>
    /// Parameterless constructor for Entity Framework
    /// </summary>
    private SMSOrganizationalGroup() : base(new SMSOrganizationalGroupID(string.Empty), string.Empty, DateTime.UtcNow)
    {
        Id = new SMSOrganizationalGroupID(string.Empty);
    }

    /// <summary>
    /// Creates a new organizational group with the specified ID
    /// </summary>
    /// <param name="id">The unique identifier for the organizational group</param>
    public SMSOrganizationalGroup(SMSOrganizationalGroupID id) : base(id, DefaultActor, DateTime.UtcNow)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Code = id.Value;
        CreatedDate = DateTime.UtcNow;
        CreatedBy = DefaultActor;
    }

    /// <summary>
    /// Creates a new organizational group with auto-generated ID
    /// </summary>
    /// <param name="name">The name of the organizational group</param>
    /// <param name="groupType">The type of organizational group</param>
    /// <param name="createdBy">The user creating the group</param>
    public static SMSOrganizationalGroup Create(string name, string groupType = "Department", string createdBy = DefaultActor)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Group name cannot be null or empty.", nameof(name));

        var groupCode = $"OG-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
        var group = new SMSOrganizationalGroup(new SMSOrganizationalGroupID(groupCode))
        {
            Name = name,
            GroupType = groupType,
            CreatedBy = createdBy
        };

        return group;
    }

    /// <summary>
    /// Updates the group information
    /// </summary>
    /// <param name="name">New group name</param>
    /// <param name="description">New description</param>
    /// <param name="groupType">New group type</param>
    /// <param name="authorityLevel">New authority level</param>
    /// <param name="updatedBy">User making the update</param>
    public void Update(string name, string? description, string groupType, string authorityLevel, string updatedBy)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Group name cannot be null or empty.", nameof(name));

        Name = name;
        Description = description;
        GroupType = groupType;
        AuthorityLevel = authorityLevel;
        UpdatedBy = updatedBy;
        UpdatedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Activates the organizational group
    /// </summary>
    /// <param name="activatedBy">User activating the group</param>
    public void Activate(string activatedBy)
    {
        IsActive = true;
        UpdatedBy = activatedBy;
        UpdatedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivates the organizational group
    /// </summary>
    /// <param name="deactivatedBy">User deactivating the group</param>
    public void Deactivate(string deactivatedBy)
    {
        IsActive = false;
        UpdatedBy = deactivatedBy;
        UpdatedDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the display name for the organizational group
    /// </summary>
    public string DisplayName => $"{Name} ({GroupType})";

    /// <summary>
    /// Gets the full description including authority level
    /// </summary>
    public string FullDescription =>
        $"{Name} - {GroupType}" +
        (!string.IsNullOrEmpty(AuthorityLevel) && AuthorityLevel != "Standard" ? $" | Authority: {AuthorityLevel}" : "") +
        (!string.IsNullOrEmpty(Description) ? $" | {Description}" : "");
}
