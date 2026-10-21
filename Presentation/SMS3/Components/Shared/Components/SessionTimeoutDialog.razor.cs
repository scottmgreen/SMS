using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SMS_Application.Services;
using SMS_Application.Interfaces;
namespace SMS3.Components.Shared.Components;

public partial class SessionTimeoutDialog
{

[Parameter] public EventCallback OnLogout { get; set; }
    [Parameter] public EventCallback OnContinue { get; set; }
    
    private bool IsVisible { get; set; } = false;
    private CancellationTokenSource? _countdownCts;
    private TimeSpan _warningTriggeredAt = TimeSpan.Zero;

    protected override void OnInitialized()
    {
        // Subscribe to session timer service events
        SessionTimerService.OnWarningThreshold += HandleWarningThreshold;
        SessionTimerService.OnSessionExpired += HandleSessionExpired;
    }

    private async void HandleWarningThreshold()
    {
        var remaining = SessionTimerService.GetRemainingTime();
        
        // Show modal when 2 minutes or less remaining
        if (remaining <= TimeSpan.FromMinutes(2) && remaining > TimeSpan.Zero)
        {
            _warningTriggeredAt = remaining;
            ShowModal();
        }
    }

    private async void HandleSessionExpired()
    {
        Logger.LogWarning("Session expired - forcing logout flow");
        await InvokeAsync(async () => await ForceLogout());
    }

    private async void ShowModal()
    {
        if (!IsVisible)
        {
            await InvokeAsync(async () =>
            {
                IsVisible = true;
                Logger.LogInformation("Session timeout modal displayed - {RemainingTime} remaining", 
                    SessionTimerService.GetRemainingTime());
                
                // Add modal-open class to body to prevent scrolling
                try
                {
                    await JSRuntime.InvokeVoidAsync("document.body.classList.add", "modal-open");
                }
                catch { /* Ignore JS errors */ }
                
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

        _ = RunSynchronizedCountdownAsync(_countdownCts.Token);
    }

    private async Task RunSynchronizedCountdownAsync(CancellationToken cancellationToken)
    {
        try
        {
            var initialDelayMs = 1000 - (DateTime.UtcNow.Millisecond % 1000);
            if (initialDelayMs < 1000)
            {
                await Task.Delay(initialDelayMs, cancellationToken);
            }

            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                await InvokeAsync(() =>
                {
                    var remaining = SessionTimerService.GetRemainingTime();

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
        catch (OperationCanceledException)
        {
            // Expected when modal closes or component disposes
        }
    }

    private async void HideModal()
    {
        await InvokeAsync(async () =>
        {
            await HideModalInternal();
        });
    }

    private async Task OnContinueSession()
    {
        try
        {
            Logger.LogInformation("User chose to continue session");
            
            // Update activity to extend session
            SessionTimerService.UpdateActivity();
            
            // Hide modal
            await InvokeAsync(async () =>
            {
                await HideModalInternal();
            });
            
            // Trigger continue callback if provided
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
        // Hide modal first
        await InvokeAsync(async () =>
        {
            await HideModalInternal();
            SessionTimerService.StopTimer();
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
                SessionTimerService.StopTimer();
            });

            await ExecuteLogoutAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during force logout");
            // Last resort - reload to PDX entry page
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
        
        // Remove modal-open class from body
        try
        {
            await JSRuntime.InvokeVoidAsync("document.body.classList.remove", "modal-open");
        }
        catch { /* Ignore JS errors */ }
        
        StateHasChanged();
    }

    private string FormatRemainingTime()
    {
        var remaining = SessionTimerService.GetRemainingTime();
        
        if (remaining <= TimeSpan.Zero)
            return "00:00";

        return $"{remaining.Minutes:D2}:{remaining.Seconds:D2}";
    }

    private string GetProgressPercentage()
    {
        var remaining = SessionTimerService.GetRemainingTime();
        
        if (_warningTriggeredAt <= TimeSpan.Zero || remaining <= TimeSpan.Zero)
            return "0";
        
        // Calculate percentage based on time elapsed since warning
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
        
        if (SessionTimerService != null)
        {
            SessionTimerService.OnWarningThreshold -= HandleWarningThreshold;
            SessionTimerService.OnSessionExpired -= HandleSessionExpired;
        }
    }
}


