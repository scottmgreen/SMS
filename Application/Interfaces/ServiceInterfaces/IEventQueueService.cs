//-----------------------------------------------------------------------
// <copyright file="IEventQueueService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Interface for event queue management and manual execution.
//                  Supports testing and controlled event processing scenarios.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.ValueObjects;
using SMS_Domain.Common;
using SMS_Domain.Interfaces;

namespace SMS_Application.Interfaces;

/// <summary>
/// Service for managing queued events and manual execution
/// Enables testing and controlled processing of EventBus events
/// </summary>
public interface IEventQueueService
{
    /// <summary>
    /// Queues a domain event for manual execution
    /// </summary>
    Task<Result> QueueDomainEventAsync<T>(T domainEvent, string? queuedBy = null) where T : IBaseDomainEvent;

    /// <summary>
    /// Queues an integration event for manual execution
    /// </summary>
    Task<Result> QueueIntegrationEventAsync<T>(T integrationEvent, string? queuedBy = null) where T : IBaseIntegrationEvent;

    /// <summary>
    /// Queues a UI event for manual execution
    /// </summary>
    Task<Result> QueueUIEventAsync<T>(T uiEvent, string? queuedBy = null) where T : IBaseUIEvent;

    /// <summary>
    /// Gets all queued events with optional filtering
    /// </summary>
    Task<Result<IEnumerable<QueuedEvent>>> GetQueuedEventsAsync(
        QueuedEventStatus? status = null,
        EventCategory? eventType = null,
        int? maxResults = null);

    /// <summary>
    /// Gets a specific queued event by ID
    /// </summary>
    Task<Result<QueuedEvent>> GetQueuedEventAsync(Guid eventId);

    /// <summary>
    /// Manually executes a queued event
    /// </summary>
    Task<Result> ExecuteQueuedEventAsync(Guid eventId, string? executedBy = null);

    /// <summary>
    /// Executes all pending events of a specific type
    /// </summary>
    Task<Result<int>> ExecuteAllPendingEventsAsync(EventCategory? eventType = null, string? executedBy = null);

    /// <summary>
    /// Cancels a queued event
    /// </summary>
    Task<Result> CancelQueuedEventAsync(Guid eventId, string? cancelledBy = null);

    /// <summary>
    /// Clears all processed and failed events
    /// </summary>
    Task<Result<int>> ClearCompletedEventsAsync();

    /// <summary>
    /// Clears ALL events from the queue (pending, processed, failed, cancelled)
    /// WARNING: This removes everything - typically used with database truncate/reimport
    /// </summary>
    Task<Result<int>> ClearAllEventsAsync();

    /// <summary>
    /// Gets queue statistics
    /// </summary>
    Task<Result<QueueStatistics>> GetQueueStatisticsAsync();
}
