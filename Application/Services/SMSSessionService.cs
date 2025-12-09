using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SMS_Application.Interfaces;
using SMS_Domain.Entities;
using SMS_Domain.Enums;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

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
    /// Creates SMS session using simple session approach (FORGET CIRCUITS!)
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

            // **SIMPLE SESSION APPROACH** - Just set the session data directly
            session.SetString("SMS_UserId", user.Code);
            session.SetString("SMS_UserCode", user.Code);
            session.SetString("SMS_UserType", userType.Value);
            session.SetString("SMS_Email", user.UserName.Value);
            session.SetString("SMS_DisplayName", user.DisplayName);
            session.SetString("SMS_FirstName", user.FirstName.Value);
            session.SetString("SMS_LastName", user.LastName.Value);
            session.SetString("IsAuthenticated", "true");

            _logger.LogInformation("Session data set directly: UserId={UserId}, UserType={UserType}, DisplayName={DisplayName}",
                user.Code, userType.Value, user.DisplayName);

            // Store user role information
            if (user.UserRole != null)
            {
                session.SetString("SMS_UserRoleCode", user.UserRole.Code ?? string.Empty);
                session.SetString("SMS_UserRoleName", user.UserRole.Name ?? string.Empty);

                if (user.UserRole.Permissions != null && user.UserRole.Permissions.Any())
                {
                    var permissionsData = user.UserRole.Permissions.Select(p => new {
                        Module = p.SMSModule ?? string.Empty,
                        Create = p.Create,
                        Read = p.Read,
                        Update = p.Update,
                        Delete = p.Delete
                    }).ToList();

                    var permissionsJson = System.Text.Json.JsonSerializer.Serialize(permissionsData);
                    session.SetString("SMS_UserPermissions", permissionsJson);
                }
                else
                {
                    session.SetString("SMS_UserPermissions", "[]");
                }
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

            _logger.LogInformation("SMS Session data set successfully for user {UserId}", user.Code);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Session creation failed for user {UserId} - but continuing anyway", user.Code);
            // Don't throw - let the calling code continue
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
    /// Checks if current session is authenticated (SIMPLE SESSION ONLY)
    /// </summary>
    public bool IsAuthenticated()
    {
        try
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return false;

            // **SIMPLE**: Just check session - NO CIRCUIT COMPLEXITY
            var session = context.Session;
            var isAuth = session.GetString("IsAuthenticated") == "true" &&
                        !string.IsNullOrEmpty(session.GetString("SMS_UserId"));
            
            _logger.LogInformation("Simple session auth check: {IsAuth}", isAuth);
            return isAuth;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking authentication status");
            return false;
        }
    }

    /// <summary>
    /// Gets current user ID (SIMPLE SESSION ONLY)
    /// </summary>
    public string? GetCurrentUserId()
    {
        try
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return null;

            return context.Session.GetString("SMS_UserId");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user ID");
            return null;
        }
    }

    /// <summary>
    /// Gets current user display name (SIMPLE SESSION ONLY)
    /// </summary>
    public string? GetCurrentUserDisplayName()
    {
        try
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return null;

            return context.Session.GetString("SMS_DisplayName");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user display name");
            return null;
        }
    }

    /// <summary>
    /// Gets current user type (SIMPLE SESSION ONLY)
    /// </summary>
    public string? GetCurrentUserType()
    {
        try
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return null;

            return context.Session.GetString("SMS_UserType");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user type");
            return null;
        }
    }
}