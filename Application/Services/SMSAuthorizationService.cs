using SMS_Domain.Interfaces;
using SMS_Domain.Entities;
using SMS_Domain.Enums;
using SMS_Domain.ValueObjects;
using SMS_Shared.Common;
using SMS_Infrastructure.Interfaces;

namespace SMS_Application.Services;

/// <summary>
/// SMS Authorization Service implementation providing comprehensive access control
/// Integrates user permissions, role-based authority, and business rules
/// </summary>
public class SMSAuthorizationService : ISMSAuthorizationService
{
    private readonly ISMSApplicationUserRepository _applicationUserRepository;
    private readonly ISMSOrganizationalUserRepository _organizationalUserRepository;
    private readonly ISMSStakeholderUserRepository _stakeholderUserRepository;
    private readonly ISMSRoleService _roleService;

    public SMSAuthorizationService(
        ISMSApplicationUserRepository applicationUserRepository,
        ISMSOrganizationalUserRepository organizationalUserRepository,
        ISMSStakeholderUserRepository stakeholderUserRepository,
        ISMSRoleService roleService)
    {
        _applicationUserRepository = applicationUserRepository ?? throw new ArgumentNullException(nameof(applicationUserRepository));
        _organizationalUserRepository = organizationalUserRepository ?? throw new ArgumentNullException(nameof(organizationalUserRepository));
        _stakeholderUserRepository = stakeholderUserRepository ?? throw new ArgumentNullException(nameof(stakeholderUserRepository));
        _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
    }

    public async Task<Result<bool>> CanUserPerformActionAsync(string userId, string action, string? context = null)
    {
        try
        {
            // Get user and their permissions
            var userResult = await GetAnyUserAsync(userId);
            if (userResult.IsFailure)
                return Result<bool>.Failure<bool>(userResult.Error);

            var user = userResult.Value;

            // Check user-specific permissions
            var hasUserPermission = CheckUserPermission(user, action);
            
            // Check role-based authority if needed
            var hasRoleAuthority = await CheckRoleAuthorityAsync(userId, action, context);

            // Both checks must pass
            return Result<bool>.Success(hasUserPermission && hasRoleAuthority.IsSuccess && hasRoleAuthority.Value);
        }
        catch (Exception)
        {
            return Result<bool>.Failure<bool>(DomainErrors.GeneralError.ServerError);
        }
    }

    public async Task<Result<bool>> CanUserAccessAreaAsync(string userId, string area)
    {
        try
        {
            var userResult = await GetAnyUserAsync(userId);
            if (userResult.IsFailure)
                return Result<bool>.Failure<bool>(userResult.Error);

            var user = userResult.Value;

            return user switch
            {
                SMSApplicationUser appUser => Result<bool>.Success(appUser.CanAccess(area)),
                SMSOrganizationalUser orgUser => Result<bool>.Success(CanOrganizationalUserAccessArea(orgUser, area)),
                SMSStakeholderUser stakeholderUser => Result<bool>.Success(CanStakeholderAccessArea(stakeholderUser, area)),
                _ => Result<bool>.Success(false)
            };
        }
        catch (Exception )
        {
            return Result<bool>.Failure<bool>(DomainErrors.GeneralError.ServerError);
        }
    }

    public async Task<Result<bool>> CanUserViewDataAsync(string userId, string dataType, string? organizationFilter = null)
    {
        try
        {
            var userResult = await GetAnyUserAsync(userId);
            if (userResult.IsFailure)
                return Result<bool>.Failure<bool>(userResult.Error);

            var user = userResult.Value;

            return user switch
            {
                SMSApplicationUser => Result<bool>.Success(true), // App users have broad access
                SMSOrganizationalUser orgUser => Result<bool>.Success(CanOrganizationalUserViewData(orgUser, dataType)),
                SMSStakeholderUser stakeholderUser => Result<bool>.Success(CanStakeholderViewData(stakeholderUser, dataType, organizationFilter)),
                _ => Result<bool>.Success(false)
            };
        }
        catch (Exception)
        {
            return Result<bool>.Failure<bool>(DomainErrors.GeneralError.ServerError);
        }
    }

