//-----------------------------------------------------------------------
// <copyright file="TwoFactorSessionTimerService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Service for managing 2FA session timeout tracking during verification phase.
//                  Provides countdown and automatic redirect when 2FA session expires.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using SMS_Application.Configuration;
using SMS_Application.Interfaces;

namespace SMS_Application.Services;

/// <summary>
/// Service for managing 2FA session timeout tracking during verification
/// Provides real-time countdown and automatic redirect when 2FA session expires
/// </summary>
public class TwoFactorSessionTimerService : IDisposable
{
    private readonly ILogger<TwoFactorSessionTimerService> _logger;
    private readonly TwoFactorAuthConfiguration _twoFactorConfig;
    private readonly ISMSSessionService _sessionService;
    
    private Timer? _timer;
    private DateTime _startTime;
    private bool _isActive;
    
    public event Action<TimeSpan>? OnTimeRemaining;
    public event Action? OnSessionExpired;
    public event Action? OnWarningThreshold;

    public TwoFactorSessionTimerService(
        ILogger<TwoFactorSessionTimerService> logger,
        TwoFactorAuthConfiguration twoFactorConfig,
        ISMSSessionService sessionService)
    {
        _logger = logger;
        _twoFactorConfig = twoFactorConfig;
        _sessionService = sessionService;
    }

    /// <summary>
    /// Start the 2FA timer when user enters 2FA verification phase
    /// </summary>
    public void StartTimer()
    {
        if (!_sessionService.HasPending2FAUser())
        {
            _logger.LogDebug("Cannot start 2FA timer - no pending 2FA user");
            return;
        }

        _startTime = DateTime.UtcNow;
        _isActive = true;

        // Create timer that checks every 10 seconds during 2FA
        _timer = new Timer(CheckSessionTimeout, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
        
        _logger.LogDebug("?? 2FA session timer started - timeout in {TimeoutMinutes} minutes", _twoFactorConfig.TwoFASessionTimeoutMinutes);
    }

    /// <summary>
    /// Stop the timer (call when 2FA is completed or cancelled)
    /// </summary>
    public void StopTimer()
    {
        _isActive = false;
        _timer?.Dispose();
        _timer = null;
        _logger.LogDebug("?? 2FA session timer stopped");
    }

    /// <summary>
    /// Get remaining time until 2FA session expires
    /// </summary>
    public TimeSpan GetRemainingTime()
    {
        if (!_isActive)
            return TimeSpan.Zero;

        var elapsed = DateTime.UtcNow - _startTime;
        var remaining = _twoFactorConfig.TwoFASessionTimeout - elapsed;
        
        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }

    /// <summary>
    /// Check if 2FA session is about to expire (within 2 minutes)
    /// </summary>
    public bool IsNearExpiry()
    {
        var remaining = GetRemainingTime();
        return remaining <= TimeSpan.FromMinutes(2) && remaining > TimeSpan.Zero;
    }

    /// <summary>
    /// Format remaining time for display
    /// </summary>
    public string FormatRemainingTime()
    {
        var remaining = GetRemainingTime();
        
        if (remaining <= TimeSpan.Zero)
            return "Expired";

        if (remaining.TotalHours >= 1)
            return $"{remaining.Hours:D2}:{remaining.Minutes:D2}:{remaining.Seconds:D2}";
        else
            return $"{remaining.Minutes:D2}:{remaining.Seconds:D2}";
    }

    /// <summary>
    /// Get CSS class for timer display based on remaining time
    /// </summary>
    public string GetTimerCssClass()
    {
        var remaining = GetRemainingTime();
        
        if (remaining <= TimeSpan.Zero)
            return "twofa-expired";
        else if (remaining <= TimeSpan.FromMinutes(1))
            return "twofa-critical";
        else if (remaining <= TimeSpan.FromMinutes(2))
            return "twofa-warning";
        else
            return "twofa-normal";
    }

    /// <summary>
    /// Check if user is currently in 2FA verification phase
    /// </summary>
    public bool IsIn2FAVerification()
    {
        return _sessionService.HasPending2FAUser() && _isActive;
    }

    private void CheckSessionTimeout(object? state)
    {
        try
        {
            if (!_isActive)
                return;

            // Double-check if we still have a pending 2FA user
            if (!_sessionService.HasPending2FAUser())
            {
                _logger.LogInformation("?? No pending 2FA user found - stopping 2FA timer");
                StopTimer();
                return;
            }

            var remaining = GetRemainingTime();
            
            // Notify subscribers of remaining time
            OnTimeRemaining?.Invoke(remaining);

            // Check for warning threshold (2 minutes remaining)
            if (remaining <= TimeSpan.FromMinutes(2) && remaining > TimeSpan.FromMinutes(1.5))
            {
                _logger.LogWarning("?? 2FA session approaching timeout - {RemainingMinutes} minutes remaining", remaining.TotalMinutes);
                OnWarningThreshold?.Invoke();
            }

            // Check for session expiry
            if (remaining <= TimeSpan.Zero)
            {
                _logger.LogWarning("?? 2FA session expired - {TimeoutMinutes} minutes elapsed", _twoFactorConfig.TwoFASessionTimeoutMinutes);
                _isActive = false;
                OnSessionExpired?.Invoke();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "?? Error in 2FA session timeout check");
        }
    }

    public void Dispose()
    {
        StopTimer();
    }
}