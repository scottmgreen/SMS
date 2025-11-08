using SMS_Domain.Common;
using SMS_Shared.Common;
using SMS_Domain.Errors;

namespace SMS_Domain.ValueObjects;

/// <summary>
/// Application permissions for SMS Application Users
/// Controls UI navigation, feature access, and system administration
/// </summary>
public sealed class ApplicationPermissions : BaseValueObject
{
    // UI Navigation Permissions
    public bool CanAccessDashboard { get; }
    public bool CanAccessReports { get; }
    public bool CanAccessAnalytics { get; }
    public bool CanAccessCommittees { get; }
    public bool CanAccessUserManagement { get; }
    public bool CanAccessSystemSettings { get; }
    
    // Feature Permissions
    public bool CanCreateReports { get; }
    public bool CanEditReports { get; }
    public bool CanDeleteReports { get; }
    public bool CanExportData { get; }
    public bool CanImportData { get; }
    public bool CanManageUsers { get; }
    public bool CanManageRoles { get; }
    public bool CanConfigureSystem { get; }
    
    // Administrative Permissions
    public bool CanViewAuditLogs { get; }
    public bool CanManageBackups { get; }
    public bool CanAccessDeveloperTools { get; }

    private ApplicationPermissions(
        bool canAccessDashboard, bool canAccessReports, bool canAccessAnalytics,
        bool canAccessCommittees, bool canAccessUserManagement, bool canAccessSystemSettings,
        bool canCreateReports, bool canEditReports, bool canDeleteReports,
        bool canExportData, bool canImportData, bool canManageUsers, bool canManageRoles,
        bool canConfigureSystem, bool canViewAuditLogs, bool canManageBackups, bool canAccessDeveloperTools)
    {
        CanAccessDashboard = canAccessDashboard;
        CanAccessReports = canAccessReports;
        CanAccessAnalytics = canAccessAnalytics;
        CanAccessCommittees = canAccessCommittees;
        CanAccessUserManagement = canAccessUserManagement;
        CanAccessSystemSettings = canAccessSystemSettings;
        CanCreateReports = canCreateReports;
        CanEditReports = canEditReports;
        CanDeleteReports = canDeleteReports;
        CanExportData = canExportData;
        CanImportData = canImportData;
        CanManageUsers = canManageUsers;
        CanManageRoles = canManageRoles;
        CanConfigureSystem = canConfigureSystem;
        CanViewAuditLogs = canViewAuditLogs;
        CanManageBackups = canManageBackups;
        CanAccessDeveloperTools = canAccessDeveloperTools;
    }

    /// <summary>
    /// Creates application permissions based on permission level
    /// </summary>
    public static Result<ApplicationPermissions> Create(string permissionLevel) =>
        Result.Create(permissionLevel, DomainErrors.SMSApplicationUserError.PermissionLevelRequired)
            .Ensure(p => !string.IsNullOrWhiteSpace(p), DomainErrors.SMSApplicationUserError.PermissionLevelRequired)
            .Ensure(p => IsValidPermissionLevel(p), DomainErrors.SMSApplicationUserError.InvalidPermissionLevel)
            .Map(p => CreateFromLevel(p));

    /// <summary>
    /// Creates application permissions from individual permission flags
    /// </summary>
    public static ApplicationPermissions CreateCustom(
        bool canAccessDashboard = true, bool canAccessReports = true, bool canAccessAnalytics = false,
        bool canAccessCommittees = false, bool canAccessUserManagement = false, bool canAccessSystemSettings = false,
        bool canCreateReports = false, bool canEditReports = false, bool canDeleteReports = false,
        bool canExportData = false, bool canImportData = false, bool canManageUsers = false, bool canManageRoles = false,
        bool canConfigureSystem = false, bool canViewAuditLogs = false, bool canManageBackups = false, bool canAccessDeveloperTools = false)
    {
        return new ApplicationPermissions(
            canAccessDashboard, canAccessReports, canAccessAnalytics,
            canAccessCommittees, canAccessUserManagement, canAccessSystemSettings,
            canCreateReports, canEditReports, canDeleteReports,
            canExportData, canImportData, canManageUsers, canManageRoles,
            canConfigureSystem, canViewAuditLogs, canManageBackups, canAccessDeveloperTools);
    }

    private static bool IsValidPermissionLevel(string level)
    {
        var validLevels = new[] { "Read", "Write", "Admin", "SuperAdmin" };
        return validLevels.Contains(level, StringComparer.OrdinalIgnoreCase);
    }

