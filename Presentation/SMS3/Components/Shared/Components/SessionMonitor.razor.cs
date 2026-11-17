using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.JSInterop;
using SMS_Application.Interfaces;
using SMS_Application.Services;

namespace SMS3.Components.Shared.Components;

public partial class SessionMonitor
{
    [Parameter] public bool ShowTimer { get; set; } = true;
    [Parameter] public EventCallback OnSessionExpired { get; set; }
    [Parameter] public EventCallback OnSessionWarning { get; set; }
    [Parameter] public EventCallback OnLogout { get; set; }
    [Parameter] public EventCallback OnContinue { get; set; }

    private System.Threading.Timer? _updateTimer;
    private bool _warningShown = false;
    private DateTime _lastWarningCheck = DateTime.MinValue;
    private bool IsVisible { get; set; } = false;
    private CancellationTokenSource? _countdownCts;
    private Task? _countdownTask;
    private TimeSpan _warningTriggeredAt = TimeSpan.Zero;

    protected override Task OnInitializedAsync()
    {
        if (_currentUserService.IsAuthenticated && IsSessionTimerEnabled())
        {
            SessionTimerSvc.OnTimeRemaining += HandleTimeRemaining;
            SessionTimerSvc.OnSessionExpired += HandleSessionExpired;
            SessionTimerSvc.OnWarningThreshold += HandleWarningThreshold;

            SessionTimerSvc.StartTimer();

            _updateTimer = new System.Threading.Timer(async _ =>
            {
                await InvokeAsync(() =>
                {
                    CheckForWarnings();
                    CheckModalState();
                    StateHasChanged();
                });
            }, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));

            Logger.LogDebug("Session monitor component initialized");
        }

