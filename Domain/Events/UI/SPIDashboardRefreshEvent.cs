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
using SMS_Domain.Events;

namespace SMS_Domain.Events.UI;

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
        RefreshMetadata = refreshMetadata ?? new Dictionary<string, object>();
        UserId = userId;
        UserRoles = userRoles ?? new List<string>();
    }
}

/// <summary>
/// SPI dashboard sections that can be refreshed
/// </summary>
public enum SPIDashboardSection
{
    Overview = 1,
    ThresholdMonitoring = 2,
    ComplianceStatus = 3,
    TrendAnalysis = 4,
    Alerts = 5,
    RecentActivity = 6
}