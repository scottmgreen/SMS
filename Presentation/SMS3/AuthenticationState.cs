using SMS_Domain.Entities;
using SMS_Domain.Enums;

namespace SMS3;

/// <summary>
/// CLEAN Authentication state with full user and permission support
/// </summary>
public class AuthenticationState
{
    // Basic Auth Properties
    public bool IsAuthenticated { get; private set; }
    public string? UserId { get; private set; }
    public string? UserType { get; private set; }
    public string? DisplayName { get; private set; }
    public string? Email { get; private set; }
    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }
    public DateTime LoginTime { get; private set; }

    // **NEW: Full User and Role Access**
    public BaseUser? CurrentUser { get; private set; }
    public SMSUserRole? UserRole { get; private set; }
    public List<SMSUserRolePermission> Permissions { get; private set; } = new();

    /// <summary>
    /// Set authentication from successful login - COMPLETE USER SETUP
    /// </summary>
    public void SetAuthentication(BaseUser user, SMSUserType userType)
    {
        CurrentUser = user;
        UserRole = user.UserRole;
        Permissions = user.UserRole?.Permissions ?? new List<SMSUserRolePermission>();

        UserId = user.Code;
        UserType = userType.Value;
        DisplayName = user.DisplayName;
        Email = user.UserName.Value;
        FirstName = user.FirstName.Value;
        LastName = user.LastName.Value;
        IsAuthenticated = true;
        LoginTime = DateTime.UtcNow;
    }

    /// <summary>
    /// Clear authentication state for logout
    /// </summary>
    public void ClearAuthentication()
    {
        IsAuthenticated = false;
        CurrentUser = null;
        UserRole = null;
        Permissions.Clear();
        UserId = null;
        UserType = null;
        DisplayName = null;
        Email = null;
        FirstName = null;
        LastName = null;
        LoginTime = default;
    }

    // **🔥 PERMISSION METHODS - USE THESE THROUGHOUT THE APP**

    /// <summary>
    /// Check if user can CREATE in a specific module
    /// </summary>
    public bool CanCreate(string module)
    {
        return IsAuthenticated &&
               Permissions.Any(p => p.SMSModule == module && p.Create);
    }

    /// <summary>
    /// Check if user can READ in a specific module
    /// </summary>
    public bool CanRead(string module)
    {
        return IsAuthenticated &&
               Permissions.Any(p => p.SMSModule == module && p.Read);
    }

    /// <summary>
    /// Check if user can UPDATE in a specific module
    /// </summary>
    public bool CanUpdate(string module)
    {
        return IsAuthenticated &&
               Permissions.Any(p => p.SMSModule == module && p.Update);
    }

    /// <summary>
    /// Check if user can DELETE in a specific module
    /// </summary>
    public bool CanDelete(string module)
    {
        return IsAuthenticated &&
               Permissions.Any(p => p.SMSModule == module && p.Delete);
    }

    /// <summary>
    /// Check if user has ANY permission in a module
    /// </summary>
    public bool CanAccess(string module)
    {
        return IsAuthenticated &&
               Permissions.Any(p => p.SMSModule == module && (p.Create || p.Read || p.Update || p.Delete));
    }

    /// <summary>
    /// Get user type as Smart Enum
    /// </summary>
    public SMSUserType? GetUserTypeEnum()
    {
        if (!IsAuthenticated || string.IsNullOrEmpty(UserType))
            return null;

        return SMSUserType.FromValue(UserType);
    }

    /// <summary>
    /// Check if user has administrative access
    /// </summary>
    public bool HasAdministrativeAccess()
    {
        return IsAuthenticated &&
               (UserType == "APPLICATION" || UserType == "ORGANIZATIONAL");
    }
}