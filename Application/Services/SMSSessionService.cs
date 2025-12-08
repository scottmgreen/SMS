using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SMS_Application.Interfaces;
using SMS_Domain.Entities;
using SMS_Domain.Enums;
using SMS_Application.Messaging.CircuitHandlers;
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
    /// Creates SMS session using Circuit Handler (no session timing issues)
    /// FIXED - Uses circuit-based authentication instead of session
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
            // Get the current circuit ID
            var circuitId = GetCurrentCircuitId(context);
            
            if (string.IsNullOrEmpty(circuitId))
            {
                _logger.LogWarning("No circuit ID found - using fallback session approach");
                await CreateFallbackSession(context, user, userType);
                return;
            }

            // Store authentication in circuit handler - NO TIMING ISSUES!
            SMS_CircuitHandler.SetCircuitAuthentication(circuitId, user, userType);
            
            _logger.LogInformation("Circuit authentication set successfully: CircuitId={CircuitId}, UserId={UserId}, UserType={UserType}", 
                circuitId, user.Code, userType.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating circuit authentication for user {UserId}", user.Code);
            
            // Fallback to session if circuit approach fails
            await CreateFallbackSession(context, user, userType);
        }
    }

    /// <summary>
    /// Fallback session creation (original approach)
    /// </summary>
    private async Task CreateFallbackSession(HttpContext context, BaseUser user, SMSUserType userType)
    {
        try
        {
            var session = context.Session;
            await session.LoadAsync();

            session.SetString("SMS_UserId", user.Code);
            session.SetString("SMS_UserCode", user.Code);
            session.SetString("SMS_UserType", userType.Value);
            session.SetString("SMS_Email", user.UserName.Value);
            session.SetString("SMS_DisplayName", user.DisplayName);
            session.SetString("SMS_FirstName", user.FirstName.Value);
            session.SetString("SMS_LastName", user.LastName.Value);
            session.SetString("IsAuthenticated", "true");

            await session.CommitAsync();
            
            _logger.LogInformation("Fallback session created for user {UserId}", user.Code);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("response has started"))
        {
            _logger.LogWarning("Fallback session also failed due to timing for user {UserId}", user.Code);
        }
    }

    /// <summary>
    /// Clears SMS session data (circuit and session)
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
            // Clear circuit authentication
            var circuitId = GetCurrentCircuitId(context);
            if (!string.IsNullOrEmpty(circuitId))
            {
                SMS_CircuitHandler.ClearCircuitAuthentication(circuitId);
                _logger.LogInformation("Cleared circuit authentication for circuit {CircuitId}", circuitId);
            }

            // Clear session as well
            var session = context.Session;
            var userId = session.GetString("SMS_UserId");
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
    /// Checks if current session is authenticated (circuit-first approach)
    /// Safe to call from Blazor components
    /// </summary>
    public bool IsAuthenticated()
    {
        try
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return false;

            // Try circuit authentication first
            var circuitId = GetCurrentCircuitId(context);
            if (!string.IsNullOrEmpty(circuitId) && SMS_CircuitHandler.IsCircuitAuthenticated(circuitId))
            {
                return true;
            }

            // Fallback to session
            var session = context.Session;
            return session.GetString("IsAuthenticated") == "true" &&
                   !string.IsNullOrEmpty(session.GetString("SMS_UserId"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking authentication status");
            return false;
        }
    }

    /// <summary>
    /// Gets current user ID (circuit-first approach)
    /// </summary>
    public string? GetCurrentUserId()
    {
        try
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return null;

            // Try circuit authentication first
            var circuitId = GetCurrentCircuitId(context);
            if (!string.IsNullOrEmpty(circuitId))
            {
                var authState = SMS_CircuitHandler.GetCircuitAuthentication(circuitId);
                if (authState?.IsAuthenticated == true)
                {
                    return authState.UserId;
                }
            }

            // Fallback to session
            return context.Session.GetString("SMS_UserId");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user ID");
            return null;
        }
    }

    /// <summary>
    /// Gets current user display name (circuit-first approach)
    /// </summary>
    public string? GetCurrentUserDisplayName()
    {
        try
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return null;

            // Try circuit authentication first
            var circuitId = GetCurrentCircuitId(context);
            if (!string.IsNullOrEmpty(circuitId))
            {
                var authState = SMS_CircuitHandler.GetCircuitAuthentication(circuitId);
                if (authState?.IsAuthenticated == true)
                {
                    return authState.DisplayName;
                }
            }

            // Fallback to session
            return context.Session.GetString("SMS_DisplayName");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user display name");
            return null;
        }
    }

    /// <summary>
    /// Gets current user type (circuit-first approach)
    /// </summary>
    public string? GetCurrentUserType()
    {
        try
        {
            var context = _httpContextAccessor.HttpContext;
            if (context == null) return null;

            // Try circuit authentication first
            var circuitId = GetCurrentCircuitId(context);
            if (!string.IsNullOrEmpty(circuitId))
            {
                var authState = SMS_CircuitHandler.GetCircuitAuthentication(circuitId);
                if (authState?.IsAuthenticated == true)
                {
                    return authState.UserType;
                }
            }

            // Fallback to session
            return context.Session.GetString("SMS_UserType");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user type");
            return null;
        }
    }

    /// <summary>
    /// Get current circuit ID from various possible sources
    /// </summary>
    private string? GetCurrentCircuitId(HttpContext context)
    {
        // Try to get circuit ID from items first
        if (context.Items.TryGetValue("CircuitId", out var circuitIdObj))
        {
            return circuitIdObj?.ToString();
        }

        // Try to get from connection feature
        var connectionFeature = context.Features.Get<Microsoft.AspNetCore.Http.Features.IHttpConnectionFeature>();
        if (connectionFeature != null)
        {
            return connectionFeature.ConnectionId;
        }

        return null;
    }
}