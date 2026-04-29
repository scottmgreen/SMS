//-----------------------------------------------------------------------
// <copyright file="AuthorizationService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Application layer authorization service providing presentation-independent permission checking.
//                  Moved authorization logic from CurrentUserService and presentation layer.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using SMS_Application.Interfaces;

using SMS_Domain.Entities;
using SMS_Domain.Enums;

namespace SMS_Application.Services;

/// <summary>
/// Authorization service providing presentation-independent permission checking and authorization logic.
/// Centralizes all authorization concerns in the Application layer for clean architecture compliance.
/// </summary>
public class AuthorizationService : IAuthorizationService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuthorizationService> _logger;

    public AuthorizationService(IHttpContextAccessor httpContextAccessor, ILogger<AuthorizationService> logger)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Basic Permission Checks

    public async Task<bool> CanCreateAsync(string userId, string module, CancellationToken cancellationToken = default)
    {
        var context = await GetUserContextAsync(userId, cancellationToken);
        return context?.CanCreate(module) ?? false;
    }

    public async Task<bool> CanReadAsync(string userId, string module, CancellationToken cancellationToken = default)
    {
        var context = await GetUserContextAsync(userId, cancellationToken);
        return context?.CanRead(module) ?? false;
    }

    public async Task<bool> CanUpdateAsync(string userId, string module, CancellationToken cancellationToken = default)
    {
        var context = await GetUserContextAsync(userId, cancellationToken);
        return context?.CanUpdate(module) ?? false;
    }

    public async Task<bool> CanDeleteAsync(string userId, string module, CancellationToken cancellationToken = default)
    {
        var context = await GetUserContextAsync(userId, cancellationToken);
        return context?.CanDelete(module) ?? false;
    }

    public async Task<bool> CanAccessAsync(string userId, string module, CancellationToken cancellationToken = default)
    {
        var context = await GetUserContextAsync(userId, cancellationToken);
        return context?.CanAccess(module) ?? false;
    }

    #endregion

    #region Advanced Permission Checks

    public async Task<SMSUserType?> GetUserTypeAsync(string userId, CancellationToken cancellationToken = default)
    {
        var context = await GetUserContextAsync(userId, cancellationToken);
        
        if (context?.UserType == null) return null;
        
        return SMSUserType.FromValue(context.UserType);
    }

    public async Task<List<string>> GetAccessibleModulesAsync(string userId, CancellationToken cancellationToken = default)
    {
        var context = await GetUserContextAsync(userId, cancellationToken);
        return context?.GetAccessibleModules() ?? new List<string>();
    }

    public async Task<List<SMSUserRolePermission>> GetUserPermissionsAsync(string userId, CancellationToken cancellationToken = default)
    {
        var context = await GetUserContextAsync(userId, cancellationToken);
        return context?.Permissions ?? new List<SMSUserRolePermission>();
    }

    #endregion

    #region User Context

    public async Task<UserAuthorizationContext?> GetUserContextAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId) || userId == "SYSTEM")
        {
            return null;
        }

        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                _logger.LogWarning("HttpContext not available for authorization context");
                return null;
            }

            // Check if this is the current authenticated user by comparing session user ID
            var sessionUserId = httpContext.Session?.GetString("SMS_UserId");
            if (sessionUserId != userId)
            {
                _logger.LogDebug("User ID {UserId} does not match session user {SessionUserId}", userId, sessionUserId);
                return null;
            }

            // Check if user is authenticated
            var isAuthenticated = IsUserAuthenticated(httpContext);
            if (!isAuthenticated)
            {
                _logger.LogDebug("User {UserId} is not authenticated", userId);
                return null;
            }

            // Build context from session data
            var context = new UserAuthorizationContext
            {
                UserId = userId,
                UserDisplayName = GetSessionString(httpContext, "SMS_DisplayName") ?? "Unknown User",
                UserType = GetSessionString(httpContext, "SMS_UserType"),
                UserRoleCode = GetSessionString(httpContext, "SMS_UserRoleCode"),
                UserRoleName = GetSessionString(httpContext, "SMS_UserRoleName"),
                Permissions = await LoadPermissionsFromSessionAsync(httpContext),
                IsAuthenticated = isAuthenticated
            };

            _logger.LogDebug("Authorization context loaded for user: {UserId}, Permissions: {PermissionCount}", 
                userId, context.Permissions.Count);

            return context;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading authorization context for user: {UserId}", userId);
            return null;
        }
    }

    #endregion

    #region Private Helper Methods

    /// <summary>
    /// Check if user is authenticated from session/items
    /// </summary>
    private bool IsUserAuthenticated(HttpContext httpContext)
    {
        try
        {
            // Check session first
            try
            {
                var sessionAuth = httpContext.Session?.GetString("IsAuthenticated");
                var sessionUserId = httpContext.Session?.GetString("SMS_UserId");
                
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

    /// <summary>
    /// Get string value from session with error handling
    /// </summary>
    private string? GetSessionString(HttpContext httpContext, string key)
    {
        try
        {
            return httpContext.Session?.GetString(key);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Error getting session value for key: {Key}", key);
            return null;
        }
    }

    /// <summary>
    /// Load permissions from session data (using existing CurrentUserService logic)
    /// </summary>
    private async Task<List<SMSUserRolePermission>> LoadPermissionsFromSessionAsync(HttpContext httpContext)
    {
        try
        {
            var permissionsString = GetSessionString(httpContext, "SMS_UserPermissions");
            var userRoleCode = GetSessionString(httpContext, "SMS_UserRoleCode");
            
            if (string.IsNullOrEmpty(permissionsString) || string.IsNullOrEmpty(userRoleCode))
            {
                return new List<SMSUserRolePermission>();
            }

            // Parse the simple permission strings back to entities (same logic as CurrentUserService)
            var permissions = new List<SMSUserRolePermission>();
            var permissionPairs = permissionsString.Split('|');
            
            foreach (var pair in permissionPairs)
            {
                var parts = pair.Split(':');
                if (parts.Length == 2)
                {
                    var module = parts[0];
                    var actions = parts[1].Split(",");
                    
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

    #endregion
}