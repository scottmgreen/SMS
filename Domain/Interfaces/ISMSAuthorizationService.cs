namespace SMS_Domain.Interfaces;

/// <summary>
/// SMS Authorization Service interface for comprehensive permission and role-based access control
/// Integrates user-specific permissions with role-based authority levels
/// </summary>
public interface ISMSAuthorizationService
{
    // User Permission Validation
    Task<Result<bool>> CanUserPerformActionAsync(string userId, string action, string? context = null);
    Task<Result<bool>> CanUserAccessAreaAsync(string userId, string area);
    Task<Result<bool>> CanUserViewDataAsync(string userId, string dataType, string? organizationFilter = null);

    // Role-Based Authority Validation
    Task<Result<bool>> HasUserRoleAuthorityAsync(string userId, int requiredAuthorityLevel);
    Task<Result<bool>> CanUserApproveRiskAsync(string userId, RiskLevel riskLevel);
    Task<Result<bool>> CanUserEscalateAsync(string userId, string escalationType);

    // Committee Authorization
    Task<Result<bool>> CanUserParticipateInCommitteeAsync(string userId, CommitteeType committeeType);
    Task<Result<bool>> CanUserChairCommitteeAsync(string userId, CommitteeType committeeType);
    Task<Result<bool>> CanUserCreateCommitteeAsync(string userId, CommitteeType committeeType);

    // Investigation Authorization
    Task<Result<bool>> CanUserInitiateInvestigationAsync(string userId, string investigationType);
    Task<Result<bool>> CanUserLeadInvestigationAsync(string userId, string investigationType);
    Task<Result<bool>> CanUserCloseInvestigationAsync(string userId, string investigationType);

    // Department and Cross-Department Authorization
    Task<Result<bool>> CanUserAccessDepartmentDataAsync(string userId, string department);
    Task<Result<bool>> CanUserPerformCrossDepartmentActionAsync(string userId, string action);

    // Workflow Authorization
    Task<Result<bool>> CanUserExecuteWorkflowAsync(string userId, string workflowType, string stage);
    Task<Result<string>> GetRequiredApproverForRiskAsync(RiskLevel riskLevel, string department);
    Task<Result<IEnumerable<string>>> GetEligibleApproversAsync(DecisionAuthority requiredAuthority, string? department = null);

    // Stakeholder-Specific Authorization
    Task<Result<bool>> CanStakeholderAccessOperationalDataAsync(string userId, string dataScope);
    Task<Result<bool>> CanStakeholderSubmitReportsAsync(string userId, string reportType);

    // Administrative Authorization
    Task<Result<bool>> CanUserManageUsersAsync(string userId, string targetUserType);
    Task<Result<bool>> CanUserViewAuditLogsAsync(string userId, string logType);
    Task<Result<bool>> CanUserConfigureSystemAsync(string userId, string configurationArea);

    // Permission Summary and Reporting
    Task<Result<UserAuthorizationSummary>> GetUserAuthorizationSummaryAsync(string userId);
    Task<Result<IEnumerable<string>>> GetUserPermissionsAsync(string userId);
    Task<Result<IEnumerable<string>>> GetUserAccessibleAreasAsync(string userId);
}

/// <summary>
/// User authorization summary for reporting and administrative purposes
/// </summary>
public class UserAuthorizationSummary
{
    public string UserId { get; set; } = string.Empty;
    public string UserType { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string PrimaryRole { get; set; } = string.Empty;
    public int MaxAuthorityLevel { get; set; }
    public string[] AssignedRoles { get; set; } = Array.Empty<string>();
    public string[] Permissions { get; set; } = Array.Empty<string>();
    public string[] AccessibleAreas { get; set; } = Array.Empty<string>();
    public string[] RiskApprovalLevels { get; set; } = Array.Empty<string>();
    public string[] CommitteeRoles { get; set; } = Array.Empty<string>();
    public bool CanEscalate { get; set; }
    public bool CanLeadInvestigations { get; set; }
    public DateTime LastUpdated { get; set; }
}