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


namespace SMS_Domain.ValueObjects;

/// <summary>
/// Value object representing a queued event for manual execution
/// Supports EventBus management and testing scenarios
/// </summary>
public record QueuedEvent
{
    /// <summary>
    /// Unique identifier for the queued event
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Type of event (DomainEvent, UIEvent, IntegrationEvent)
    /// </summary>
    public EventCategory EventCategory { get; init; }

    /// <summary>
    /// Specific event type name
    /// </summary>
    public string EventType { get; init; } = string.Empty;

    /// <summary>
    /// Serialized event data
    /// </summary>
    public string EventData { get; init; } = string.Empty;

    /// <summary>
    /// Report identifier the event originated from.
    /// </summary>
    public string ReportId { get; init; } = string.Empty;

    /// <summary>
    /// Current status of the queued event
    /// </summary>
    public QueuedEventStatus Status { get; init; } = QueuedEventStatus.Pending;

    /// <summary>
    /// When the event was queued
    /// </summary>
    public DateTime QueuedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// When the event was processed (if processed)
    /// </summary>
    public DateTime? ProcessedAt { get; init; }

    /// <summary>
    /// Number of processing attempts
    /// </summary>
    public int AttemptCount { get; init; } = 0;

    /// <summary>
    /// Last error message (if any)
    /// </summary>
    public string? LastError { get; init; }

    /// <summary>
    /// Priority level for processing order
    /// </summary>
    public EventPriority Priority { get; init; } = EventPriority.Normal;

    /// <summary>
    /// Target system for integration events
    /// </summary>
    public string? TargetSystem { get; init; }

    /// <summary>
    /// User or system that queued the event
    /// </summary>
    public string? QueuedBy { get; init; }

    /// <summary>
    /// Creates a new QueuedEvent from a domain event
    /// </summary>
    public static QueuedEvent FromDomainEvent<T>(T domainEvent, string? queuedBy = null) where T : IBaseDomainEvent
    {
        return new QueuedEvent
        {
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
        return new QueuedEvent
        {
            EventCategory = EventCategory.IntegrationEvent,
            EventType = integrationEvent.EventType, // Use the event's own EventType property instead of C# type name
            EventData = System.Text.Json.JsonSerializer.Serialize(integrationEvent),
            ReportId = integrationEvent.ReportId,
            TargetSystem = integrationEvent.TargetSystem,
            QueuedBy = queuedBy ?? string.Empty,
            Priority = EventPriority.High // Integration events are typically high priority
        };
    }

    /// <summary>
    /// Creates a new QueuedEvent from a UI event
    /// </summary>
    public static QueuedEvent FromUIEvent<T>(T uiEvent, string? queuedBy = null) where T : IBaseUIEvent
    {
        return new QueuedEvent
        {
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
        return this with
        {
            Status = QueuedEventStatus.Processed,
            ProcessedAt = DateTime.UtcNow,
            AttemptCount = AttemptCount + 1
        };
    }

    /// <summary>
    /// Marks the event as failed with error message
    /// </summary>
    public QueuedEvent MarkAsFailed(string errorMessage)
    {
        return this with
        {
            Status = QueuedEventStatus.Failed,
            AttemptCount = AttemptCount + 1,
            LastError = errorMessage
        };
    }

    /// <summary>
    /// Increments the attempt count for retry scenarios
    /// </summary>
    public QueuedEvent IncrementAttempt()
    {
        return this with
        {
            AttemptCount = AttemptCount + 1
        };
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