        return Task.CompletedTask;
    }

    private bool IsSessionTimerEnabled()
    {
        return Configuration.GetValue<bool>("FeatureManagement:EnableSessionTimer", true);
    }

    private void CheckForWarnings()
    {
        var remaining = SessionTimerSvc.GetRemainingTime();
        var now = DateTime.Now;

        if (remaining <= TimeSpan.FromMinutes(5) && remaining > TimeSpan.Zero)
        {
            if (now - _lastWarningCheck > TimeSpan.FromSeconds(30))
            {
                _lastWarningCheck = now;
                if (OnSessionWarning.HasDelegate)
                {
                    _ = InvokeAsync(() => OnSessionWarning.InvokeAsync());
                }
            }
        }
    }

    private void CheckModalState()
    {
        var remaining = SessionTimerSvc.GetRemainingTime();

        if (remaining > TimeSpan.FromMinutes(2))
        {
            if (IsVisible)
            {
                _ = HideModalInternal();
            }

            return;
        }

        if (remaining <= TimeSpan.Zero)
        {
            return;
        }

        if (!IsVisible)
        {
            _warningTriggeredAt = remaining;
            ShowModal();
        }
    }

    private async void HandleTimeRemaining(TimeSpan remaining)
    {
        await InvokeAsync(() =>
        {
            CheckModalState();
            StateHasChanged();
        });
    }

    private async void HandleSessionExpired()
    {
        try
        {
            Logger.LogWarning("Session expired - initiating logout flow");

            await InvokeAsync(async () =>
            {
                await HideModalInternal();

                if (OnSessionExpired.HasDelegate)
                {
                    await OnSessionExpired.InvokeAsync();
                }
                else
                {
                    await ForceLogout();
                }
            });
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error handling session expiry");
            _navigation.NavigateTo("/pdx", forceLoad: true);
        }
    }

    private async void HandleWarningThreshold()
    {
        var remaining = SessionTimerSvc.GetRemainingTime();

        if (!_warningShown)
        {
            try
            {
                _warningShown = true;
                Logger.LogInformation("Session warning threshold reached for user");

                if (OnSessionWarning.HasDelegate)
                {
                    await InvokeAsync(() => OnSessionWarning.InvokeAsync());
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error handling session warning");
            }
        }

        if (remaining <= TimeSpan.FromMinutes(2) && remaining > TimeSpan.Zero)
        {
            _warningTriggeredAt = remaining;
            ShowModal();
        }
    }

    private string GetIconClass()
    {
        var remaining = SessionTimerSvc.GetRemainingTime();

        if (remaining <= TimeSpan.Zero)
            return "timer-icon-critical";
        else if (remaining <= TimeSpan.FromMinutes(2))
            return "timer-icon-critical";
        else if (remaining <= TimeSpan.FromMinutes(5))
            return "timer-icon-warning";
        else
            return "timer-icon-normal";
    }

    private string GetTooltipText()
    {
        var remaining = SessionTimerSvc.GetRemainingTime();

        if (remaining <= TimeSpan.Zero)
            return "Session expired - please login again";
        else if (remaining <= TimeSpan.FromMinutes(2))
            return "Session expires very soon! Save your work immediately.";
        else if (remaining <= TimeSpan.FromMinutes(5))
            return "Session will expire soon - save your work";
        else
            return $"Session expires in {SessionTimerSvc.FormatRemainingTime()}";
    }

    private void ShowModal()
    {
        if (!IsVisible)
        {
            _ = InvokeAsync(async () =>
            {
                IsVisible = true;
                Logger.LogInformation("Session timeout modal displayed - {RemainingTime} remaining",
                    SessionTimerSvc.GetRemainingTime());

                try
                {
                    await JSRuntime.InvokeVoidAsync("document.body.classList.add", "modal-open");
                }
                catch
                {
                }

                StateHasChanged();
            });

            StartSynchronizedCountdown();
        }
    }

    private void StartSynchronizedCountdown()
    {
        _countdownCts?.Cancel();
        _countdownCts?.Dispose();
        _countdownCts = new CancellationTokenSource();

        _countdownTask = RunSynchronizedCountdownAsync(_countdownCts.Token);
    }

    private async Task RunSynchronizedCountdownAsync(CancellationToken cancellationToken)
    {
        try
        {
            var initialDelayMs = 1000 - (DateTime.Now.Millisecond % 1000);
            if (initialDelayMs < 1000)
            {
                await Task.Delay(initialDelayMs);
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
            while (await timer.WaitForNextTickAsync())
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }

                await InvokeAsync(() =>
                {
                    var remaining = SessionTimerSvc.GetRemainingTime();

                    if (remaining > TimeSpan.FromMinutes(2))
                    {
                        _ = HideModalInternal();
                        return;
                    }

                    if (remaining <= TimeSpan.Zero)
                    {
                        _ = InvokeAsync(ForceLogout);
                        return;
                    }

                    StateHasChanged();
                });
            }
        }
        catch (TaskCanceledException)
        {
        }
        catch (OperationCanceledException)
        {
        }
        catch (ObjectDisposedException)
        {
        }
        catch (Exception ex)
        {
            Logger.LogDebug(ex, "Session timeout countdown loop ended unexpectedly.");
        }
    }

    private async Task OnContinueSession()
    {
        try
        {
            Logger.LogInformation("User chose to continue session");

            SessionTimerSvc.UpdateActivity();

            await InvokeAsync(async () =>
            {
                await HideModalInternal();
            });

            if (OnContinue.HasDelegate)
            {
                await OnContinue.InvokeAsync();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error continuing session");
        }
    }

    private async Task OnLogoutClick()
    {
        try
        {
            Logger.LogInformation("User chose to logout from session timeout modal");
            await PerformLogout();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during logout from modal");
            await ForceLogout();
        }
    }

    private async Task PerformLogout()
    {
        await InvokeAsync(async () =>
        {
            await HideModalInternal();
            SessionTimerSvc.StopTimer();
        });

        await ExecuteLogoutAsync();
    }

    private async Task ForceLogout()
    {
        try
        {
            Logger.LogWarning("Force logout due to session expiry");

            await InvokeAsync(async () =>
            {
                await HideModalInternal();
                SessionTimerSvc.StopTimer();
            });

            await ExecuteLogoutAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during force logout");
            _navigation.NavigateTo("/pdx", forceLoad: true);
        }
    }

    private async Task ExecuteLogoutAsync()
    {
        await InvokeAsync(async () =>
        {
            if (OnLogout.HasDelegate)
            {
                await OnLogout.InvokeAsync();
                return;
            }

            _navigation.NavigateTo("/pdx", forceLoad: true);
        });
    }

    private async Task HideModalInternal()
    {
        IsVisible = false;
        _countdownCts?.Cancel();
        _countdownCts?.Dispose();
        _countdownCts = null;

        try
        {
            await JSRuntime.InvokeVoidAsync("document.body.classList.remove", "modal-open");
        }
        catch
        {
        }

        StateHasChanged();
    }

    private string FormatRemainingTime()
    {
        var remaining = SessionTimerSvc.GetRemainingTime();

        if (remaining <= TimeSpan.Zero)
            return "00:00";

        return $"{remaining.Minutes:D2}:{remaining.Seconds:D2}";
    }

    private string GetProgressPercentage()
    {
        var remaining = SessionTimerSvc.GetRemainingTime();

        if (_warningTriggeredAt <= TimeSpan.Zero || remaining <= TimeSpan.Zero)
            return "0";

        var totalWarningTime = _warningTriggeredAt.TotalSeconds;
        var remainingTime = remaining.TotalSeconds;
        var elapsed = totalWarningTime - remainingTime;
        var percentage = (elapsed / totalWarningTime) * 100;

        return Math.Max(0, Math.Min(100, percentage)).ToString("F0");
    }

    public void Dispose()
    {
        _countdownCts?.Cancel();
        _countdownCts?.Dispose();
        _countdownTask = null;

        if (SessionTimerSvc != null)
        {
            SessionTimerSvc.OnTimeRemaining -= HandleTimeRemaining;
            SessionTimerSvc.OnSessionExpired -= HandleSessionExpired;
            SessionTimerSvc.OnWarningThreshold -= HandleWarningThreshold;
        }

        _updateTimer?.Dispose();
    }
}