    public async Task<Result<bool>> HasUserRoleAuthorityAsync(string userId, int requiredAuthorityLevel)
    {
        try
        {
            var maxAuthority = await _roleService.GetUserMaxAuthorityLevelAsync(userId);
            return Result<bool>.Success(maxAuthority >= requiredAuthorityLevel);
        }
        catch (Exception)
        {
            return Result<bool>.Failure<bool>(DomainErrors.GeneralError.ServerError);
        }
    }

    public async Task<Result<bool>> CanUserApproveRiskAsync(string userId, RiskLevel riskLevel)
    {
        try
        {
            // Check role-based authority
            var roleAuthority = await _roleService.UserCanApproveAsync(userId, riskLevel.RequiredAuthorityLevel);
            if (!roleAuthority)
                return Result<bool>.Success(false);

            // Check user-specific permissions for organizational users
            var userResult = await GetAnyUserAsync(userId);
            if (userResult.IsFailure)
                return Result<bool>.Success(roleAuthority); // Fall back to role authority

            if (userResult.Value is SMSOrganizationalUser orgUser)
            {
                return Result<bool>.Success(orgUser.Permissions.CanApproveRiskLevel(riskLevel));
            }

            return Result<bool>.Success(roleAuthority);
        }
        catch (Exception)
        {
            return Result<bool>.Failure<bool>(DomainErrors.GeneralError.ServerError);
        }
    }

    public async Task<Result<bool>> CanUserEscalateAsync(string userId, string escalationType)
    {
        try
        {
            var userResult = await GetAnyUserAsync(userId);
            if (userResult.IsFailure)
                return Result<bool>.Failure<bool>(userResult.Error);

            if (userResult.Value is SMSOrganizationalUser orgUser)
            {
                return Result<bool>.Success(orgUser.CanEscalate(escalationType));
            }

            // Non-organizational users generally cannot escalate
            return Result<bool>.Success(false);
        }
        catch (Exception)
        {
            return Result<bool>.Failure<bool>(DomainErrors.GeneralError.ServerError);
        }
    }

    public async Task<Result<bool>> CanUserParticipateInCommitteeAsync(string userId, CommitteeType committeeType)
    {
        try
        {
            var userResult = await GetAnyUserAsync(userId);
            if (userResult.IsFailure)
                return Result<bool>.Failure<bool>(userResult.Error);

            return userResult.Value switch
            {
                SMSApplicationUser => Result<bool>.Success(true), // App users can participate
                SMSOrganizationalUser orgUser => Result<bool>.Success(orgUser.CanPerformCommitteeAction("PARTICIPATE")),
                SMSStakeholderUser stakeholderUser => Result<bool>.Success(stakeholderUser.CanParticipate("COMMITTEES")),
                _ => Result<bool>.Success(false)
            };
        }
        catch (Exception)
        {
            return Result<bool>.Failure<bool>(DomainErrors.GeneralError.ServerError);
        }
    }

    public async Task<Result<bool>> CanUserChairCommitteeAsync(string userId, CommitteeType committeeType)
    {
        try
        {
            var userResult = await GetAnyUserAsync(userId);
            if (userResult.IsFailure)
                return Result<bool>.Failure<bool>(userResult.Error);

            // Only organizational users can chair committees
            if (userResult.Value is SMSOrganizationalUser orgUser)
            {
                // Check both permission and sufficient authority level for committee type
                var hasPermission = orgUser.CanPerformCommitteeAction("CHAIR");
                var hasAuthority = await HasUserRoleAuthorityAsync(userId, committeeType.AuthorityLevel);
                
                return Result<bool>.Success(hasPermission && hasAuthority.IsSuccess && hasAuthority.Value);
            }

            return Result<bool>.Success(false);
        }
        catch (Exception)
        {
            return Result<bool>.Failure<bool>(DomainErrors.GeneralError.ServerError);
        }
    }