    private static ApplicationPermissions CreateFromLevel(string level)
    {
        return level.ToUpperInvariant() switch
        {
            "READ" => ReadOnly,
            "WRITE" => StandardUser,
            "ADMIN" => Administrator,
            "SUPERADMIN" => SuperAdmin,
            _ => ReadOnly
        };
    }

    // Predefined permission sets
    public static ApplicationPermissions ReadOnly => new(
        canAccessDashboard: true, canAccessReports: true, canAccessAnalytics: true,
        canAccessCommittees: true, canAccessUserManagement: false, canAccessSystemSettings: false,
        canCreateReports: false, canEditReports: false, canDeleteReports: false,
        canExportData: true, canImportData: false, canManageUsers: false, canManageRoles: false,
        canConfigureSystem: false, canViewAuditLogs: false, canManageBackups: false, canAccessDeveloperTools: false);

    public static ApplicationPermissions StandardUser => new(
        canAccessDashboard: true, canAccessReports: true, canAccessAnalytics: true,
        canAccessCommittees: true, canAccessUserManagement: false, canAccessSystemSettings: false,
        canCreateReports: true, canEditReports: true, canDeleteReports: false,
        canExportData: true, canImportData: false, canManageUsers: false, canManageRoles: false,
        canConfigureSystem: false, canViewAuditLogs: false, canManageBackups: false, canAccessDeveloperTools: false);

    public static ApplicationPermissions Administrator => new(
        canAccessDashboard: true, canAccessReports: true, canAccessAnalytics: true,
        canAccessCommittees: true, canAccessUserManagement: true, canAccessSystemSettings: true,
        canCreateReports: true, canEditReports: true, canDeleteReports: true,
        canExportData: true, canImportData: true, canManageUsers: true, canManageRoles: true,
        canConfigureSystem: true, canViewAuditLogs: true, canManageBackups: true, canAccessDeveloperTools: false);

    public static ApplicationPermissions SuperAdmin => new(
        canAccessDashboard: true, canAccessReports: true, canAccessAnalytics: true,
        canAccessCommittees: true, canAccessUserManagement: true, canAccessSystemSettings: true,
        canCreateReports: true, canEditReports: true, canDeleteReports: true,
        canExportData: true, canImportData: true, canManageUsers: true, canManageRoles: true,
        canConfigureSystem: true, canViewAuditLogs: true, canManageBackups: true, canAccessDeveloperTools: true);

    /// <summary>
    /// Checks if a specific permission is granted
    /// </summary>
    public bool HasPermission(string permission)
    {
        return permission.ToUpperInvariant() switch
        {
            "ACCESS_DASHBOARD" => CanAccessDashboard,
            "ACCESS_REPORTS" => CanAccessReports,
            "ACCESS_ANALYTICS" => CanAccessAnalytics,
            "ACCESS_COMMITTEES" => CanAccessCommittees,
            "ACCESS_USER_MANAGEMENT" => CanAccessUserManagement,
            "ACCESS_SYSTEM_SETTINGS" => CanAccessSystemSettings,
            "CREATE_REPORTS" => CanCreateReports,
            "EDIT_REPORTS" => CanEditReports,
            "DELETE_REPORTS" => CanDeleteReports,
            "EXPORT_DATA" => CanExportData,
            "IMPORT_DATA" => CanImportData,
            "MANAGE_USERS" => CanManageUsers,
            "MANAGE_ROLES" => CanManageRoles,
            "CONFIGURE_SYSTEM" => CanConfigureSystem,
            "VIEW_AUDIT_LOGS" => CanViewAuditLogs,
            "MANAGE_BACKUPS" => CanManageBackups,
            "ACCESS_DEVELOPER_TOOLS" => CanAccessDeveloperTools,
            _ => false
        };
    }

    /// <summary>
    /// Gets the permission level string representation
    /// </summary>
    public string GetPermissionLevel()
    {
        if (CanAccessDeveloperTools) return "SuperAdmin";
        if (CanManageUsers && CanConfigureSystem) return "Admin";
        if (CanCreateReports && CanEditReports) return "Write";
        return "Read";
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return CanAccessDashboard;
        yield return CanAccessReports;
        yield return CanAccessAnalytics;
        yield return CanAccessCommittees;
        yield return CanAccessUserManagement;
        yield return CanAccessSystemSettings;
        yield return CanCreateReports;
        yield return CanEditReports;
        yield return CanDeleteReports;
        yield return CanExportData;
        yield return CanImportData;
        yield return CanManageUsers;
        yield return CanManageRoles;
        yield return CanConfigureSystem;
        yield return CanViewAuditLogs;
        yield return CanManageBackups;
        yield return CanAccessDeveloperTools;
    }

    public override string ToString() => $"ApplicationPermissions({GetPermissionLevel()})";
}