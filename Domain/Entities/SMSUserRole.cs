using SMS_Domain.Common;
using SMS_Domain.Enums;
using SMS_Domain.ValueObjects;

namespace SMS_Domain.Entities;

/// <summary>
/// Entity representing the assignment of SMS roles to users with context and lifecycle management
/// </summary>
public sealed class SMSUserRole : BaseAuditableEntity
{
    public new SMSUserRoleID Id { get; private set; }
    public string UserID { get; private set; } // Links to SMS Application/Organizational/Stakeholder User
    public string UserType { get; private set; } // "ApplicationUser", "OrganizationalUser", "StakeholderUser"
    public string RoleValue { get; private set; } // SMSRole.Value
    public string RoleName { get; private set; } // SMSRole.Name
    public string RoleCategory { get; private set; } // SMSRole.Category
    public string Department { get; private set; } // User's department
    public DateTime EffectiveDate { get; private set; }
    public DateTime? ExpirationDate { get; private set; }
    public bool IsActive { get; private set; }
    public string AssignedBy { get; private set; }
    public string? AssignmentNotes { get; private set; }
    public string? DeactivationReason { get; private set; }
    public DateTime? DeactivatedDate { get; private set; }
    public string? DeactivatedBy { get; private set; }

    // For Entity Framework
    private SMSUserRole() : base(new SMSUserRoleID(Guid.NewGuid().ToString()), "System", DateTime.UtcNow) 
    { 
        Id = new SMSUserRoleID(Guid.NewGuid().ToString());
        UserID = string.Empty;
        UserType = string.Empty;
        RoleValue = string.Empty;
        RoleName = string.Empty;
        RoleCategory = string.Empty;
        Department = string.Empty;
        EffectiveDate = DateTime.UtcNow;
        IsActive = true;
        AssignedBy = string.Empty;
    }

    private SMSUserRole(
        SMSUserRoleID id,
        string userID,
        string userType,
        SMSRole role,
        string department,
        DateTime effectiveDate,
        string assignedBy,
        DateTime? expirationDate = null,
        string? assignmentNotes = null) : base(id, assignedBy, DateTime.UtcNow)
    {
        Id = id;
        UserID = userID;
        UserType = userType;
        RoleValue = role.Value;
        RoleName = role.Name;
        RoleCategory = role.Category;
        Department = department;
        EffectiveDate = effectiveDate;
        ExpirationDate = expirationDate;
        IsActive = true;
        AssignedBy = assignedBy;
        AssignmentNotes = assignmentNotes;
    }

    public static SMSUserRole Create(
        string userID,
        string userType,
        SMSRole role,
        string department,
        string assignedBy,
        DateTime? effectiveDate = null,
        DateTime? expirationDate = null,
        string? assignmentNotes = null)
    {
        return new SMSUserRole(
            new SMSUserRoleID(Guid.NewGuid().ToString()),
            userID,
            userType,
            role,
            department,
            effectiveDate ?? DateTime.UtcNow,
            assignedBy,
            expirationDate,
            assignmentNotes);
    }

    /// <summary>
    /// Gets the SMS role enum instance for this assignment
    /// </summary>
    public SMSRole GetRole()
    {
        var role = SMSRole.GetAllValues().FirstOrDefault(r => r.Value == RoleValue);
        return role ?? throw new InvalidOperationException($"Invalid role value: {RoleValue}");
    }

    /// <summary>
    /// Checks if the role assignment is currently active and within valid dates
    /// </summary>
    public bool IsCurrentlyActive()
    {
        var now = DateTime.UtcNow;
        return IsActive && 
               EffectiveDate <= now && 
               (ExpirationDate == null || ExpirationDate > now);
    }

    /// <summary>
    /// Deactivates the role assignment with reason and audit trail
    /// </summary>
    public void Deactivate(string deactivatedBy, string reason)
    {
        IsActive = false;
        DeactivationReason = reason;
        DeactivatedDate = DateTime.UtcNow;
        DeactivatedBy = deactivatedBy;
    }

    /// <summary>
    /// Reactivates the role assignment
    /// </summary>
    public void Reactivate()
    {
        IsActive = true;
        DeactivationReason = null;
        DeactivatedDate = null;
        DeactivatedBy = null;
    }

    /// <summary>
    /// Extends the role assignment with a new expiration date
    /// </summary>
    public void ExtendAssignment(DateTime newExpirationDate, string updatedBy)
    {
        ExpirationDate = newExpirationDate;
        // Update audit fields would be handled by BaseAuditableEntity
    }

    /// <summary>
    /// Updates assignment notes
    /// </summary>
    public void UpdateNotes(string notes, string updatedBy)
    {
        AssignmentNotes = notes;
        // Update audit fields would be handled by BaseAuditableEntity
    }

    /// <summary>
    /// Checks if this role assignment grants approval authority for the specified level
    /// </summary>
    public bool CanApprove(int requiredAuthorityLevel)
    {
        if (!IsCurrentlyActive()) return false;
        
        var role = GetRole();
        return role.CanApprove(requiredAuthorityLevel);
    }

    /// <summary>
    /// Checks if this is an executive-level role assignment
    /// </summary>
    public bool IsExecutiveRole => RoleCategory == "Executive";

    /// <summary>
    /// Checks if this is a management-level role assignment
    /// </summary>
    public bool IsManagementRole => RoleCategory == "Management";

    /// <summary>
    /// Checks if this is an operational-level role assignment
    /// </summary>
    public bool IsOperationalRole => RoleCategory == "Operational";

    /// <summary>
    /// Checks if this is a committee-specific role assignment
    /// </summary>
    public bool IsCommitteeRole => RoleCategory == "Committee";

    /// <summary>
    /// Checks if this is an external stakeholder role assignment
    /// </summary>
    public bool IsExternalRole => RoleCategory == "External";
}