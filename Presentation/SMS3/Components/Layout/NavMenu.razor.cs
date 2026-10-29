using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Radzen;
using Radzen.Blazor;
using SMS3.Components.Shared.Components;
using SMS3.Configuration.Extensions;
using SMS3.Security;
using SMS_Application.Interfaces;
using SMS_Application.Services;
using SMS3.Components.Shared;
using SMS3.Hubs.Contracts;
namespace SMS3.Components.Layout;

public partial class NavMenu
{
    private const bool EnableSessionTimer = false;
    private const bool EnableActivityTracking = false;

[Inject] private ICurrentUserService _currentUserService { get; set; } = default!;
    [Inject] private AuthenticationStateProvider _authenticationStateProvider { get; set; } = default!;
    [Inject] private IHttpClientFactory _httpClientFactory { get; set; } = default!;
    [Inject] private SessionTimerService _sessionTimerService { get; set; } = default!;
    [Inject] private IEventQueueService _eventQueueService { get; set; } = default!;
    [Inject] private IActiveUserSessionRegistry _activeUserSessionRegistry { get; set; } = default!;
    [Inject] private IHttpContextAccessor _httpContextAccessor { get; set; } = default!;
[Inject] private IRecordLockLeaseCoordinator _recordLockLeaseCoordinator { get; set; } = default!;

    [Parameter]
    public EventCallback OnLogoutRequested { get; set; }

    // Session Timer
    private CancellationTokenSource _cancellationTokenSource = new();
    private int _pendingIntegrationEventCount;
    private Task? _notificationPollingTask;
    private DotNetObjectReference<NavMenu>? _dotNetRef;
    private List<ActiveUserSessionInfo> _activeUserSessions = new();
    private List<RecordLockInfo> _activeRecordLocks = new();
    private bool _isFullyAuthenticated;
    private bool _isLogoutInitiated;
    private bool _hasRegisteredCurrentSession;
    private bool _isNavigationInProgress;
    private bool _hasAuthenticatedCircuitPrincipal;
    private bool _hasPerformedRestrictedRouteRedirect;
    private DateTime _lastAuthenticationSuccessUtc;
    private DateTime _lastServerAuthHydrationAttemptUtc;
    private bool _isServerAuthHydrationInProgress;
    private readonly string _navFallbackSessionKey = $"nav:{Guid.NewGuid():N}";
    private static readonly TimeSpan AuthStateDropGuardWindow = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan ServerAuthHydrationRetryInterval = TimeSpan.FromSeconds(2);
    private bool? _lastLoggedAuthState;
    private bool ShowSessionTimerInNav => EnableSessionTimer && _configuration.GetValue<bool>("FeatureManagement:ShowSessionTimerInNav", true);

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
        _navigation.RegisterLocationChangingHandler(OnLocationChangingAsync);
        _navigation.LocationChanged += OnLocationChanged;
        _dotNetRef = DotNetObjectReference.Create(this);

