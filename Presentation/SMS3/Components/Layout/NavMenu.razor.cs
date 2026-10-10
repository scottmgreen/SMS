using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;
using Radzen.Blazor;
using SMS3.Components.Shared.Components;
using SMS3.Configuration.Extensions;
using SMS3.Security;
using SMS_Application.Interfaces;
using SMS_Application.Services;
namespace SMS3.Components.Layout;

public partial class NavMenu
{

[Inject] private ICurrentUserService _currentUserService { get; set; } = default!;
    [Inject] private SessionTimerService _sessionTimerService { get; set; } = default!;
    [Inject] private IEventQueueService _eventQueueService { get; set; } = default!;
    [Inject] private IActiveUserSessionRegistry _activeUserSessionRegistry { get; set; } = default!;
    [Inject] private IHttpContextAccessor _httpContextAccessor { get; set; } = default!;

    [Parameter]
    public EventCallback OnLogoutRequested { get; set; }

    // Session Timer
    private CancellationTokenSource _cancellationTokenSource = new();
    private int _pendingIntegrationEventCount;
    private Task? _notificationPollingTask;
    private DotNetObjectReference<NavMenu>? _dotNetRef;
    private List<ActiveUserSessionInfo> _activeUserSessions = new();

    // Environment Detection - Always production appearance
    private string BrandCssClass => "";
    
    private string AssemblyVersion => System.Reflection.Assembly.GetExecutingAssembly()
        .GetCustomAttributes<System.Reflection.AssemblyInformationalVersionAttribute>()
        .FirstOrDefault()?.InformationalVersion?.Split('+')[0] ?? "Unknown";

    private string DevelopmentDisplayVersion => BuildDevelopmentDisplayVersion();

    private string EnvironmentSuffix => _environment.IsDevelopment() ? $"Development {DevelopmentDisplayVersion}" : string.Empty;
    private bool CanViewActiveUsers => _currentUserService?.CanRead("SMS_System") == true;

    private string BuildDevelopmentDisplayVersion()
    {
        var versionPart = AssemblyVersion.Split('-', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? AssemblyVersion;
        var versionSegments = versionPart.Split('.', StringSplitOptions.RemoveEmptyEntries);

        if (versionSegments.Length >= 3)
        {
            return $"{versionSegments[0]}.{versionSegments[1]}.{versionSegments[2]}";
        }

        if (versionSegments.Length >= 2)
        {
            return $"{versionSegments[0]}.{versionSegments[1]}";
        }

        return versionPart;
    }

    // URL-based login visibility - show login only on specific paths or encrypted login
    private bool ShouldShowLoginToAnonymous => _navigation.Uri.Contains("/pdx") || 
                                              _navigation.Uri.Contains("/admin") ||
                                              _navigation.Uri.Contains("/login") ||
                                              IsObfuscatedLogin();

    /// <summary>
    /// Check if current URL is an obfuscated login page
    /// </summary>
    private bool IsObfuscatedLogin()
    {
        try
        {
            var currentPath = _navigation.ToBaseRelativePath(_navigation.Uri);
            if (currentPath.StartsWith("s/"))
            {
                var encryptedPart = currentPath.Substring(2);
                var decrypted = _secureRoutingService.DecryptRouteParameter(encryptedPart);
                return decrypted.Contains("/login");
            }
        }
        catch { /* Ignore decrypt errors */ }
        
        return false;
    }

    protected override async Task OnInitializedAsync()
    {
        _navigation.LocationChanged += OnLocationChanged;
        _dotNetRef = DotNetObjectReference.Create(this);

        try
        {
            _logger.LogDebug("NavMenu OnInitializedAsync - Checking authentication state");
            
            // Single check - no loops
            if (_currentUserService?.IsFullyAuthenticated == true)
            {
                _sessionTimerService.StartTimer();
                RefreshActiveUserSessions();
                await LoadPendingIntegrationEventCountAsync();
                _logger.LogDebug("NavMenu initialized with session timer for user: {UserCode}", _currentUserService.UserCode);
            }

            _notificationPollingTask ??= PollPendingIntegrationEventCountAsync(_cancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in NavMenu OnInitializedAsync");
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // Only check on first render to avoid constant polling
        if (firstRender)
        {
            await RegisterActivityTrackingAsync();

            if (_currentUserService?.IsFullyAuthenticated == true)
            {
                _sessionTimerService.StartTimer();
                RefreshActiveUserSessions();
                await LoadPendingIntegrationEventCountAsync();
            }

            _notificationPollingTask ??= PollPendingIntegrationEventCountAsync(_cancellationTokenSource.Token);
        }
    }

    [JSInvokable]
    public void ReportClientActivity()
    {
        if (_currentUserService?.IsFullyAuthenticated != true)
        {
            return;
        }

        _sessionTimerService.UpdateActivity();
        RegisterCurrentUserActivity();
    }

    private async Task RegisterActivityTrackingAsync()
    {
        try
        {
            if (_dotNetRef is null)
            {
                return;
            }

            await JSRuntime.InvokeVoidAsync("smsSessionActivity.register", _dotNetRef);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to register browser activity tracking for session timeout.");
        }
    }

    private void OnLocationChanged(object? sender, Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs e)
    {
        _ = InvokeAsync(() =>
        {
            RefreshActiveUserSessions();
            _ = LoadPendingIntegrationEventCountAsync();
            StateHasChanged();
        });
    }
    
    protected override async Task OnParametersSetAsync()
    {
        // Update session activity when parameters change - only for fully authenticated users
        if (_currentUserService.IsFullyAuthenticated)
        {
            _sessionTimerService.UpdateActivity();
            RegisterCurrentUserActivity();
            RefreshActiveUserSessions();
                await LoadPendingIntegrationEventCountAsync();
        }
    }

    private async Task LoadPendingIntegrationEventCountAsync()
    {
        try
        {
            if (_currentUserService?.IsFullyAuthenticated != true || !_currentUserService.CanRead("SMS_System"))
            {
                _pendingIntegrationEventCount = 0;
                return;
            }

            var result = await _eventQueueService.GetQueuedEventsAsync(
                status: QueuedEventStatus.Pending,
                eventType: EventCategory.IntegrationEvent,
                maxResults: 2000);

            _pendingIntegrationEventCount = result.IsSuccess && result.Value is not null
                ? result.Value.Count()
                : 0;

            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load pending integration event count for nav bell.");
            _pendingIntegrationEventCount = 0;
        }
    }

    private async Task PollPendingIntegrationEventCountAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(15));

            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                RefreshActiveUserSessions();
                if (_currentUserService?.IsFullyAuthenticated == true)
                {
                    RegisterCurrentUserActivity();
                    await LoadPendingIntegrationEventCountAsync();
                }

                await InvokeAsync(StateHasChanged);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected during disposal
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Notification polling failed in NavMenu.");
        }
    }

