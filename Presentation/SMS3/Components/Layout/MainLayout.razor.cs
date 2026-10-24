using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Radzen;
using SMS3.Components.Layout;
using SMS3.Components.Shared;
using SMS3.Hubs.Contracts;
using SMS3.Configuration.Extensions;
using SMS_Application.Services;
using SMS_Domain.Entities;
using SMS_Domain.Enums;
namespace SMS3.Components.Layout;

public partial class MainLayout
{
    private const string RemoteLogoutSignalTitle = "SMS_REMOTE_LOGOUT_SIGNAL";
    private const char RemoteLogoutPayloadSeparator = '|';
    private readonly string _layoutFallbackSessionKey = $"layout:{Guid.NewGuid():N}";
    private bool _isLogoutInProgress;

[Inject] private ICurrentUserService _currentUserService { get; set; } = default!;
    [Inject] private ISMSSessionService _sessionService { get; set; } = default!;
    [Inject] private NavigationManager _navigation { get; set; } = default!;
    [Inject] private IWebHostEnvironment _environment { get; set; } = default!;
    [Inject] private IBaseMediator _mediator { get; set; } = default!;
    [Inject] private ILogger<MainLayout> _logger { get; set; } = default!;
    [Inject] private NotificationService _notificationService { get; set; } = default!;
[Inject] private DialogService _dialogService { get; set; } = default!;
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
        if (string.Equals(title, RemoteLogoutSignalTitle, StringComparison.Ordinal))
        {
            await HandleRemoteLogoutSignalAsync(message);
            return;
        }

