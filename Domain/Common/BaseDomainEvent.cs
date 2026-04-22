//-----------------------------------------------------------------------
// <copyright file="BaseDomainEvent.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Base domain event class providing event infrastructure for SMS domain-driven design patterns.
//                  Enhanced base class supporting EventBus integration for workflow notifications,
//                  SPI automation, hazard escalation, and mitigation assignment workflows.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Common;

/// <summary>
/// Base implementation for all domain events in the SMS system
/// Integrates with EventBus for workflow notifications and SPI automation
/// </summary>
public abstract class BaseDomainEvent : IBaseDomainEvent
{
    protected BaseDomainEvent()
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        Version = 1;
    }

    protected BaseDomainEvent(DateTime datetime)
    {
        EventId = Guid.NewGuid();
        OccurredOn = datetime;
        Version = 1;
    }

    /// <summary>
    /// Unique identifier for this event instance
    /// </summary>
    public Guid EventId { get; private set; }

    /// <summary>
    /// When the event occurred
    /// </summary>
    public DateTime OccurredOn { get; protected set; }

    /// <summary>
    /// Type identifier for the event (used by EventBus routing)
    /// Must be implemented by concrete event classes
    /// </summary>
    public abstract string EventType { get; }

    /// <summary>
    /// The aggregate identifier this event relates to
    /// </summary>
    public string AggregateId { get; protected set; } = string.Empty;

    /// <summary>
    /// Event version for handling schema evolution
    /// </summary>
    public int Version { get; protected set; }
}