    private void NavigateToNotificationManagement()
    {
        if (_currentUserService.IsFullyAuthenticated)
        {
            _sessionTimerService.UpdateActivity();
        }

        _navigation.NavigateToSecure("/SMSSystem/EventBus/QueueManager");
    }

    /// <summary>
    /// Handle session timer expiry - automatic logout
    /// </summary>
    private async Task OnSessionTimerExpired()
    {
        try
        {
            _logger.LogWarning("Session timer expired - initiating automatic logout");
            
            // Stop the session timer
            _sessionTimerService.StopTimer();
            
            // Trigger the logout process
            await OnLogoutRequested.InvokeAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling session timer expiry");
            
            // Force navigation to login as fallback
            _navigation.NavigateTo("/pdx", forceLoad: true);
        }
    }

    /// <summary>
    /// Handle session warning - user notification
    /// </summary>
    private async Task OnSessionWarning()
    {
        try
        {
            var remaining = _sessionTimerService.GetRemainingTime();
            _logger.LogInformation("Session warning: {RemainingTime} remaining for user {UserId}", 
                remaining, _currentUserService.UserCode);
            
            // The modal warning is handled by the SessionTimeoutModal component
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling session warning");
        }
    }

    /// <summary>
    /// Handle session continuation from modal
    /// </summary>
    private async Task OnSessionContinued()
    {
        try
        {
            _logger.LogInformation("Session continued by user {UserId}", _currentUserService.UserCode);
            
            // Update session activity (already done in modal, but ensure it's logged)
            _sessionTimerService.UpdateActivity();
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling session continuation");
        }
    }

