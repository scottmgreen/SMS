//-----------------------------------------------------------------------
// <copyright file="SPIDashboardRefreshEvent.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: UI event for triggering SPI dashboard refreshes in the SMS Blazor application.
//                  Supports real-time dashboard updates when SPI data changes, threshold violations occur,
//                  or compliance status changes require immediate user interface updates.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Interfaces;

namespace SMS_Domain.Events;

/// <summary>
/// UI event for triggering SPI dashboard refreshes
/// Ensures real-time updates when SPI data changes
/// </summary>
public class SPIDashboardRefreshEvent : IUIEvent
{
    public Guid EventId { get; private set; }
    public DateTime OccurredOn { get; private set; }
    public string EventType => "UI.SPIDashboard.Refresh";
    public string TargetComponent { get; private set; }
    public UIEventPriority Priority { get; private set; }

    // SPI-specific refresh data
    public List<string> AffectedSPICodes { get; private set; }
    public string RefreshReason { get; private set; }
    public SPIDashboardSection DashboardSection { get; private set; }
    public bool RefreshEntireDashboard { get; private set; }
    public Dictionary<string, object> RefreshMetadata { get; private set; }

    // User context
    public string? UserId { get; private set; }
    public List<string> UserRoles { get; private set; }

    public SPIDashboardRefreshEvent(
        List<string> affectedSPICodes,
        string refreshReason,
        SPIDashboardSection dashboardSection = SPIDashboardSection.Overview,
        bool refreshEntireDashboard = false,
        string? userId = null,
        List<string>? userRoles = null,
        UIEventPriority priority = UIEventPriority.Normal,
        Dictionary<string, object>? refreshMetadata = null)
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        TargetComponent = "SPIDashboard";
        Priority = priority;

        AffectedSPICodes = affectedSPICodes ?? new List<string>();
        RefreshReason = refreshReason ?? throw new ArgumentNullException(nameof(refreshReason));
        DashboardSection = dashboardSection;
        RefreshEntireDashboard = refreshEntireDashboard;
        UserId = userId;
        UserRoles = userRoles ?? new List<string>();
        RefreshMetadata = refreshMetadata ?? new Dictionary<string, object>();
    }
}

/// <summary>
/// SPI dashboard sections for targeted refreshes
/// </summary>
public enum SPIDashboardSection
{
    Overview = 0,
    Thresholds = 1,
    Trends = 2,
    Compliance = 3,
    Alerts = 4,
    All = 5
}