    public async Task<Result<bool>> CanUserCreateCommitteeAsync(string userId, CommitteeType committeeType)
    {
        try
        {
            var userResult = await GetAnyUserAsync(userId);
            if (userResult.IsFailure)
                return Result<bool>.Failure<bool>(userResult.Error);

            return userResult.Value switch
            {
                SMSApplicationUser appUser => Result<bool>.Success(appUser.HasPermission("ACCESS_COMMITTEES")),
                SMSOrganizationalUser orgUser => Result<bool>.Success(orgUser.CanPerformCommitteeAction("CREATE")),
                _ => Result<bool>.Success(false)
            };
        }
        catch (Exception)
        {
            return Result<bool>.Failure<bool>(DomainErrors.GeneralError.ServerError);
        }
    }

    public async Task<Result<bool>> CanUserInitiateInvestigationAsync(string userId, string investigationType)
    {
        try
        {
            var userResult = await GetAnyUserAsync(userId);
            if (userResult.IsFailure)
                return Result<bool>.Failure<bool>(userResult.Error);

            return userResult.Value switch
            {
                SMSApplicationUser => Result<bool>.Success(true),
                SMSOrganizationalUser orgUser => Result<bool>.Success(orgUser.CanPerformInvestigationAction("INITIATE")),
                _ => Result<bool>.Success(false)
            };
        }
        catch (Exception)
        {
            return Result<bool>.Failure<bool>(DomainErrors.GeneralError.ServerError);
        }
    }

    public async Task<Result<bool>> CanUserLeadInvestigationAsync(string userId, string investigationType)
    {
        try
        {
            var userResult = await GetAnyUserAsync(userId);
            if (userResult.IsFailure)
                return Result<bool>.Failure<bool>(userResult.Error);

            return userResult.Value switch
            {
                SMSApplicationUser => Result<bool>.Success(true),
                SMSOrganizationalUser orgUser => Result<bool>.Success(orgUser.CanPerformInvestigationAction("LEAD")),
                _ => Result<bool>.Success(false)
            };
        }
        catch (Exception)
        {
            return Result<bool>.Failure<bool>(DomainErrors.GeneralError.ServerError);
        }
    }

    public async Task<Result<bool>> CanUserCloseInvestigationAsync(string userId, string investigationType)
    {
        try
        {
            var userResult = await GetAnyUserAsync(userId);
            if (userResult.IsFailure)
                return Result<bool>.Failure<bool>(userResult.Error);

            return userResult.Value switch
            {
                SMSApplicationUser => Result<bool>.Success(true),
                SMSOrganizationalUser orgUser => Result<bool>.Success(orgUser.CanPerformInvestigationAction("CLOSE")),
                _ => Result<bool>.Success(false)
            };
        }
        catch (Exception)
        {
            return Result<bool>.Failure<bool>(DomainErrors.GeneralError.ServerError);
        }
    }

    public async Task<Result<bool>> CanUserAccessDepartmentDataAsync(string userId, string department)
    {
        try
        {
            var userResult = await GetAnyUserAsync(userId);
            if (userResult.IsFailure)
                return Result<bool>.Failure<bool>(userResult.Error);

            return userResult.Value switch
            {
                SMSApplicationUser => Result<bool>.Success(true), // App users have broad access
                SMSOrganizationalUser orgUser => Result<bool>.Success(
                    orgUser.IsInDepartment(department) || orgUser.CanViewReports("CROSS_DEPARTMENT")),
                SMSStakeholderUser => Result<bool>.Success(false), // Stakeholders don't have department access
                _ => Result<bool>.Success(false)
            };
        }
        catch (Exception)
        {
            return Result<bool>.Failure<bool>(DomainErrors.GeneralError.ServerError);
        }
    }

