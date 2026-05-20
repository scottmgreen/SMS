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

namespace SMS_Domain.Common;

/// <summary>
/// Base implementation for all domain events in the SMS system
/// Integrates with EventBus for workflow notifications and SPI automation
/// </summary>
public abstract class BaseDomainEvent :BaseAuditableEntity, IBaseDomainEvent
{
    
    protected BaseDomainEvent(SMSEventID id) : base(id, "SYSTEM", DateTime.UtcNow)
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;

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


}