        if (title.Contains(LiveUpdateEventNames.RecordEditReleased, StringComparison.OrdinalIgnoreCase)
            && message.Contains("StaleTimeout", StringComparison.OrdinalIgnoreCase))
        {
            await InvokeAsync(() =>
            {
                _notificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Info,
                    Summary = "Record Lock Released",
                    Detail = "Record lock has been released.",
                    Duration = duration
                });
            });
            return;
        }

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
        if (_isLogoutInProgress)
        {
            return;
        }

        _isLogoutInProgress = true;
        var logoutStartTime = DateTime.Now;
        string currentUserName = "Unknown";
        SMSUserType userType = SMSUserType.Application;
        string sessionId = "Unknown";
        TimeSpan sessionDuration = TimeSpan.Zero;
        var initiatingSessionKey = BuildCurrentSessionKey();
        var initiatingUserCode = _currentUserService?.UserCode ?? string.Empty;

        try
        {
            if (!await ConfirmMultiSessionLogoutAsync(initiatingUserCode, initiatingSessionKey))
            {
                return;
            }

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

            if (!string.IsNullOrWhiteSpace(initiatingUserCode))
            {
                var remoteLogoutPayload = $"{initiatingUserCode}{RemoteLogoutPayloadSeparator}{initiatingSessionKey}";
                await EventBusDispatcher.DispatchNotificationAsync(
                    NotificationSeverity.Info,
                    RemoteLogoutSignalTitle,
                    remoteLogoutPayload,
                    duration: 0);

                await WaitForOtherSessionsToLogoutAsync(initiatingUserCode, initiatingSessionKey, TimeSpan.FromSeconds(3));
            }

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

            var context = _httpContextAccessor.HttpContext;
            if (context is not null)
            {
                TryDeleteCookieIfPossible(context, "SMS_UserCodeHint", new CookieOptions { Path = "/" });
            }
            
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

                var context = _httpContextAccessor.HttpContext;
                if (context is not null)
                {
                    TryDeleteCookieIfPossible(context, "SMS_UserCodeHint", new CookieOptions { Path = "/" });
                }
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
        finally
        {
            _isLogoutInProgress = false;
        }

    }

    private async Task HandleRemoteLogoutSignalAsync(string initiatingSessionKey)
    {
        if (_isLogoutInProgress)
        {
            return;
        }

        try
        {
            var initiatingUserCode = string.Empty;
            var signalSessionKey = initiatingSessionKey;

            if (!string.IsNullOrWhiteSpace(initiatingSessionKey))
            {
                var separatorIndex = initiatingSessionKey.IndexOf(RemoteLogoutPayloadSeparator);
                if (separatorIndex > 0 && separatorIndex < initiatingSessionKey.Length - 1)
                {
                    initiatingUserCode = initiatingSessionKey[..separatorIndex];
                    signalSessionKey = initiatingSessionKey[(separatorIndex + 1)..];
                }
            }

            var currentSessionKey = BuildCurrentSessionKey();

            if (!string.IsNullOrWhiteSpace(initiatingUserCode))
            {
                var currentUserCode = ResolveUserCodeForSession(currentSessionKey);
                if (!string.IsNullOrWhiteSpace(currentUserCode)
                    && !string.Equals(currentUserCode, initiatingUserCode, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }

            if (string.Equals(currentSessionKey, signalSessionKey, StringComparison.Ordinal))
            {
                return;
            }

            _logger.LogInformation("Remote logout signal received. Clearing local session for user {UserCode}.", _currentUserService?.UserCode);
            _activeUserSessionRegistry.RemoveSession(currentSessionKey);
            await _currentUserService.ClearAuthentication();
            await _sessionService.ClearSMSSessionAsync();

            var context = _httpContextAccessor.HttpContext;
            if (context is not null)
            {
                TryDeleteCookieIfPossible(context, "SMS_UserCodeHint", new CookieOptions { Path = "/" });
            }

            await InvokeAsync(() =>
            {
                try
                {
                    _navigation.NavigateTo("/pdx", forceLoad: true);
                }
                catch (NavigationException ex)
                {
                    _logger.LogDebug(ex, "Navigation exception while applying remote logout signal.");
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process remote logout signal.");
        }
    }

    private void TryDeleteCookieIfPossible(HttpContext context, string cookieName, CookieOptions cookieOptions)
    {
        if (context.Response.HasStarted)
        {
            _logger.LogDebug("Skipping cookie delete for {CookieName} because response has already started.", cookieName);
            return;
        }

        context.Response.Cookies.Delete(cookieName, cookieOptions);
    }

    private string BuildCurrentSessionKey()
    {
        var context = _httpContextAccessor.HttpContext;
        var sessionId = context?.Features.Get<Microsoft.AspNetCore.Http.Features.ISessionFeature>()?.Session?.Id;
        if (string.IsNullOrWhiteSpace(sessionId) && context is not null)
        {
            try
            {
                sessionId = context.Session.Id;
            }
            catch (InvalidOperationException)
            {
                sessionId = null;
            }
        }

        if (!string.IsNullOrWhiteSpace(sessionId))
        {
            return $"session:{sessionId}";
        }

        var connectionId = context?.Connection?.Id;
        if (!string.IsNullOrWhiteSpace(connectionId))
        {
            return $"conn:{connectionId}";
        }

        return _layoutFallbackSessionKey;
    }

    private string ResolveUserCodeForSession(string sessionKey)
    {
        var currentUserCode = _currentUserService?.UserCode ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(currentUserCode))
        {
            return currentUserCode;
        }

        if (string.IsNullOrWhiteSpace(sessionKey))
        {
            return string.Empty;
        }

        return _activeUserSessionRegistry
            .GetActiveSessions()
            .FirstOrDefault(x => string.Equals(x.SessionKey, sessionKey, StringComparison.OrdinalIgnoreCase))
            ?.UserCode
            ?? string.Empty;
    }

    private async Task<bool> ConfirmMultiSessionLogoutAsync(string userCode, string currentSessionKey)
    {
        if (string.IsNullOrWhiteSpace(userCode))
        {
            return true;
        }

        var otherSessionCount = _activeUserSessionRegistry
            .GetActiveSessions()
            .Count(x => string.Equals(x.UserCode, userCode, StringComparison.OrdinalIgnoreCase)
                        && !string.Equals(x.SessionKey, currentSessionKey, StringComparison.OrdinalIgnoreCase));

        var displayedOtherSessionCount = Math.Max(0, otherSessionCount + 1);
        var confirmMessage = displayedOtherSessionCount <= 1
            ? "This action will end your current session. Continue?"
            : $"This action will end this session and {displayedOtherSessionCount} other active session(s). Continue?";

        var confirmed = await _dialogService.Confirm(
            confirmMessage,
            "Confirm Logoff",
            new ConfirmOptions
            {
                OkButtonText = "Log Off All",
                CancelButtonText = "Cancel"
            });

        return confirmed == true;
    }

    private async Task WaitForOtherSessionsToLogoutAsync(string userCode, string currentSessionKey, TimeSpan timeout)
    {
        if (string.IsNullOrWhiteSpace(userCode) || timeout <= TimeSpan.Zero)
        {
            return;
        }

        var timeoutAt = DateTime.UtcNow.Add(timeout);

        while (DateTime.UtcNow < timeoutAt)
        {
            var hasOtherSessions = _activeUserSessionRegistry
                .GetActiveSessions()
                .Any(x => string.Equals(x.UserCode, userCode, StringComparison.OrdinalIgnoreCase)
                          && !string.Equals(x.SessionKey, currentSessionKey, StringComparison.OrdinalIgnoreCase));

            if (!hasOtherSessions)
            {
                return;
            }

            await Task.Delay(100);
        }
    }
}


