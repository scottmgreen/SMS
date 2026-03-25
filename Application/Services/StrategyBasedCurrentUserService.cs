//-----------------------------------------------------------------------
// <copyright file="StrategyBasedCurrentUserService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Strategy-aware Current User Service that integrates with Authentication Strategy Manager.
//                  Provides real-time authentication state based on strategy manager data.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Application.Interfaces;
using SMS_Domain.Entities;
using SMS_Domain.Enums;

namespace SMS_Application.Services;

/// <summary>
/// Strategy-Aware Current User Service that integrates with Authentication Strategy Manager
/// Provides real-time authentication state and user information from active strategies
/// </summary>
public class StrategyBasedCurrentUserService : ICurrentUserService
{
    private readonly ILogger<StrategyBasedCurrentUserService> _logger;
    private readonly IAuthenticationStrategyManager _strategyManager;
    private readonly ISMSSessionService _sessionService;
    
    // Performance optimization: Simple cache for authentication state
    private bool? _cachedAuthState;
    private DateTime _lastAuthCheck = DateTime.MinValue;
    private readonly TimeSpan _authCacheTimeout = TimeSpan.FromMilliseconds(100); // Very short cache - just prevent immediate duplicates
    
    // No user code caching - keep it simple and immediate

    public StrategyBasedCurrentUserService(
        ILogger<StrategyBasedCurrentUserService> logger,
        IAuthenticationStrategyManager strategyManager,
        ISMSSessionService sessionService)
    {
        _logger = logger;
        _strategyManager = strategyManager;
        _sessionService = sessionService;
    }

    #region Basic Auth Properties