        try
        {
            _logger.LogDebug("NavMenu OnInitializedAsync - Checking authentication state");
            await HydrateCurrentUserFromPrincipalAsync();
            await HydrateCurrentUserFromServerAsync();
            _isFullyAuthenticated = await ResolveAuthenticationStateAsync();
            
            // Single check - no loops
            if (_isFullyAuthenticated)
            {
                if (EnableSessionTimer)
                {
                    _sessionTimerService.StartTimer();
                }
                RefreshActiveUserSessions();
                await LoadPendingIntegrationEventCountAsync();
                NavigateToDefaultPageIfAuthenticatedOnRestrictedPage();
                _logger.LogDebug("NavMenu initialized with session timer for user: {UserCode}", _currentUserService.UserCode);
            }

            if (_isFullyAuthenticated)
            {
                _notificationPollingTask ??= PollPendingIntegrationEventCountAsync(_cancellationTokenSource.Token);
            }
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
            var wasFullyAuthenticated = _isFullyAuthenticated;

            if (EnableActivityTracking)
            {
                await RegisterActivityTrackingAsync();
            }

            if (!_isFullyAuthenticated)
            {
                await RefreshAuthenticationStateAsync();

                // No retry loop on first render; auth resolves from principal/session when available.
            }

            if (_isFullyAuthenticated)
            {
                if (EnableSessionTimer)
                {
                    _sessionTimerService.StartTimer();
                }
                RefreshActiveUserSessions();
                await LoadPendingIntegrationEventCountAsync();
                NavigateToDefaultPageIfAuthenticatedOnRestrictedPage();
            }

            if (!wasFullyAuthenticated && _isFullyAuthenticated)
            {
                await InvokeAsync(StateHasChanged);
            }

            if (_isFullyAuthenticated)
            {
                _notificationPollingTask ??= PollPendingIntegrationEventCountAsync(_cancellationTokenSource.Token);
            }
        }
    }

    [JSInvokable]
    public void ReportClientActivity()
    {
        if (!EnableActivityTracking)
        {
            return;
        }

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
        _ = InvokeAsync(async () =>
        {
            _isNavigationInProgress = true;
            await HydrateCurrentUserFromPrincipalAsync();
            await HydrateCurrentUserFromServerAsync();

            await RefreshAuthenticationStateAsync();

            RefreshActiveUserSessions();
            _ = LoadPendingIntegrationEventCountAsync();
            _isNavigationInProgress = false;
            StateHasChanged();
        });
    }

    private ValueTask OnLocationChangingAsync(Microsoft.AspNetCore.Components.Routing.LocationChangingContext context)
    {
        _isNavigationInProgress = true;
        return ValueTask.CompletedTask;
    }

    protected override async Task OnParametersSetAsync()
    {
        if (_isFullyAuthenticated)
        {
            await RefreshAuthenticationStateAsync();
        }

        // Update session activity when parameters change - only for fully authenticated users
        if (_isFullyAuthenticated)
        {
            if (EnableSessionTimer)
            {
                _sessionTimerService.UpdateActivity();
            }
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
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));

            while (await timer.WaitForNextTickAsync())
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                if (!_isFullyAuthenticated)
                {
                    continue;
                }

                RefreshActiveUserSessions();
                if (_isFullyAuthenticated)
                {
                    if (!IsCurrentSessionStillActive())
                    {
                        await HandleRemoteLogoutDetectedAsync();
                        continue;
                    }

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
        if (_isFullyAuthenticated && EnableSessionTimer)
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
            _isLogoutInitiated = true;
            
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
                var requiresLockReleaseConfirmation = await RequiresLockReleaseConfirmationAsync(value);

                if (requiresLockReleaseConfirmation
                    && !await ConfirmLockReleaseBeforeNavigationAsync())
                {
                    return;
                }

                if (requiresLockReleaseConfirmation)
                {
                    await ReleaseCurrentUserLocksForNavigationAsync();
                }

                if (!string.IsNullOrEmpty(value) && value.StartsWith("/"))
                {
                    _logger.LogInformation("Navigating to secure URL: {Url}", value);
                    _navigation.NavigateToSecure(value);
                }
                else if (Uri.TryCreate(value, UriKind.Absolute, out var absoluteUri)
                         && (absoluteUri.Scheme == Uri.UriSchemeHttps || absoluteUri.Scheme == Uri.UriSchemeHttp))
                {
                    _logger.LogInformation("Navigating to external URL: {Url}", value);
                    _navigation.NavigateTo(value, forceLoad: false);
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

    private Task<bool> RequiresLockReleaseConfirmationAsync(string? targetValue)
    {
        if (string.IsNullOrWhiteSpace(targetValue) || !_isFullyAuthenticated)
        {
            return Task.FromResult(false);
        }

        var currentUri = _navigation.ToAbsoluteUri(_navigation.Uri);
        var hasOwnedLocks = _recordLockLeaseCoordinator.GetOwnedLockKeys().Any();

        if (!hasOwnedLocks)
        {
            return Task.FromResult(false);
        }

        if (targetValue.StartsWith("/"))
        {
            var targetUri = _navigation.ToAbsoluteUri(targetValue);
            var isSamePath = string.Equals(currentUri.AbsolutePath, targetUri.AbsolutePath, StringComparison.OrdinalIgnoreCase);
            return Task.FromResult(!isSamePath);
        }

        if (Uri.TryCreate(targetValue, UriKind.Absolute, out var absoluteUri))
        {
            var isSameAbsoluteUri = string.Equals(currentUri.AbsoluteUri, absoluteUri.AbsoluteUri, StringComparison.OrdinalIgnoreCase);
            return Task.FromResult(!isSameAbsoluteUri);
        }

        return Task.FromResult(false);
    }

    private async Task<bool> ConfirmLockReleaseBeforeNavigationAsync()
    {
        var result = await _dialogService.Confirm(
            "You currently have an active record lock. If you continue, the lock will be released. Continue navigation?",
            "Release Record Lock",
            new ConfirmOptions
            {
                OkButtonText = "Yes, Continue",
                CancelButtonText = "Stay Here"
            });

        return result == true;
    }

    private async Task ReleaseCurrentUserLocksForNavigationAsync()
    {
        await _recordLockLeaseCoordinator.ReleaseOwnedLocksAsync(
            _currentUserService?.UserCode,
            async (lockKey, isLocked, action) =>
            {
                RecordLockStateStore.SetReleased(lockKey);

                await EventBusDispatcher.DispatchNotificationAsync(
                    NotificationSeverity.Info,
                    LiveUpdateEventNames.RecordEditReleased,
                    LiveUpdateRecordLockMessage.Build(lockKey, action, _currentUserService?.UserCode),
                    3000);
            },
            "NavigationAway");
    }

    private async Task OnLogoutClick()
    {
        _isLogoutInitiated = true;
        _hasRegisteredCurrentSession = false;
        // Stop session timer before logout
        _sessionTimerService.StopTimer();
        _isFullyAuthenticated = false;
        RemoveCurrentUserSession();
        
        await OnLogoutRequested.InvokeAsync();
        _cancellationTokenSource?.Cancel();
    }

    private void RegisterCurrentUserActivity()
    {
        if (!_isFullyAuthenticated)
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
        _hasRegisteredCurrentSession = true;
    }

    private void RemoveCurrentUserSession()
    {
        if (!_isFullyAuthenticated)
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
            _activeRecordLocks = [];
            return;
        }

        _activeUserSessions = _activeUserSessionRegistry
            .GetActiveSessions()
            .OrderBy(x => x.DisplayName)
            .ThenBy(x => x.UserCode)
            .Take(50)
            .ToList();

        _activeRecordLocks = RecordLockStateStore
            .GetActiveLocks()
            .OrderByDescending(x => x.LastHeartbeatUtc)
            .Take(50)
            .ToList();
    }

    private string GetLockStatusText(RecordLockInfo lockInfo)
    {
        var expiresAtUtc = lockInfo.LastHeartbeatUtc.Add(RecordLockStateStore.LockStaleThreshold);
        var remaining = expiresAtUtc - DateTime.UtcNow;

        if (remaining <= TimeSpan.Zero)
        {
            return "Stale";
        }

        return $"Stale in {Math.Ceiling(remaining.TotalSeconds):0}s";
    }

    private bool IsCurrentSessionStillActive()
    {
        if (!_isFullyAuthenticated)
        {
            return false;
        }

        if (!_hasRegisteredCurrentSession)
        {
            return true;
        }

        var sessionId = _httpContextAccessor.HttpContext?.Features.Get<Microsoft.AspNetCore.Http.Features.ISessionFeature>()?.Session?.Id;
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            // Session key is not reliable in this phase; avoid false remote-logout detection.
            return true;
        }

        var currentUserCode = _currentUserService?.UserCode ?? string.Empty;
        var activeSessions = _activeUserSessionRegistry
            .GetActiveSessions()
            .Where(x => string.Equals(x.UserCode, currentUserCode, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (activeSessions.Count == 0)
        {
            return false;
        }

        return activeSessions.Count > 0;
    }

    private async Task HandleRemoteLogoutDetectedAsync()
    {
        _logger.LogInformation("NavMenu detected remote logout for user {UserCode}; redirecting to login.", _currentUserService?.UserCode);

        _isLogoutInitiated = true;
        _hasRegisteredCurrentSession = false;
        _isFullyAuthenticated = false;
        _pendingIntegrationEventCount = 0;
        _activeUserSessions = [];
        _sessionTimerService.StopTimer();

        await InvokeAsync(StateHasChanged);
        try
        {
            _navigation.NavigateTo("/pdx", forceLoad: true);
        }
        catch (NavigationException ex)
        {
            _logger.LogDebug(ex, "Navigation exception while handling remote logout for user {UserCode}.", _currentUserService?.UserCode);
        }
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

        if (!string.IsNullOrWhiteSpace(sessionId))
        {
            return $"session:{sessionId}";
        }

        if (context is not null)
        {
            try
            {
                sessionId = context.Session.Id;
                if (!string.IsNullOrWhiteSpace(sessionId))
                {
                    return $"session:{sessionId}";
                }
            }
            catch (InvalidOperationException)
            {
                // Session unavailable in this phase; continue to deterministic per-circuit fallback.
            }
        }

        if (!string.IsNullOrWhiteSpace(connectionId))
        {
            return $"conn:{connectionId}";
        }

        return _navFallbackSessionKey;
    }

    /// <summary>
    /// Navigate to About page using secure navigation
    /// </summary>
    private void NavigateToAbout()
    {
        // Update session activity - only for fully authenticated users
        if (_isFullyAuthenticated)
        {
            if (EnableSessionTimer)
            {
                _sessionTimerService.UpdateActivity();
            }
        }

        _logger.LogInformation("Navigating to About page via secure navigation");
        _navigation.NavigateToSecure("/About");
    }

    
    private async Task ShowUserProfileModal()
    {
        if (!_isFullyAuthenticated) return;

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
        if (EnableSessionTimer)
        {
            _sessionTimerService?.StopTimer();
        }
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _dotNetRef?.Dispose();
    }

    // Hide all menu items on anonymous entry pages.
    private bool IsOnRestrictedPage => _navigation.Uri.Contains("/pdx")
        || _navigation.Uri.Contains("/login")
        || IsObfuscatedLogin();

    // Fundamentally stable gate: on protected routes, render nav when either full session auth
    // is ready OR the cookie principal is already authenticated for this request/circuit.
    private bool ShouldRenderAuthenticatedNav => _isFullyAuthenticated
        || _hasAuthenticatedCircuitPrincipal
        || HasAuthenticatedServerPrincipal();

    
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

    private async Task RefreshAuthenticationStateAsync()
    {
        await HydrateCurrentUserFromPrincipalAsync();
        await HydrateCurrentUserFromServerAsync();
        var resolvedState = await ResolveAuthenticationStateAsync();

        // Stabilize authenticated nav state: avoid dropping to anonymous due transient auth resolution misses.
        if (!resolvedState && _isFullyAuthenticated && !_isLogoutInitiated)
        {
            _logger.LogWarning("NavMenu auth resolve returned false while currently authenticated; keeping existing authenticated state for stability. Uri={Uri}", _navigation.Uri);
            return;
        }

        if (resolvedState == _isFullyAuthenticated)
        {
            LogAuthStateTransition(_isFullyAuthenticated);
            return;
        }

        _isFullyAuthenticated = resolvedState;

        if (_isFullyAuthenticated)
        {
            if (EnableSessionTimer)
            {
                _sessionTimerService.StartTimer();
            }

            _notificationPollingTask ??= PollPendingIntegrationEventCountAsync(_cancellationTokenSource.Token);

            RegisterCurrentUserActivity();
            RefreshActiveUserSessions();
            await LoadPendingIntegrationEventCountAsync();
            NavigateToDefaultPageIfAuthenticatedOnRestrictedPage();
        }
        else
        {
            _pendingIntegrationEventCount = 0;
            _activeUserSessions = [];
        }

        LogAuthStateTransition(_isFullyAuthenticated);
    }

    private void NavigateToDefaultPageIfAuthenticatedOnRestrictedPage()
    {
        if (!_isFullyAuthenticated || _isLogoutInitiated || !IsOnRestrictedPage)
        {
            return;
        }

        // Login flow now performs server-side redirect. Avoid issuing a second client-side navigation
        // from the current circuit while it may be disposing.
        _logger.LogDebug("Skipping NavMenu restricted-page auto-redirect; login completion is handled server-side.");
    }

    private async Task<bool> ResolveAuthenticationStateAsync()
    {
        if (_currentUserService?.IsFullyAuthenticated == true)
        {
            _lastAuthenticationSuccessUtc = DateTime.UtcNow;
            return true;
        }

        _hasAuthenticatedCircuitPrincipal = await HasAuthenticatedCircuitPrincipalAsync();

        if (_hasAuthenticatedCircuitPrincipal && HasServerBackedAuthSnapshot())
        {
            _lastAuthenticationSuccessUtc = DateTime.UtcNow;
            return true;
        }

        return false;
    }

    private async Task HydrateCurrentUserFromPrincipalAsync()
    {
        if (_currentUserService is not SessionCurrentUserService sessionCurrentUserService)
        {
            return;
        }

        AuthenticationState authState;
        try
        {
            authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
        }
        catch
        {
            return;
        }

        var user = authState.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var payload = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["IsAuthenticated"] = "true"
        };

        static void AddIfNotEmpty(Dictionary<string, string> target, string key, string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                target[key] = value;
            }
        }

        AddIfNotEmpty(payload, "SMS_UserCode", user.FindFirstValue("SMS_UserCode") ?? user.FindFirstValue(ClaimTypes.NameIdentifier));
        AddIfNotEmpty(payload, "SMS_UserType", user.FindFirstValue("SMS_UserType"));
        AddIfNotEmpty(payload, "SMS_DisplayName", user.FindFirstValue(ClaimTypes.Name));
        AddIfNotEmpty(payload, "SMS_UserRoleCode", user.FindFirstValue("SMS_UserRoleCode"));
        AddIfNotEmpty(payload, "SMS_UserRoleName", user.FindFirstValue("SMS_UserRoleName"));
        AddIfNotEmpty(payload, "SMS_UserPermissions", user.FindFirstValue("SMS_UserPermissions"));

        sessionCurrentUserService.HydrateFromAuthPayload(payload);
    }

    private async Task HydrateCurrentUserFromServerAsync()
    {
        if (_currentUserService is not SessionCurrentUserService sessionCurrentUserService)
        {
            return;
        }

        if (_isServerAuthHydrationInProgress)
        {
            return;
        }

        if (!HasAuthenticatedServerPrincipal() || HasServerBackedAuthSnapshot())
        {
            return;
        }

        var now = DateTime.UtcNow;
        if (now - _lastServerAuthHydrationAttemptUtc < ServerAuthHydrationRetryInterval)
        {
            return;
        }

        _lastServerAuthHydrationAttemptUtc = now;
        _isServerAuthHydrationInProgress = true;

        try
        {
            var client = _httpClientFactory.CreateClient();
            var uri = _navigation.ToAbsoluteUri("/auth/current-user");
            using var response = await client.GetAsync(uri);
            if (!response.IsSuccessStatusCode)
            {
                return;
            }

            await using var stream = await response.Content.ReadAsStreamAsync();
            using var json = await JsonDocument.ParseAsync(stream);
            var root = json.RootElement;

            if (!root.TryGetProperty("isAuthenticated", out var isAuthenticatedElement)
                || isAuthenticatedElement.ValueKind != JsonValueKind.True)
            {
                return;
            }

            if (!root.TryGetProperty("authValues", out var authValuesElement)
                || authValuesElement.ValueKind != JsonValueKind.Object)
            {
                return;
            }

            var payload = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var prop in authValuesElement.EnumerateObject())
            {
                if (prop.Value.ValueKind != JsonValueKind.String)
                {
                    continue;
                }

                var value = prop.Value.GetString();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    payload[prop.Name] = value;
                }
            }

            if (payload.Count > 0)
            {
                sessionCurrentUserService.HydrateFromAuthPayload(payload);
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "NavMenu could not hydrate auth snapshot from /auth/current-user.");
        }
        finally
        {
            _isServerAuthHydrationInProgress = false;
        }
    }

    private void LogAuthStateTransition(bool isAuthenticated)
    {
        if (_lastLoggedAuthState == isAuthenticated)
        {
            return;
        }

        _lastLoggedAuthState = isAuthenticated;
        _logger.LogInformation("NavMenu auth state transition: FullyAuth={IsFullyAuthenticated}, UserCode={UserCode}, Uri={Uri}",
            isAuthenticated,
            _currentUserService?.UserCode,
            _navigation.Uri);
    }

    private bool HasAuthenticatedServerPrincipal()
    {
        if (_hasAuthenticatedCircuitPrincipal)
        {
            return true;
        }

        var contextPrincipal = _httpContextAccessor.HttpContext?.User;
        if (contextPrincipal?.Identity?.IsAuthenticated == true)
        {
            return true;
        }

        var threadPrincipal = Thread.CurrentPrincipal as ClaimsPrincipal;
        return threadPrincipal?.Identity?.IsAuthenticated == true;
    }

    private async Task<bool> HasAuthenticatedCircuitPrincipalAsync()
    {
        try
        {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            return authState.User?.Identity?.IsAuthenticated == true;
        }
        catch
        {
            return false;
        }
    }

    private bool HasServerBackedAuthSnapshot()
    {
        if (!string.IsNullOrWhiteSpace(_currentUserService?.UserRoleCode)
            && _currentUserService?.Permissions?.Count > 0)
        {
            return true;
        }

        return false;
    }

}


