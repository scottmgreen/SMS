using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using SMS_Application.Services;
using SMS_Application.Interfaces;
namespace SMS3.Components.Shared.Components;

public partial class TwoFactorTimeoutModal
{

[Parameter] public EventCallback OnStartOver { get; set; }
    [Parameter] public EventCallback OnDismiss { get; set; }
    
    private bool IsVisible { get; set; } = false;
    private Timer? _countdownTimer;
    private TimeSpan _warningTriggeredAt = TimeSpan.Zero;

    protected override void OnInitialized()
    {
        // Subscribe to 2FA timer service events
        TwoFactorTimer.OnWarningThreshold += HandleWarningThreshold;
        TwoFactorTimer.OnSessionExpired += HandleSessionExpired;
    }

    private async void HandleWarningThreshold()
    {
        var remaining = TwoFactorTimer.GetRemainingTime();
        
        // Show modal when 2 minutes or less remaining in 2FA verification
        if (remaining <= TimeSpan.FromMinutes(2) && remaining > TimeSpan.Zero)
        {
            _warningTriggeredAt = remaining;
            ShowModal();
        }
    }

    private async void HandleSessionExpired()
    {
        // If modal is showing and 2FA session expires, force redirect to login
        if (IsVisible)
        {
            Logger.LogWarning("🔐 2FA session expired while timeout modal was displayed");
            await InvokeAsync(async () => await ForceRedirectToLogin());
        }
    }

    private async void ShowModal()
    {
        if (!IsVisible)
        {
            await InvokeAsync(async () =>
            {
                IsVisible = true;
                Logger.LogInformation("🔐 2FA timeout modal displayed - {RemainingTime} remaining", 
                    TwoFactorTimer.GetRemainingTime());
                
                StateHasChanged();
            });
            
            // Start countdown timer for real-time updates
            _countdownTimer = new Timer(async _ => 
            {
                await InvokeAsync(() =>
                {
                    var remaining = TwoFactorTimer.GetRemainingTime();
                    
                    // Auto-redirect if time runs out
                    if (remaining <= TimeSpan.Zero)
                    {
                        _ = Task.Run(async () => await ForceRedirectToLogin());
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

    private async Task HideModalInternal()
    {
        IsVisible = false;
        _countdownTimer?.Dispose();
        _countdownTimer = null;
        
        StateHasChanged();
    }

    private async Task OnDismissClick()
    {
        try
        {
            Logger.LogInformation("🔐 User dismissed 2FA timeout warning");
            
            // Hide modal
            await InvokeAsync(async () =>
            {
                await HideModalInternal();
            });
            
            // Trigger dismiss callback if provided
            if (OnDismiss.HasDelegate)
            {
                await OnDismiss.InvokeAsync();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "🔐 Error dismissing 2FA timeout modal");
        }
    }

    private async Task OnStartOverClick()
    {
        try
        {
            Logger.LogInformation("🔐 User chose to start over from 2FA timeout modal");
            await PerformStartOver();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "🔐 Error during start over from 2FA modal");
            await ForceRedirectToLogin();
        }
    }

    private async Task PerformStartOver()
    {
        // Hide modal first
        await InvokeAsync(async () =>
        {
            await HideModalInternal();
            TwoFactorTimer.StopTimer();
        });
        
        // Clear pending 2FA session
        await SessionService.ClearPending2FAUserAsync();
        
        // Trigger start over callback if provided
        if (OnStartOver.HasDelegate)
        {
            await OnStartOver.InvokeAsync();
        }
        else
        {
            // Default action - navigate to login
            Navigation.NavigateTo("/login", forceLoad: true);
        }
    }

    private async Task ForceRedirectToLogin()
    {
        try
        {
            Logger.LogWarning("🔐 Force redirect to login due to 2FA session expiry");
            
            await InvokeAsync(async () =>
            {
                await HideModalInternal();
                TwoFactorTimer.StopTimer();
            });
            
            // Clear pending 2FA session
            await SessionService.ClearPending2FAUserAsync();
            Navigation.NavigateTo("/login", forceLoad: true);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "🔐 Error during force redirect to login");
            // Last resort - reload page
            Navigation.NavigateTo("/login", forceLoad: true);
        }
    }

    private string FormatRemainingTime()
    {
        var remaining = TwoFactorTimer.GetRemainingTime();
        
        if (remaining <= TimeSpan.Zero)
            return "00:00";

        return $"{remaining.Minutes:D2}:{remaining.Seconds:D2}";
    }

    private string GetProgressPercentage()
    {
        var remaining = TwoFactorTimer.GetRemainingTime();
        
        if (_warningTriggeredAt <= TimeSpan.Zero || remaining <= TimeSpan.Zero)
            return "100";
        
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
        
        if (TwoFactorTimer != null)
        {
            TwoFactorTimer.OnWarningThreshold -= HandleWarningThreshold;
            TwoFactorTimer.OnSessionExpired -= HandleSessionExpired;
        }
    }
}


