//-----------------------------------------------------------------------
// <copyright file="SessionBasedCurrentUserService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Session-based current user service that replaces StaticCurrentUserService
//                  Uses HttpContext.Session for secure, per-user authentication state
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SMS_Application.Interfaces;

namespace SMS_Application.Services;

/// <summary>
/// Session-Based Current User Service - Secure session-based authentication
/// Replaces StaticCurrentUserService with proper session isolation and security
/// Uses exact same interface and permission logic as StaticCurrentUserService
/// </summary>
public class SessionBasedCurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<SessionBasedCurrentUserService> _logger;

    public SessionBasedCurrentUserService(
        IHttpContextAccessor httpContextAccessor, 
        ILogger<SessionBasedCurrentUserService> logger)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Basic Auth Properties - Direct from Session

    public bool IsAuthenticated => SafeGetSessionBool("IsAuthenticated");

    public string UserCode => SafeGetSessionString("SMS_UserId") ?? "SYSTEM";

    public string? UserType => SafeGetSessionString("SMS_UserType");

    public string UserDisplayName => SafeGetSessionString("SMS_DisplayName") ?? "System User";

    public string? Email => SafeGetSessionString("SMS_Email");

    public string? FirstName => SafeGetSessionString("SMS_FirstName");

    public string? LastName => SafeGetSessionString("SMS_LastName");

    public DateTime? LoginTime => SafeGetSessionDateTime("SMS_LoginTime");

    #endregion

    #region Full User and Role Access

    public string? UserRoleCode => SafeGetSessionString("SMS_UserRoleCode");

    public string? UserRoleName => SafeGetSessionString("SMS_UserRoleName");

    public List<SMSUserRolePermission> Permissions => SafeReconstructPermissionsFromSession();

    #endregion

    #region Permission Check Methods - Identical Logic to StaticCurrentUserService

    public bool CanCreate(string module)
    {
        return IsAuthenticated && Permissions.Any(p => p.SMSModule == module && p.Create);
    }

    public bool CanRead(string module)
    {
        return IsAuthenticated && Permissions.Any(p => p.SMSModule == module && p.Read);
    }

    public bool CanUpdate(string module)
    {
        return IsAuthenticated && Permissions.Any(p => p.SMSModule == module && p.Update);
    }

    public bool CanDelete(string module)
    {
        return IsAuthenticated && Permissions.Any(p => p.SMSModule == module && p.Delete);
    }

    public SMSUserType? GetUserTypeEnum()
    {
        if (!IsAuthenticated || string.IsNullOrEmpty(UserType))
            return null;

        return SMSUserType.FromValue(UserType);
    }

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

    #region Session Management

    public async Task ClearAuthentication()
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.Session != null)
            {
                _logger.LogInformation("Clearing session authentication for user: {UserId}", UserCode);
                httpContext.Session.Clear();
                await httpContext.Session.CommitAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error clearing session authentication");
        }
    }

    #endregion

    #region Private Session Helper Methods - SAFE VERSIONS

    /// <summary>
    /// SAFE: Get string value from session with comprehensive error handling and startup protection
    /// ENHANCED: Also checks HttpContext.Items as fallback for Blazor Server compatibility
    /// </summary>
    private string? SafeGetSessionString(string key)
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            
            // During startup, HttpContext may be null - return null safely
            if (httpContext == null)
            {
                return null;
            }

            // Try session first (if available and response hasn't started)
            if (httpContext.Session != null && !httpContext.Response.HasStarted)
            {
                try
                {
                    var sessionValue = httpContext.Session.GetString(key);
                    if (!string.IsNullOrEmpty(sessionValue))
                    {
                        return sessionValue;
                    }
                }
                catch (InvalidOperationException)
                {
                    // Session not available, fall through to Items check
                }
            }

            // ?? BLAZOR SERVER FALLBACK: Check HttpContext.Items
            if (httpContext.Items.TryGetValue(key, out var itemValue))
            {
                return itemValue?.ToString();
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Error reading session/items key: {Key}", key);
            return null;
        }
    }

    /// <summary>
    /// SAFE: Get boolean value from session
    /// </summary>
    private bool SafeGetSessionBool(string key)
    {
        var value = SafeGetSessionString(key);
        return bool.TryParse(value, out var result) && result;
    }

    /// <summary>
    /// SAFE: Get DateTime value from session
    /// </summary>
    private DateTime? SafeGetSessionDateTime(string key)
    {
        var value = SafeGetSessionString(key);
        return DateTime.TryParse(value, out var result) ? result : null;
    }

    /// <summary>
    /// SAFE: Reconstruct SMSUserRolePermission objects from session string data
    /// Uses EXACT same logic as AuthorizationService.LoadPermissionsFromSessionAsync()
    /// </summary>
    private List<SMSUserRolePermission> SafeReconstructPermissionsFromSession()
    {
        try
        {
            var permissionsString = SafeGetSessionString("SMS_UserPermissions");
            var userRoleCode = SafeGetSessionString("SMS_UserRoleCode");
            
            if (string.IsNullOrEmpty(permissionsString) || string.IsNullOrEmpty(userRoleCode))
            {
                return new List<SMSUserRolePermission>();
            }

            _logger.LogDebug("Reconstructing permissions from session for user role: {UserRoleCode}", userRoleCode);

            // Parse the permission strings using IDENTICAL logic from SMSSessionService
            var permissions = new List<SMSUserRolePermission>();
            var permissionPairs = permissionsString.Split('|');
            
            foreach (var pair in permissionPairs)
            {
                var parts = pair.Split(':');
                if (parts.Length == 2)
                {
                    var module = parts[0];
                    var actions = parts[1].Split(",");
                    
                    // Create permission entity with proper ID
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
            
            _logger.LogDebug("Reconstructed {PermissionCount} permissions from session", permissions.Count);
            return permissions;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Error reconstructing permissions from session - returning empty list");
            return new List<SMSUserRolePermission>();
        }
    }

    #endregion

    #region Legacy Methods - Kept for compatibility

    /// <summary>
    /// LEGACY: Get string value from session with comprehensive error handling
    /// </summary>
    private string? GetSessionString(string key) => SafeGetSessionString(key);

    /// <summary>
    /// LEGACY: Get boolean value from session
    /// </summary>
    private bool GetSessionBool(string key) => SafeGetSessionBool(key);

    /// <summary>
    /// LEGACY: Get DateTime value from session
    /// </summary>
    private DateTime? GetSessionDateTime(string key) => SafeGetSessionDateTime(key);

    /// <summary>
    /// LEGACY: Reconstruct SMSUserRolePermission objects from session string data
    /// </summary>
    private List<SMSUserRolePermission> ReconstructPermissionsFromSession() => SafeReconstructPermissionsFromSession();

    #endregion
}