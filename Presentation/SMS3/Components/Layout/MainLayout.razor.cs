using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Radzen;
using SMS3.Components.Layout;
using SMS3.Components.Shared;
using SMS3.Configuration.Extensions;
using SMS_Application.Services;
using SMS_Domain.Entities;
using SMS_Domain.Enums;
namespace SMS3.Components.Layout;

public partial class MainLayout
{

[Inject] private ICurrentUserService _currentUserService { get; set; } = default!;
    [Inject] private ISMSSessionService _sessionService { get; set; } = default!;
    [Inject] private NavigationManager _navigation { get; set; } = default!;
    [Inject] private IWebHostEnvironment _environment { get; set; } = default!;
    [Inject] private IBaseMediator _mediator { get; set; } = default!;
    [Inject] private ILogger<MainLayout> _logger { get; set; } = default!;
    [Inject] private NotificationService _notificationService { get; set; } = default!;
    [Inject] private IHttpContextAccessor _httpContextAccessor { get; set; } = default!;
    [Inject] private IActiveUserSessionRegistry _activeUserSessionRegistry { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        EventBusDispatcher.Register(this, _currentUserService?.UserCode ?? string.Empty, BuildCurrentSessionKey());
        await Task.CompletedTask;
    }

    protected override async Task OnParametersSetAsync()
    {
        if (_currentUserService?.IsFullyAuthenticated == true)
        {
            _activeUserSessionRegistry.RefreshActivity(BuildCurrentSessionKey());
        }

        EventBusDispatcher.Register(this, _currentUserService?.UserCode ?? string.Empty, BuildCurrentSessionKey());
        await Task.CompletedTask;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // MainLayout first render complete
        }
    }

    public async Task HandleEventAsync(NotificationSeverity severity, string title, string message, int duration)
    {
        await InvokeAsync(() =>
        {
            _notificationService.Notify(new NotificationMessage
            {
                Severity = severity,
                Summary = title,
                Detail = message,
                Duration = duration
            });
        });
    }

    public void Dispose()
    {
        EventBusDispatcher.Unregister(this);
        if (_currentUserService?.IsFullyAuthenticated == true)
        {
            _activeUserSessionRegistry.RemoveSession(BuildCurrentSessionKey());
        }
    }
    

    /// <summary>
    /// Handle logout request from NavMenu - SESSION-BASED AUTHENTICATION APPROACH
    /// </summary>
    private async Task OnLogoutRequested()
    {
        var logoutStartTime = DateTime.Now;
        string currentUserName = "Unknown";
        SMSUserType userType = SMSUserType.Application;
        string sessionId = "Unknown";
        TimeSpan sessionDuration = TimeSpan.Zero;

        try
        {
            // Get current user info BEFORE clearing session authentication
            if (_currentUserService.IsAuthenticated)
            {
                currentUserName = _currentUserService.UserDisplayName ?? _currentUserService.UserCode ?? "Unknown";
                
                // Get user type from the service
                var userTypeEnum = _currentUserService.GetUserTypeEnum();
                if (userTypeEnum != null)
                {
                    userType = userTypeEnum;
                }
                
                // Generate a session ID for audit purposes
                sessionId = $"SESSION_{DateTime.Now:yyyyMMddHHmmss}";
                
                // Calculate session duration based on login time
                if (_currentUserService.LoginTime.HasValue)
                {
                    sessionDuration = logoutStartTime - _currentUserService.LoginTime.Value;
                }
                else
                {
                    sessionDuration = TimeSpan.FromMinutes(30); // Default fallback
                }
            }

            _logger.LogInformation("Starting logout process for user: {UserName}", currentUserName);

            // ?? Record logout audit BEFORE clearing authentication (and wait for it)
            try
            {
                var logoutCommand = new RecordAuthenticationLogoutCommand(
                    currentUserName,
                    userType,
                    sessionId,
                    "Manual", // Logout type - user clicked logout
                    sessionDuration
                );
                
                var auditResult = await _mediator.SendAsync(logoutCommand, CancellationToken.None);
                _logger.LogInformation("Logout audit recorded for user: {UserName}, Result: {IsSuccess}", 
                    currentUserName, auditResult.IsSuccess);
            }
            catch (Exception auditEx)
            {
                _logger.LogError(auditEx, "Failed to record logout audit for {UserName}", currentUserName);
                // Continue with logout even if audit fails
            }

            // **?? CLEAR SESSION-BASED AUTHENTICATION - Replaces static authentication**
            var sessionKey = BuildCurrentSessionKey();
            _activeUserSessionRegistry.RemoveSession(sessionKey);
            _activeUserSessionRegistry.RemoveSessionsForUser(_currentUserService?.UserCode ?? string.Empty);

            await _currentUserService.ClearAuthentication();
            await _sessionService.ClearSMSSessionAsync();
            
            _logger.LogInformation("Session authentication cleared for user: {UserName}", currentUserName);
            
            // Navigate to login after logout
            _navigation.NavigateTo("/pdx", forceLoad: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout process for user: {UserName}", currentUserName);
            
            // Still try to clear authentication and redirect even if audit fails
            try
            {
                _activeUserSessionRegistry.RemoveSessionsForUser(_currentUserService?.UserCode ?? string.Empty);
                await _currentUserService.ClearAuthentication();
                await _sessionService.ClearSMSSessionAsync();
                // Force navigation to login page
                _navigation.NavigateTo("/login", forceLoad: true);
            }
            catch (Exception clearEx)
            {
                _logger.LogError(clearEx, "Critical: Failed to clear session authentication during logout");
                // Force navigation to login even on clear error
                _navigation.NavigateTo("/login", forceLoad: true);
            }
        }

    }

    private string BuildCurrentSessionKey()
    {
        var context = _httpContextAccessor.HttpContext;
        var sessionId = context?.Features.Get<Microsoft.AspNetCore.Http.Features.ISessionFeature>()?.Session?.Id;
        if (!string.IsNullOrWhiteSpace(sessionId))
        {
            return $"session:{sessionId}";
        }

        var userCode = _currentUserService?.UserCode ?? "unknown";
        var remoteIp = context?.Connection?.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = context?.Request?.Headers["User-Agent"].FirstOrDefault() ?? "unknown";
        return $"fallback:{userCode}:{remoteIp}:{userAgent.GetHashCode()}";
    }
}


