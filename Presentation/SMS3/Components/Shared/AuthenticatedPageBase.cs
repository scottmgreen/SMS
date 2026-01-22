namespace SMS3.Components.Shared;

/// <summary>
/// Base component that provides AuthenticationService to all pages
/// Eliminates the need to add [CascadingParameter] to every page
/// Provides comprehensive role-based access control and audit functionality
/// </summary>
public abstract class AuthenticatedPageBase : ComponentBase
{
    [CascadingParameter(Name = "AuthService")]
    public AuthenticationService AuthService { get; set; } = default!;

    #region Authentication State Access

    /// <summary>
    /// Get the complete authentication state with user and permission data
    /// </summary>
    protected AuthenticationState AuthState => AuthService?.User ?? new AuthenticationState();

    /// <summary>
    /// Get the current authenticated user ID for audit fields
    /// Returns actual user ID instead of "SYSTEM" hardcoding
    /// </summary>
    protected string GetCurrentUserId()
    {
        return AuthService?.CurrentUserId ?? "SYSTEM";
    }

    /// <summary>
    /// Get the current authenticated user display name
    /// </summary>
    protected string GetCurrentUserDisplayName()
    {
        return AuthService?.CurrentUserDisplayName ?? "System User";
    }

    /// <summary>
    /// Check if user is authenticated
    /// </summary>
    protected bool IsUserAuthenticated()
    {
        return AuthService?.IsAuthenticated ?? false;
    }

    /// <summary>
    /// Get the current user entity for advanced operations
    /// </summary>
    protected SMS_Domain.Entities.BaseUser? GetCurrentUser()
    {
        return AuthState.CurrentUser;
    }

    /// <summary>
    /// Get the current user's role information
    /// </summary>
    protected SMS_Domain.Entities.SMSUserRole? GetUserRole()
    {
        return AuthState.UserRole;
    }

    #endregion

    #region Permission-Based Access Control

    /// <summary>
    /// Check if current user can CREATE in the specified SMS module
    /// Usage: if (CanCreate("SMS_RiskManagement")) { ... }
    /// </summary>
    protected bool CanCreate(string module)
    {
        return AuthState.CanCreate(module);
    }

    /// <summary>
    /// Check if current user can READ in the specified SMS module
    /// Usage: if (CanRead("SMS_RiskManagement")) { ... }
    /// </summary>
    protected bool CanRead(string module)
    {
        return AuthState.CanRead(module);
    }

    /// <summary>
    /// Check if current user can UPDATE in the specified SMS module
    /// Usage: if (CanUpdate("SMS_RiskManagement")) { ... }
    /// </summary>
    protected bool CanUpdate(string module)
    {
        return AuthState.CanUpdate(module);
    }

    /// <summary>
    /// Check if current user can DELETE in the specified SMS module
    /// Usage: if (CanDelete("SMS_RiskManagement")) { ... }
    /// </summary>
    protected bool CanDelete(string module)
    {
        return AuthState.CanDelete(module);
    }

    /// <summary>
    /// Check if current user has ANY access to the specified SMS module
    /// Usage: if (CanAccess("SMS_RiskManagement")) { ... }
    /// </summary>
    protected bool CanAccess(string module)
    {
        return AuthState.CanAccess(module);
    }

    /// <summary>
    /// Check if current user has administrative access
    /// Usage: if (HasAdminAccess()) { ... }
    /// </summary>
    protected bool HasAdminAccess()
    {
        return AuthState.HasAdministrativeAccess();
    }

    #endregion

    #region Page-Level Security Guards

    /// <summary>
    /// Throws unauthorized access exception if user lacks permission
    /// Usage: RequirePermission("SMS_RiskManagement", "CREATE");
    /// </summary>
    protected void RequirePermission(string module, string operation)
    {
        var hasPermission = operation.ToUpperInvariant() switch
        {
            "CREATE" => CanCreate(module),
            "READ" => CanRead(module),
            "UPDATE" => CanUpdate(module),
            "DELETE" => CanDelete(module),
            _ => CanAccess(module)
        };

        if (!hasPermission)
        {
            throw new UnauthorizedAccessException(
                $"User {GetCurrentUserDisplayName()} does not have {operation} permission for module {module}");
        }
    }

    /// <summary>
    /// Throws unauthorized access exception if user is not authenticated
    /// Usage: RequireAuthentication();
    /// </summary>
    protected void RequireAuthentication()
    {
        if (!IsUserAuthenticated())
        {
            throw new UnauthorizedAccessException("User must be authenticated to access this resource");
        }
    }

    /// <summary>
    /// Throws unauthorized access exception if user is not an administrator
    /// Usage: RequireAdminAccess();
    /// </summary>
    protected void RequireAdminAccess()
    {
        if (!HasAdminAccess())
        {
            throw new UnauthorizedAccessException(
                $"User {GetCurrentUserDisplayName()} does not have administrative access");
        }
    }

    #endregion

    #region Audit and Logging Helpers

    /// <summary>
    /// Get comprehensive audit information for the current user action
    /// Returns object with user, timestamp, and context information
    /// </summary>
    protected object GetAuditContext(string action, string resource = "")
    {
        return new
        {
            UserId = GetCurrentUserId(),
            UserName = GetCurrentUserDisplayName(),
            UserType = AuthState.UserType,
            Action = action,
            Resource = resource,
            Timestamp = DateTime.UtcNow,
            SessionStart = AuthState.LoginTime
        };
    }

    /// <summary>
    /// Set standard audit fields on any entity
    /// Usage: SetAuditFields(entity, "CREATE");
    /// </summary>
    protected void SetAuditFields(SMS_Domain.Common.BaseAuditableEntity entity, string operation)
    {
        var userId = GetCurrentUserId();
        var timestamp = DateTime.UtcNow;

        switch (operation.ToUpperInvariant())
        {
            case "CREATE":
                entity.CreatedBy = userId;
                entity.CreatedDate = timestamp;
                entity.UpdatedBy = userId;
                entity.UpdatedDate = timestamp;
                break;
            case "UPDATE":
                entity.UpdatedBy = userId;
                entity.UpdatedDate = timestamp;
                break;
        }
    }

    #endregion

    #region SMS Module Constants

    /// <summary>
    /// SMS Module identifiers for permission checking
    /// Use these constants instead of magic strings
    /// </summary>
    protected static class SMSModules
    {
        public const string RiskManagement = "SMS_RiskManagement";
        public const string Assurance = "SMS_Assurance";
        public const string Policy = "SMS_Policy";
        public const string Promotion = "SMS_Promotion";
        public const string SystemAdmin = "System_Administration";
        public const string UserManagement = "User_Management";
        public const string AuditManagement = "Audit_Management";
        public const string HazardManagement = "Hazard_Management";
        public const string RiskAssessment = "Risk_Assessment";
        public const string MitigationManagement = "Mitigation_Management";
    }

    #endregion
}