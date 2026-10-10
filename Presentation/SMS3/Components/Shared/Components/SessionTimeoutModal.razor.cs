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

public partial class SessionTimeoutModal
{

[Parameter] public EventCallback OnLogout { get; set; }
    [Parameter] public EventCallback OnContinue { get; set; }
    
    private bool IsVisible { get; set; } = false;
    private Timer? _countdownTimer;
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
            
            // Start countdown timer for real-time updates
            _countdownTimer = new Timer(async _ => 
            {
                await InvokeAsync(() =>
                {
                    var remaining = SessionTimerService.GetRemainingTime();

                    // If user activity reset the session, hide the warning modal.
                    if (remaining > TimeSpan.FromMinutes(2))
                    {
                        _ = HideModalInternal();
                        return;
                    }
                    
                    // Auto-logout if time runs out
                    if (remaining <= TimeSpan.Zero)
                    {
                        _ = Task.Run(async () => await ForceLogout());
                        return;
                    }
                    
                    StateHasChanged();
                });
            }, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
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
        if (OnLogout.HasDelegate)
        {
            await OnLogout.InvokeAsync();
            return;
        }

        _navigation.NavigateTo("/pdx", forceLoad: true);
    }

    private async Task HideModalInternal()
    {
        IsVisible = false;
        _countdownTimer?.Dispose();
        _countdownTimer = null;
        
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
        _countdownTimer?.Dispose();
        
        if (SessionTimerService != null)
        {
            SessionTimerService.OnWarningThreshold -= HandleWarningThreshold;
            SessionTimerService.OnSessionExpired -= HandleSessionExpired;
        }
    }
}


