// <copyright file="UINotificationEvent.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: UI event for displaying popup notifications via INotificationHelper.
//                  Specifically designed for toast/popup notifications that replace direct
//                  NotificationService calls throughout the application.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Events.UIEvents;

/// <summary>
/// UI event for displaying popup/toast notifications
/// Replaces direct NotificationService usage with event-driven approach
/// Supports success, error, warning, and info notifications
/// </summary>
public class UINotificationEvent : IBaseUIEvent
{
    public Guid EventId { get; private set; }
    public DateTime OccurredOn { get; private set; }
    public string EventType => SMS_Domain.Enums.EventType.UINotification.Value;
    public string ReportId { get; private set; }
    public string TargetComponent { get; private set; }
    public UIEventPriority Priority { get; private set; }

    // Notification Content
    public UINotificationSeverity Severity { get; private set; }
    public string Title { get; private set; }
    public string Message { get; private set; }
    public int Duration { get; private set; }

    // Context & Categorization
    public string Category { get; private set; }
    public string? SourceLayer { get; private set; }
    public Dictionary<string, object>? Metadata { get; private set; }

    public UINotificationEvent(
        UINotificationSeverity severity,
        string title,
        string message,
        int duration = 5000,
        string category = "UserAction",
        string? sourceLayer = null,
        string? reportId = null,
        string targetComponent = "NotificationCenter",
        UIEventPriority priority = UIEventPriority.Normal,
        Dictionary<string, object>? metadata = null)
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;

        Severity = severity;
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Message = message ?? throw new ArgumentNullException(nameof(message));
        Duration = duration > 0 ? duration : 5000;
        Category = category ?? "UserAction";
        SourceLayer = sourceLayer;
        ReportId = reportId ?? string.Empty;
        TargetComponent = targetComponent ?? "NotificationCenter";
        Priority = priority;
        Metadata = metadata ?? new Dictionary<string, object>();
    }

    /// <summary>
    /// Creates a success notification event
    /// </summary>
    public static UINotificationEvent Success(string title, string message, int duration = 4000, string category = "UserAction")
        => new(UINotificationSeverity.Success, title, message, duration, category, sourceLayer: "Presentation");

    /// <summary>
    /// Creates an error notification event
    /// </summary>
    public static UINotificationEvent Error(string title, string message, int duration = 6000, string category = "UserAction")
        => new(UINotificationSeverity.Error, title, message, duration, category, sourceLayer: "Application");

    /// <summary>
    /// Creates a warning notification event
    /// </summary>
    public static UINotificationEvent Warning(string title, string message, int duration = 5000, string category = "SystemEvent")
        => new(UINotificationSeverity.Warning, title, message, duration, category, sourceLayer: "Application");

    /// <summary>
    /// Creates an info notification event
    /// </summary>
    public static UINotificationEvent Info(string title, string message, int duration = 4000, string category = "SystemEvent")
        => new(UINotificationSeverity.Info, title, message, duration, category, sourceLayer: "Application");
}

/// <summary>
/// Notification severity levels matching Radzen NotificationSeverity
/// </summary>
public enum UINotificationSeverity
{
    Success,
    Info,
    Warning,
    Error
}