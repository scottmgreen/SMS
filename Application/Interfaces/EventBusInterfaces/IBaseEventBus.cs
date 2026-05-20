//-----------------------------------------------------------------------
// <copyright file="IBaseEventBus.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Enhanced EventBus interface for SMS workflow notifications and comprehensive event publishing.
//                  Provides event-driven architecture supporting domain events, UI updates, and external integrations
//                  for SPI monitoring, hazard escalation, mitigation assignment, and dashboard notifications.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Common;

namespace SMS_Application.Interfaces;

/// <summary>
/// Enhanced EventBus interface for comprehensive SMS event handling
/// Supports three distinct event categories: Domain, UI, and Integration events
/// Integrates with existing SMS mediator patterns and service infrastructure
/// </summary>
public interface IBaseEventBus
{
    #region Domain Event Publishing
    /// <summary>
    /// Publishes a domain event with immediate execution (default mode)
    /// Domain events represent business state changes and core workflow events
    /// Examples: HazardCreated, SPIThresholdExceeded, MitigationCompleted
    /// </summary>
    /// <typeparam name="T">Domain event type implementing IBaseDomainEvent</typeparam>
    /// <param name="domainEvent">The domain event to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure</returns>
    Task<Result> PublishDomainEventAsync<T>(T domainEvent, CancellationToken cancellationToken = default) where T : IBaseDomainEvent;

    /// <summary>
    /// Publishes a domain event with specified execution mode
    /// Supports immediate, queued, or manual execution for different workflow scenarios
    /// </summary>
    /// <typeparam name="T">Domain event type implementing IBaseDomainEvent</typeparam>
    /// <param name="domainEvent">The domain event to publish</param>
    /// <param name="mode">Execution mode (Immediate, Queued, Manual)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure</returns>
    Task<Result> PublishDomainEventAsync<T>(T domainEvent, EventExecutionMode mode, CancellationToken cancellationToken = default) where T : IBaseDomainEvent;
    #endregion

    #region UI Event Publishing
    /// <summary>
    /// Publishes a UI event for dashboard updates and user interface notifications
    /// UI events handle real-time updates to Blazor components and dashboards
    /// Examples: SPIDashboardRefresh, UserNotification, ComponentStateChanged
    /// </summary>
    /// <typeparam name="T">UI event type implementing IBaseUIEvent</typeparam>
    /// <param name="uiEvent">The UI event to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure</returns>
    Task<Result> PublishUIEventAsync<T>(T uiEvent, CancellationToken cancellationToken = default) where T : IBaseUIEvent;

    /// <summary>
    /// Publishes a UI event with specified execution mode
    /// UI events typically use immediate execution for responsive user experience
    /// </summary>
    /// <typeparam name="T">UI event type implementing IBaseUIEvent</typeparam>
    /// <param name="uiEvent">The UI event to publish</param>
    /// <param name="mode">Execution mode (typically Immediate for UI responsiveness)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure</returns>
    Task<Result> PublishUIEventAsync<T>(T uiEvent, EventExecutionMode mode, CancellationToken cancellationToken = default) where T : IBaseUIEvent;
    #endregion

    #region Integration Event Publishing
    /// <summary>
    /// Publishes an integration event for external system notifications
    /// Integration events handle communication with external services and systems
    /// Examples: EmailNotification, SMSAlert, AuditLogEntry, ComplianceReport
    /// </summary>
    /// <typeparam name="T">Integration event type implementing IBaseIntegrationEvent</typeparam>
    /// <param name="integrationEvent">The integration event to publish</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure</returns>
    Task<Result> PublishIntegrationEventAsync<T>(T integrationEvent, CancellationToken cancellationToken = default) where T : IBaseIntegrationEvent;

    /// <summary>
    /// Publishes an integration event with specified execution mode
    /// Integration events often use queued execution for reliable external delivery
    /// </summary>
    /// <typeparam name="T">Integration event type implementing IBaseIntegrationEvent</typeparam>
    /// <param name="integrationEvent">The integration event to publish</param>
    /// <param name="mode">Execution mode (Queued recommended for external integrations)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure</returns>
    Task<Result> PublishIntegrationEventAsync<T>(T integrationEvent, EventExecutionMode mode, CancellationToken cancellationToken = default) where T : IBaseIntegrationEvent;
    #endregion

    #region Subscription Management
    /// <summary>
    /// Registers an event handler for a specific domain event type
    /// Integrates with existing SMS dependency injection patterns
    /// </summary>
    /// <typeparam name="T">Domain event type</typeparam>
    /// <typeparam name="THandler">Handler type implementing IBaseEventHandler</typeparam>
    void Subscribe<T, THandler>() where T : IBaseDomainEvent where THandler : class, IBaseEventHandler<T>;

    /// <summary>
    /// Registers a UI event handler for a specific UI event type
    /// Supports dynamic subscription for UI component event handling
    /// </summary>
    /// <typeparam name="T">UI event type</typeparam>
    /// <typeparam name="THandler">Handler type implementing IBaseEventHandler</typeparam>
    void SubscribeUI<T, THandler>() where T : IBaseUIEvent where THandler : class, IBaseEventHandler<T>;

    /// <summary>
    /// Registers an integration event handler for a specific integration event type
    /// Supports external system integration and notification handling
    /// </summary>
    /// <typeparam name="T">Integration event type</typeparam>
    /// <typeparam name="THandler">Handler type implementing IBaseEventHandler</typeparam>
    void SubscribeIntegration<T, THandler>() where T : IBaseIntegrationEvent where THandler : class, IBaseEventHandler<T>;

    /// <summary>
    /// Gets active subscriptions for monitoring and debugging
    /// Returns all registered handlers across domain, UI, and integration events
    /// </summary>
    /// <returns>Dictionary of event types and their registered handlers</returns>
    Task<Dictionary<string, List<string>>> GetActiveSubscriptionsAsync();
    #endregion
}

/// <summary>
/// Event execution modes for different workflow scenarios
/// </summary>
public enum EventExecutionMode
{
    /// <summary>
    /// Execute immediately - for internal workflow steps and real-time processing
    /// </summary>
    Immediate,

    /// <summary>
    /// Queue for background processing - for external notifications (email, SMS)
    /// </summary>
    Queued,

    /// <summary>
    /// Store for manual execution - for testing and controlled notification delivery
    /// </summary>
    Manual
}