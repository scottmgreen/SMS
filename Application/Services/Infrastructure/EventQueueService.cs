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
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
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
    private readonly ConcurrentDictionary<Guid, QueuedEvent> _eventQueue = new();
    private static readonly JsonSerializerOptions EventJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public EventQueueService(
        ILogger<EventQueueService> logger,
        IBaseEventBus eventBus)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _eventBus = eventBus ?? throw new ArgumentNullException(nameof(eventBus));
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

            var queuedEvent = QueuedEvent.FromDomainEvent(domainEvent, queuedBy);
            _eventQueue.TryAdd(queuedEvent.Id, queuedEvent);

            _logger.LogInformation("?? Queued domain event {EventType} with ID {EventId} (Queue ID: {QueueId}). Total queue size: {QueueSize}",
                domainEvent.EventType, domainEvent.EventId, queuedEvent.Id, _eventQueue.Count);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to queue domain event {EventType}", typeof(T).Name);
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

            var queuedEvent = QueuedEvent.FromIntegrationEvent(integrationEvent, queuedBy);
            _eventQueue.TryAdd(queuedEvent.Id, queuedEvent);

            _logger.LogInformation("Queued integration event {EventType} for {TargetSystem} (Queue ID: {QueueId})",
                integrationEvent.EventType, integrationEvent.TargetSystem, queuedEvent.Id);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to queue integration event {EventType}", typeof(T).Name);
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

            var queuedEvent = QueuedEvent.FromUIEvent(uiEvent, queuedBy);
            _eventQueue.TryAdd(queuedEvent.Id, queuedEvent);

            _logger.LogInformation("Queued UI event {EventType} for {TargetComponent} (Queue ID: {QueueId})",
                uiEvent.EventType, uiEvent.TargetComponent, queuedEvent.Id);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to queue UI event {EventType}", typeof(T).Name);
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
            var events = _eventQueue.Values.AsEnumerable();

            if (status.HasValue)
            {
                events = events.Where(e => e.Status == status.Value);
            }

            if (eventType.HasValue)
            {
                events = events.Where(e => e.EventCategory == eventType.Value);
            }

            // Order by priority (descending) then by queued time (ascending)
            events = events.OrderByDescending(e => (int)e.Priority)
                           .ThenBy(e => e.QueuedAt);

            if (maxResults.HasValue)
            {
                events = events.Take(maxResults.Value);
            }

            var result = events.ToList();

            _logger.LogDebug("Retrieved {EventCount} queued events (Status: {Status}, Type: {EventType}). Total in queue: {TotalCount}",
                result.Count, status, eventType, _eventQueue.Count);

            return Result.Success<IEnumerable<QueuedEvent>>(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve queued events");
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
            if (_eventQueue.TryGetValue(eventId, out var queuedEvent))
            {
                return Result.Success(queuedEvent);
            }

            return Result.Failure<QueuedEvent>(new Error("QUEUE_EVENT_NOT_FOUND", $"Queued event with ID {eventId} not found"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve queued event {EventId}", eventId);
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
            if (!_eventQueue.TryGetValue(eventId, out var queuedEvent))
            {
                return Result.Failure(new Error("QUEUE_EVENT_NOT_FOUND", $"Queued event with ID {eventId} not found"));
            }

            if (queuedEvent.Status != QueuedEventStatus.Pending)
            {
                return Result.Failure(new Error("QUEUE_EVENT_NOT_PENDING", $"Event {eventId} is not in pending status (Current: {queuedEvent.Status})"));
            }

            // Mark as processing
            var processingEvent = queuedEvent with { Status = QueuedEventStatus.Processing };
            _eventQueue.TryUpdate(eventId, processingEvent, queuedEvent);

            _logger.LogInformation("Executing queued event {EventType} (Queue ID: {QueueId}) by {ExecutedBy}",
                queuedEvent.EventType, eventId, executedBy ?? "System");

            Result executionResult;

            // Execute based on event category
            switch (queuedEvent.EventCategory)
            {
                case EventCategory.DomainEvent:
                    executionResult = await ExecuteDomainEvent(queuedEvent);
                    break;

                case EventCategory.IntegrationEvent:
                    executionResult = await ExecuteIntegrationEvent(queuedEvent);
                    break;

                case EventCategory.UIEvent:
                    executionResult = await ExecuteUIEvent(queuedEvent);
                    break;

                default:
                    executionResult = Result.Failure(new Error("QUEUE_UNKNOWN_TYPE", $"Unknown event type: {queuedEvent.EventCategory}"));
                    break;
            }

            // Update event status based on execution result
            QueuedEvent updatedEvent;
            if (executionResult.IsSuccess)
            {
                updatedEvent = processingEvent.MarkAsProcessed();
                _logger.LogInformation("Successfully executed queued event {EventType} (Queue ID: {QueueId})",
                    queuedEvent.EventType, eventId);
            }
            else
            {
                updatedEvent = processingEvent.MarkAsFailed(executionResult.Error.Message);
                _logger.LogWarning("Failed to execute queued event {EventType} (Queue ID: {QueueId}): {Error}",
                    queuedEvent.EventType, eventId, executionResult.Error.Message);
            }

            _eventQueue.TryUpdate(eventId, updatedEvent, processingEvent);
            return executionResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute queued event {EventId}", eventId);

            // Mark as failed if we can
            if (_eventQueue.TryGetValue(eventId, out var failedEvent))
            {
                var errorEvent = failedEvent.MarkAsFailed(ex.Message);
                _eventQueue.TryUpdate(eventId, errorEvent, failedEvent);
            }

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
            var pendingEvents = _eventQueue.Values
                .Where(e => e.Status == QueuedEventStatus.Pending)
                .Where(e => eventType == null || e.EventCategory == eventType.Value)
                .OrderByDescending(e => (int)e.Priority)
                .ThenBy(e => e.QueuedAt)
                .ToList();

            _logger.LogInformation("Executing {EventCount} pending events (Type: {EventType}) by {ExecutedBy}",
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
                _logger.LogWarning(errorMessage);
                return Result.Failure<int>(new Error("QUEUE_BATCH_PARTIAL_FAILURE", errorMessage));
            }

            _logger.LogInformation("Successfully executed all {EventCount} pending events", successCount);
            return Result.Success(successCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute pending events");
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
            if (!_eventQueue.TryGetValue(eventId, out var queuedEvent))
            {
                return Result.Failure(new Error("QUEUE_EVENT_NOT_FOUND", $"Queued event with ID {eventId} not found"));
            }

            if (queuedEvent.Status != QueuedEventStatus.Pending)
            {
                return Result.Failure(new Error("QUEUE_EVENT_NOT_PENDING", $"Event {eventId} cannot be cancelled (Current status: {queuedEvent.Status})"));
            }

            var cancelledEvent = queuedEvent with { Status = QueuedEventStatus.Cancelled };
            _eventQueue.TryUpdate(eventId, cancelledEvent, queuedEvent);

            _logger.LogInformation("Cancelled queued event {EventType} (Queue ID: {QueueId}) by {CancelledBy}",
                queuedEvent.EventType, eventId, cancelledBy ?? "System");

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cancel queued event {EventId}", eventId);
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
            var completedEvents = _eventQueue.Values
                .Where(e => e.Status == QueuedEventStatus.Processed || 
                           e.Status == QueuedEventStatus.Failed || 
                           e.Status == QueuedEventStatus.Cancelled)
                .ToList();

            int removedCount = 0;
            foreach (var completedEvent in completedEvents)
            {
                if (_eventQueue.TryRemove(completedEvent.Id, out _))
                {
                    removedCount++;
                }
            }

            _logger.LogInformation("Cleared {RemovedCount} completed events from queue", removedCount);
            return Result.Success(removedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear completed events");
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
            var events = _eventQueue.Values.ToList();

            var statistics = new QueueStatistics
            {
                PendingCount = events.Count(e => e.Status == QueuedEventStatus.Pending),
                ProcessedCount = events.Count(e => e.Status == QueuedEventStatus.Processed),
                FailedCount = events.Count(e => e.Status == QueuedEventStatus.Failed),
                CancelledCount = events.Count(e => e.Status == QueuedEventStatus.Cancelled),
                ByEventType = events.GroupBy(e => e.EventCategory)
                    .ToDictionary(g => g.Key, g => new EventTypeStatistics
                    {
                        Pending = g.Count(e => e.Status == QueuedEventStatus.Pending),
                        Processed = g.Count(e => e.Status == QueuedEventStatus.Processed),
                        Failed = g.Count(e => e.Status == QueuedEventStatus.Failed),
                        Cancelled = g.Count(e => e.Status == QueuedEventStatus.Cancelled)
                    }),
                ByPriority = events.GroupBy(e => e.Priority)
                    .ToDictionary(g => g.Key, g => g.Count())
            };

            return Result.Success(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get queue statistics");
            return Result.Failure<QueueStatistics>(new Error("QUEUE_STATS_FAILED", $"Failed to get statistics: {ex.Message}"));
        }
    }

    #region Private Event Execution Methods

    /// <summary>
    /// Executes a domain event by reconstructing it from the queued event data and publishing it immediately
    /// </summary>
    // Generic map from EventType.Value to .NET event type for domain events
    private static readonly Dictionary<string, Type> DomainEventTypeMap = new()
    {
        { Domain.Enums.EventType.HazardCreated.Value, typeof(SMS_Domain.Events.HazardCreatedEvent) },
        { Domain.Enums.EventType.HazardUpdated.Value, typeof(SMS_Domain.Events.HazardUpdatedEvent) },
        { Domain.Enums.EventType.HazardDeleted.Value, typeof(SMS_Domain.Events.HazardDeletedEvent) },
        { Domain.Enums.EventType.ReportCreated.Value, typeof(SMS_Domain.Events.ReportCreatedEvent) },
        { Domain.Enums.EventType.ReportUpdated.Value, typeof(SMS_Domain.Events.ReportUpdatedEvent) },
        { Domain.Enums.EventType.ReportClosed.Value, typeof(SMS_Domain.Events.ReportClosedEvent) },
        { Domain.Enums.EventType.RiskAssessmentCreated.Value, typeof(SMS_Domain.Events.RiskAssessmentCreatedEvent) },
        { Domain.Enums.EventType.RiskAssessmentUpdated.Value, typeof(SMS_Domain.Events.RiskAssessmentUpdatedEvent) },
        { Domain.Enums.EventType.MitigationCreated.Value, typeof(SMS_Domain.Events.MitigationCreatedEvent) },
        { Domain.Enums.EventType.MitigationApprovalRequested.Value, typeof(SMS_Domain.Events.MitigationApprovalRequestedEvent) },
        { Domain.Enums.EventType.MitigationApprovalApproved.Value, typeof(SMS_Domain.Events.MitigationApprovalApprovedEvent) },
        { Domain.Enums.EventType.MitigationStatusChanged.Value, typeof(SMS_Domain.Events.MitigationStatusChangedEvent) },
        // Add more as needed
    };
    private async Task<Result> ExecuteDomainEvent(QueuedEvent queuedEvent)
    {
        try
        {
            _logger.LogInformation("[QUEUE] Executing domain event {EventType} (Queue ID: {QueueId})", queuedEvent.EventType, queuedEvent.Id);

            if (!DomainEventTypeMap.TryGetValue(queuedEvent.EventType, out var eventType))
            {
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

            // 3. Invoke with the correct parameters
            var task = (Task<Result>)publishMethod.Invoke(_eventBus, new object[] { domainEvent, EventExecutionMode.Immediate, CancellationToken.None });
            var executionResult = await task;

            if (executionResult.IsFailure)
            {
                _logger.LogError("[QUEUE] Failed to execute domain event {EventType}: {Error}", queuedEvent.EventType, executionResult.Error.Message);
            }
            

            return executionResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[QUEUE] Exception executing domain event {EventType}", queuedEvent.EventType);
            return Result.Failure(new Error("DOMAIN_EVENT_EXECUTION_FAILED", $"Failed to execute domain event: {ex.Message}"));
        }
    }

    private object? DeserializeDomainEvent(string eventData, Type eventType)
    {
        try
        {
            return JsonSerializer.Deserialize(eventData, eventType, EventJsonOptions);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogDebug(ex, "[QUEUE] Standard deserialization failed for {EventType}. Attempting fallback construction.", eventType.Name);
            return DeserializeWithIdCtorFallback(eventData, eventType);
        }
    }

    private object? DeserializeWithIdCtorFallback(string eventData, Type eventType)
    {
        using var doc = JsonDocument.Parse(eventData);
        var root = doc.RootElement;

        var ctor = eventType
            .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(c =>
            {
                var parameters = c.GetParameters();
                return parameters.Length == 1 &&
                       (parameters[0].ParameterType == typeof(SMSEventID) || parameters[0].ParameterType == typeof(string));
            });

        if (ctor == null)
        {
            _logger.LogWarning("[QUEUE] No compatible constructor found for fallback on {EventType}", eventType.Name);
            return null;
        }

        var idValue = TryGetIdValue(root) ?? Guid.NewGuid().ToString();

        object? instance = ctor.GetParameters()[0].ParameterType == typeof(SMSEventID)
            ? ctor.Invoke(new object[] { new SMSEventID(idValue) })
            : ctor.Invoke(new object[] { idValue });

        if (instance == null)
        {
            return null;
        }

        PopulateSettableProperties(instance, root);
        MapNestedIdToHazardId(instance, root);

        return instance;
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
                var value = jsonProperty.Deserialize(property.PropertyType, EventJsonOptions);
                property.SetValue(instance, value);
            }
            catch
            {
                // best-effort hydration for replayed events
            }
        }
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
    // Generic map from EventType.Value to .NET event type for integration events
    private static readonly Dictionary<string, Type> IntegrationEventTypeMap = new()
    {
        { Domain.Enums.EventType.EmailNotification.Value, typeof(SMS_Domain.Events.EmailNotificationEvent) },
        // Add more integration event types as needed
    };

    private static readonly Dictionary<string, Type> UIEventTypeMap = new()
    {
        { Domain.Enums.EventType.UINotification.Value, typeof(SMS_Domain.Events.UIEvents.UINotificationEvent) },
        { SMS_Domain.Events.SPIDashboardRefreshEvent.TypeValue, typeof(SMS_Domain.Events.SPIDashboardRefreshEvent) },
        { SMS_Domain.Events.Test.TestUIEvent.TypeValue, typeof(SMS_Domain.Events.Test.TestUIEvent) },
    };

    private async Task<Result> ExecuteIntegrationEvent(QueuedEvent queuedEvent)
    {
        try
        {
            if (!IntegrationEventTypeMap.TryGetValue(queuedEvent.EventType, out var eventType))
            {
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
                _logger.LogError("[QUEUE] Failed to execute integration event {EventType}: {Error}", queuedEvent.EventType, executionResult.Error.Message);
            }
            
            return executionResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[QUEUE] Exception executing integration event {EventType}", queuedEvent.EventType);
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
            if (!UIEventTypeMap.TryGetValue(queuedEvent.EventType, out var eventType))
            {
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
                _logger.LogError("[QUEUE] Failed to execute UI event {EventType}: {Error}", queuedEvent.EventType, executionResult.Error.Message);
            }

            return executionResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[QUEUE] Exception executing UI event {EventType}", queuedEvent.EventType);
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
            var totalCount = _eventQueue.Count;
            _eventQueue.Clear();

            _logger.LogWarning("Cleared ALL {Count} events from queue - Complete reset", totalCount);
            return Result<int>.Success(totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear all events from queue");
            return Result<int>.Failure<int>(new Error("CLEAR_ALL_FAILED", $"Failed to clear all events: {ex.Message}"));
        }
    }

    #endregion
}
