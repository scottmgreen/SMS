using SMS_Domain.Common;
using SMS_Domain.ValueObjects;

namespace SMS_Domain.Entities;

/// <summary>
/// Represents an SMS organizational user entity for internal Port of Portland employees
/// </summary>
public sealed class SMSOrganizationalUser : BaseUser
{
    public SMSOrganizationalUserID OrganizationalUserId { get; private set; }
    public string Department { get; private set; }
    public string Position { get; private set; }
    public string OrganizationLevel { get; private set; }
    public WorkflowPermissions Permissions { get; private set; }

    // For Entity Framework
    private SMSOrganizationalUser() : base()
    {
        OrganizationalUserId = new SMSOrganizationalUserID(Guid.NewGuid().ToString());
        Department = string.Empty;
        Position = string.Empty;
        OrganizationLevel = string.Empty;
        Permissions = WorkflowPermissions.ForStaff;
    }

    private SMSOrganizationalUser(
        string code,
        FirstName firstName,
        LastName lastName,
        UserName userName,
        Password password,
        string department,
        string position,
        string organizationLevel,
        WorkflowPermissions permissions,
        string createdBy) : base(code, firstName, lastName, userName, password, createdBy)
    {
        OrganizationalUserId = new SMSOrganizationalUserID(UserId.Value);
        Department = department;
        Position = position;
        OrganizationLevel = organizationLevel;
        Permissions = permissions;
    }

    public static SMSOrganizationalUser Create(
        string code,
        FirstName firstName,
        LastName lastName,
        UserName userName,
        Password password,
        string department,
        string position,
        string organizationLevel,
        string createdBy)
    {
        // Create permissions based on organization level
        var permissionsResult = WorkflowPermissions.Create(organizationLevel);
        var permissions = permissionsResult.IsSuccess ? permissionsResult.Value : WorkflowPermissions.ForStaff;

        return new SMSOrganizationalUser(
            code,
            firstName,
            lastName,
            userName,
            password,
            department,
            position,
            organizationLevel,
            permissions,
            createdBy);
    }

    public static SMSOrganizationalUser CreateWithCustomPermissions(
        string code,
        FirstName firstName,
        LastName lastName,
        UserName userName,
        Password password,
        string department,
        string position,
        WorkflowPermissions permissions,
        string createdBy)
    {
        return new SMSOrganizationalUser(
            code,
            firstName,
            lastName,
            userName,
            password,
            department,
            position,
            permissions.GetOrganizationLevel(),
            permissions,
            createdBy);
    }

    /// <summary>
    /// Updates the organizational-specific properties
    /// </summary>
    public void UpdateOrganizationalInfo(string department, string position, string organizationLevel)
    {
        Department = department;
        Position = position;
        OrganizationLevel = organizationLevel;
        
        // Update permissions based on new organization level
        var permissionsResult = WorkflowPermissions.Create(organizationLevel);
        if (permissionsResult.IsSuccess)
        {
            Permissions = permissionsResult.Value;
        }
    }

    /// <summary>
    /// Updates workflow permissions directly
    /// </summary>
    public void UpdatePermissions(WorkflowPermissions permissions)
    {
        Permissions = permissions;
        OrganizationLevel = permissions.GetOrganizationLevel();
    }