    public bool IsAuthenticated 
    { 
        get 
        {
            try
            {
                // Check cache first to avoid excessive strategy manager calls
                var now = DateTime.UtcNow;
                if (_cachedAuthState.HasValue && 
                    (now - _lastAuthCheck) < _authCacheTimeout)
                {
                    return _cachedAuthState.Value;
                }

                _logger.LogDebug("?? StrategyBasedCurrentUserService.IsAuthenticated - Starting check");
                var result = _strategyManager.IsUserAuthenticatedAsync().GetAwaiter().GetResult();
                _logger.LogDebug("?? StrategyBasedCurrentUserService.IsAuthenticated - Result: {Result}", result);
                
                // Cache the result
                _cachedAuthState = result;
                _lastAuthCheck = now;
                
                // Enhanced logging for debugging - only log detailed info when authentication succeeds
                if (result && _logger.IsEnabled(LogLevel.Information))
                {
                    var userResult = _strategyManager.RetrieveUserAsync().GetAwaiter().GetResult();
                    if (userResult.IsSuccess && userResult.Value.HasValue)
                    {
                        var user = userResult.Value.Value.User;
                        var userType = userResult.Value.Value.UserType;
                        _logger.LogInformation("? IsAuthenticated: TRUE - User: {UserCode}, Type: {UserType}, Role: {RoleCode}, Permissions: {PermissionCount}", 
                            user.Code, userType.Value, user.UserRole?.Code ?? "None", user.UserRole?.Permissions?.Count ?? 0);
                    }
                    else
                    {
                        _logger.LogWarning("?? IsAuthenticated: TRUE but RetrieveUser FAILED - {Error}", userResult.Error?.Message ?? "Unknown error");
                    }
                }
                else if (!result)
                {
                    _logger.LogDebug("?? IsAuthenticated: FALSE - No authenticated user found");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? Error checking authentication status");
                // Clear cache on error
                _cachedAuthState = null;
                _lastAuthCheck = DateTime.MinValue;
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
                // Keep this simple - no caching for UserCode to avoid complexity
                var userCode = _strategyManager.GetCurrentUserIdAsync().GetAwaiter().GetResult() ?? "SYSTEM";
                _logger.LogDebug("?? UserCode check: Result={UserCode}", userCode);
                
                return userCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user code");
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
                var result = _strategyManager.RetrieveUserAsync().GetAwaiter().GetResult();
                return result.IsSuccess && result.Value.HasValue ? result.Value.Value.UserType.Value : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user type");
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
                return _strategyManager.GetCurrentUserDisplayNameAsync().GetAwaiter().GetResult() ?? "System User";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user display name");
                return "System User";
            }
        }
    }

    public string? Email => GetUserProperty("SMS_Email");

    public string? FirstName => GetUserProperty("SMS_FirstName");

    public string? LastName => GetUserProperty("SMS_LastName");

    public DateTime? LoginTime 
    { 
        get 
        {
            try
            {
                var loginTimeStr = GetUserProperty("SMS_LoginTime");
                return DateTime.TryParse(loginTimeStr, out var loginTime) ? loginTime : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting login time");
                return null;
            }
        }
    }

    #endregion

    #region Full User and Role Access

    public string? UserRoleCode => GetUserProperty("SMS_UserRoleCode");

    public string? UserRoleName => GetUserProperty("SMS_UserRoleName");

    public List<SMSUserRolePermission> Permissions 
    { 
        get 
        {
            try
            {
                var result = _strategyManager.RetrieveUserAsync().GetAwaiter().GetResult();
                if (result.IsSuccess && result.Value.HasValue && result.Value.Value.User.UserRole?.Permissions != null)
                {
                    return result.Value.Value.User.UserRole.Permissions.ToList();
                }
                return new List<SMSUserRolePermission>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user permissions");
                return new List<SMSUserRolePermission>();
            }
        }
    }

    #endregion

    #region Permission Check Methods

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

    public async Task ClearAuthentication()
    {
        try
        {
            await _strategyManager.ClearUserAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing authentication");
        }
    }

    #endregion

    #region 2FA State Management

    public bool RequiresTwoFactorAuth 
    { 
        get 
        {
            try
            {
                // Check if the current user has 2FA enabled
                var result = _strategyManager.RetrieveUserAsync().GetAwaiter().GetResult();
                if (result.IsSuccess && result.Value.HasValue)
                {
                    var user = result.Value.Value.User;
                    var requires2FA = user.TwoFactorEnabled;
                    
                    _logger.LogDebug("?? RequiresTwoFactorAuth: User {UserCode} - TwoFactorEnabled: {Enabled}", 
                        user.Code, requires2FA);
                    
                    return requires2FA;
                }
                
                _logger.LogDebug("?? RequiresTwoFactorAuth: No user data available, assuming 2FA not required");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking 2FA requirement");
                return false;
            }
        }
    }

    public bool IsPending2FAVerification 
    { 
        get 
        {
            try
            {
                // FIRST: Check if user is authenticated at all
                if (!IsAuthenticated)
                {
                    _logger.LogDebug("?? IsPending2FAVerification: User not authenticated, cannot be pending 2FA");
                    return false;
                }

                // SECOND: Check if this user actually requires 2FA
                // If 2FA is not required for this user, they should never be in pending state
                if (!RequiresTwoFactorAuth)
                {
                    _logger.LogDebug("?? IsPending2FAVerification: User does not require 2FA, not pending");
                    return false;
                }

                // THIRD: User requires 2FA, check if they are in the pending verification state
                // This means they passed password auth but haven't completed 2FA yet
                var hasPendingData = _sessionService.HasPending2FAUser();
                
                _logger.LogDebug("?? IsPending2FAVerification: User requires 2FA, checking pending state: {HasPending}", hasPendingData);
                
                return hasPendingData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking pending 2FA status");
                return false;
            }
        }
    }

    public bool IsFullyAuthenticated 
    { 
        get 
        {
            try
            {
                // User is fully authenticated if:
                // 1. They are authenticated via strategy manager AND
                // 2. Either they don't require 2FA OR they completed 2FA verification
                var isAuth = IsAuthenticated;
                var requires2FA = RequiresTwoFactorAuth;
                var isPending = IsPending2FAVerification;
                
                bool result;
                if (!isAuth)
                {
                    // Not authenticated at all
                    result = false;
                }
                else if (!requires2FA)
                {
                    // Authenticated and doesn't need 2FA - fully authenticated
                    result = true;
                }
                else
                {
                    // Authenticated and requires 2FA - only fully authenticated if not pending
                    result = !isPending;
                }
                
                _logger.LogInformation("?? IsFullyAuthenticated check: Authenticated={IsAuth}, Requires2FA={Requires2FA}, Pending2FA={IsPending}, Result={Result}", 
                    isAuth, requires2FA, isPending, result);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking full authentication status");
                return false;
            }
        }
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Get a specific property from the current user's stored data
    /// </summary>
    private string? GetUserProperty(string propertyName)
    {
        try
        {
            var result = _strategyManager.RetrieveUserAsync().GetAwaiter().GetResult();
            if (result.IsSuccess && result.Value.HasValue)
            {
                // Try to get serialized user data if available
                // This would require extending the strategy manager to expose serialized data
                // For now, return null for non-core properties
                return null;
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user property {PropertyName}", propertyName);
            return null;
        }
    }

    #endregion

    #region Cache Management

    /// <summary>
    /// Clear cached authentication data
    /// Call this when authentication state changes
    /// </summary>
    public void ClearCache()
    {
        _cachedAuthState = null;
        _lastAuthCheck = DateTime.MinValue;
        
        _logger.LogDebug("?? Authentication cache cleared");
    }

    #endregion
}