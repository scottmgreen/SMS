//-----------------------------------------------------------------------
// <copyright file="UserNotificationEvent.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: UI event for displaying user notifications in the SMS Blazor application.
//                  Supports real-time user alerts, system messages, and workflow notifications
//                  that require immediate user attention or acknowledgment.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Interfaces;

namespace SMS_Domain.Events;

/// <summary>
/// UI event for displaying user notifications
/// Supports real-time alerts and system messages
/// </summary>
public class UserNotificationEvent : IUIEvent
{
    public Guid EventId { get; private set; }
    public DateTime OccurredOn { get; private set; }
    public string EventType => "UI.User.Notification";
    public string TargetComponent { get; private set; }
    public UIEventPriority Priority { get; private set; }

    // Notification content
    public string Title { get; private set; }
    public string Message { get; private set; }
    public NotificationType NotificationType { get; private set; }
    public string? ActionUrl { get; private set; }
    public string? ActionText { get; private set; }

    // User targeting
    public List<string> TargetUserIds { get; private set; }
    public List<string> TargetRoles { get; private set; }
    public bool IsGlobalNotification { get; private set; }

    // Behavior settings
    public TimeSpan? AutoDismissAfter { get; private set; }
    public bool RequiresAcknowledgment { get; private set; }
    public bool IsPersistent { get; private set; }

    // Context information
    public string? RelatedEntityId { get; private set; }
    public string? RelatedEntityType { get; private set; }
    public Dictionary<string, object> NotificationMetadata { get; private set; }

    public UserNotificationEvent(
        string title,
        string message,
        NotificationType notificationType,
        UIEventPriority priority = UIEventPriority.Normal,
        List<string>? targetUserIds = null,
        List<string>? targetRoles = null,
        bool isGlobalNotification = false,
        string? actionUrl = null,
        string? actionText = null,
        TimeSpan? autoDismissAfter = null,
        bool requiresAcknowledgment = false,
        bool isPersistent = false,
        string? relatedEntityId = null,
        string? relatedEntityType = null,
        Dictionary<string, object>? notificationMetadata = null)
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        TargetComponent = "UserNotifications";
        Priority = priority;

        Title = title ?? throw new ArgumentNullException(nameof(title));
        Message = message ?? throw new ArgumentNullException(nameof(message));
        NotificationType = notificationType;
        ActionUrl = actionUrl;
        ActionText = actionText;
        TargetUserIds = targetUserIds ?? new List<string>();
        TargetRoles = targetRoles ?? new List<string>();
        IsGlobalNotification = isGlobalNotification;
        AutoDismissAfter = autoDismissAfter;
        RequiresAcknowledgment = requiresAcknowledgment;
        IsPersistent = isPersistent;
        RelatedEntityId = relatedEntityId;
        RelatedEntityType = relatedEntityType;
        NotificationMetadata = notificationMetadata ?? new Dictionary<string, object>();
    }
}

/// <summary>
/// Types of user notifications
/// </summary>
public enum NotificationType
{
    Info = 0,
    Success = 1,
    Warning = 2,
    Error = 3,
    Alert = 4
}