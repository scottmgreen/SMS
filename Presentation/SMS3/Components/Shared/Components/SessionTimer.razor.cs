using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using SMS_Application.Interfaces;
using SMS_Application.Services;
namespace SMS3.Components.Shared.Components;

public partial class SessionTimer
{

[Parameter] public bool ShowTimer { get; set; } = true;
    [Parameter] public EventCallback OnSessionExpired { get; set; }
    [Parameter] public EventCallback OnSessionWarning { get; set; }
    
    private System.Threading.Timer? _updateTimer;
    private bool _warningShown = false;
    private DateTime _lastWarningCheck = DateTime.MinValue;
    
    protected override async Task OnInitializedAsync()
    {
        if (_currentUserService.IsAuthenticated && IsSessionTimerEnabled())
        {
            // Subscribe to session timer events
            SessionTimerSvc.OnTimeRemaining += HandleTimeRemaining;
            SessionTimerSvc.OnSessionExpired += HandleSessionExpired;
            SessionTimerSvc.OnWarningThreshold += HandleWarningThreshold;
            
            // Start the session timer
            SessionTimerSvc.StartTimer();
            
            // Create UI update timer (updates every second for smooth countdown)
            _updateTimer = new System.Threading.Timer(async _ => 
            {
                await InvokeAsync(() =>
                {
                    CheckForWarnings();
                    StateHasChanged();
                });
            }, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
                
            Logger.LogDebug("Session timer component initialized");
        }
    }

    private bool IsSessionTimerEnabled()
    {
        return Configuration.GetValue<bool>("FeatureManagement:EnableSessionTimer", true);
    }

    private void CheckForWarnings()
    {
        var remaining = SessionTimerSvc.GetRemainingTime();
        var now = DateTime.Now;
        
        // Show warning every 30 seconds when < 5 minutes remaining
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

    private async void HandleTimeRemaining(TimeSpan remaining)
    {
        await InvokeAsync(StateHasChanged);
    }

    private async void HandleSessionExpired()
    {
        try
        {
            Logger.LogWarning("Session expired - redirecting to login");
            
            // Trigger callback if provided
            if (OnSessionExpired.HasDelegate)
            {
                await InvokeAsync(() => OnSessionExpired.InvokeAsync());
            }
            else
            {
                // Default action - redirect to login
                Navigation.NavigateTo("/login", forceLoad: true);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error handling session expiry");
        }
    }

    private async void HandleWarningThreshold()
    {
        if (_warningShown) return;
        
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

    private bool IsNearExpiry()
    {
        var remaining = SessionTimerSvc.GetRemainingTime();
        return remaining <= TimeSpan.FromMinutes(5) && remaining > TimeSpan.Zero;
    }

    public void Dispose()
    {
        // Unsubscribe from events
        if (SessionTimerSvc != null)
        {
            SessionTimerSvc.OnTimeRemaining -= HandleTimeRemaining;
            SessionTimerSvc.OnSessionExpired -= HandleSessionExpired;
            SessionTimerSvc.OnWarningThreshold -= HandleWarningThreshold;
        }
        
        // Dispose update timer
        _updateTimer?.Dispose();
    }
}