    public async Task<Result<bool>> CanUserPerformCrossDepartmentActionAsync(string userId, string action)
    {
        try
        {
            var userResult = await GetAnyUserAsync(userId);
            if (userResult.IsFailure)
                return Result<bool>.Failure<bool>(userResult.Error);

            return userResult.Value switch
            {
                SMSApplicationUser => Result<bool>.Success(true),
                SMSOrganizationalUser orgUser => Result<bool>.Success(orgUser.CanEscalate("ACROSS_DEPARTMENTS")),
                _ => Result<bool>.Success(false)
            };
        }
        catch (Exception)
        {
            return Result<bool>.Failure<bool>(DomainErrors.GeneralError.ServerError);
        }
    }

    public async Task<Result<bool>> CanUserExecuteWorkflowAsync(string userId, string workflowType, string stage)
    {
        // Implementation would check workflow-specific permissions
        return await CanUserPerformActionAsync(userId, $"{workflowType}_{stage}");
    }

    public async Task<Result<string>> GetRequiredApproverForRiskAsync(RiskLevel riskLevel, string department)
    {
        try
        {
            // Get users with sufficient authority for this risk level
            var eligibleApprovers = await _roleService.GetUsersWhoCanApproveAsync(riskLevel.RequiredAuthorityLevel);
            
            // Prefer approvers from the same department
            var departmentApprover = eligibleApprovers.FirstOrDefault(ua => 
                string.Equals(ua.Department, department, StringComparison.OrdinalIgnoreCase));
            
            if (departmentApprover != null)
                return Result<string>.Success(departmentApprover.UserID);

            // Fall back to any eligible approver
            var anyApprover = eligibleApprovers.FirstOrDefault();
            if (anyApprover != null)
                return Result<string>.Success(anyApprover.UserID);

            return Result<string>.Failure<string>(DomainErrors.WorkflowError.NoApproverFound);
        }
        catch (Exception)
        {
            return Result<string>.Failure<string>(DomainErrors.GeneralError.ServerError);
        }
    }

    public async Task<Result<IEnumerable<string>>> GetEligibleApproversAsync(DecisionAuthority requiredAuthority, string? department = null)
    {
        try
        {
            var eligibleApprovers = await _roleService.GetUsersWhoCanApproveAsync(requiredAuthority.RequiredAuthorityLevel);
            
            var approverIds = eligibleApprovers.Select(ua => ua.UserID);
            
            if (!string.IsNullOrWhiteSpace(department))
            {
                approverIds = eligibleApprovers
                    .Where(ua => string.Equals(ua.Department, department, StringComparison.OrdinalIgnoreCase))
                    .Select(ua => ua.UserID);
            }

            return Result<IEnumerable<string>>.Success(approverIds);
        }
        catch (Exception)
        {
            return Result<IEnumerable<string>>.Failure<IEnumerable<string>>(DomainErrors.GeneralError.ServerError);
        }
    }

    public async Task<Result<bool>> CanStakeholderAccessOperationalDataAsync(string userId, string dataScope)
    {
        try
        {
            var userResult = await _stakeholderUserRepository.GetByIdAsync(userId);
            if (userResult.IsFailure)
                return Result<bool>.Success(false);

            return Result<bool>.Success(userResult.Value.CanAccessData(dataScope));
        }
        catch (Exception)
        {
            return Result<bool>.Failure<bool>(DomainErrors.GeneralError.ServerError);
        }
    }

    public async Task<Result<bool>> CanStakeholderSubmitReportsAsync(string userId, string reportType)
    {
        try
        {
            var userResult = await _stakeholderUserRepository.GetByIdAsync(userId);
            if (userResult.IsFailure)
                return Result<bool>.Success(false);

            return Result<bool>.Success(userResult.Value.CanParticipate("SUBMIT_HAZARD_REPORTS"));
        }
        catch (Exception)
        {
            return Result<bool>.Failure<bool>(DomainErrors.GeneralError.ServerError);
        }
    }

    public async Task<Result<bool>> CanUserManageUsersAsync(string userId, string targetUserType)
    {
        try
        {
            var userResult = await GetAnyUserAsync(userId);
            if (userResult.IsFailure)
                return Result<bool>.Failure<bool>(userResult.Error);

            // Only application users can manage users
            if (userResult.Value is SMSApplicationUser appUser)
            {
                return Result<bool>.Success(appUser.HasPermission("MANAGE_USERS"));
            }

            return Result<bool>.Success(false);
        }
        catch (Exception)
        {
            return Result<bool>.Failure<bool>(DomainErrors.GeneralError.ServerError);
        }
    }

