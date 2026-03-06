//-----------------------------------------------------------------------
// <copyright file="SMSSessionService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Application service providing business logic operations for SMS domain entities.
//                  Provides business logic operations and coordinates domain entities
//                  through the CQRS pattern via Mediator services.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace SMS_Application.Services;

/// <summary>
/// SMS Session Management Service Implementation for Blazor Server
/// Handles the session lifecycle differences between Razor Pages and Blazor Server
/// </summary>
public class SMSSessionService : ISMSSessionService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<SMSSessionService> _logger;

    public SMSSessionService(IHttpContextAccessor httpContextAccessor, ILogger<SMSSessionService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    /// <summary>
    /// Creates SMS session using simple session approach
    /// </summary>
    public async Task CreateSMSSessionAsync(BaseUser user, SMSUserType userType)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            _logger.LogError("HttpContext is null - cannot create SMS session");
            throw new InvalidOperationException("HttpContext not available");
        }

        try
        {
            var session = context.Session;
            
            // Load session first
            await session.LoadAsync();

            // **SESSION DATA SETUP**
            session.SetString("SMS_UserId", user.Code);
            session.SetString("SMS_UserCode", user.Code);
            session.SetString("SMS_UserType", userType.Value);
            session.SetString("SMS_Email", user.UserName.Value);
            session.SetString("SMS_DisplayName", user.DisplayName);
            session.SetString("SMS_FirstName", user.FirstName.Value);
            session.SetString("SMS_LastName", user.LastName.Value);
            session.SetString("SMS_LoginTime", DateTime.UtcNow.ToString("O"));
            session.SetString("IsAuthenticated", "true");

            // Store user role information
            if (user.UserRole != null)
            {
                session.SetString("SMS_UserRoleCode", user.UserRole.Code ?? string.Empty);
                session.SetString("SMS_UserRoleName", user.UserRole.Name ?? string.Empty);

                _logger.LogInformation("User role stored - Code: {RoleCode}, Name: {RoleName}", 
                    user.UserRole.Code, user.UserRole.Name);

                // Store permissions as simple string data - no JSON needed
                if (user.UserRole.Permissions != null && user.UserRole.Permissions.Any())
                {
                    var permissionPairs = new List<string>();
                    foreach (var perm in user.UserRole.Permissions)
                    {
                        // Store as "Module:Create,Read,Update,Delete"
                        var actions = new List<string>();
                        if (perm.Create) actions.Add("Create");
                        if (perm.Read) actions.Add("Read");
                        if (perm.Update) actions.Add("Update");
                        if (perm.Delete) actions.Add("Delete");
                        
                        permissionPairs.Add($"{perm.SMSModule}:{string.Join(",", actions)}");
                    }
                    
                    session.SetString("SMS_UserPermissions", string.Join("|", permissionPairs));
                    _logger.LogInformation("Stored {PermissionCount} permissions for user", user.UserRole.Permissions.Count);
                }
            }
            else
            {
                _logger.LogWarning("🔍 DEBUG: No UserRole found for user {UserId}", user.Code);
            }

            // Store user type-specific data
            switch (userType)
            {
                case var type when type == SMSUserType.Application && user is SMSApplicationUser appUser:
                    session.SetString("SMS_ApplicationUserCode", appUser.Code ?? string.Empty);
                    break;

                case var type when type == SMSUserType.Organizational && user is SMSOrganizationalUser orgUser:
                    session.SetString("SMS_Department", orgUser.Department ?? string.Empty);
                    session.SetString("SMS_Position", orgUser.Position ?? string.Empty);
                    session.SetString("SMS_OrganizationLevel", orgUser.OrganizationLevel ?? string.Empty);
                    break;

                case var type when type == SMSUserType.Stakeholder && user is SMSStakeholderUser stakeholderUser:
                    session.SetString("SMS_Organization", stakeholderUser.Organization ?? string.Empty);
                    session.SetString("SMS_StakeholderType", stakeholderUser.StakeholderType ?? string.Empty);
                    break;
            }

            // Commit the session
            await session.CommitAsync();

            _logger.LogInformation("✅ SMS Session created successfully for user {UserId}", user.Code);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to create SMS session for user {UserId}", user.Code);
            throw new InvalidOperationException($"Failed to create SMS session: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Create authentication state that persists across requests when session fails
    /// </summary>
    private async Task CreatePersistentAuthenticationState(BaseUser user, SMSUserType userType)
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null) throw new InvalidOperationException("HttpContext not available");

        _logger.LogInformation("🔄 Creating persistent authentication state for user {UserId}", user.Code);

        try
        {
            // 🔧 FALLBACK APPROACH: Use cookies for authentication state when session fails
            var authData = new Dictionary<string, string>
            {
                ["SMS_UserId"] = user.Code,
                ["SMS_UserType"] = userType.Value,
                ["SMS_DisplayName"] = user.DisplayName,
                ["SMS_Email"] = user.UserName.Value,
                ["SMS_FirstName"] = user.FirstName.Value,
                ["SMS_LastName"] = user.LastName.Value,
                ["IsAuthenticated"] = "true",
                ["SMS_LoginTime"] = DateTime.UtcNow.ToString("O")
            };

            if (user.UserRole != null)
            {
                authData["SMS_UserRoleCode"] = user.UserRole.Code ?? string.Empty;
                authData["SMS_UserRoleName"] = user.UserRole.Name ?? string.Empty;
            }

            // Store as secure cookies
            foreach (var kvp in authData)
            {
                context.Response.Cookies.Append($"SMS_{kvp.Key}", kvp.Value, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTimeOffset.UtcNow.AddHours(8) // Match session timeout
                });
            }

            _logger.LogInformation("✅ Persistent authentication state created using cookies for user {UserId}", user.Code);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to create persistent authentication state for user {UserId}", user.Code);
            throw;
        }
    }

    /// <summary>
    /// Clears SMS session data (SIMPLE SESSION ONLY)
    /// </summary>
    public async Task ClearSMSSessionAsync()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            _logger.LogWarning("HttpContext is null - cannot clear SMS session");
            return;
        }

        try
        {
            var session = context.Session;
            var userId = session.GetString("SMS_UserId");

            // Simple session clear
            session.Clear();

            _logger.LogInformation("Cleared session for user: {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing SMS session");
            throw;
        }
    }

    /// <summary>
    /// Enhanced all getter methods to check both Session and HttpContext.Items for robust fallback support, and implemented IsUsingFallbackMethod
    /// </summary>
    public string? GetCurrentUserId()
    {
        try
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return null;

            // Try session first
            var sessionUserId = context.Session.GetString("SMS_UserId");
            if (!string.IsNullOrEmpty(sessionUserId))
                return sessionUserId;

            // Fallback to Items
            var itemsUserId = context.Items["SMS_UserId"]?.ToString();
            return itemsUserId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user ID");
            return null;
        }
    }

    /// <summary>
    /// Gets current user display name (ENHANCED: Checks both session and Items)
    /// </summary>
    public string? GetCurrentUserDisplayName()
    {
        try
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return null;

            // Try session first
            var sessionDisplayName = context.Session.GetString("SMS_DisplayName");
            if (!string.IsNullOrEmpty(sessionDisplayName))
                return sessionDisplayName;

            // Fallback to Items
            var itemsDisplayName = context.Items["SMS_DisplayName"]?.ToString();
            return itemsDisplayName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user display name");
            return null;
        }
    }

    /// <summary>
    /// Gets current user type (ENHANCED: Checks both session and Items)
    /// </summary>
    public string? GetCurrentUserType()
    {
        try
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return null;

            // Try session first
            var sessionUserType = context.Session.GetString("SMS_UserType");
            if (!string.IsNullOrEmpty(sessionUserType))
                return sessionUserType;

            // Fallback to Items
            var itemsUserType = context.Items["SMS_UserType"]?.ToString();
            return itemsUserType;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user type");
            return null;
        }
    }

    /// <summary>
    /// Checks if current session is authenticated (ENHANCED: Checks both session and Items)
    /// </summary>
    public bool IsAuthenticated()
    {
        try
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return false;

            // Check session first
            var sessionAuth = context.Session.GetString("IsAuthenticated") == "true" &&
                             !string.IsNullOrEmpty(context.Session.GetString("SMS_UserId"));
            
            if (sessionAuth)
            {
                _logger.LogDebug("Session authentication confirmed");
                return true;
            }

            // Check Items fallback
            var itemsAuth = context.Items["IsAuthenticated"]?.ToString() == "true" &&
                           !string.IsNullOrEmpty(context.Items["SMS_UserId"]?.ToString());

            if (itemsAuth)
            {
                _logger.LogDebug("Items fallback authentication confirmed");
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking authentication status");
            return false;
        }
    }
}
