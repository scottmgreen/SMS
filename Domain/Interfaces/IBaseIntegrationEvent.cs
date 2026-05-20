//-----------------------------------------------------------------------
// <copyright file="IBaseIntegrationEvent.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Integration event interface for SMS external system notifications and third-party integrations.
//                  Provides event-driven architecture for email notifications, SMS alerts, audit logging,
//                  and integration with external safety management systems.
// </copyright>
//-----------------------------------------------------------------------

namespace SMS_Domain.Interfaces;

/// <summary>
/// Base interface for all integration events in the SMS system
/// Integration events handle external system notifications, third-party integrations,
/// and cross-system communication requirements
/// </summary>
public interface IBaseIntegrationEvent
{
    /// <summary>
    /// Unique identifier for this integration event instance
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// When the integration event occurred
    /// </summary>
    DateTime OccurredOn { get; }

    /// <summary>
    /// Type identifier for the integration event (used by EventBus routing)
    /// </summary>
    string EventType { get; }

    /// <summary>
    /// Report identifier this event originated from.
    /// </summary>
    string ReportId { get; }

    /// <summary>
    /// The external system or integration target this event relates to
    /// Examples: "EmailService", "SMSProvider", "AuditSystem", "ComplianceReporting"
    /// </summary>
    string TargetSystem { get; }

    /// <summary>
    /// Delivery mode for the integration event
    /// Determines how the event should be processed and delivered
    /// </summary>
    IntegrationDeliveryMode DeliveryMode { get; }

    /// <summary>
    /// Retry policy for failed integrations
    /// Determines how many times to retry failed external communications
    /// </summary>
    int MaxRetryAttempts { get; }
}

/// <summary>
/// Delivery modes for integration events
/// </summary>
public enum IntegrationDeliveryMode
{
    /// <summary>
    /// Fire and forget - send once, don't wait for confirmation
    /// </summary>
    FireAndForget = 1,

    /// <summary>
    /// Best effort - try to deliver but don't guarantee
    /// </summary>
    BestEffort = 2,

    /// <summary>
    /// Guaranteed delivery - ensure event is delivered or queued for retry
    /// </summary>
    Guaranteed = 3,

    /// <summary>
    /// Synchronous - wait for external system confirmation before continuing
    /// </summary>
    Synchronous = 4
}