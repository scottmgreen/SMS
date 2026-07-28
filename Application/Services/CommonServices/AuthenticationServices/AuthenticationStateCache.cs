//-----------------------------------------------------------------------
// <copyright file="AuthenticationStateCache.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Cached authentication state service to reduce redundant authentication checks.
//                  Provides component-level caching with automatic invalidation.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using SMS_Application.Interfaces;

namespace SMS_Application.Services;

/// <summary>
/// Interface for authentication state caching
/// </summary>
public interface IAuthenticationStateCache
{
    bool IsAuthenticated { get; }
    string UserCode { get; }
    bool IsFullyAuthenticated { get; }
    void InvalidateCache();
    void InvalidateUserCache();
    void InvalidateFullAuthCache();
    void RefreshCache();
    Dictionary<string, object> GetCacheStatus();
}

/// <summary>
/// Cached authentication state service to reduce redundant authentication checks
/// Provides component-level caching with automatic invalidation
/// </summary>
public class AuthenticationStateCache : IAuthenticationStateCache
{
    private readonly ILogger<AuthenticationStateCache> _logger;
    private readonly ICurrentUserService _currentUserService;
    
    // Cache for authentication state
    private bool? _cachedIsAuthenticated;
    private DateTime _lastAuthCheck = DateTime.MinValue;
    private readonly TimeSpan _authCacheTimeout = TimeSpan.FromMilliseconds(100); // Very short cache
    
    // Cache for user identity
    private string? _cachedUserCode;
    private DateTime _lastUserCheck = DateTime.MinValue;
    private readonly TimeSpan _userCacheTimeout = TimeSpan.FromSeconds(1); // Short cache for user info
    
    // Cache for 2FA state
    private bool? _cachedIsFullyAuthenticated;
    private DateTime _lastFullAuthCheck = DateTime.MinValue;
    private readonly TimeSpan _fullAuthCacheTimeout = TimeSpan.FromMilliseconds(200);

    public AuthenticationStateCache(
        ILogger<AuthenticationStateCache> logger,
        ICurrentUserService currentUserService)
    {
        _logger = logger;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Get cached authentication state with automatic refresh
    /// </summary>
    public bool IsAuthenticated
    {
        get
        {
            var now = DateTime.UtcNow;
            
            // Return cached value if still valid
            if (_cachedIsAuthenticated.HasValue && (now - _lastAuthCheck) < _authCacheTimeout)
            {
                return _cachedIsAuthenticated.Value;
            }

            // Refresh cache
            try
            {
                var isAuth = _currentUserService.IsAuthenticated;
                _cachedIsAuthenticated = isAuth;
                _lastAuthCheck = now;
                
                return isAuth;
            }
            catch (Exception ex)
            {
                _logger.LogApplicationError("Error checking cached authentication state", ApplicationEventIds.Error, ex);
                InvalidateCache();
                return false;
            }
        }
    }

    /// <summary>
    /// Get cached user code with automatic refresh
    /// </summary>
    public string UserCode
    {
        get
        {
            var now = DateTime.UtcNow;
            
            // Return cached value if still valid
            if (!string.IsNullOrEmpty(_cachedUserCode) && (now - _lastUserCheck) < _userCacheTimeout)
            {
                return _cachedUserCode;
            }

            // Refresh cache
            try
            {
                var userCode = _currentUserService.UserCode;
                _cachedUserCode = userCode;
                _lastUserCheck = now;
                
                return userCode;
            }
            catch (Exception ex)
            {
                _logger.LogApplicationError("Error getting cached user code", ApplicationEventIds.Error, ex);
                InvalidateUserCache();
                return string.Empty;
            }
        }
    }

    /// <summary>
    /// Get cached full authentication state (including 2FA) with automatic refresh
    /// </summary>
    public bool IsFullyAuthenticated
    {
        get
        {
            var now = DateTime.UtcNow;
            
            // Return cached value if still valid
            if (_cachedIsFullyAuthenticated.HasValue && (now - _lastFullAuthCheck) < _fullAuthCacheTimeout)
            {
                return _cachedIsFullyAuthenticated.Value;
            }

            // Refresh cache
            try
            {
                var isFullyAuth = _currentUserService.IsFullyAuthenticated;
                _cachedIsFullyAuthenticated = isFullyAuth;
                _lastFullAuthCheck = now;
                
                return isFullyAuth;
            }
            catch (Exception ex)
            {
                _logger.LogApplicationError("Error checking cached full authentication state", ApplicationEventIds.Error, ex);
                InvalidateFullAuthCache();
                return false;
            }
        }
    }

    /// <summary>
    /// Invalidate all cached authentication data
    /// Call this when authentication state changes
    /// </summary>
    public void InvalidateCache()
    {
        _cachedIsAuthenticated = null;
        _lastAuthCheck = DateTime.MinValue;
        InvalidateUserCache();
        InvalidateFullAuthCache();
        
        _logger.LogApplicationDebug("Authentication state cache invalidated", ApplicationEventIds.Debug);
    }

    /// <summary>
    /// Invalidate only user identity cache
    /// </summary>
    public void InvalidateUserCache()
    {
        _cachedUserCode = null;
        _lastUserCheck = DateTime.MinValue;
    }

    /// <summary>
    /// Invalidate only full authentication cache
    /// </summary>
    public void InvalidateFullAuthCache()
    {
        _cachedIsFullyAuthenticated = null;
        _lastFullAuthCheck = DateTime.MinValue;
    }

    /// <summary>
    /// Force refresh of all cached data
    /// </summary>
    public void RefreshCache()
    {
        InvalidateCache();
        
        // Trigger immediate refresh by accessing properties
        _ = IsAuthenticated;
        _ = UserCode;
        _ = IsFullyAuthenticated;
        
        _logger.LogApplicationDebug("Authentication state cache refreshed", ApplicationEventIds.Debug);
    }

    /// <summary>
    /// Get cache status for debugging
    /// </summary>
    public Dictionary<string, object> GetCacheStatus()
    {
        var now = DateTime.UtcNow;
        
        return new Dictionary<string, object>
        {
            ["IsAuthenticatedCached"] = _cachedIsAuthenticated,
            ["AuthCacheAge"] = (now - _lastAuthCheck).TotalMilliseconds,
            ["AuthCacheValid"] = _cachedIsAuthenticated.HasValue && (now - _lastAuthCheck) < _authCacheTimeout,
            
            ["UserCodeCached"] = _cachedUserCode,
            ["UserCacheAge"] = (now - _lastUserCheck).TotalMilliseconds,
            ["UserCacheValid"] = !string.IsNullOrEmpty(_cachedUserCode) && (now - _lastUserCheck) < _userCacheTimeout,
            
            ["FullAuthCached"] = _cachedIsFullyAuthenticated,
            ["FullAuthCacheAge"] = (now - _lastFullAuthCheck).TotalMilliseconds,
            ["FullAuthCacheValid"] = _cachedIsFullyAuthenticated.HasValue && (now - _lastFullAuthCheck) < _fullAuthCacheTimeout
        };
    }
}
