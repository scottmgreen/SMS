using SMS_Domain.Common;
using SMS_Domain.ValueObjects;

namespace SMS_Domain.Entities;

/// <summary>
/// Represents an SMS organizational user entity for internal Port of Portland employees
/// </summary>
public sealed class SMSOrganizationalUser : BaseUser
{
    public SMSOrganizationalUser (SMSOrganizationalUserID id):base(id,"SYSTEM", DateTime.UtcNow)
    {

    }
    public SMSOrganizationalUserID OrganizationalUserId { get; private set; }
    public string Department { get;  set; }
    public string Position { get;  set; }
    public string OrganizationLevel { get;  set; }
    //public WorkflowPermissions Permissions { get; private set; }

    // For Entity Framework
    //private SMSOrganizationalUser(SMSOrganizationalUserID id) : base(id,"SYSTEM", DateTime.UtcNow)
    //{
    //    Department = string.Empty;
    //    Position = string.Empty;
    //    OrganizationLevel = string.Empty;
    //    Permissions = WorkflowPermissions.ForStaff;
    //}

    

    

    

    /// <summary>
    /// Updates the organizational-specific properties
    /// </summary>
    

    /// <summary>
    /// Updates workflow permissions directly
    /// </summary>
    

    /// <summary>
    /// Checks if the user belongs to a specific department
    /// </summary>
    

    /// <summary>
    /// Checks if the user has a specific position
    /// </summary>
    

    /// <summary>
    /// Checks if the user is at or above a specific organization level
    /// </summary>
    

    /// <summary>
    /// Checks if the user has a specific workflow permission
    /// </summary>
    
    /// <summary>
    /// Checks if user can approve a specific risk level
    /// </summary>
    //public bool CanApproveRisk(string riskLevel)
    //{
    //    return riskLevel.ToUpperInvariant() switch
    //    {
    //        "LOW" => Permissions.CanApproveLowRisk,
    //        "MEDIUM" => Permissions.CanApproveMediumRisk,
    //        "HIGH" => Permissions.CanApproveHighRisk,
    //        "CRITICAL" => Permissions.CanApproveCriticalRisk,
    //        _ => false
    //    };
    //}

    /// <summary>
    /// Checks if user can escalate to a specific level
    /// </summary>
    //public bool CanEscalate(string escalationType)
    //{
    //    return escalationType.ToUpperInvariant() switch
    //    {
    //        "WITHIN_DEPARTMENT" => Permissions.CanEscalateWithinDepartment,
    //        "ACROSS_DEPARTMENTS" => Permissions.CanEscalateAcrossDepartments,
    //        "TO_EXECUTIVE" => Permissions.CanEscalateToExecutiveLevel,
    //        _ => false
    //    };
    //}

    /// <summary>
    /// Checks if user can perform committee-related actions
    /// </summary>
    //public bool CanPerformCommitteeAction(string action)
    //{
    //    return action.ToUpperInvariant() switch
    //    {
    //        "PARTICIPATE" => Permissions.CanParticipateInCommittees,
    //        "CHAIR" => Permissions.CanChairCommittees,
    //        "CREATE" => Permissions.CanCreateCommittees,
    //        "SCHEDULE_MEETINGS" => Permissions.CanScheduleMeetings,
    //        _ => false
    //    };
    //}

    /// <summary>
    /// Checks if user can perform investigation-related actions
    /// </summary>
    //public bool CanPerformInvestigationAction(string action)
    //{
    //    return action.ToUpperInvariant() switch
    //    {
    //        "INITIATE" => Permissions.CanInitiateInvestigations,
    //        "LEAD" => Permissions.CanLeadInvestigations,
    //        "CLOSE" => Permissions.CanCloseInvestigations,
    //        _ => false
    //    };
    //}

    /// <summary>
    /// Checks if user can view specific report types
    /// </summary>
    //public bool CanViewReports(string reportType)
    //{
    //    return reportType.ToUpperInvariant() switch
    //    {
    //        "DEPARTMENT" => Permissions.CanViewDepartmentReports,
    //        "CROSS_DEPARTMENT" => Permissions.CanViewCrossDepartmentReports,
    //        "EXECUTIVE" => Permissions.CanViewExecutiveReports,
    //        _ => false
    //    };
    //}

    /// <summary>
    /// Gets the user's organizational hierarchy display
    /// </summary>
        

    /// <summary>
    /// Checks if this user can supervise another organizational user
    /// </summary>
    //public bool CanSupervise(SMSOrganizationalUser otherUser)
    //{
    //    // Same department and higher organization level
    //    return IsInDepartment(otherUser.Department) && 
    //           HasOrganizationLevel("Supervisor") &&
    //           Array.IndexOf(new[] { "Staff", "Supervisor", "Manager", "Director", "Executive" }, OrganizationLevel) >
    //           Array.IndexOf(new[] { "Staff", "Supervisor", "Manager", "Director", "Executive" }, otherUser.OrganizationLevel);
    //}

    /// <summary>
    /// Gets workflow permission summary for administrative purposes
    /// </summary>
    //public string GetWorkflowPermissionSummary()
    //{
    //    var capabilities = new List<string>();
        
    //    // Risk approval capabilities
    //    var riskLevels = new List<string>();
    //    if (Permissions.CanApproveLowRisk) riskLevels.Add("Low");
    //    if (Permissions.CanApproveMediumRisk) riskLevels.Add("Medium");
    //    if (Permissions.CanApproveHighRisk) riskLevels.Add("High");
    //    if (Permissions.CanApproveCriticalRisk) riskLevels.Add("Critical");
    //    if (riskLevels.Any()) capabilities.Add($"Risk Approval: {string.Join(", ", riskLevels)}");

    //    // Committee capabilities
    //    if (Permissions.CanChairCommittees) capabilities.Add("Committee Chair");
    //    else if (Permissions.CanParticipateInCommittees) capabilities.Add("Committee Member");

    //    // Investigation capabilities
    //    if (Permissions.CanLeadInvestigations) capabilities.Add("Investigation Lead");
    //    else if (Permissions.CanInitiateInvestigations) capabilities.Add("Investigation Initiate");

    //    return string.Join(" | ", capabilities);
    //}
}