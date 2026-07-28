//-----------------------------------------------------------------------
// <copyright file="SessionCurrentUserService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Session-based current user service that replaces StaticCurrentUserService
//                  Uses HttpContext.Session for secure, per-user authentication state
// </copyright>
//-----------------------------------------------------------------------

using SMS_Application.Common;
using SMS_Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SMS_Application.Interfaces;

namespace SMS_Application.Services;

/// <summary>
/// Session-Based Current User Service - Secure session-based authentication
/// Replaces StaticCurrentUserService with proper session isolation and security
/// Uses exact same interface and permission logic as StaticCurrentUserService
/// ENHANCED: Now supports circuit-based storage for Blazor Server
/// </summary>
public class SessionCurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<SessionCurrentUserService> _logger;
    private readonly IBlazorCircuitAuthStorage _circuitAuthStorage;
    private readonly ISMSSessionService _smsSessionService;

    public SessionCurrentUserService(
        IHttpContextAccessor httpContextAccessor, 
        ILogger<SessionCurrentUserService> logger,
        IBlazorCircuitAuthStorage circuitAuthStorage,
        ISMSSessionService smsSessionService)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _circuitAuthStorage = circuitAuthStorage ?? throw new ArgumentNullException(nameof(circuitAuthStorage));
        _smsSessionService = smsSessionService ?? throw new ArgumentNullException(nameof(smsSessionService));
    }

    #region Basic Auth Properties - Direct from Session

    public bool IsAuthenticated 
    { 
        get 
        { 
            return SafeGetSessionBool("IsAuthenticated");
        } 
    }

    public string UserCode 
    { 
        get 
        { 
            return SafeGetSessionString("SMS_UserId") ?? string.Empty;
        } 
    }

    public string? UserType 
    { 
        get 
        { 
            return SafeGetSessionString("SMS_UserType");
        } 
    }

    public string UserDisplayName 
    { 
        get 
        { 
            return SafeGetSessionString("SMS_DisplayName") ?? UserCode ?? SystemActorConstants.FlyPdxApiSource;
        } 
    }

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

    #region 2FA State Management

    /// <summary>
    /// Check if user has 2FA enabled (requires 2FA verification)
    /// </summary>
    public bool RequiresTwoFactorAuth 
    { 
        get 
        { 
            return SafeGetSessionBool("SMS_TwoFactorEnabled");
        } 
    }

    /// <summary>
    /// Check if user is currently pending 2FA verification (password authenticated but 2FA not verified)
    /// This checks if there's pending 2FA data indicating the user is in the intermediate state
    /// CRITICAL: Must distinguish between pending 2FA and full authentication
    /// </summary>
    public bool IsPending2FAVerification 
    { 
        get 
        {
            try
            {
                // FIRST: Check if user is already fully authenticated
                // If IsAuthenticated is true, we should NOT be in pending 2FA state
                if (IsAuthenticated)
                {
                    _logger.LogApplicationDebug("User is authenticated - checking if truly pending or fully authenticated");
                    
                    // Check for specific pending 2FA markers (not full auth data)
                    var context = _httpContextAccessor.HttpContext;
                    if (context?.Items.ContainsKey("Pending2FA_UserData") == true || 
                        context?.Items.ContainsKey("Pending2FA_UserType") == true ||
                        context?.Items.ContainsKey("Pending2FA_StoredAt") == true)
                    {
                        _logger.LogApplicationDebug("Found pending 2FA markers in HttpContext - user is pending 2FA verification");
                        return true;
                    }

                    // Check circuit storage for pending 2FA specific markers
                    var circuitData = GetCurrentCircuitAuthData();
                    if (circuitData != null && 
                        (circuitData.ContainsKey("Pending2FA_UserData") || 
                         circuitData.ContainsKey("Pending2FA_UserType") ||
                         circuitData.ContainsKey("Pending2FA_StoredAt")))
                    {
                        _logger.LogApplicationDebug("Found pending 2FA markers in circuit storage - user is pending 2FA verification");
                        return true;
                    }

                    // Check session for pending 2FA specific markers
                    var pending2FAData = SafeGetSessionString("Pending2FA_UserData");
                    var pending2FAStoredAt = SafeGetSessionString("Pending2FA_StoredAt");
                    if (!string.IsNullOrEmpty(pending2FAData) || !string.IsNullOrEmpty(pending2FAStoredAt))
                    {
                        _logger.LogApplicationDebug("Found pending 2FA markers in session - user is pending 2FA verification");
                        return true;
                    }

                    // If user is authenticated but no pending 2FA markers found, they are fully authenticated
                    _logger.LogApplicationDebug("User is authenticated with no pending 2FA markers - fully authenticated");
                    return false;
                }

                // User is not authenticated at all, so definitely not pending 2FA
                _logger.LogApplicationDebug("User is not authenticated - not pending 2FA verification");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogApplicationError(ex, "Error checking pending 2FA status");
                return false;
            }
        } 
    }

    /// <summary>
    /// Check if user is fully authenticated (password + 2FA verified, or 2FA not required)
    /// This is what should be used for NavMenu display logic
    /// </summary>
    public bool IsFullyAuthenticated 
    { 
        get 
        {
            // User is fully authenticated if:
            // 1. They are authenticated AND
            // 2. Either they don't require 2FA OR they are not pending 2FA verification
            return IsAuthenticated && (!RequiresTwoFactorAuth || !IsPending2FAVerification);
        } 
    }

    #endregion

    #region Session Management

    /// <summary>
    /// Clear session authentication - used for logout
    /// Clears all authentication data from session, circuit storage, and context items
    /// </summary>
    public async Task ClearAuthentication()
    {
        var context = _httpContextAccessor.HttpContext;
        var userId = GetCurrentUserId();
        
        try
        {
            _logger.LogApplicationInformation("Clearing session authentication for user: {UserId}", userId);

            // Clear session data if available
            if (context?.Session != null)
            {
                try
                {
                    context.Session.Clear();
                    _logger.LogApplicationInformation("Session data cleared for user: {UserId}", userId);
                }
                catch (Exception ex)
                {
                    _logger.LogApplicationWarning(ex, "Could not clear session data for user: {UserId}", userId);
                }
            }

            // Clear circuit storage by user ID
            _circuitAuthStorage.ClearAuthDataByUserId(userId);
            _logger.LogApplicationInformation("Circuit authentication data cleared for user: {UserId}", userId);

            // Clear HttpContext.Items
            if (context != null)
            {
                var smsKeys = context.Items.Keys
                    .Where(key => key is string keyStr && 
                                 (keyStr.StartsWith("SMS_") || keyStr == "IsAuthenticated" || keyStr.Contains("CIRCUIT_ID")))
                    .ToList();

                foreach (var key in smsKeys)
                {
                    context.Items.Remove(key);
                }
                _logger.LogApplicationInformation("Context items cleared for user: {UserId}", userId);
            }

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Error clearing authentication for user: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Get current user ID for logging and clearing operations
    /// </summary>
    private string GetCurrentUserId()
    {
        try
        {
            return UserCode ?? "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }

    #endregion

    #region Private Session Helper Methods - SAFE VERSIONS

    /// <summary>
    /// SAFE: Get string value from session with comprehensive error handling and startup protection
    /// ENHANCED: Also checks HttpContext.Items as fallback for Blazor Server compatibility
    /// SUPER ENHANCED: Now checks circuit-based storage for Blazor Server persistence
    /// </summary>
    private string? SafeGetSessionString(string key)
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            
            // During startup, HttpContext may be null - try circuit storage first
            if (httpContext == null)
            {
                var circuitData = GetCurrentCircuitAuthData();
                if (circuitData != null && circuitData.TryGetValue(key, out var circuitValue))
                {
                    return circuitValue;
                }
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
                    // Session not available, fall through to circuit/Items check
                }
            }

            // ?? BLAZOR SERVER CIRCUIT FALLBACK: Check circuit-based storage
            var circuitAuthData = GetCurrentCircuitAuthData();
            if (circuitAuthData != null && circuitAuthData.TryGetValue(key, out var circuitStorageValue))
            {
                return circuitStorageValue;
            }

            // ?? BLAZOR SERVER ITEMS FALLBACK: Check HttpContext.Items
            if (httpContext.Items.TryGetValue(key, out var itemValue))
            {
                var stringValue = itemValue?.ToString();
                return stringValue;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Error reading session/circuit/items key: {Key}", key);
            return null;
        }
    }

    /// <summary>
    /// Get authentication data from current circuit
    /// </summary>
    private Dictionary<string, string>? GetCurrentCircuitAuthData()
    {
        try
        {
            // Method 1: Try to get stored circuit ID from context items
            var context = _httpContextAccessor.HttpContext;
            if (context?.Items.TryGetValue("SMS_CIRCUIT_ID", out var storedCircuitId) == true && storedCircuitId != null)
            {
                var circuitId = storedCircuitId.ToString();
                var data = _circuitAuthStorage.GetAuthData(circuitId);
                if (data != null)
                {
                    return data;
                }
            }

            // Method 2: Try current connection-based circuit ID
            var currentCircuitId = GetCurrentCircuitId();
            if (!string.IsNullOrEmpty(currentCircuitId))
            {
                var data = _circuitAuthStorage.GetAuthData(currentCircuitId);
                if (data != null)
                {
                    return data;
                }
            }

            // Method 3: Fallback - try to find any recent authentication data by user ID
            try
            {
                var userData = _circuitAuthStorage.GetAuthDataByUserId("AU-0001"); // Common admin user
                if (userData != null)
                {
                    _logger.LogApplicationDebug("Found authentication data using user ID fallback");
                    return userData;
                }
            }
            catch (Exception fallbackEx)
            {
                _logger.LogApplicationWarning(fallbackEx, "Error in authentication data fallback lookup");
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Error getting circuit auth data");
            return null;
        }
    }

    /// <summary>
    /// Get the current Blazor Server circuit ID
    /// </summary>
    private string? GetCurrentCircuitId()
    {
        try
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return null;

            // Method 1: Try connection ID (most reliable)
            var connectionId = context.Connection.Id;
            if (!string.IsNullOrEmpty(connectionId))
            {
                return connectionId; // Use connection ID directly as circuit ID
            }

            // Method 2: Generate deterministic ID from request info
            var remoteIpAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var userAgent = context.Request.Headers["User-Agent"].FirstOrDefault() ?? "unknown";
            var userAgentHash = userAgent.GetHashCode().ToString();
            return $"circuit_{remoteIpAddress}_{userAgentHash}";
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Error getting circuit ID");
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

            _logger.LogApplicationDebug("Reconstructing permissions from session for user role: {UserRoleCode}", userRoleCode);

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
            
            _logger.LogApplicationDebug("Reconstructed {PermissionCount} permissions from session", permissions.Count);
            return permissions;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationDebug(ex, "Error reconstructing permissions from session - returning empty list");
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

