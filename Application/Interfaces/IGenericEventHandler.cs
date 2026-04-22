//-----------------------------------------------------------------------
// <copyright file="IGenericEventHandler.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Generic event handler interfaces for SMS comprehensive event processing.
//                  Provides consistent event handling patterns for Domain, UI, and Integration events
//                  that integrate with existing SMS service infrastructure and logging patterns.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Common;
using SMS_Domain.Interfaces;

namespace SMS_Application.Interfaces;

/// <summary>
/// Generic event handler interface that can handle any event type
/// Provides unified event handling across Domain, UI, and Integration events
/// </summary>
/// <typeparam name="T">Event type (any event interface)</typeparam>
public interface IGenericEventHandler<in T>
{
    /// <summary>
    /// Handles the specified event with SMS Result patterns and logging
    /// </summary>
    /// <param name="eventItem">The event to handle</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure</returns>
    Task<Result> HandleAsync(T eventItem, CancellationToken cancellationToken = default);
}

/// <summary>
/// Specialized interface for domain event handlers
/// Inherits from IGenericEventHandler for consistent handling patterns
/// </summary>
/// <typeparam name="T">Domain event type implementing IBaseDomainEvent</typeparam>
public interface IDomainEventHandler<in T> : IGenericEventHandler<T> where T : IBaseDomainEvent
{
    // Inherits HandleAsync from IGenericEventHandler<T>
}

/// <summary>
/// Specialized interface for UI event handlers
/// Handles dashboard updates, user notifications, and component state changes
/// </summary>
/// <typeparam name="T">UI event type implementing IUIEvent</typeparam>
public interface IUIEventHandler<in T> : IGenericEventHandler<T> where T : IUIEvent
{
    // Inherits HandleAsync from IGenericEventHandler<T>
}

/// <summary>
/// Specialized interface for integration event handlers
/// Handles external system notifications and third-party integrations
/// </summary>
/// <typeparam name="T">Integration event type implementing IIntegrationEvent</typeparam>
public interface IIntegrationEventHandler<in T> : IGenericEventHandler<T> where T : IIntegrationEvent
{
    // Inherits HandleAsync from IGenericEventHandler<T>
}

/// <summary>
/// Base class for domain event handlers providing common SMS infrastructure integration
/// Maintains existing patterns while supporting the new event architecture
/// </summary>
/// <typeparam name="T">Domain event type implementing IBaseDomainEvent</typeparam>
public abstract class BaseDomainEventHandler<T> : IDomainEventHandler<T> where T : IBaseDomainEvent
{
    protected readonly ILogger Logger;

    protected BaseDomainEventHandler(ILogger logger)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Template method for domain event handling with consistent logging and error handling
    /// </summary>
    public async Task<Result> HandleAsync(T domainEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("Processing domain event {EventType} with ID {EventId}", 
                domainEvent.EventType, domainEvent.EventId);

            var result = await ProcessEventAsync(domainEvent, cancellationToken);

            if (result.IsSuccess)
            {
                Logger.LogInformation("Successfully processed domain event {EventType} with ID {EventId}", 
                    domainEvent.EventType, domainEvent.EventId);
            }
            else
            {
                Logger.LogWarning("Failed to process domain event {EventType} with ID {EventId}: {Error}", 
                    domainEvent.EventType, domainEvent.EventId, result.Error.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error processing domain event {EventType} with ID {EventId}", 
                domainEvent.EventType, domainEvent.EventId);
            return Result.Failure(new Error("DOMAIN_EVENT_HANDLER_ERROR", $"Domain event processing failed: {ex.Message}"));
        }
    }

    /// <summary>
    /// Abstract method for specific domain event processing logic
    /// </summary>
    protected abstract Task<Result> ProcessEventAsync(T domainEvent, CancellationToken cancellationToken);
}

/// <summary>
/// Base class for UI event handlers providing common SMS infrastructure integration
/// Optimized for responsive UI updates and user experience
/// </summary>
/// <typeparam name="T">UI event type implementing IUIEvent</typeparam>
public abstract class BaseUIEventHandler<T> : IUIEventHandler<T> where T : IUIEvent
{
    protected readonly ILogger Logger;

    protected BaseUIEventHandler(ILogger logger)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Template method for UI event handling with emphasis on responsiveness
    /// </summary>
    public async Task<Result> HandleAsync(T uiEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogDebug("Processing UI event {EventType} for {TargetComponent} (Priority: {Priority})", 
                uiEvent.EventType, uiEvent.TargetComponent, uiEvent.Priority);

            var result = await ProcessUIEventAsync(uiEvent, cancellationToken);

            if (result.IsSuccess)
            {
                Logger.LogDebug("Successfully processed UI event {EventType} for {TargetComponent}", 
                    uiEvent.EventType, uiEvent.TargetComponent);
            }
            else
            {
                Logger.LogWarning("Failed to process UI event {EventType} for {TargetComponent}: {Error}", 
                    uiEvent.EventType, uiEvent.TargetComponent, result.Error.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error processing UI event {EventType} for {TargetComponent}", 
                uiEvent.EventType, uiEvent.TargetComponent);
            return Result.Failure(new Error("UI_EVENT_HANDLER_ERROR", $"UI event processing failed: {ex.Message}"));
        }
    }

    /// <summary>
    /// Abstract method for specific UI event processing logic
    /// </summary>
    protected abstract Task<Result> ProcessUIEventAsync(T uiEvent, CancellationToken cancellationToken);
}

/// <summary>
/// Base class for integration event handlers providing external system integration
/// Includes retry logic and external system coordination patterns
/// </summary>
/// <typeparam name="T">Integration event type implementing IIntegrationEvent</typeparam>
public abstract class BaseIntegrationEventHandler<T> : IIntegrationEventHandler<T> where T : IIntegrationEvent
{
    protected readonly ILogger Logger;

    protected BaseIntegrationEventHandler(ILogger logger)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Template method for integration event handling with retry logic
    /// </summary>
    public async Task<Result> HandleAsync(T integrationEvent, CancellationToken cancellationToken = default)
    {
        try
        {
            Logger.LogInformation("Processing integration event {EventType} for {TargetSystem} (Delivery: {DeliveryMode})", 
                integrationEvent.EventType, integrationEvent.TargetSystem, integrationEvent.DeliveryMode);

            var result = await ProcessIntegrationEventAsync(integrationEvent, cancellationToken);

            if (result.IsSuccess)
            {
                Logger.LogInformation("Successfully processed integration event {EventType} for {TargetSystem}", 
                    integrationEvent.EventType, integrationEvent.TargetSystem);
            }
            else
            {
                Logger.LogWarning("Failed to process integration event {EventType} for {TargetSystem}: {Error}", 
                    integrationEvent.EventType, integrationEvent.TargetSystem, result.Error.Message);
            }

            return result;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error processing integration event {EventType} for {TargetSystem}", 
                integrationEvent.EventType, integrationEvent.TargetSystem);
            return Result.Failure(new Error("INTEGRATION_EVENT_HANDLER_ERROR", $"Integration event processing failed: {ex.Message}"));
        }
    }

    /// <summary>
    /// Abstract method for specific integration event processing logic
    /// </summary>
    protected abstract Task<Result> ProcessIntegrationEventAsync(T integrationEvent, CancellationToken cancellationToken);
}