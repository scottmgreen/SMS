//-----------------------------------------------------------------------
// <copyright file="HazardStatusChangedEvent.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Domain event triggered when a hazard status changes in the SMS system.
//                  Supports hazard lifecycle management and escalation workflows based on
//                  status transitions and time-based escalation rules.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Common;
using SMS_Domain.Interfaces;
using SMS_Domain.Enums;

namespace SMS_Domain.Events;

/// <summary>
/// Domain event triggered when a hazard status changes
/// Supports escalation workflows and status-based notifications
/// </summary>
public class HazardStatusChangedEvent : BaseDomainEvent
{
    public override string EventType => "Hazard.StatusChanged";

    public string HazardId { get; private set; }
    public string HazardCode { get; private set; }
    public HazardStatus PreviousStatus { get; private set; }
    public HazardStatus NewStatus { get; private set; }
    public string StatusChangeReason { get; private set; }
    public string ChangedBy { get; private set; }
    public DateTime StatusChangeDate { get; private set; }
    public List<string> NotificationRecipients { get; private set; }
    public Dictionary<string, string> StatusMetadata { get; private set; }

    // Escalation tracking
    public bool RequiresEscalation { get; private set; }
    public TimeSpan? TimeInCurrentStatus { get; private set; }
    public DateTime? EscalationDeadline { get; private set; }

    public HazardStatusChangedEvent(
        string hazardId,
        string hazardCode,
        HazardStatus previousStatus,
        HazardStatus newStatus,
        string statusChangeReason,
        string changedBy,
        DateTime statusChangeDate,
        List<string>? notificationRecipients = null,
        Dictionary<string, string>? statusMetadata = null,
        bool requiresEscalation = false,
        TimeSpan? timeInCurrentStatus = null,
        DateTime? escalationDeadline = null,
        string aggregateId = "")
    {
        HazardId = hazardId ?? throw new ArgumentNullException(nameof(hazardId));
        HazardCode = hazardCode ?? throw new ArgumentNullException(nameof(hazardCode));
        PreviousStatus = previousStatus;
        NewStatus = newStatus;
        StatusChangeReason = statusChangeReason ?? string.Empty;
        ChangedBy = changedBy ?? throw new ArgumentNullException(nameof(changedBy));
        StatusChangeDate = statusChangeDate;
        NotificationRecipients = notificationRecipients ?? new List<string>();
        StatusMetadata = statusMetadata ?? new Dictionary<string, string>();
        RequiresEscalation = requiresEscalation;
        TimeInCurrentStatus = timeInCurrentStatus;
        EscalationDeadline = escalationDeadline;
        AggregateId = aggregateId ?? hazardId;
    }
}