    public async Task<Result<bool>> CanUserViewAuditLogsAsync(string userId, string logType)
    {
        try
        {
            var userResult = await GetAnyUserAsync(userId);
            if (userResult.IsFailure)
                return Result<bool>.Failure<bool>(userResult.Error);

            if (userResult.Value is SMSApplicationUser appUser)
            {
                return Result<bool>.Success(appUser.HasPermission("VIEW_AUDIT_LOGS"));
            }

            return Result<bool>.Success(false);
        }
        catch (Exception)
        {
            return Result<bool>.Failure<bool>(DomainErrors.GeneralError.ServerError);
        }
    }

    public async Task<Result<bool>> CanUserConfigureSystemAsync(string userId, string configurationArea)
    {
        try
        {
            var userResult = await GetAnyUserAsync(userId);
            if (userResult.IsFailure)
                return Result<bool>.Failure<bool>(userResult.Error);

            if (userResult.Value is SMSApplicationUser appUser)
            {
                return Result<bool>.Success(appUser.HasPermission("CONFIGURE_SYSTEM"));
            }

            return Result<bool>.Success(false);
        }
        catch (Exception)
        {
            return Result<bool>.Failure<bool>(DomainErrors.GeneralError.ServerError);
        }
    }

    public async Task<Result<UserAuthorizationSummary>> GetUserAuthorizationSummaryAsync(string userId)
    {
        try
        {
            var userResult = await GetAnyUserAsync(userId);
            if (userResult.IsFailure)
                return Result<UserAuthorizationSummary>.Failure<UserAuthorizationSummary>(userResult.Error);

            var user = userResult.Value;
            var userRoles = await _roleService.GetActiveUserRolesAsync(userId);
            var maxAuthority = await _roleService.GetUserMaxAuthorityLevelAsync(userId);

            var summary = new UserAuthorizationSummary
            {
                UserId = userId,
                UserType = user.GetUserType(),
                DisplayName = user.DisplayName,
                Department = user.GetDepartmentInfo(),
                MaxAuthorityLevel = maxAuthority,
                AssignedRoles = userRoles.Select(r => r.RoleValue).ToArray(),
                LastUpdated = DateTime.UtcNow
            };

            // Add user-specific details
            switch (user)
            {
                case SMSApplicationUser appUser:
                    summary.Permissions = GetApplicationUserPermissions(appUser);
                    summary.AccessibleAreas = GetApplicationUserAreas(appUser);
                    break;
                case SMSOrganizationalUser orgUser:
                    summary.Permissions = GetOrganizationalUserPermissions(orgUser);
                    summary.RiskApprovalLevels = GetRiskApprovalLevels(orgUser);
                    summary.CanEscalate = orgUser.CanEscalate("WITHIN_DEPARTMENT");
                    summary.CanLeadInvestigations = orgUser.CanPerformInvestigationAction("LEAD");
                    break;
                case SMSStakeholderUser stakeholderUser:
                    summary.Permissions = GetStakeholderUserPermissions(stakeholderUser);
                    summary.AccessibleAreas = GetStakeholderUserAreas(stakeholderUser);
                    break;
            }

            return Result<UserAuthorizationSummary>.Success(summary);
        }
        catch (Exception)
        {
            return Result<UserAuthorizationSummary>.Failure<UserAuthorizationSummary>(DomainErrors.GeneralError.ServerError);
        }
    }

    public async Task<Result<IEnumerable<string>>> GetUserPermissionsAsync(string userId)
    {
        var summaryResult = await GetUserAuthorizationSummaryAsync(userId);
        if (summaryResult.IsFailure)
            return Result<IEnumerable<string>>.Failure<IEnumerable<string>>(summaryResult.Error);

        return Result<IEnumerable<string>>.Success(summaryResult.Value.Permissions.AsEnumerable());
    }

