using SMS_Domain.Common;
using SMS_Domain.ValueObjects;

namespace SMS_Domain.Entities;

/// <summary>
/// Represents an SMS application user entity for system administrators and application-level users
/// </summary>
public sealed class SMSApplicationUser : BaseUser
{
    public SMSApplicationUserID ApplicationUserId { get; private set; }
    public string ApplicationRole { get; private set; }
    public string PermissionLevel { get; private set; }
    public ApplicationPermissions Permissions { get; private set; }

    // For Entity Framework
    private SMSApplicationUser() : base()
    {
        ApplicationUserId = new SMSApplicationUserID(Guid.NewGuid().ToString());
        ApplicationRole = string.Empty;
        PermissionLevel = string.Empty;
        Permissions = ApplicationPermissions.ReadOnly;
    }

    private SMSApplicationUser(
        string code,
        FirstName firstName,
        LastName lastName,
        UserName userName,
        Password password,
        string applicationRole,
        string permissionLevel,
        ApplicationPermissions permissions,
        string createdBy) : base(code, firstName, lastName, userName, password, createdBy)
    {
        ApplicationUserId = new SMSApplicationUserID(UserId.Value);
        ApplicationRole = applicationRole;
        PermissionLevel = permissionLevel;
        Permissions = permissions;
    }

    public static SMSApplicationUser Create(
        string code,
        FirstName firstName,
        LastName lastName,
        UserName userName,
        Password password,
        string applicationRole,
        string permissionLevel,
        string createdBy)
    {
        // Create permissions based on permission level
        var permissionsResult = ApplicationPermissions.Create(permissionLevel);
        var permissions = permissionsResult.IsSuccess ? permissionsResult.Value : ApplicationPermissions.ReadOnly;

        return new SMSApplicationUser(
            code,
            firstName,
            lastName,
            userName,
            password,
            applicationRole,
            permissionLevel,
            permissions,
            createdBy);
    }

    public static SMSApplicationUser CreateWithCustomPermissions(
        string code,
        FirstName firstName,
        LastName lastName,
        UserName userName,
        Password password,
        string applicationRole,
        ApplicationPermissions permissions,
        string createdBy)
    {
        return new SMSApplicationUser(
            code,
            firstName,
            lastName,
            userName,
            password,
            applicationRole,
            permissions.GetPermissionLevel(),
            permissions,
            createdBy);
    }

    /// <summary>
    /// Updates the application-specific properties
    /// </summary>
    public void UpdateApplicationInfo(string applicationRole, string permissionLevel)
    {
        ApplicationRole = applicationRole;
        PermissionLevel = permissionLevel;
        
        // Update permissions based on new permission level
        var permissionsResult = ApplicationPermissions.Create(permissionLevel);
        if (permissionsResult.IsSuccess)
        {
            Permissions = permissionsResult.Value;
        }
    }

    /// <summary>
    /// Updates permissions directly
    /// </summary>
    public void UpdatePermissions(ApplicationPermissions permissions)
    {
        Permissions = permissions;
        PermissionLevel = permissions.GetPermissionLevel();
    }

    /// <summary>
    /// Checks if the user has a specific application role
    /// </summary>
    public bool HasApplicationRole(string role)
    {
        return string.Equals(ApplicationRole, role, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if the user has sufficient permission level
    /// </summary>
    public bool HasPermissionLevel(string requiredLevel)
    {
        // Define permission hierarchy (could be moved to an enum or constants)
        var levels = new[] { "Read", "Write", "Admin", "SuperAdmin" };
        
        var userLevelIndex = Array.IndexOf(levels, PermissionLevel);
        var requiredLevelIndex = Array.IndexOf(levels, requiredLevel);
        
        return userLevelIndex >= requiredLevelIndex;
    }

    /// <summary>
    /// Checks if the user has a specific permission
    /// </summary>
    public bool HasPermission(string permission)
    {
        return Permissions.HasPermission(permission);
    }

    /// <summary>
    /// Checks if the user can access a specific area
    /// </summary>
    public bool CanAccess(string area)
    {
        return area.ToUpperInvariant() switch
        {
            "DASHBOARD" => Permissions.CanAccessDashboard,
            "REPORTS" => Permissions.CanAccessReports,
            "ANALYTICS" => Permissions.CanAccessAnalytics,
            "COMMITTEES" => Permissions.CanAccessCommittees,
            "USER_MANAGEMENT" => Permissions.CanAccessUserManagement,
            "SYSTEM_SETTINGS" => Permissions.CanAccessSystemSettings,
            _ => false
        };
    }

    /// <summary>
    /// Checks if the user can perform a specific action
    /// </summary>
    public bool CanPerform(string action)
    {
        return action.ToUpperInvariant() switch
        {
            "CREATE_REPORTS" => Permissions.CanCreateReports,
            "EDIT_REPORTS" => Permissions.CanEditReports,
            "DELETE_REPORTS" => Permissions.CanDeleteReports,
            "EXPORT_DATA" => Permissions.CanExportData,
            "IMPORT_DATA" => Permissions.CanImportData,
            "MANAGE_USERS" => Permissions.CanManageUsers,
            "MANAGE_ROLES" => Permissions.CanManageRoles,
            "CONFIGURE_SYSTEM" => Permissions.CanConfigureSystem,
            "VIEW_AUDIT_LOGS" => Permissions.CanViewAuditLogs,
            "MANAGE_BACKUPS" => Permissions.CanManageBackups,
            "ACCESS_DEVELOPER_TOOLS" => Permissions.CanAccessDeveloperTools,
            _ => false
        };
    }

    public override string GetUserType() => "ApplicationUser";

    public override string GetDepartmentInfo() => $"Application Role: {ApplicationRole}, Permission Level: {PermissionLevel}";

    /// <summary>
    /// Gets application-specific user information for display
    /// </summary>
    public string GetApplicationSummary()
    {
        return $"{DisplayName} ({ApplicationRole} - {PermissionLevel})";
    }

    /// <summary>
    /// Gets detailed permission summary for administrative purposes
    /// </summary>
    public string GetPermissionSummary()
    {
        var accessAreas = new List<string>();
        if (Permissions.CanAccessDashboard) accessAreas.Add("Dashboard");
        if (Permissions.CanAccessReports) accessAreas.Add("Reports");
        if (Permissions.CanAccessAnalytics) accessAreas.Add("Analytics");
        if (Permissions.CanAccessCommittees) accessAreas.Add("Committees");
        if (Permissions.CanAccessUserManagement) accessAreas.Add("User Management");
        if (Permissions.CanAccessSystemSettings) accessAreas.Add("System Settings");

        return $"Access: [{string.Join(", ", accessAreas)}]";
    }
}