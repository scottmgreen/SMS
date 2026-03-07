//-----------------------------------------------------------------------
// <copyright file="CurrentUserService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Enhanced current user service with full authentication state support.
//                  Mirrors the functionality of Presentation layer AuthenticationState.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SMS_Application.Messaging.Queries;
using SMS_Application.Interfaces;

namespace SMS_Application.Services;

/// <summary>
/// Enhanced Current User Service - Provides direct session-based user context
/// Optimized for synchronous access to current user information from session data
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<CurrentUserService> _logger;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor, ILogger<CurrentUserService> logger)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Basic Auth Properties

    public bool IsAuthenticated
    {
        get
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null) return false;

                // Check session first
                try
                {
                    var sessionAuth = httpContext.Session.GetString("IsAuthenticated");
                    var sessionUserId = httpContext.Session.GetString("SMS_UserId");
                    
                    if (bool.TryParse(sessionAuth, out var sessionResult) && sessionResult && !string.IsNullOrEmpty(sessionUserId))
                    {
                        return true;
                    }
                }
                catch (InvalidOperationException)
                {
                    // Session not configured - skip
                }

                // Check Items as fallback
                var itemsAuth = httpContext.Items["IsAuthenticated"]?.ToString();
                var itemsUserId = httpContext.Items["SMS_UserId"]?.ToString();
                
                if (bool.TryParse(itemsAuth, out var itemsResult) && itemsResult && !string.IsNullOrEmpty(itemsUserId))
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error checking authentication status");
                return false;
            }
        }
    }

    public string UserCode
    {
        get
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null) return "SYSTEM";

                // Try from Items first (set by middleware)
                var userIdFromItems = httpContext.Items["SMS_UserId"]?.ToString();
                if (!string.IsNullOrEmpty(userIdFromItems))
                    return userIdFromItems;

                // Try from session (with safe access)
                try
                {
                    var userIdFromSession = httpContext.Session?.GetString("SMS_UserId");
                    if (!string.IsNullOrEmpty(userIdFromSession))
                        return userIdFromSession;
                }
                catch (InvalidOperationException)
                {
                    // Session not configured - skip
                }

                // Try from cookies as fallback
                var userIdFromCookie = httpContext.Request.Cookies["SMS_SMS_UserId"];
                if (!string.IsNullOrEmpty(userIdFromCookie))
                    return userIdFromCookie;

                return "SYSTEM";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting current user ID, defaulting to SYSTEM");
                return "SYSTEM";
            }
        }
    }

    public string? UserType
    {
        get
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null) return null;

                var userType = httpContext.Items["SMS_UserType"]?.ToString() ??
                              httpContext.Session?.GetString("SMS_UserType");
                return userType;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting current user type");
                return null;
            }
        }
    }

    public string UserDisplayName
    {
        get
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null) return "System User";

                // Try from Items first
                var displayNameFromItems = httpContext.Items["SMS_DisplayName"]?.ToString();
                if (!string.IsNullOrEmpty(displayNameFromItems))
                    return displayNameFromItems;

                // Try from session (with safe access)
                try
                {
                    var displayNameFromSession = httpContext.Session?.GetString("SMS_DisplayName");
                    if (!string.IsNullOrEmpty(displayNameFromSession))
                        return displayNameFromSession;
                }
                catch (InvalidOperationException)
                {
                    // Session not configured - skip
                }

                // Try from cookies
                var displayNameFromCookie = httpContext.Request.Cookies["SMS_SMS_DisplayName"];
                if (!string.IsNullOrEmpty(displayNameFromCookie))
                    return displayNameFromCookie;

                return "System User";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting current user display name");
                return "System User";
            }
        }
    }

    public string? Email
    {
        get
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                return httpContext?.Session?.GetString("SMS_Email");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting user email");
                return null;
            }
        }
    }

    public string? FirstName
    {
        get
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                return httpContext?.Session?.GetString("SMS_FirstName");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting user first name");
                return null;
            }
        }
    }

    public string? LastName
    {
        get
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                return httpContext?.Session?.GetString("SMS_LastName");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting user last name");
                return null;
            }
        }
    }

    public DateTime? LoginTime
    {
        get
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                var loginTimeStr = httpContext?.Session?.GetString("SMS_LoginTime");
                
                if (DateTime.TryParse(loginTimeStr, out var loginTime))
                    return loginTime;
                
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting login time");
                return null;
            }
        }
    }

    #endregion

    #region Full User and Role Access

    public string? UserRoleCode
    {
        get
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                return httpContext?.Session?.GetString("SMS_UserRoleCode");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting user role code");
                return null;
            }
        }
    }

    public string? UserRoleName
    {
        get
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                return httpContext?.Session?.GetString("SMS_UserRoleName");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting user role name");
                return null;
            }
        }
    }

    public List<SMSUserRolePermission> Permissions
    {
        get
        {
            try
            {
                if (!IsAuthenticated) return new List<SMSUserRolePermission>();

                var httpContext = _httpContextAccessor.HttpContext;
                var permissionsString = httpContext?.Session?.GetString("SMS_UserPermissions");
                var userRoleCode = httpContext?.Session?.GetString("SMS_UserRoleCode");
                
                if (string.IsNullOrEmpty(permissionsString) || string.IsNullOrEmpty(userRoleCode))
                {
                    return new List<SMSUserRolePermission>();
                }

                // Parse the simple permission strings back to entities
                var permissions = new List<SMSUserRolePermission>();
                var permissionPairs = permissionsString.Split('|');
                
                foreach (var pair in permissionPairs)
                {
                    var parts = pair.Split(':');
                    if (parts.Length == 2)
                    {
                        var module = parts[0];
                        var actions = parts[1].Split(',');
                        
                        var permissionId = new SMSUserRolePermissionID($"PRM-{module}");
                        var permission = new SMSUserRolePermission(permissionId)
                        {
                            Code = $"PRM-{module}",
                            SMSUserRoleCode = userRoleCode,
                            SMSModule = module,
                            Create = actions.Contains("Create"),
                            Read = actions.Contains("Read"),
                            Update = actions.Contains("Update"),
                            Delete = actions.Contains("Delete")
                        };
                        permissions.Add(permission);
                    }
                }
                
                return permissions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user permissions from session");
                return new List<SMSUserRolePermission>();
            }
        }
    }

    #endregion

    #region Permission Check Methods (Use Direct Session Data - No AuthorizationService)

    /// <summary>
    /// Check if user can CREATE in a specific module - uses direct session data
    /// </summary>
    public bool CanCreate(string module)
    {
        return IsAuthenticated && Permissions.Any(p => p.SMSModule == module && p.Create);
    }

    /// <summary>
    /// Check if user can READ in a specific module - uses direct session data
    /// </summary>
    public bool CanRead(string module)
    {
        return IsAuthenticated && Permissions.Any(p => p.SMSModule == module && p.Read);
    }

    /// <summary>
    /// Check if user can UPDATE in a specific module - uses direct session data
    /// </summary>
    public bool CanUpdate(string module)
    {
        return IsAuthenticated && Permissions.Any(p => p.SMSModule == module && p.Update);
    }

    /// <summary>
    /// Check if user can DELETE in a specific module - uses direct session data
    /// </summary>
    public bool CanDelete(string module)
    {
        return IsAuthenticated && Permissions.Any(p => p.SMSModule == module && p.Delete);
    }

    /// <summary>
    /// Get user type as Smart Enum - uses direct session data
    /// </summary>
    public SMSUserType? GetUserTypeEnum()
    {
        if (!IsAuthenticated || string.IsNullOrEmpty(UserType))
            return null;

        return SMSUserType.FromValue(UserType);
    }

    /// <summary>
    /// Get all modules the user has access to - uses direct session data
    /// </summary>
    public List<string> GetAccessibleModules()
    {
        if (!IsAuthenticated) return new List<string>();
        
        var permissions = Permissions;
        return permissions
            .Where(p => p.Read || p.Create || p.Update || p.Delete)
            .Select(p => p.SMSModule ?? "Unknown")
            .Distinct()
            .ToList();
    }

    #endregion

    #region Session Management Methods

    /// <summary>
    /// Clear authentication for logout (delegates to ISMSSessionService)
    /// Use ISMSSessionService.ClearSMSSessionAsync() for logout operations
    /// </summary>
    public async Task ClearAuthentication()
    {
        // This service is for reading current user info only
        // Use ISMSSessionService.ClearSMSSessionAsync() directly for logout operations
        throw new NotSupportedException("Use ISMSSessionService.ClearSMSSessionAsync() for logout operations");
    }

    #endregion
}