    public async Task<Result<IEnumerable<string>>> GetUserAccessibleAreasAsync(string userId)
    {
        var summaryResult = await GetUserAuthorizationSummaryAsync(userId);
        if (summaryResult.IsFailure)
            return Result<IEnumerable<string>>.Failure<IEnumerable<string>>(summaryResult.Error);

        return Result<IEnumerable<string>>.Success(summaryResult.Value.AccessibleAreas.AsEnumerable());
    }

    // Private helper methods
    private async Task<Result<BaseUser>> GetAnyUserAsync(string userId)
    {
        // Try application user first
        var appUserResult = await _applicationUserRepository.GetByIdAsync(userId);
        if (appUserResult.IsSuccess)
            return Result<BaseUser>.Success((BaseUser)appUserResult.Value);

        // Try organizational user
        var orgUserResult = await _organizationalUserRepository.GetByIdAsync(userId);
        if (orgUserResult.IsSuccess)
            return Result<BaseUser>.Success((BaseUser)orgUserResult.Value);

        // Try stakeholder user
        var stakeholderUserResult = await _stakeholderUserRepository.GetByIdAsync(userId);
        if (stakeholderUserResult.IsSuccess)
            return Result<BaseUser>.Success((BaseUser)stakeholderUserResult.Value);

        return Result<BaseUser>.Failure<BaseUser>(DomainErrors.BaseUserError.UserNotFound);
    }

    private bool CheckUserPermission(BaseUser user, string action)
    {
        return user switch
        {
            SMSApplicationUser appUser => appUser.HasPermission(action) || appUser.CanPerform(action),
            SMSOrganizationalUser orgUser => orgUser.HasPermission(action),
            SMSStakeholderUser stakeholderUser => stakeholderUser.HasPermission(action),
            _ => false
        };
    }

    private async Task<Result<bool>> CheckRoleAuthorityAsync(string userId, string action, string? context)
    {
        // Check if action requires specific role authority
        var requiredAuthority = GetRequiredAuthorityForAction(action, context);
        if (requiredAuthority > 0)
        {
            return await HasUserRoleAuthorityAsync(userId, requiredAuthority);
        }

        return Result<bool>.Success(true);
    }

    private int GetRequiredAuthorityForAction(string action, string? context)
    {
        // Define authority requirements for specific actions
        return action.ToUpperInvariant() switch
        {
            "APPROVE_CRITICAL_RISK" => 10,
            "APPROVE_HIGH_RISK" => 9,
            "APPROVE_MEDIUM_RISK" => 8,
            "APPROVE_LOW_RISK" => 7,
            "CREATE_COMMITTEE" => 8,
            "CHAIR_COMMITTEE" => 7,
            "CLOSE_INVESTIGATION" => 7,
            _ => 0 // No specific authority required
        };
    }

    private bool CanOrganizationalUserAccessArea(SMSOrganizationalUser user, string area)
    {
        return area.ToUpperInvariant() switch
        {
            "COMMITTEES" => user.CanPerformCommitteeAction("PARTICIPATE"),
            "INVESTIGATIONS" => user.CanPerformInvestigationAction("INITIATE"),
            "REPORTS" => user.CanViewReports("DEPARTMENT"),
            "WORKFLOW" => true,
            _ => false
        };
    }

    private bool CanStakeholderAccessArea(SMSStakeholderUser user, string area)
    {
        return area.ToUpperInvariant() switch
        {
            "REPORTS" => user.CanAccessData("PUBLIC_REPORTS"),
            "COMMITTEES" => user.CanParticipate("COMMITTEES"),
            "OPERATIONAL_DATA" => user.CanAccessData("AIRSIDE_DATA"),
            _ => false
        };
    }

    private bool CanOrganizationalUserViewData(SMSOrganizationalUser user, string dataType)
    {
        return dataType.ToUpperInvariant() switch
        {
            "DEPARTMENT_REPORTS" => user.CanViewReports("DEPARTMENT"),
            "CROSS_DEPARTMENT_REPORTS" => user.CanViewReports("CROSS_DEPARTMENT"),
            "EXECUTIVE_REPORTS" => user.CanViewReports("EXECUTIVE"),
            _ => false
        };
    }