    /// <summary>
    /// Handle menu clicks for SECURE navigation - all URLs automatically encrypted
    /// Also updates session activity
    /// </summary>
    private async Task OnMenuClick(MenuItemEventArgs args)
    {
        try
        {
            // Update session activity on menu interaction - only for fully authenticated users
            if (_currentUserService.IsFullyAuthenticated)
            {
                _sessionTimerService.UpdateActivity();
            }

            //_logger.LogInformation("OnMenuClick called with value: {Text}", args.Text?.ToString());

            if (!string.IsNullOrEmpty(args.Value?.ToString()))
            {
                var value = args.Value.ToString();
                if (!string.IsNullOrEmpty(value) && value.StartsWith("/"))
                {
                    _logger.LogInformation("Navigating to secure URL: {Url}", value);
                    _navigation.NavigateToSecure(value);
                }
                else if (Uri.TryCreate(value, UriKind.Absolute, out var absoluteUri)
                         && (absoluteUri.Scheme == Uri.UriSchemeHttps || absoluteUri.Scheme == Uri.UriSchemeHttp))
                {
                    _logger.LogInformation("Navigating to external URL: {Url}", value);
                    _navigation.NavigateTo(value, forceLoad: true);
                }
                else
                {
                    _logger.LogWarning("URL does not start with /: {Url}", value);
                }
            }
            else
            {
                _logger.LogInformation("OnMenuClick called with value: {Text}", args.Text?.ToString());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in OnMenuClick");
        }
    }

    private async Task OnLogoutClick()
    {
        // Stop session timer before logout
        _sessionTimerService.StopTimer();
        RemoveCurrentUserSession();
        
        await OnLogoutRequested.InvokeAsync();
        _cancellationTokenSource?.Cancel();
    }

    private void RegisterCurrentUserActivity()
    {
        if (_currentUserService?.IsFullyAuthenticated != true)
        {
            return;
        }

        var sessionKey = BuildCurrentSessionKey();
        _activeUserSessionRegistry.UpsertSession(new ActiveUserSessionInfo
        {
            SessionKey = sessionKey,
            UserCode = _currentUserService.UserCode,
            DisplayName = _currentUserService.UserDisplayName,
            UserType = _currentUserService.UserType ?? string.Empty,
            LoginTimeUtc = _currentUserService.LoginTime?.ToUniversalTime() ?? DateTime.UtcNow,
            LastSeenUtc = DateTime.UtcNow
        });

        _activeUserSessionRegistry.RefreshActivity(sessionKey);
    }

    private void RemoveCurrentUserSession()
    {
        if (_currentUserService?.IsFullyAuthenticated != true)
        {
            return;
        }

        _activeUserSessionRegistry.RemoveSession(BuildCurrentSessionKey());
    }

    private void RefreshActiveUserSessions()
    {
        if (!CanViewActiveUsers)
        {
            _activeUserSessions = [];
            return;
        }

        _activeUserSessions = _activeUserSessionRegistry
            .GetActiveSessions()
            .OrderBy(x => x.DisplayName)
            .ThenBy(x => x.UserCode)
            .Take(50)
            .ToList();
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

    /// <summary>
    /// Navigate to About page using secure navigation
    /// </summary>
    private void NavigateToAbout()
    {
        // Update session activity - only for fully authenticated users
        if (_currentUserService.IsFullyAuthenticated)
        {
            _sessionTimerService.UpdateActivity();
        }

        _logger.LogInformation("Navigating to About page via secure navigation");
        _navigation.NavigateToSecure("/About");
    }

    
    private async Task ShowUserProfileModal()
    {
        if (_currentUserService?.IsFullyAuthenticated != true) return;

        // Update session activity
        _sessionTimerService.UpdateActivity();

        var options = new DialogOptions()
        {
            Width = "900px",
            Height = "700px",
            Resizable = true,
            Draggable = true,
            CloseDialogOnOverlayClick = true,
            ShowTitle = true,
            ShowClose = true
        };

        await _dialogService.OpenAsync<UserProfileDialog>("User Profile & Permissions",
            new Dictionary<string, object?>()
            {
                { "_currentUserService", _currentUserService }                
            },
            options);
    }

    public void Dispose()
    {
        _navigation.LocationChanged -= OnLocationChanged;
        _sessionTimerService?.StopTimer();
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _dotNetRef?.Dispose();
    }

    // Hide all menu items on PDX and Login pages
    private bool IsOnRestrictedPage => _navigation.Uri.Contains("/pdx") || IsObfuscatedLogin();

    
    private string CreateHazardReportMenuValue
    {
        get
        {
            var configuredUrl = _configuration.GetValue<string>("Navigation:CreateHazardReportUrl");
            if (string.IsNullOrWhiteSpace(configuredUrl))
            {
                return "/SMSRiskManagement/HazardReporting";
            }

            return configuredUrl.Trim();
        }
    }
}