    /// <summary>
    /// Checks if the user belongs to a specific department
    /// </summary>
    public bool IsInDepartment(string department)
    {
        return string.Equals(Department, department, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if the user has a specific position
    /// </summary>
    public bool HasPosition(string position)
    {
        return string.Equals(Position, position, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if the user is at or above a specific organization level
    /// </summary>
    public bool HasOrganizationLevel(string requiredLevel)
    {
        // Define organization hierarchy levels
        var levels = new[] { "Staff", "Supervisor", "Manager", "Director", "Executive" };
        
        var userLevelIndex = Array.IndexOf(levels, OrganizationLevel);
        var requiredLevelIndex = Array.IndexOf(levels, requiredLevel);
        
        return userLevelIndex >= requiredLevelIndex;
    }

    /// <summary>
    /// Checks if the user has a specific workflow permission
    /// </summary>
    public bool HasPermission(string permission)
    {
        return Permissions.HasPermission(permission);
    }

    /// <summary>
    /// Checks if user can approve a specific risk level
    /// </summary>
    public bool CanApproveRisk(string riskLevel)
    {
        return riskLevel.ToUpperInvariant() switch
        {
            "LOW" => Permissions.CanApproveLowRisk,
            "MEDIUM" => Permissions.CanApproveMediumRisk,
            "HIGH" => Permissions.CanApproveHighRisk,
            "CRITICAL" => Permissions.CanApproveCriticalRisk,
            _ => false
        };
    }

    /// <summary>
    /// Checks if user can escalate to a specific level
    /// </summary>
    public bool CanEscalate(string escalationType)
    {
        return escalationType.ToUpperInvariant() switch
        {
            "WITHIN_DEPARTMENT" => Permissions.CanEscalateWithinDepartment,
            "ACROSS_DEPARTMENTS" => Permissions.CanEscalateAcrossDepartments,
            "TO_EXECUTIVE" => Permissions.CanEscalateToExecutiveLevel,
            _ => false
        };
    }

    /// <summary>
    /// Checks if user can perform committee-related actions
    /// </summary>
    public bool CanPerformCommitteeAction(string action)
    {
        return action.ToUpperInvariant() switch
        {
            "PARTICIPATE" => Permissions.CanParticipateInCommittees,
            "CHAIR" => Permissions.CanChairCommittees,
            "CREATE" => Permissions.CanCreateCommittees,
            "SCHEDULE_MEETINGS" => Permissions.CanScheduleMeetings,
            _ => false
        };
    }

    /// <summary>
    /// Checks if user can perform investigation-related actions
    /// </summary>
    public bool CanPerformInvestigationAction(string action)
    {
        return action.ToUpperInvariant() switch
        {
            "INITIATE" => Permissions.CanInitiateInvestigations,
            "LEAD" => Permissions.CanLeadInvestigations,
            "CLOSE" => Permissions.CanCloseInvestigations,
            _ => false
        };
    }

    /// <summary>
    /// Checks if user can view specific report types
    /// </summary>
    public bool CanViewReports(string reportType)
    {
        return reportType.ToUpperInvariant() switch
        {
            "DEPARTMENT" => Permissions.CanViewDepartmentReports,
            "CROSS_DEPARTMENT" => Permissions.CanViewCrossDepartmentReports,
            "EXECUTIVE" => Permissions.CanViewExecutiveReports,
            _ => false
        };
    }

    /// <summary>
    /// Gets the user's organizational hierarchy display
    /// </summary>
    public string GetOrganizationalHierarchy()
    {
        return $"{Department} - {Position} ({OrganizationLevel})";
    }

    public override string GetUserType() => "OrganizationalUser";

    public override string GetDepartmentInfo() => Department;

    /// <summary>
    /// Gets organizational-specific user information for display
    /// </summary>
    public string GetOrganizationalSummary()
    {
        return $"{DisplayName} - {Position}, {Department}";
    }

    /// <summary>
    /// Checks if this user can supervise another organizational user
    /// </summary>
    public bool CanSupervise(SMSOrganizationalUser otherUser)
    {
        // Same department and higher organization level
        return IsInDepartment(otherUser.Department) && 
               HasOrganizationLevel("Supervisor") &&
               Array.IndexOf(new[] { "Staff", "Supervisor", "Manager", "Director", "Executive" }, OrganizationLevel) >
               Array.IndexOf(new[] { "Staff", "Supervisor", "Manager", "Director", "Executive" }, otherUser.OrganizationLevel);
    }

    /// <summary>
    /// Gets workflow permission summary for administrative purposes
    /// </summary>
    public string GetWorkflowPermissionSummary()
    {
        var capabilities = new List<string>();
        
        // Risk approval capabilities
        var riskLevels = new List<string>();
        if (Permissions.CanApproveLowRisk) riskLevels.Add("Low");
        if (Permissions.CanApproveMediumRisk) riskLevels.Add("Medium");
        if (Permissions.CanApproveHighRisk) riskLevels.Add("High");
        if (Permissions.CanApproveCriticalRisk) riskLevels.Add("Critical");
        if (riskLevels.Any()) capabilities.Add($"Risk Approval: {string.Join(", ", riskLevels)}");

        // Committee capabilities
        if (Permissions.CanChairCommittees) capabilities.Add("Committee Chair");
        else if (Permissions.CanParticipateInCommittees) capabilities.Add("Committee Member");

        // Investigation capabilities
        if (Permissions.CanLeadInvestigations) capabilities.Add("Investigation Lead");
        else if (Permissions.CanInitiateInvestigations) capabilities.Add("Investigation Initiate");

        return string.Join(" | ", capabilities);
    }
}