    private bool CanStakeholderViewData(SMSStakeholderUser user, string dataType, string? organizationFilter)
    {
        var canAccess = user.CanAccessData(dataType);
        
        // Apply organization filter for organization-specific data
        if (canAccess && !string.IsNullOrWhiteSpace(organizationFilter) && dataType.Contains("ORGANIZATION"))
        {
            return user.IsFromOrganization(organizationFilter);
        }

        return canAccess;
    }

    private string[] GetApplicationUserPermissions(SMSApplicationUser user)
    {
        var permissions = new List<string>();
        if (user.Permissions.CanAccessDashboard) permissions.Add("Access Dashboard");
        if (user.Permissions.CanAccessReports) permissions.Add("Access Reports");
        if (user.Permissions.CanCreateReports) permissions.Add("Create Reports");
        if (user.Permissions.CanManageUsers) permissions.Add("Manage Users");
        if (user.Permissions.CanConfigureSystem) permissions.Add("Configure System");
        return permissions.ToArray();
    }

    private string[] GetApplicationUserAreas(SMSApplicationUser user)
    {
        var areas = new List<string>();
        if (user.Permissions.CanAccessDashboard) areas.Add("Dashboard");
        if (user.Permissions.CanAccessReports) areas.Add("Reports");
        if (user.Permissions.CanAccessAnalytics) areas.Add("Analytics");
        if (user.Permissions.CanAccessCommittees) areas.Add("Committees");
        if (user.Permissions.CanAccessUserManagement) areas.Add("User Management");
        return areas.ToArray();
    }

    private string[] GetOrganizationalUserPermissions(SMSOrganizationalUser user)
    {
        var permissions = new List<string>();
        if (user.Permissions.CanApproveLowRisk) permissions.Add("Approve Low Risk");
        if (user.Permissions.CanApproveMediumRisk) permissions.Add("Approve Medium Risk");
        if (user.Permissions.CanApproveHighRisk) permissions.Add("Approve High Risk");
        if (user.Permissions.CanApproveCriticalRisk) permissions.Add("Approve Critical Risk");
        if (user.Permissions.CanChairCommittees) permissions.Add("Chair Committees");
        if (user.Permissions.CanLeadInvestigations) permissions.Add("Lead Investigations");
        return permissions.ToArray();
    }

    private string[] GetRiskApprovalLevels(SMSOrganizationalUser user)
    {
        var levels = new List<string>();
        if (user.Permissions.CanApproveLowRisk) levels.Add("Low");
        if (user.Permissions.CanApproveMediumRisk) levels.Add("Medium");
        if (user.Permissions.CanApproveHighRisk) levels.Add("High");
        if (user.Permissions.CanApproveCriticalRisk) levels.Add("Critical");
        return levels.ToArray();
    }

    private string[] GetStakeholderUserPermissions(SMSStakeholderUser user)
    {
        var permissions = new List<string>();
        if (user.Permissions.CanViewPublicReports) permissions.Add("View Public Reports");
        if (user.Permissions.CanSubmitHazardReports) permissions.Add("Submit Hazard Reports");
        if (user.Permissions.CanParticipateInCommittees) permissions.Add("Participate in Committees");
        if (user.Permissions.CanViewAirsideOperationalData) permissions.Add("View Airside Data");
        return permissions.ToArray();
    }

    private string[] GetStakeholderUserAreas(SMSStakeholderUser user)
    {
        var areas = new List<string>();
        if (user.Permissions.CanViewPublicReports) areas.Add("Public Reports");
        if (user.Permissions.CanViewAirsideOperationalData) areas.Add("Airside Operations");
        if (user.Permissions.CanAccessAOAInformation) areas.Add("AOA Information");
        if (user.Permissions.CanParticipateInCommittees) areas.Add("Committees");
        return areas.ToArray();
    }
}