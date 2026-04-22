//-----------------------------------------------------------------------
// <copyright file="IBaseDomainEvent.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Domain event contract defining event structure for SMS domain notifications and EventBus integration.
//                  Enhanced domain event interface supporting workflow notifications,
//                  SPI monitoring, hazard escalation, and mitigation assignment events.
// </copyright>
//-----------------------------------------------------------------------


namespace SMS_Domain.Interfaces;

/// <summary>
/// Base interface for all domain events in the SMS system
/// Supports EventBus integration for workflow notifications and SPI automation
/// </summary>
public interface IBaseDomainEvent
{
    /// <summary>
    /// Unique identifier for this event instance
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// When the event occurred
    /// </summary>
    DateTime OccurredOn { get; }

    /// <summary>
    /// Type identifier for the event (used by EventBus routing)
    /// </summary>
    string EventType { get; }

    /// <summary>
    /// The aggregate identifier this event relates to
    /// </summary>
    string AggregateId { get; }

    /// <summary>
    /// Event version for handling schema evolution
    /// </summary>
    int Version { get; }
}

