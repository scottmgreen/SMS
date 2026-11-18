//-----------------------------------------------------------------------
// <copyright file="BaseDomainEvent.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Base domain event class providing event infrastructure for SMS domain-driven design patterns.
//                  Enhanced base class supporting EventBus integration for workflow notifications,
//                  SPI automation, hazard escalation, and mitigation assignment workflows.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Entities;
using SMS_Domain.Interfaces;

namespace SMS_Domain.Common;

/// <summary>
/// Base implementation for all domain events in the SMS system
/// Integrates with EventBus for workflow notifications and SPI automation
/// </summary>
public abstract class BaseDomainEvent :BaseAuditableEntity, IBaseDomainEvent
{
    private const string DomainEventCategoryName = "DomainEvent";
    
    protected BaseDomainEvent(SMSEventID id) : base(id, string.Empty, DateTime.Now)
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTime.Now;

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
    /// Report identifier this event originated from.
    /// </summary>
    public string ReportId { get; set; } = string.Empty;

    /// <summary>
    /// Runtime event class name.
    /// </summary>
    public virtual string EventClassName => GetType().Name;

    /// <summary>
    /// Logical event category name.
    /// </summary>
    public virtual string EventCategoryName => DomainEventCategoryName;

    /// <summary>
    /// Stable trigger key used by EventBus configuration.
    /// </summary>
    public virtual string TriggerEventKey => $"{EventCategoryName}:{EventClassName}";

    /// <summary>
    /// Human-readable trigger display name.
    /// Uses IEventSource display metadata when available.
    /// </summary>
    public virtual string TriggerDisplayName
    {
        get
        {
            if (this is IEventSource eventSource
                && !string.IsNullOrWhiteSpace(eventSource.EventSourceDisplayName))
            {
                return eventSource.EventSourceDisplayName;
            }

            return EventClassName;
        }
    }


}


