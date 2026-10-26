using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
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
    private readonly string _layoutReceiverSessionKey = $"layout-receiver:{Guid.NewGuid():N}";
    private readonly string _layoutFallbackSessionKey = $"layout:{Guid.NewGuid():N}";
    private bool _isLogoutInProgress;
    private DotNetObjectReference<MainLayout>? _dotNetRef;

    [Inject] private ICurrentUserService _currentUserService { get; set; } = default!;
    [Inject] private ISMSSessionService _sessionService { get; set; } = default!;
    [Inject] private NavigationManager _navigation { get; set; } = default!;
    [Inject] private IWebHostEnvironment _environment { get; set; } = default!;
    [Inject] private IBaseMediator _mediator { get; set; } = default!;
    [Inject] private ILogger<MainLayout> _logger { get; set; } = default!;
    [Inject] private NotificationService _notificationService { get; set; } = default!;
    [Inject] private DialogService _dialogService { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private IHttpContextAccessor _httpContextAccessor { get; set; } = default!;
    [Inject] private IActiveUserSessionRegistry _activeUserSessionRegistry { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        EventBusDispatcher.Register(this, _currentUserService?.UserCode ?? string.Empty, _layoutReceiverSessionKey);
        await Task.CompletedTask;
    }

    protected override async Task OnParametersSetAsync()
    {
        if (_currentUserService?.IsFullyAuthenticated == true)
        {
            _activeUserSessionRegistry.RefreshActivity(BuildCurrentSessionKey());
        }

        EventBusDispatcher.Register(this, _currentUserService?.UserCode ?? string.Empty, _layoutReceiverSessionKey);
        await Task.CompletedTask;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _dotNetRef ??= DotNetObjectReference.Create(this);
            try
            {
                await JSRuntime.InvokeVoidAsync("smsLogoutSync.register", _dotNetRef);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Could not register browser logout sync listener.");
            }
        }
    }

    [JSInvokable]
    public async Task ReceiveBrowserLogoutAsync(string initiatingReceiverSessionKey)
    {
        if (_isLogoutInProgress)
        {
            return;
        }

        if (string.Equals(_layoutReceiverSessionKey, initiatingReceiverSessionKey, StringComparison.Ordinal))
        {
            return;
        }

        try
        {
            var currentSessionKey = BuildCurrentSessionKey();
            _activeUserSessionRegistry.RemoveSession(currentSessionKey);
            await _currentUserService.ClearAuthentication();
            await _sessionService.ClearSMSSessionAsync();

            _navigation.NavigateTo("/pdx", forceLoad: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to apply browser logout sync signal.");
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
        var currentSessionKey = BuildCurrentSessionKey();

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

            try
            {
                await JSRuntime.InvokeVoidAsync("smsLogoutSync.broadcast", _layoutReceiverSessionKey);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Could not broadcast browser logout sync signal.");
            }

            var initiatingUserCode = _currentUserService?.UserCode ?? string.Empty;
            var machineScopeId = ResolveMachineScopeId();
            if (!string.IsNullOrWhiteSpace(initiatingUserCode))
            {
                var remoteLogoutPayload = string.Join(
                    RemoteLogoutPayloadSeparator,
                    initiatingUserCode,
                    _layoutReceiverSessionKey,
                    machineScopeId ?? string.Empty,
                    currentSessionKey);

                await EventBusDispatcher.DispatchNotificationAsync(
                    NotificationSeverity.Info,
                    RemoteLogoutSignalTitle,
                    remoteLogoutPayload,
                    duration: 0);
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
            _activeUserSessionRegistry.RemoveSession(currentSessionKey);

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
                _activeUserSessionRegistry.RemoveSession(currentSessionKey);
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

    private async Task HandleRemoteLogoutSignalAsync(string payload)
    {
        if (_isLogoutInProgress || string.IsNullOrWhiteSpace(payload))
        {
            return;
        }

        try
        {
            var segments = payload.Split(RemoteLogoutPayloadSeparator, StringSplitOptions.TrimEntries);
            if (segments.Length < 3)
            {
                return;
            }

            var signalUserCode = segments[0];
            var initiatingReceiverSessionKey = segments[1];
            var signalMachineScopeId = segments[2];
            var initiatingSessionKey = segments.Length >= 4 ? segments[3] : string.Empty;

            var currentSessionKey = BuildCurrentSessionKey();
            if (string.Equals(_layoutReceiverSessionKey, initiatingReceiverSessionKey, StringComparison.Ordinal))
            {
                return;
            }

            var currentUserCode = ResolveCurrentUserCode(currentSessionKey);
            if (!string.IsNullOrWhiteSpace(signalUserCode)
                && !string.IsNullOrWhiteSpace(currentUserCode)
                && !string.Equals(signalUserCode, currentUserCode, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var currentMachineScopeId = ResolveMachineScopeId();
            if (!string.IsNullOrWhiteSpace(currentMachineScopeId)
                && !string.IsNullOrWhiteSpace(signalMachineScopeId)
                && !string.Equals(currentMachineScopeId, signalMachineScopeId, StringComparison.Ordinal))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(currentMachineScopeId) || string.IsNullOrWhiteSpace(signalMachineScopeId))
            {
                var currentSessionId = TryExtractSessionId(currentSessionKey);
                var initiatingSessionId = TryExtractSessionId(initiatingSessionKey);
                if (string.IsNullOrWhiteSpace(currentSessionId)
                    || string.IsNullOrWhiteSpace(initiatingSessionId)
                    || !string.Equals(currentSessionId, initiatingSessionId, StringComparison.Ordinal))
                {
                    return;
                }
            }

            _logger.LogInformation("Remote logout signal received for same machine/user. Logging out this session.");

            _isLogoutInProgress = true;
            _activeUserSessionRegistry.RemoveSession(currentSessionKey);
            await _currentUserService.ClearAuthentication();
            await _sessionService.ClearSMSSessionAsync();

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
        finally
        {
            _isLogoutInProgress = false;
        }
    }

    private string ResolveMachineScopeId()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context is null)
        {
            return string.Empty;
        }

        return context.Request.Cookies.TryGetValue("SMS_MachineId", out var machineId)
            ? machineId?.Trim() ?? string.Empty
            : string.Empty;
    }

    private string ResolveCurrentUserCode(string currentSessionKey)
    {
        var userCode = _currentUserService?.UserCode ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(userCode))
        {
            return userCode;
        }

        return _activeUserSessionRegistry
            .GetActiveSessions()
            .FirstOrDefault(x => string.Equals(x.SessionKey, currentSessionKey, StringComparison.OrdinalIgnoreCase))
            ?.UserCode
            ?? string.Empty;
    }

    private static string TryExtractSessionId(string sessionKey)
    {
        if (string.IsNullOrWhiteSpace(sessionKey) || !sessionKey.StartsWith("session:", StringComparison.OrdinalIgnoreCase))
        {
            return string.Empty;
        }

        var remainder = sessionKey["session:".Length..];
        var connMarkerIndex = remainder.IndexOf(":conn:", StringComparison.OrdinalIgnoreCase);
        if (connMarkerIndex > 0)
        {
            return remainder[..connMarkerIndex];
        }

        return remainder;
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
        var connectionId = context?.Connection?.Id;
        var sessionId = context?.Features.Get<Microsoft.AspNetCore.Http.Features.ISessionFeature>()?.Session?.Id;

        if (!string.IsNullOrWhiteSpace(sessionId) && !string.IsNullOrWhiteSpace(connectionId))
        {
            return $"session:{sessionId}:conn:{connectionId}";
        }

        if (!string.IsNullOrWhiteSpace(connectionId))
        {
            return $"conn:{connectionId}";
        }

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

        return _layoutFallbackSessionKey;
    }

}


