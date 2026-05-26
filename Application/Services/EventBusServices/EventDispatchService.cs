//-----------------------------------------------------------------------
// <copyright file="EventDispatchService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: EventBus service implementation leveraging existing SMS infrastructure patterns.
//                  Integrates with SMS logging, service resolution, and error handling patterns
//                  to provide event-driven workflow capabilities for SPI monitoring and notifications.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Application.Interfaces;
using SMS_Application.Interfaces;
using SMS_Domain.Common;
using SMS_Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace SMS_Application.Services;

/// <summary>
/// EventBus service implementation leveraging existing SMS infrastructure patterns
/// Provides event-driven workflow capabilities while maintaining consistency with
/// existing SMS service patterns, logging, and error handling approaches
/// </summary>
public sealed class EventDispatchService : IBaseEventBus
{
    private readonly ILogger<EventDispatchService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<Type, List<Type>> _eventHandlerMappings = new();
    private readonly object _lock = new object();

    public EventDispatchService(
        ILogger<EventDispatchService> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    private async Task<Result> ExecuteWithQueueServiceAsync(Func<IEventQueueService, Task<Result>> operation)
    {
        using var scope = _serviceProvider.CreateScope();
        var queueService = scope.ServiceProvider.GetRequiredService<IEventQueueService>();
        return await operation(queueService).ConfigureAwait(false);
    }

    /// <summary>
    /// Publishes an event with immediate execution (default mode)
    /// Follows existing SMS service patterns for consistent execution and error handling
    /// </summary>
    public async Task<Result> PublishAsync<T>(T domainEvent, CancellationToken cancellationToken = default) where T : IBaseDomainEvent
    {
        return await PublishAsync(domainEvent, EventExecutionMode.Immediate, cancellationToken);
    }

    /// <summary>
    /// Publishes an event with specified execution mode
    /// Integrates with existing SMS infrastructure while providing flexible execution options
    /// </summary>
    public async Task<Result> PublishAsync<T>(T domainEvent, EventExecutionMode mode, CancellationToken cancellationToken = default) where T : IBaseDomainEvent
    {
        try
        {
            if (domainEvent == null)
            {
                _logger.LogWarning("Attempted to publish null domain event");
                return Result.Failure(new Error("EVENTBUS_NULL_EVENT", "Domain event cannot be null"));
            }

            _logger.LogInformation("?? Publishing event {EventType} (ID: {EventId}) with mode {ExecutionMode}", 
                domainEvent.EventType, domainEvent.EventId, mode);

            switch (mode)
            {
                case EventExecutionMode.Immediate:
                    return await ExecuteHandlersImmediately(domainEvent, cancellationToken);

                case EventExecutionMode.Queued:
                    return await QueueEventForProcessing(domainEvent, cancellationToken);

                case EventExecutionMode.Manual:
                    return await StoreEventForManualExecution(domainEvent, cancellationToken);

                default:
                    _logger.LogWarning("Unknown execution mode {ExecutionMode} for event {EventType}", mode, domainEvent.EventType);
                    return Result.Failure(new Error("EVENTBUS_UNKNOWN_MODE", $"Unknown execution mode: {mode}"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish event {EventType} (ID: {EventId})", 
                domainEvent.EventType, domainEvent.EventId);
            return Result.Failure(new Error("EVENTBUS_PUBLISH_FAILED", $"Event publishing failed: {ex.Message}"));
        }
    }

    #region Domain Event Publishing
    /// <summary>
    /// Publishes a domain event with immediate execution (default mode)
    /// Domain events represent business state changes and core workflow events
    /// </summary>
    public async Task<Result> PublishDomainEventAsync<T>(T domainEvent, CancellationToken cancellationToken = default) where T : IBaseDomainEvent
    {
        return await PublishDomainEventAsync(domainEvent, EventExecutionMode.Immediate, cancellationToken);
    }

    /// <summary>
    /// Publishes a domain event with specified execution mode
    /// Supports immediate, queued, or manual execution for different workflow scenarios
    /// </summary>
    public async Task<Result> PublishDomainEventAsync<T>(T domainEvent, EventExecutionMode mode, CancellationToken cancellationToken = default) where T : IBaseDomainEvent
    {
        // Route to existing domain event implementation
        _logger.LogDebug("?? PublishDomainEventAsync called for {EventType} with mode {Mode}", typeof(T).FullName, mode);
        var result = await PublishAsync(domainEvent, mode, cancellationToken);
        _logger.LogDebug("?? PublishDomainEventAsync result: {IsSuccess}", result.IsSuccess);
        return result;
    }
    #endregion

    #region UI Event Publishing
    /// <summary>
    /// Publishes a UI event for dashboard updates and user interface notifications
    /// UI events typically use immediate execution for responsive user experience
    /// </summary>
    public async Task<Result> PublishUIEventAsync<T>(T uiEvent, CancellationToken cancellationToken = default) where T : IBaseUIEvent
    {
        return await PublishUIEventAsync(uiEvent, EventExecutionMode.Immediate, cancellationToken);
    }

    /// <summary>
    /// Publishes a UI event with specified execution mode
    /// UI events are optimized for immediate execution and user responsiveness
    /// </summary>
    public async Task<Result> PublishUIEventAsync<T>(T uiEvent, EventExecutionMode mode, CancellationToken cancellationToken = default) where T : IBaseUIEvent
    {
        try
        {
            if (uiEvent == null)
            {
                _logger.LogWarning("Attempted to publish null UI event");
                return Result.Failure(new Error("EVENTBUS_NULL_UI_EVENT", "UI event cannot be null"));
            }

            _logger.LogInformation("Publishing UI event {EventType} for {TargetComponent} with mode {ExecutionMode}", 
                uiEvent.EventType, uiEvent.TargetComponent, mode);

            switch (mode)
            {
                case EventExecutionMode.Immediate:
                    return await ExecuteUIHandlersImmediately(uiEvent, cancellationToken);

                case EventExecutionMode.Queued:
                    return await QueueUIEventForProcessing(uiEvent, cancellationToken);

                case EventExecutionMode.Manual:
                    return await StoreUIEventForManualExecution(uiEvent, cancellationToken);

                default:
                    _logger.LogWarning("Unknown execution mode {ExecutionMode} for UI event {EventType}", mode, uiEvent.EventType);
                    return Result.Failure(new Error("EVENTBUS_UNKNOWN_UI_MODE", $"Unknown execution mode for UI event: {mode}"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish UI event {EventType} for {TargetComponent}", 
                uiEvent.EventType, uiEvent.TargetComponent);
            return Result.Failure(new Error("EVENTBUS_UI_PUBLISH_FAILED", $"UI event publishing failed: {ex.Message}"));
        }
    }
    #endregion

    #region Integration Event Publishing
    /// <summary>
    /// Publishes an integration event for external system notifications
    /// Integration events often use queued execution for reliable external delivery
    /// </summary>
    public async Task<Result> PublishIntegrationEventAsync<T>(T integrationEvent, CancellationToken cancellationToken = default) where T : IBaseIntegrationEvent
    {
        // Default to queued execution for integration events (external reliability)
        return await PublishIntegrationEventAsync(integrationEvent, EventExecutionMode.Queued, cancellationToken);
    }

    /// <summary>
    /// Publishes an integration event with specified execution mode
    /// Supports different delivery patterns based on external system requirements
    /// </summary>
    public async Task<Result> PublishIntegrationEventAsync<T>(T integrationEvent, EventExecutionMode mode, CancellationToken cancellationToken = default) where T : IBaseIntegrationEvent
    {
        try
        {
            if (integrationEvent == null)
            {
                _logger.LogWarning("Attempted to publish null integration event");
                return Result.Failure(new Error("EVENTBUS_NULL_INTEGRATION_EVENT", "Integration event cannot be null"));
            }

            _logger.LogInformation("Publishing integration event {EventType} for {TargetSystem} with mode {ExecutionMode} (Delivery: {DeliveryMode})", 
                integrationEvent.EventType, integrationEvent.TargetSystem, mode, integrationEvent.DeliveryMode);

            switch (mode)
            {
                case EventExecutionMode.Immediate:
                    return await ExecuteIntegrationHandlersImmediately(integrationEvent, cancellationToken);

                case EventExecutionMode.Queued:
                    return await QueueIntegrationEventForProcessing(integrationEvent, cancellationToken);

                case EventExecutionMode.Manual:
                    return await StoreIntegrationEventForManualExecution(integrationEvent, cancellationToken);

                default:
                    _logger.LogWarning("Unknown execution mode {ExecutionMode} for integration event {EventType}", mode, integrationEvent.EventType);
                    return Result.Failure(new Error("EVENTBUS_UNKNOWN_INTEGRATION_MODE", $"Unknown execution mode for integration event: {mode}"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish integration event {EventType} for {TargetSystem}", 
                integrationEvent.EventType, integrationEvent.TargetSystem);
            return Result.Failure(new Error("EVENTBUS_INTEGRATION_PUBLISH_FAILED", $"Integration event publishing failed: {ex.Message}"));
        }
    }
    #endregion

    #region Subscription Management
    public void Subscribe<T, THandler>() where T : IBaseDomainEvent where THandler : class, IBaseEventHandler<T>
    {
        lock (_lock)
        {
            var eventType = typeof(T);
            var handlerType = typeof(THandler);

            if (!_eventHandlerMappings.ContainsKey(eventType))
            {
                _eventHandlerMappings[eventType] = new List<Type>();
            }

            if (!_eventHandlerMappings[eventType].Contains(handlerType))
            {
                _eventHandlerMappings[eventType].Add(handlerType);
                _logger.LogInformation("Subscribed handler {HandlerType} to event {EventTypeFullName}", 
                    handlerType.Name, eventType.FullName);
            }
            else
            {
                _logger.LogWarning("Handler {HandlerType} already subscribed to event {EventTypeFullName}", 
                    handlerType.Name, eventType.FullName);
            }
        }
    }

    /// <summary>
    /// Registers a UI event handler for a specific UI event type
    /// Supports dynamic subscription for UI component event handling
    /// </summary>
    public void SubscribeUI<T, THandler>() where T : IBaseUIEvent where THandler : class, IBaseEventHandler<T>
    {
        lock (_lock)
        {
            var eventType = typeof(T);
            var handlerType = typeof(THandler);

            if (!_eventHandlerMappings.ContainsKey(eventType))
            {
                _eventHandlerMappings[eventType] = new List<Type>();
            }

            if (!_eventHandlerMappings[eventType].Contains(handlerType))
            {
                _eventHandlerMappings[eventType].Add(handlerType);
                _logger.LogInformation("Subscribed UI handler {HandlerType} to event {EventType}", 
                    handlerType.Name, eventType.Name);
            }
            else
            {
                _logger.LogWarning("UI Handler {HandlerType} already subscribed to event {EventType}", 
                    handlerType.Name, eventType.Name);
            }
        }
    }

    /// <summary>
    /// Registers an integration event handler for a specific integration event type
    /// Supports external system integration and notification handling
    /// </summary>
    public void SubscribeIntegration<T, THandler>() where T : IBaseIntegrationEvent where THandler : class, IBaseEventHandler<T>
    {
        lock (_lock)
        {
            var eventType = typeof(T);
            var handlerType = typeof(THandler);

            if (!_eventHandlerMappings.ContainsKey(eventType))
            {
                _eventHandlerMappings[eventType] = new List<Type>();
            }

            if (!_eventHandlerMappings[eventType].Contains(handlerType))
            {
                _eventHandlerMappings[eventType].Add(handlerType);
                _logger.LogInformation("Subscribed integration handler {HandlerType} to event {EventType}", 
                    handlerType.Name, eventType.Name);
            }
            else
            {
                _logger.LogWarning("Integration Handler {HandlerType} already subscribed to event {EventType}", 
                    handlerType.Name, eventType.Name);
            }
        }
    }

    /// <summary>
    /// Gets active subscriptions for monitoring and debugging
    /// Provides visibility into event handler registrations
    /// </summary>
    public async Task<Dictionary<string, List<string>>> GetActiveSubscriptionsAsync()
    {
        await Task.CompletedTask; // Async for future extensibility

        lock (_lock)
        {
            return _eventHandlerMappings.ToDictionary(
                kvp => kvp.Key.Name,
                kvp => kvp.Value.Select(h => h.Name).ToList()
            );
        }
    }
    #endregion

    #region Private Implementation Methods

    /// <summary>
    /// Executes all registered handlers immediately
    /// Uses existing SMS service resolution patterns
    /// </summary>
    private async Task<Result> ExecuteHandlersImmediately<T>(T domainEvent, CancellationToken cancellationToken) where T : IBaseDomainEvent
    {
        var eventType = typeof(T);

        _logger.LogDebug("?? Looking up handlers for event type: {EventTypeFullName} (Name: {EventTypeName})", 
            eventType.FullName, eventType.Name);

        lock (_lock)
        {
            _logger.LogDebug("?? Registered event types: {RegisteredTypes}", 
                string.Join(", ", _eventHandlerMappings.Keys.Select(k => k.FullName)));

            if (!_eventHandlerMappings.ContainsKey(eventType) || !_eventHandlerMappings[eventType].Any())
            {
                _logger.LogWarning("No handlers registered for event type {EventTypeFullName}", eventType.FullName);
                return Result.Success(); // Not an error - just no handlers
            }
        }

        var handlerTypes = _eventHandlerMappings[eventType].ToList(); // Safe copy outside lock
        var results = new List<Result>();

        foreach (var handlerType in handlerTypes)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var handler = scope.ServiceProvider.GetService(handlerType) as IBaseEventHandler<T>;
                if (handler != null)
                {
                    _logger.LogDebug("Executing handler {HandlerType} for event {EventType} (ID: {EventId})", 
                        handlerType.Name, domainEvent.EventType, domainEvent.EventId);

                    var result = await handler.HandleAsync(domainEvent, cancellationToken);
                    results.Add(result);

                    if (!result.IsSuccess)
                    {
                        _logger.LogWarning("Handler {HandlerType} failed for event {EventType} (ID: {EventId}): {Error}",
                            handlerType.Name, domainEvent.EventType, domainEvent.EventId, result.Error);
                    }
                }
                else
                {
                    _logger.LogWarning("Could not resolve handler {HandlerType} for event {EventType}",
                        handlerType.Name, domainEvent.EventType);
                    results.Add(Result.Failure(new Error("EVENTBUS_HANDLER_RESOLVE_FAILED", $"Handler {handlerType.Name} could not be resolved")));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing handler {HandlerType} for event {EventType} (ID: {EventId})",
                    handlerType.Name, domainEvent.EventType, domainEvent.EventId);
                results.Add(Result.Failure(new Error("EVENTBUS_HANDLER_EXECUTION_FAILED", $"Handler execution failed: {ex.Message}")));
            }
        }

        // Check if any handlers failed
        var failedResults = results.Where(r => !r.IsSuccess).ToList();
        if (failedResults.Any())
        {
            var errorMessages = string.Join("; ", failedResults.Select(r => r.Error.Message));
            return Result.Failure(new Error("EVENTBUS_HANDLERS_FAILED", $"Some event handlers failed: {errorMessages}"));
        }

        _logger.LogInformation("Successfully executed {HandlerCount} handlers for event {EventType} (ID: {EventId})",
            results.Count, domainEvent.EventType, domainEvent.EventId);

        return Result.Success();
    }

    /// <summary>
    /// Queues event for background processing using EventQueueService
    /// Events are stored for later execution
    /// </summary>
    private async Task<Result> QueueEventForProcessing<T>(T domainEvent, CancellationToken cancellationToken) where T : IBaseDomainEvent
    {
        try
        {
            return await ExecuteWithQueueServiceAsync(queueService => queueService.QueueDomainEventAsync(domainEvent, "EventBus"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to queue domain event {EventType} (ID: {EventId}). Falling back to immediate execution.",
                domainEvent.EventType, domainEvent.EventId);

            // Fallback to immediate execution if queue service is not available
            return await ExecuteHandlersImmediately(domainEvent, cancellationToken);
        }
    }

    /// <summary>
    /// Stores event for manual execution using EventQueueService
    /// Events are stored and require manual triggering
    /// </summary>
    private async Task<Result> StoreEventForManualExecution<T>(T domainEvent, CancellationToken cancellationToken) where T : IBaseDomainEvent
    {
        try
        {
            return await ExecuteWithQueueServiceAsync(async queueService =>
            {
                _logger.LogDebug("?? Got EventQueueService instance for manual execution: {ServiceType}", queueService.GetType().Name);
                return await queueService.QueueDomainEventAsync(domainEvent, "ManualExecution");
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store domain event {EventType} (ID: {EventId}) for manual execution. Falling back to immediate execution.",
                domainEvent.EventType, domainEvent.EventId);

            // Fallback to immediate execution if queue service is not available
            return await ExecuteHandlersImmediately(domainEvent, cancellationToken);
        }
    }

    #region UI Event Execution Methods
    /// <summary>
    /// Executes all registered UI event handlers immediately
    /// Optimized for responsive user interface updates
    /// </summary>
    private async Task<Result> ExecuteUIHandlersImmediately<T>(T uiEvent, CancellationToken cancellationToken) where T : IBaseUIEvent
    {
        var eventType = typeof(T);

        lock (_lock)
        {
            if (!_eventHandlerMappings.ContainsKey(eventType) || !_eventHandlerMappings[eventType].Any())
            {
                _logger.LogDebug("No handlers registered for UI event type {EventType}", eventType.Name);
                return Result.Success(); // Not an error - just no handlers
            }
        }

        var handlerTypes = _eventHandlerMappings[eventType].ToList();
        var results = new List<Result>();

        foreach (var handlerType in handlerTypes)
        {
            try
            {
                // Create a scope to resolve scoped services (like UI handlers with INotificationHelper dependencies)
                using var scope = _serviceProvider.CreateScope();
                var handler = scope.ServiceProvider.GetService(handlerType) as IBaseEventHandler<T>;

                if (handler != null)
                {
                    _logger.LogDebug("Executing UI handler {HandlerType} for event {EventType}", 
                        handlerType.Name, uiEvent.EventType);

                    var result = await handler.HandleAsync(uiEvent, cancellationToken);
                    results.Add(result);

                    if (!result.IsSuccess)
                    {
                        _logger.LogWarning("UI Handler {HandlerType} failed for event {EventType}: {Error}",
                            handlerType.Name, uiEvent.EventType, result.Error.Message);
                    }
                }
                else
                {
                    _logger.LogWarning("Could not resolve UI handler {HandlerType} for event {EventType}",
                        handlerType.Name, uiEvent.EventType);
                    results.Add(Result.Failure(new Error("EVENTBUS_UI_HANDLER_RESOLVE_FAILED", $"UI Handler {handlerType.Name} could not be resolved")));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing UI handler {HandlerType} for event {EventType}",
                    handlerType.Name, uiEvent.EventType);
                results.Add(Result.Failure(new Error("EVENTBUS_UI_HANDLER_EXECUTION_FAILED", $"UI Handler execution failed: {ex.Message}")));
            }
        }

        // Check if any UI handlers failed
        var failedResults = results.Where(r => !r.IsSuccess).ToList();
        if (failedResults.Any())
        {
            var errorMessages = string.Join("; ", failedResults.Select(r => r.Error.Message));
            return Result.Failure(new Error("EVENTBUS_UI_HANDLERS_FAILED", $"Some UI event handlers failed: {errorMessages}"));
        }

        _logger.LogDebug("Successfully executed {HandlerCount} UI handlers for event {EventType}",
            results.Count, uiEvent.EventType);

        return Result.Success();
    }
    #endregion

    #region Integration Event Execution Methods
    /// <summary>
    /// Executes all registered integration event handlers immediately
    /// Handles external system notifications and integrations
    /// </summary>
    private async Task<Result> ExecuteIntegrationHandlersImmediately<T>(T integrationEvent, CancellationToken cancellationToken) where T : IBaseIntegrationEvent
    {
        var eventType = typeof(T);

        lock (_lock)
        {
            if (!_eventHandlerMappings.ContainsKey(eventType) || !_eventHandlerMappings[eventType].Any())
            {
                _logger.LogInformation("No handlers registered for integration event type {EventType}", eventType.Name);
                return Result.Success(); // Not an error - just no handlers
            }
        }

        var handlerTypes = _eventHandlerMappings[eventType].ToList();
        var results = new List<Result>();

        foreach (var handlerType in handlerTypes)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var handler = scope.ServiceProvider.GetService(handlerType) as IBaseEventHandler<T>;
                if (handler != null)
                {
                    _logger.LogInformation("Executing integration handler {HandlerType} for event {EventType} to {TargetSystem}", 
                        handlerType.Name, integrationEvent.EventType, integrationEvent.TargetSystem);

                    var result = await handler.HandleAsync(integrationEvent, cancellationToken);
                    results.Add(result);

                    if (!result.IsSuccess)
                    {
                        _logger.LogWarning("Integration Handler {HandlerType} failed for event {EventType} to {TargetSystem}: {Error}",
                            handlerType.Name, integrationEvent.EventType, integrationEvent.TargetSystem, result.Error.Message);
                    }
                }
                else
                {
                    _logger.LogWarning("Could not resolve integration handler {HandlerType} for event {EventType}",
                        handlerType.Name, integrationEvent.EventType);
                    results.Add(Result.Failure(new Error("EVENTBUS_INTEGRATION_HANDLER_RESOLVE_FAILED", $"Integration Handler {handlerType.Name} could not be resolved")));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing integration handler {HandlerType} for event {EventType}",
                    handlerType.Name, integrationEvent.EventType);
                results.Add(Result.Failure(new Error("EVENTBUS_INTEGRATION_HANDLER_EXECUTION_FAILED", $"Integration Handler execution failed: {ex.Message}")));
            }
        }

        // Check if any integration handlers failed
        var failedResults = results.Where(r => !r.IsSuccess).ToList();
        if (failedResults.Any())
        {
            var errorMessages = string.Join("; ", failedResults.Select(r => r.Error.Message));
            return Result.Failure(new Error("EVENTBUS_INTEGRATION_HANDLERS_FAILED", $"Some integration event handlers failed: {errorMessages}"));
        }

        _logger.LogInformation("Successfully executed {HandlerCount} integration handlers for event {EventType} to {TargetSystem}",
            results.Count, integrationEvent.EventType, integrationEvent.TargetSystem);

        return Result.Success();
    }

    /// <summary>
    /// Queues integration event for background processing using EventQueueService
    /// Integration events are stored for later reliable delivery
    /// </summary>
    private async Task<Result> QueueIntegrationEventForProcessing<T>(T integrationEvent, CancellationToken cancellationToken) where T : IBaseIntegrationEvent
    {
        try
        {
            return await ExecuteWithQueueServiceAsync(queueService => queueService.QueueIntegrationEventAsync(integrationEvent, "EventBus"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to queue integration event {EventType} to {TargetSystem}. Falling back to immediate execution.",
                integrationEvent.EventType, integrationEvent.TargetSystem);

            // Fallback to immediate execution if queue service is not available
            return await ExecuteIntegrationHandlersImmediately(integrationEvent, cancellationToken);
        }
    }

    /// <summary>
    /// Stores integration event for manual execution using EventQueueService
    /// Integration events are stored and require manual triggering
    /// </summary>
    private async Task<Result> StoreIntegrationEventForManualExecution<T>(T integrationEvent, CancellationToken cancellationToken) where T : IBaseIntegrationEvent
    {
        try
        {
            return await ExecuteWithQueueServiceAsync(queueService => queueService.QueueIntegrationEventAsync(integrationEvent, "ManualExecution"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store integration event {EventType} to {TargetSystem} for manual execution. Falling back to immediate execution.",
                integrationEvent.EventType, integrationEvent.TargetSystem);

            // Fallback to immediate execution if queue service is not available
            return await ExecuteIntegrationHandlersImmediately(integrationEvent, cancellationToken);
        }
    }
    #endregion

    #region UI Event Queue Methods
    /// <summary>
    /// Queues UI event for background processing using EventQueueService
    /// UI events are stored for later execution
    /// </summary>
    private async Task<Result> QueueUIEventForProcessing<T>(T uiEvent, CancellationToken cancellationToken) where T : IBaseUIEvent
    {
        try
        {
            return await ExecuteWithQueueServiceAsync(queueService => queueService.QueueUIEventAsync(uiEvent, "EventBus"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to queue UI event {EventType} for {TargetComponent}. Falling back to immediate execution.",
                uiEvent.EventType, uiEvent.TargetComponent);

            // Fallback to immediate execution if queue service is not available
            return await ExecuteUIHandlersImmediately(uiEvent, cancellationToken);
        }
    }

    /// <summary>
    /// Stores UI event for manual execution using EventQueueService
    /// UI events are stored and require manual triggering
    /// </summary>
    private async Task<Result> StoreUIEventForManualExecution<T>(T uiEvent, CancellationToken cancellationToken) where T : IBaseUIEvent
    {
        try
        {
            return await ExecuteWithQueueServiceAsync(queueService => queueService.QueueUIEventAsync(uiEvent, "ManualExecution"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store UI event {EventType} for {TargetComponent} for manual execution. Falling back to immediate execution.",
                uiEvent.EventType, uiEvent.TargetComponent);

            // Fallback to immediate execution if queue service is not available
            return await ExecuteUIHandlersImmediately(uiEvent, cancellationToken);
        }
    }
    #endregion

    #endregion
}