//-----------------------------------------------------------------------
// <copyright file="QueuedEvent.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Value object representing a queued event for manual execution.
//                  Supports EventBus management and manual event processing.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Common;
using SMS_Domain.Interfaces;
using SMS_Domain.Enums;
using SMS_Domain.Events;
using SMS_Domain.Entities;


namespace SMS_Domain.ValueObjects;

/// <summary>
/// Value object representing a queued event for manual execution
/// Supports EventBus management and testing scenarios
/// </summary>
public sealed class QueuedEvent : BaseEntity
{
    public QueuedEvent(EventQueueID id) : base(id)
    {
        QueueCode = id.Value;
    }

    public QueuedEvent() : this(new EventQueueID(Guid.NewGuid().ToString()))
    {
    }

    /// <summary>
    /// Primary queue code for the queued event.
    /// </summary>
    public string QueueCode { get; private set; } = string.Empty;

    /// <summary>
    /// Secondary GUID identifier maintained for compatibility with existing queue procedures.
    /// </summary>
    public Guid QueueGuid { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Type of event (DomainEvent, UIEvent, IntegrationEvent)
    /// </summary>
    public EventCategory EventCategory { get; set; }

    /// <summary>
    /// Specific event type name
    /// </summary>
    public string EventType { get; set; } = string.Empty;

    /// <summary>
    /// Serialized event data
    /// </summary>
    public string EventData { get; set; } = string.Empty;

    /// <summary>
    /// Report identifier the event originated from.
    /// </summary>
    public string ReportId { get; set; } = string.Empty;

    /// <summary>
    /// Current status of the queued event
    /// </summary>
    public QueuedEventStatus Status { get; set; } = QueuedEventStatus.Pending;

    /// <summary>
    /// When the event was queued
    /// </summary>
    public DateTime QueuedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the event was processed (if processed)
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// Number of processing attempts
    /// </summary>
    public int AttemptCount { get; set; } = 0;

    /// <summary>
    /// Last error message (if any)
    /// </summary>
    public string? LastError { get; set; }

    /// <summary>
    /// Priority level for processing order
    /// </summary>
    public EventPriority Priority { get; set; } = EventPriority.Normal;

    /// <summary>
    /// Target system for integration events
    /// </summary>
    public string? TargetSystem { get; set; }

    /// <summary>
    /// User or system that queued the event
    /// </summary>
    public string? QueuedBy { get; set; }

    /// <summary>
    /// Creates a new QueuedEvent from a domain event
    /// </summary>
    public static QueuedEvent FromDomainEvent<T>(T domainEvent, string? queuedBy = null) where T : IBaseDomainEvent
    {
        var queueGuid = Guid.NewGuid();
        return new QueuedEvent(new EventQueueID(queueGuid.ToString()))
        {
            QueueGuid = queueGuid,
            EventCategory = EventCategory.DomainEvent,
            EventType = domainEvent.EventType, // Use the event's own EventType property instead of C# type name
            EventData = System.Text.Json.JsonSerializer.Serialize(domainEvent),
            ReportId = domainEvent.ReportId,
            QueuedBy = queuedBy ?? string.Empty,
            Priority = EventPriority.Normal
        };
    }

    /// <summary>
    /// Creates a new QueuedEvent from an integration event
    /// </summary>
    public static QueuedEvent FromIntegrationEvent<T>(T integrationEvent, string? queuedBy = null) where T : IBaseIntegrationEvent
    {
        var queueGuid = Guid.NewGuid();
        return new QueuedEvent(new EventQueueID(queueGuid.ToString()))
        {
            QueueGuid = queueGuid,
            EventCategory = EventCategory.IntegrationEvent,
            EventType = integrationEvent.EventType, // Use the event's own EventType property instead of C# type name
            EventData = System.Text.Json.JsonSerializer.Serialize(integrationEvent),
            ReportId = integrationEvent.ReportId,
            TargetSystem = integrationEvent.TargetSystem,
            QueuedBy = queuedBy ?? string.Empty,
            Priority = MapIntegrationPriority(integrationEvent)
        };
    }

    private static EventPriority MapIntegrationPriority(IBaseIntegrationEvent integrationEvent)
    {
        if (integrationEvent is EmailNotificationEvent emailEvent)
        {
            return emailEvent.Priority switch
            {
                EmailPriority.Urgent => EventPriority.Critical,
                EmailPriority.High => EventPriority.High,
                EmailPriority.Normal => EventPriority.Normal,
                EmailPriority.Low => EventPriority.Low,
                _ => EventPriority.Normal
            };
        }

        return EventPriority.High;
    }

    /// <summary>
    /// Creates a new QueuedEvent from a UI event
    /// </summary>
    public static QueuedEvent FromUIEvent<T>(T uiEvent, string? queuedBy = null) where T : IBaseUIEvent
    {
        var queueGuid = Guid.NewGuid();
        return new QueuedEvent(new EventQueueID(queueGuid.ToString()))
        {
            QueueGuid = queueGuid,
            EventCategory = EventCategory.UIEvent,
            EventType = uiEvent.EventType, // Use the event's own EventType property instead of C# type name
            EventData = System.Text.Json.JsonSerializer.Serialize(uiEvent),
            ReportId = uiEvent.ReportId,
            TargetSystem = uiEvent.TargetComponent,
            QueuedBy = queuedBy ?? string.Empty,
            Priority = EventPriority.Low // UI events are typically lower priority
        };
    }

    /// <summary>
    /// Marks the event as processed successfully
    /// </summary>
    public QueuedEvent MarkAsProcessed()
    {
        Status = QueuedEventStatus.Processed;
        ProcessedAt = DateTime.UtcNow;
        AttemptCount += 1;
        return this;
    }

    /// <summary>
    /// Marks the event as failed with error message
    /// </summary>
    public QueuedEvent MarkAsFailed(string errorMessage)
    {
        Status = QueuedEventStatus.Failed;
        AttemptCount += 1;
        LastError = errorMessage;
        return this;
    }

    /// <summary>
    /// Increments the attempt count for retry scenarios
    /// </summary>
    public QueuedEvent IncrementAttempt()
    {
        AttemptCount += 1;
        return this;
    }
}

/// <summary>
/// Statistics about the event queue
/// </summary>
public record QueueStatistics
{
    public int PendingCount { get; init; }
    public int ProcessedCount { get; init; }
    public int FailedCount { get; init; }
    public int CancelledCount { get; init; }
    public Dictionary<EventCategory, EventTypeStatistics> ByEventType { get; init; } = new();
    public Dictionary<EventPriority, int> ByPriority { get; init; } = new();
}

/// <summary>
/// Statistics for a specific event type
/// </summary>
public record EventTypeStatistics
{
    public int Pending { get; init; }
    public int Processed { get; init; }
    public int Failed { get; init; }
    public int Cancelled { get; init; }
    public int Total => Pending + Processed + Failed + Cancelled;
}
