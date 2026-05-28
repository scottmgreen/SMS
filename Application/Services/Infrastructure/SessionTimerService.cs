//-----------------------------------------------------------------------
// <copyright file="SessionTimerService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Service for managing session timeout tracking and notifications in Blazor Server applications.
//                  Provides real-time session timeout countdown and automatic logout functionality.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using SMS_Application.Configuration;
using SMS_Application.Interfaces;

namespace SMS_Application.Services;

/// <summary>
/// Service for managing session timeout tracking and user notifications
/// Provides real-time countdown and automatic logout when session expires
/// </summary>
public class SessionTimerService : IDisposable
{
    private readonly ILogger<SessionTimerService> _logger;
    private readonly SessionConfiguration _sessionConfig;
    private readonly ICurrentUserService _currentUserService;
    private readonly ISMSSessionService _sessionService;
    
    private Timer? _timer;
    private DateTime _lastActivity;
    private bool _isActive;
    
    public event Action<TimeSpan>? OnTimeRemaining;
    public event Action? OnSessionExpired;
    public event Action? OnWarningThreshold;

    public SessionTimerService(
        ILogger<SessionTimerService> logger,
        SessionConfiguration sessionConfig,
        ICurrentUserService currentUserService,
        ISMSSessionService sessionService)
    {
        _logger = logger;
        _sessionConfig = sessionConfig;
        _currentUserService = currentUserService;
        _sessionService = sessionService;
    }

    /// <summary>
    /// Start the session timer when user logs in
    /// </summary>
    public void StartTimer()
    {
        if (!_currentUserService.IsAuthenticated)
        {
            _logger.LogApplicationDebug("Cannot start timer - user not authenticated", ApplicationEventIds.Debug);
            return;
        }

        _lastActivity = DateTime.UtcNow;
        _isActive = true;

        // Create timer that checks every 30 seconds
        _timer = new Timer(CheckSessionTimeout, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
        
        _logger.LogApplicationDebug("Session timer started - timeout in {TimeoutMinutes} minutes",
            ApplicationEventIds.Debug,
            _sessionConfig.TimeoutMinutes);
    }

    /// <summary>
    /// Update last activity time (call on user interactions)
    /// </summary>
    public void UpdateActivity()
    {
        if (_isActive && _currentUserService.IsAuthenticated)
        {
            _lastActivity = DateTime.UtcNow;
            _logger.LogApplicationTrace("Session activity updated", ApplicationEventIds.Trace);
        }
    }

    /// <summary>
    /// Stop the timer (call on logout)
    /// </summary>
    public void StopTimer()
    {
        _isActive = false;
        _timer?.Dispose();
        _timer = null;
        _logger.LogApplicationDebug("Session timer stopped", ApplicationEventIds.Debug);
    }

    /// <summary>
    /// Get remaining time until session expires
    /// </summary>
    public TimeSpan GetRemainingTime()
    {
        if (!_isActive || !_currentUserService.IsAuthenticated)
            return TimeSpan.Zero;

        var elapsed = DateTime.UtcNow - _lastActivity;
        var remaining = _sessionConfig.IdleTimeout - elapsed;
        
        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }

    /// <summary>
    /// Check if session is about to expire (within 2 minutes)
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
            return "session-expired";
        else if (remaining <= TimeSpan.FromMinutes(1))
            return "session-critical";
        else if (remaining <= TimeSpan.FromMinutes(2))
            return "session-warning";
        else
            return "session-normal";
    }

    private void CheckSessionTimeout(object? state)
    {
        try
        {
            if (!_isActive || !_currentUserService.IsAuthenticated)
                return;

            var remaining = GetRemainingTime();
            
            // Notify subscribers of remaining time
            OnTimeRemaining?.Invoke(remaining);

            // Check for warning threshold (2 minutes)
            if (remaining <= TimeSpan.FromMinutes(2) && remaining > TimeSpan.FromMinutes(1.5))
            {
                _logger.LogApplicationWarning("Session approaching timeout for user {UserId} - {RemainingMinutes} minutes remaining",
                    ApplicationEventIds.Warning,
                    _currentUserService.UserCode, remaining.TotalMinutes);
                OnWarningThreshold?.Invoke();
            }

            // Check for session expiry
            if (remaining <= TimeSpan.Zero)
            {
                _logger.LogApplicationWarning("Session expired for user {UserId}",
                    ApplicationEventIds.Warning,
                    _currentUserService.UserCode);
                _isActive = false;
                OnSessionExpired?.Invoke();
            }
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Error in session timeout check", ApplicationEventIds.Error, ex);
        }
    }

    public void Dispose()
    {
        StopTimer();
    }
}