//-----------------------------------------------------------------------
// <copyright file="EventQueueService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Implementation of event queue management for manual execution.
//                  Provides in-memory queue with persistence option for testing and controlled processing.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Application.Interfaces;
using SMS_Application.Interfaces;

using SMS_Domain.ValueObjects;
using SMS_Domain.Common;
using SMS_Domain.Interfaces;
using SMS_Domain.Events;
using SMS_Domain.Entities;
using SMS_Domain.Enums;
using SMS_Infrastructure.Interfaces;

using Microsoft.Extensions.Logging;

using System.Text.Json;
using System.Reflection;

namespace SMS_Application.Services;

/// <summary>
/// Service for managing queued events and manual execution
/// Provides in-memory queue with optional persistence for testing scenarios
/// </summary>
public class EventQueueService : IEventQueueService
{
    private readonly ILogger<EventQueueService> _logger;
    private readonly IBaseEventBus _eventBus;
    private readonly IEventQueueDataService _eventQueueDataService;
    private readonly QueuedEventTypeRegistry _queuedEventTypeRegistry;
    private static readonly JsonSerializerOptions EventJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public EventQueueService(
        ILogger<EventQueueService> logger,
        IBaseEventBus eventBus,
        IEventQueueDataService eventQueueDataService,
        QueuedEventTypeRegistry queuedEventTypeRegistry)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
        _eventQueueDataService = eventQueueDataService ?? throw new ArgumentNullException(nameof(eventQueueDataService));
        _queuedEventTypeRegistry = queuedEventTypeRegistry ?? throw new ArgumentNullException(nameof(queuedEventTypeRegistry));
    }

    /// <summary>
    /// Queues a domain event for manual execution
    /// </summary>
    public async Task<Result> QueueDomainEventAsync<T>(T domainEvent, string? queuedBy = null) where T : IBaseDomainEvent
    {
        try
        {
            if (domainEvent == null)
            {
                return Result.Failure(new Error("QUEUE_NULL_EVENT", "Domain event cannot be null"));
            }

            var result = await _eventQueueDataService.EnqueueDomainEventAsync(domainEvent, queuedBy);
            if (result.IsFailure)
            {
                return Result.Failure(result.Error);
            }

            var queuedEvent = result.Value;

            _logger.LogApplicationInformation("Queued domain event {EventType} with ID {EventId} (Queue ID: {QueueId})",
                domainEvent.EventType, domainEvent.EventId, queuedEvent.Id);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Failed to queue domain event {EventType}", typeof(T).Name);
            return Result.Failure(new Error("QUEUE_DOMAIN_FAILED", $"Failed to queue domain event: {ex.Message}"));
        }
    }

    /// <summary>
    /// Queues an integration event for manual execution
    /// </summary>
    public async Task<Result> QueueIntegrationEventAsync<T>(T integrationEvent, string? queuedBy = null) where T : IBaseIntegrationEvent
    {
        try
        {
            if (integrationEvent == null)
            {
                return Result.Failure(new Error("QUEUE_NULL_EVENT", "Integration event cannot be null"));
            }

            var result = await _eventQueueDataService.EnqueueIntegrationEventAsync(integrationEvent, queuedBy);
            if (result.IsFailure)
            {
                return Result.Failure(result.Error);
            }

            var queuedEvent = result.Value;

            _logger.LogApplicationInformation("Queued integration event {EventType} for {TargetSystem} (Queue ID: {QueueId})",
                integrationEvent.EventType, integrationEvent.TargetSystem, queuedEvent.Id);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Failed to queue integration event {EventType}", typeof(T).Name);
            return Result.Failure(new Error("QUEUE_INTEGRATION_FAILED", $"Failed to queue integration event: {ex.Message}"));
        }
    }

    /// <summary>
    /// Queues a UI event for manual execution
    /// </summary>
    public async Task<Result> QueueUIEventAsync<T>(T uiEvent, string? queuedBy = null) where T : IBaseUIEvent
    {
        try
        {
            if (uiEvent == null)
            {
                return Result.Failure(new Error("QUEUE_NULL_EVENT", "UI event cannot be null"));
            }

            var result = await _eventQueueDataService.EnqueueUIEventAsync(uiEvent, queuedBy);
            if (result.IsFailure)
            {
                return Result.Failure(result.Error);
            }

            var queuedEvent = result.Value;

            _logger.LogApplicationInformation("Queued UI event {EventType} for {TargetComponent} (Queue ID: {QueueId})",
                uiEvent.EventType, uiEvent.TargetComponent, queuedEvent.Id);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Failed to queue UI event {EventType}", typeof(T).Name);
            return Result.Failure(new Error("QUEUE_UI_FAILED", $"Failed to queue UI event: {ex.Message}"));
        }
    }

    /// <summary>
    /// Gets all queued events with optional filtering
    /// </summary>
    public async Task<Result<IEnumerable<QueuedEvent>>> GetQueuedEventsAsync(
        QueuedEventStatus? status = null,
        EventCategory? eventType = null,
        int? maxResults = null)
    {
        try
        {
            var max = maxResults ?? 100;
            IEnumerable<QueuedEvent> events;

            if (status.HasValue)
            {
                Result<List<QueuedEvent>> result = status.Value == QueuedEventStatus.Pending
                    ? await _eventQueueDataService.GetPendingEventsAsync(max, eventType)
                    : await _eventQueueDataService.GetEventsByStatusAsync(status.Value, max, eventType);

                if (result.IsFailure)
                {
                    return Result.Failure<IEnumerable<QueuedEvent>>(result.Error);
                }

                events = result.Value;
            }
            else
            {
                var allEvents = new List<QueuedEvent>();

                var pendingResult = await _eventQueueDataService.GetPendingEventsAsync(max, eventType);
                if (pendingResult.IsFailure)
                {
                    return Result.Failure<IEnumerable<QueuedEvent>>(pendingResult.Error);
                }

                allEvents.AddRange(pendingResult.Value);

                var nonPendingStatuses = new[]
                {
                    QueuedEventStatus.Processing,
                    QueuedEventStatus.Processed,
                    QueuedEventStatus.Failed,
                    QueuedEventStatus.Cancelled
                };

                foreach (var nonPendingStatus in nonPendingStatuses)
                {
                    var statusResult = await _eventQueueDataService.GetEventsByStatusAsync(nonPendingStatus, max, eventType);
                    if (statusResult.IsFailure)
                    {
                        return Result.Failure<IEnumerable<QueuedEvent>>(statusResult.Error);
                    }

                    allEvents.AddRange(statusResult.Value);
                }

                events = allEvents
                    .GroupBy(e => e.Id)
                    .Select(g => g.First())
                    .OrderByDescending(e => (int)e.Priority)
                    .ThenBy(e => e.QueuedAt)
                    .Take(max)
                    .ToList();
            }

            if (status.HasValue)
            {
                events = events.Where(e => e.Status == status.Value);
            }

            var filtered = events.ToList();

            _logger.LogApplicationDebug("Retrieved {EventCount} queued events (Status: {Status}, Type: {EventType})",
                filtered.Count, status, eventType);

            return Result.Success<IEnumerable<QueuedEvent>>(filtered);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Failed to retrieve queued events");
            return Result.Failure<IEnumerable<QueuedEvent>>(new Error("QUEUE_RETRIEVE_FAILED", $"Failed to retrieve events: {ex.Message}"));
        }
    }

    /// <summary>
    /// Gets a specific queued event by ID
    /// </summary>
    public async Task<Result<QueuedEvent>> GetQueuedEventAsync(Guid eventId)
    {
        try
        {
            return await _eventQueueDataService.GetQueuedEventAsync(eventId);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Failed to retrieve queued event {EventId}", eventId);
            return Result.Failure<QueuedEvent>(new Error("QUEUE_RETRIEVE_EVENT_FAILED", $"Failed to retrieve event: {ex.Message}"));
        }
    }

    /// <summary>
    /// Manually executes a queued event
    /// </summary>
    public async Task<Result> ExecuteQueuedEventAsync(Guid eventId, string? executedBy = null)
    {
        try
        {
            var queuedEventResult = await _eventQueueDataService.GetQueuedEventAsync(eventId);
            if (queuedEventResult.IsFailure)
            {
                return Result.Failure(new Error("QUEUE_EVENT_NOT_FOUND", $"Queued event with ID {eventId} not found"));
            }

            var queuedEvent = queuedEventResult.Value;

            if (queuedEvent.Status != QueuedEventStatus.Pending)
            {
                return Result.Failure(new Error("QUEUE_EVENT_NOT_PENDING", $"Event {eventId} is not in pending status (Current: {queuedEvent.Status})"));
            }

            var worker = string.IsNullOrWhiteSpace(executedBy) ? "EventQueueService" : executedBy;

            var leaseResult = await _eventQueueDataService.LeaseQueuedEventAsync(eventId, worker, 60);
            if (leaseResult.IsFailure)
            {
                return Result.Failure(new Error("QUEUE_LEASE_FAILED", $"Failed to lease event {eventId} for processing."));
            }

            _logger.LogApplicationInformation("Executing queued event {EventType} (Queue ID: {QueueId}) by {ExecutedBy}",
                queuedEvent.EventType, eventId, executedBy ?? "System");

            Result executionResult;

            // Execute based on event category
            if (queuedEvent.EventCategory == EventCategory.DomainEvent)
            {
                executionResult = await ExecuteDomainEvent(queuedEvent);
            }
            else if (queuedEvent.EventCategory == EventCategory.IntegrationEvent)
            {
                executionResult = await ExecuteIntegrationEvent(queuedEvent);
            }
            else if (queuedEvent.EventCategory == EventCategory.UIEvent)
            {
                executionResult = await ExecuteUIEvent(queuedEvent);
            }
            else
            {
                executionResult = Result.Failure(new Error("QUEUE_UNKNOWN_TYPE", $"Unknown event type: {queuedEvent.EventCategory}"));
            }

            // Update event status based on execution result
            if (executionResult.IsSuccess)
            {
                var markProcessedResult = await _eventQueueDataService.MarkProcessedAsync(eventId, worker);
                if (markProcessedResult.IsFailure || !markProcessedResult.Value)
                {
                    return Result.Failure(new Error("QUEUE_MARK_PROCESSED_FAILED", $"Event {eventId} executed but could not be marked as processed."));
                }

                _logger.LogApplicationInformation("Successfully executed queued event {EventType} (Queue ID: {QueueId})",
                    queuedEvent.EventType, eventId);
            }
            else
            {
                var markFailedResult = await _eventQueueDataService.MarkFailedAsync(eventId, worker, executionResult.Error.Message);
                if (markFailedResult.IsFailure || !markFailedResult.Value)
                {
                    _logger.LogApplicationWarning("Event execution failed and status update to failed did not persist for Queue ID: {QueueId}", eventId);
                }

                _logger.LogApplicationWarning("Failed to execute queued event {EventType} (Queue ID: {QueueId}): {Error}",
                    queuedEvent.EventType, eventId, executionResult.Error.Message);
            }

            return executionResult;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Failed to execute queued event {EventId}", eventId);

            await _eventQueueDataService.MarkFailedAsync(eventId, "EventQueueService", ex.Message);

            return Result.Failure(new Error("QUEUE_EXECUTE_FAILED", $"Failed to execute event: {ex.Message}"));
        }
    }

    /// <summary>
    /// Executes all pending events of a specific type
    /// </summary>
    public async Task<Result<int>> ExecuteAllPendingEventsAsync(EventCategory? eventType = null, string? executedBy = null)
    {
        try
        {
            var pendingResult = await _eventQueueDataService.GetPendingEventsAsync(1000, eventType);
            if (pendingResult.IsFailure)
            {
                return Result.Failure<int>(pendingResult.Error);
            }

            var pendingEvents = pendingResult.Value
                .Where(e => e.Status == QueuedEventStatus.Pending)
                .OrderByDescending(e => (int)e.Priority)
                .ThenBy(e => e.QueuedAt)
                .ToList();

            _logger.LogApplicationInformation("Executing {EventCount} pending events (Type: {EventType}) by {ExecutedBy}",
                pendingEvents.Count, eventType, executedBy ?? "System");

            int successCount = 0;
            var errors = new List<string>();

            foreach (var queuedEvent in pendingEvents)
            {
                var result = await ExecuteQueuedEventAsync(queuedEvent.Id, executedBy);
                if (result.IsSuccess)
                {
                    successCount++;
                }
                else
                {
                    errors.Add($"{queuedEvent.EventType}: {result.Error.Message}");
                }
            }

            if (errors.Any())
            {
                var errorMessage = $"Executed {successCount}/{pendingEvents.Count} events successfully. Errors: {string.Join("; ", errors)}";
                _logger.LogApplicationWarning(errorMessage);
                return Result.Failure<int>(new Error("QUEUE_BATCH_PARTIAL_FAILURE", errorMessage));
            }

            _logger.LogApplicationInformation("Successfully executed all {EventCount} pending events", successCount);
            return Result.Success(successCount);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Failed to execute pending events");
            return Result.Failure<int>(new Error("QUEUE_BATCH_EXECUTE_FAILED", $"Failed to execute pending events: {ex.Message}"));
        }
    }

    /// <summary>
    /// Cancels a queued event
    /// </summary>
    public async Task<Result> CancelQueuedEventAsync(Guid eventId, string? cancelledBy = null)
    {
        try
        {
            var queuedEventResult = await _eventQueueDataService.GetQueuedEventAsync(eventId);
            if (queuedEventResult.IsFailure)
            {
                return Result.Failure(new Error("QUEUE_EVENT_NOT_FOUND", $"Queued event with ID {eventId} not found"));
            }

            var queuedEvent = queuedEventResult.Value;

            if (queuedEvent.Status != QueuedEventStatus.Pending)
            {
                return Result.Failure(new Error("QUEUE_EVENT_NOT_PENDING", $"Event {eventId} cannot be cancelled (Current status: {queuedEvent.Status})"));
            }

            var cancelResult = await _eventQueueDataService.CancelAsync(eventId, cancelledBy ?? "System");
            if (cancelResult.IsFailure || !cancelResult.Value)
            {
                return Result.Failure(new Error("QUEUE_CANCEL_FAILED", $"Failed to cancel event {eventId}"));
            }

            _logger.LogApplicationInformation("Cancelled queued event {EventType} (Queue ID: {QueueId}) by {CancelledBy}",
                queuedEvent.EventType, eventId, cancelledBy ?? "System");

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Failed to cancel queued event {EventId}", eventId);
            return Result.Failure(new Error("QUEUE_CANCEL_FAILED", $"Failed to cancel event: {ex.Message}"));
        }
    }

    /// <summary>
    /// Clears all processed and failed events
    /// </summary>
    public async Task<Result<int>> ClearCompletedEventsAsync()
    {
        try
        {
            var clearResult = await _eventQueueDataService.ClearCompletedAsync("EventQueueService");
            if (clearResult.IsFailure)
            {
                return Result.Failure<int>(clearResult.Error);
            }

            var removedCount = clearResult.Value;

            _logger.LogApplicationInformation("Cleared {RemovedCount} completed events from queue", removedCount);
            return Result.Success(removedCount);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Failed to clear completed events");
            return Result.Failure<int>(new Error("QUEUE_CLEAR_FAILED", $"Failed to clear events: {ex.Message}"));
        }
    }

    /// <summary>
    /// Gets queue statistics
    /// </summary>
    public async Task<Result<QueueStatistics>> GetQueueStatisticsAsync()
    {
        try
        {
            var statsResult = await _eventQueueDataService.GetStatsAsync();
            if (statsResult.IsFailure)
            {
                return Result.Failure<QueueStatistics>(statsResult.Error);
            }

            return Result.Success(statsResult.Value);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Failed to get queue statistics");
            return Result.Failure<QueueStatistics>(new Error("QUEUE_STATS_FAILED", $"Failed to get statistics: {ex.Message}"));
        }
    }

    #region Private Event Execution Methods

    /// <summary>
    /// Executes a domain event by reconstructing it from the queued event data and publishing it immediately
    /// </summary>
    private async Task<Result> ExecuteDomainEvent(QueuedEvent queuedEvent)
    {
        try
        {
            _logger.LogApplicationInformation("[QUEUE] Executing domain event {EventType} (Queue ID: {QueueId})", queuedEvent.EventType, queuedEvent.Id);

            if (!_queuedEventTypeRegistry.TryResolveDomainEventType(queuedEvent.EventType, out var eventType))
            {
                var knownTypes = string.Join(", ", _queuedEventTypeRegistry.GetKnownTypes(EventCategory.DomainEvent).OrderBy(x => x));
                _logger.LogApplicationWarning("[QUEUE] Unknown domain event type {EventType}. Known domain types: {KnownTypes}", queuedEvent.EventType, knownTypes);
                return Result.Failure(new Error("UNKNOWN_EVENT_TYPE", $"Unknown domain event type: {queuedEvent.EventType}"));
            }

            var domainEvent = DeserializeDomainEvent(queuedEvent.EventData, eventType);
            if (domainEvent == null)
            {
                return Result.Failure(new Error("DESERIALIZATION_FAILED", $"Could not deserialize event: {queuedEvent.EventType}"));
            }

            // 1. Get the generic method definition (no parameters needed)
            var publishMethodDef = _eventBus.GetType()
                .GetMethods()
                .FirstOrDefault(m => m.Name == "PublishDomainEventAsync" && m.IsGenericMethodDefinition && m.GetParameters().Length == 3);

            if (publishMethodDef == null)
            {
                return Result.Failure(new Error("METHOD_NOT_FOUND", $"PublishDomainEventAsync not found for event type: {queuedEvent.EventType}"));
            }

            // 2. Make the generic method for the concrete event type
            var publishMethod = publishMethodDef.MakeGenericMethod(eventType);

            // 3. Invoke with the correct parameters (suppress audit re-persist during replay)
            using var replayScope = EventDispatchService.BeginQueueReplayScope();
            var task = (Task<Result>)publishMethod.Invoke(_eventBus, new object[] { domainEvent, EventExecutionMode.Immediate, CancellationToken.None });
            var executionResult = await task;

            if (executionResult.IsFailure)
            {
                _logger.LogApplicationError("[QUEUE] Failed to execute domain event {EventType}: {Error}", queuedEvent.EventType, executionResult.Error.Message);
            }
            

            return executionResult;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "[QUEUE] Exception executing domain event {EventType}", queuedEvent.EventType);
            return Result.Failure(new Error("DOMAIN_EVENT_EXECUTION_FAILED", $"Failed to execute domain event: {ex.Message}"));
        }
    }

    private object? DeserializeDomainEvent(string eventData, Type eventType)
    {
        var isDomainEvent = typeof(IBaseDomainEvent).IsAssignableFrom(eventType);
        var hasParameterlessConstructor = eventType.GetConstructor(Type.EmptyTypes) != null;

        if (isDomainEvent && !hasParameterlessConstructor)
        {
            _logger.LogApplicationDebug("[QUEUE] Using constructor fallback deserialization for {EventType}", eventType.Name);
            return DeserializeWithIdCtorFallback(eventData, eventType);
        }

        try
        {
            return JsonSerializer.Deserialize(eventData, eventType, EventJsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationDebug(ex, "[QUEUE] Standard deserialization failed for {EventType}. Attempting fallback construction.", eventType.Name);
            return DeserializeWithIdCtorFallback(eventData, eventType);
        }
    }

    private object? DeserializeWithIdCtorFallback(string eventData, Type eventType)
    {
        using var doc = JsonDocument.Parse(eventData);
        var root = doc.RootElement;
        var idValue = TryGetIdValue(root) ?? Guid.NewGuid().ToString();

        var constructors = eventType
            .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .OrderBy(c => c.GetParameters().Length)
            .ToList();

        object? instance = null;
        foreach (var ctor in constructors)
        {
            if (!TryBuildConstructorArgs(ctor, root, idValue, out var args))
            {
                continue;
            }

            try
            {
                instance = ctor.Invoke(args);
                break;
            }
            catch
            {
                // try next candidate ctor
            }
        }

        if (instance == null)
        {
            _logger.LogApplicationWarning("[QUEUE] No compatible constructor found for fallback on {EventType}", eventType.Name);
            return null;
        }

        if (instance == null)
        {
            return null;
        }

        PopulateSettableProperties(instance, root);
        MapNestedIdToHazardId(instance, root);

        return instance;
    }

    private static bool TryBuildConstructorArgs(ConstructorInfo ctor, JsonElement root, string idValue, out object?[] args)
    {
        var parameters = ctor.GetParameters();
        args = new object?[parameters.Length];

        for (var i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];

            if (parameter.ParameterType == typeof(SMSEventID))
            {
                args[i] = new SMSEventID(idValue);
                continue;
            }

            if (parameter.ParameterType == typeof(string) &&
                (string.Equals(parameter.Name, "id", StringComparison.OrdinalIgnoreCase)
                || string.Equals(parameter.Name, "eventId", StringComparison.OrdinalIgnoreCase)))
            {
                args[i] = idValue;
                continue;
            }

            if (TryGetJsonProperty(root, parameter.Name ?? string.Empty, out var jsonProperty) &&
                TryDeserializePropertyValue(jsonProperty, parameter.ParameterType, out var value))
            {
                args[i] = value;
                continue;
            }

            if (TryGetLegacyMitigationStatusChangedCtorValue(parameter, root, out var legacyValue))
            {
                args[i] = legacyValue;
                continue;
            }

            if (parameter.HasDefaultValue)
            {
                args[i] = parameter.DefaultValue;
                continue;
            }

            return false;
        }

        return true;
    }

    private static bool TryGetLegacyMitigationStatusChangedCtorValue(ParameterInfo parameter, JsonElement root, out object? value)
    {
        value = null;

        var name = parameter.Name ?? string.Empty;

        // Backward compatibility for older queued MITIGATION_STATUS_CHANGED payloads
        // that were produced by MitigationCompletedEvent shape.
        if (parameter.ParameterType == typeof(string) && string.Equals(name, "status", StringComparison.OrdinalIgnoreCase))
        {
            if (TryGetJsonProperty(root, "Status", out var statusProp) && statusProp.ValueKind == JsonValueKind.String)
            {
                value = statusProp.GetString() ?? string.Empty;
                return true;
            }

            value = "COMPLETED";
            return true;
        }

        if (parameter.ParameterType == typeof(string) && string.Equals(name, "changedBy", StringComparison.OrdinalIgnoreCase))
        {
            if (TryGetJsonProperty(root, "ChangedBy", out var changedByProp) && changedByProp.ValueKind == JsonValueKind.String)
            {
                value = changedByProp.GetString() ?? "SYSTEM";
                return true;
            }

            if (TryGetJsonProperty(root, "CompletionNotes", out var notesProp) && notesProp.ValueKind == JsonValueKind.String)
            {
                var notes = notesProp.GetString();
                value = string.IsNullOrWhiteSpace(notes) ? "SYSTEM" : notes;
                return true;
            }

            value = "SYSTEM";
            return true;
        }

        if (parameter.ParameterType == typeof(DateTime) && string.Equals(name, "changedDate", StringComparison.OrdinalIgnoreCase))
        {
            if (TryGetJsonProperty(root, "ChangedDate", out var changedDateProp) &&
                TryDeserializePropertyValue(changedDateProp, typeof(DateTime), out var changedDateValue) &&
                changedDateValue is DateTime changedDate)
            {
                value = changedDate;
                return true;
            }

            if (TryGetJsonProperty(root, "CompletedDate", out var completedDateProp) &&
                TryDeserializePropertyValue(completedDateProp, typeof(DateTime), out var completedDateValue) &&
                completedDateValue is DateTime completedDate)
            {
                value = completedDate;
                return true;
            }

            value = DateTime.UtcNow;
            return true;
        }

        return false;
    }

    private static string? TryGetIdValue(JsonElement root)
    {
        if (root.TryGetProperty("Id", out var idElement))
        {
            if (idElement.ValueKind == JsonValueKind.Object && idElement.TryGetProperty("Value", out var nestedValue))
            {
                return nestedValue.GetString();
            }

            if (idElement.ValueKind == JsonValueKind.String)
            {
                return idElement.GetString();
            }
        }

        if (root.TryGetProperty("EventId", out var eventIdElement) && eventIdElement.ValueKind == JsonValueKind.String)
        {
            return eventIdElement.GetString();
        }

        return null;
    }

    private static void PopulateSettableProperties(object instance, JsonElement root)
    {
        var properties = instance
            .GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite);

        foreach (var property in properties)
        {
            if (!TryGetJsonProperty(root, property.Name, out var jsonProperty))
            {
                continue;
            }

            try
            {
                if (TryDeserializePropertyValue(jsonProperty, property.PropertyType, out var value))
                {
                    property.SetValue(instance, value);
                }
            }
            catch
            {
                // best-effort hydration for replayed events
            }
        }
    }

    private static bool TryDeserializePropertyValue(JsonElement jsonProperty, Type propertyType, out object? value)
    {
        value = null;

        if (jsonProperty.ValueKind == JsonValueKind.Null)
        {
            return true;
        }

        if (TryDeserializeBaseEnumLike(jsonProperty, propertyType, out value))
        {
            return true;
        }

        try
        {
            value = jsonProperty.Deserialize(propertyType, EventJsonOptions);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool TryDeserializeBaseEnumLike(JsonElement jsonProperty, Type propertyType, out object? value)
    {
        value = null;

        if (!propertyType.IsAbstract && !propertyType.IsInterface)
        {
            return false;
        }

        string? rawValue = jsonProperty.ValueKind switch
        {
            JsonValueKind.String => jsonProperty.GetString(),
            JsonValueKind.Object when TryGetJsonProperty(jsonProperty, "Value", out var v) && v.ValueKind == JsonValueKind.String => v.GetString(),
            JsonValueKind.Object when TryGetJsonProperty(jsonProperty, "Name", out var n) && n.ValueKind == JsonValueKind.String => n.GetString(),
            _ => null
        };

        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return false;
        }

        var fromValueMethod = propertyType.GetMethod(
            "FromValue",
            BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy,
            new[] { typeof(string) });
        if (fromValueMethod is not null)
        {
            value = fromValueMethod.Invoke(null, new object[] { rawValue });
            if (value is not null)
            {
                return true;
            }
        }

        var fromNameMethod = propertyType.GetMethod(
            "FromName",
            BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy,
            new[] { typeof(string) });
        if (fromNameMethod is not null)
        {
            value = fromNameMethod.Invoke(null, new object[] { rawValue });
            if (value is not null)
            {
                return true;
            }
        }

        return false;
    }

    private static void MapNestedIdToHazardId(object instance, JsonElement root)
    {
        var hazardIdProperty = instance
            .GetType()
            .GetProperty("HazardId", BindingFlags.Public | BindingFlags.Instance);

        if (hazardIdProperty == null || !hazardIdProperty.CanWrite || hazardIdProperty.PropertyType != typeof(string))
        {
            return;
        }

        var currentValue = hazardIdProperty.GetValue(instance) as string;
        if (!string.IsNullOrWhiteSpace(currentValue))
        {
            return;
        }

        if (root.TryGetProperty("Id", out var idElement) &&
            idElement.ValueKind == JsonValueKind.Object &&
            idElement.TryGetProperty("Value", out var valueElement) &&
            valueElement.ValueKind == JsonValueKind.String)
        {
            hazardIdProperty.SetValue(instance, valueElement.GetString());
        }
    }

    private static bool TryGetJsonProperty(JsonElement root, string propertyName, out JsonElement value)
    {
        foreach (var property in root.EnumerateObject())
        {
            if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Executes an integration event by reconstructing it and publishing it immediately
    /// </summary>
    private async Task<Result> ExecuteIntegrationEvent(QueuedEvent queuedEvent)
    {
        try
        {
            if (!_queuedEventTypeRegistry.TryResolveIntegrationEventType(queuedEvent.EventType, out var eventType))
            {
                var knownTypes = string.Join(", ", _queuedEventTypeRegistry.GetKnownTypes(EventCategory.IntegrationEvent).OrderBy(x => x));
                _logger.LogApplicationWarning("[QUEUE] Unknown integration event type {EventType}. Known integration types: {KnownTypes}", queuedEvent.EventType, knownTypes);
                return Result.Failure(new Error("UNKNOWN_INTEGRATION_EVENT_TYPE", $"Unknown integration event type: {queuedEvent.EventType}"));
            }

            var integrationEvent = (IBaseIntegrationEvent?)System.Text.Json.JsonSerializer.Deserialize(queuedEvent.EventData, eventType);
            if (integrationEvent == null)
            {
                return Result.Failure(new Error("DESERIALIZATION_FAILED", $"Could not deserialize integration event: {queuedEvent.EventType}"));
            }

            var executionResult = await _eventBus.PublishIntegrationEventAsync(integrationEvent, EventExecutionMode.Immediate);

            if (executionResult.IsFailure)
            {
                _logger.LogApplicationError("[QUEUE] Failed to execute integration event {EventType}: {Error}", queuedEvent.EventType, executionResult.Error.Message);
            }
            
            return executionResult;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "[QUEUE] Exception executing integration event {EventType}", queuedEvent.EventType);
            return Result.Failure(new Error("INTEGRATION_EVENT_EXECUTION_FAILED", $"Failed to execute integration event: {ex.Message}"));
        }
    }

    /// <summary>
    /// Executes a UI event by processing it through appropriate UI handlers
    /// DEMONSTRATION: Shows how UI events should trigger actual UI updates
    /// </summary>
    private async Task<Result> ExecuteUIEvent(QueuedEvent queuedEvent)
    {
        try
        {
            if (!_queuedEventTypeRegistry.TryResolveUIEventType(queuedEvent.EventType, out var eventType))
            {
                var knownTypes = string.Join(", ", _queuedEventTypeRegistry.GetKnownTypes(EventCategory.UIEvent).OrderBy(x => x));
                _logger.LogApplicationWarning("[QUEUE] Unknown UI event type {EventType}. Known UI types: {KnownTypes}", queuedEvent.EventType, knownTypes);
                return Result.Failure(new Error("UNKNOWN_UI_EVENT_TYPE", $"Unknown UI event type: {queuedEvent.EventType}"));
            }

            var uiEvent = (IBaseUIEvent?)JsonSerializer.Deserialize(queuedEvent.EventData, eventType, EventJsonOptions);
            if (uiEvent == null)
            {
                return Result.Failure(new Error("DESERIALIZATION_FAILED", $"Could not deserialize UI event: {queuedEvent.EventType}"));
            }

            var reportId = !string.IsNullOrWhiteSpace(uiEvent.ReportId) ? uiEvent.ReportId : queuedEvent.ReportId;
            
            var executionResult = await _eventBus.PublishUIEventAsync(uiEvent, EventExecutionMode.Immediate);

            if (executionResult.IsFailure)
            {
                _logger.LogApplicationError("[QUEUE] Failed to execute UI event {EventType}: {Error}", queuedEvent.EventType, executionResult.Error.Message);
            }

            return executionResult;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "[QUEUE] Exception executing UI event {EventType}", queuedEvent.EventType);
            return Result.Failure(new Error("UI_EVENT_EXECUTION_FAILED", $"Failed to execute UI event: {ex.Message}"));
        }
    }

    #endregion

    #region Queue Management Methods

    /// <summary>
    /// Clears ALL events from the queue (pending, processed, failed, cancelled)
    /// WARNING: This removes everything - typically used with database truncate/reimport
    /// </summary>
    public async Task<Result<int>> ClearAllEventsAsync()
    {
        try
        {
            var clearResult = await _eventQueueDataService.ClearAllAsync();
            if (clearResult.IsFailure)
            {
                return Result.Failure<int>(clearResult.Error);
            }

            var totalCount = clearResult.Value;

            _logger.LogApplicationWarning("Cleared ALL {Count} events from queue - Complete reset", totalCount);
            return Result<int>.Success(totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError(ex, "Failed to clear all events from queue");
            return Result<int>.Failure<int>(new Error("CLEAR_ALL_FAILED", $"Failed to clear all events: {ex.Message}"));
        }
    }

    #endregion
}

