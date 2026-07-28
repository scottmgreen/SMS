//-----------------------------------------------------------------------
// <copyright file="IEventQueueDataService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Data service contract for persisted EventQueue operations.
//                  Infrastructure service contract defining data access operations
//                  and external system integration interfaces.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Interfaces;
using SMS_Domain.ValueObjects;
using SMS_Domain.Enums;

namespace SMS_Infrastructure.Interfaces;

/// <summary>
/// EventQueue Data Service Interface
/// </summary>
public interface IEventQueueDataService
{
    Task<Result<QueuedEvent>> EnqueueDomainEventAsync<T>(T domainEvent, string? queuedBy = null, CancellationToken ct = default) where T : IBaseDomainEvent;
    Task<Result<QueuedEvent>> EnqueueIntegrationEventAsync<T>(T integrationEvent, string? queuedBy = null, CancellationToken ct = default) where T : IBaseIntegrationEvent;
    Task<Result<QueuedEvent>> EnqueueUIEventAsync<T>(T uiEvent, string? queuedBy = null, CancellationToken ct = default) where T : IBaseUIEvent;

    Task<Result<QueuedEvent>> GetQueuedEventAsync(Guid queueGuid, CancellationToken ct = default);
    Task<Result<QueuedEvent>> LeaseQueuedEventAsync(Guid queueGuid, string worker, int lockSeconds = 60, CancellationToken ct = default);
    Task<Result<List<QueuedEvent>>> GetPendingEventsAsync(int maxResults = 100, EventCategory? eventCategory = null, CancellationToken ct = default);
    Task<Result<List<QueuedEvent>>> GetEventsByStatusAsync(QueuedEventStatus status, int maxResults = 100, EventCategory? eventCategory = null, CancellationToken ct = default);
    Task<Result<List<QueuedEvent>>> LeaseBatchAsync(int batchSize, string worker, int lockSeconds = 60, CancellationToken ct = default);

    Task<Result<bool>> MarkProcessedAsync(Guid queueGuid, string worker, CancellationToken ct = default);
    Task<Result<bool>> MarkFailedAsync(Guid queueGuid, string worker, string lastError, int backoffSeconds = 30, CancellationToken ct = default);
    Task<Result<bool>> CancelAsync(Guid queueGuid, string cancelledBy = "", CancellationToken ct = default);

    Task<Result<int>> ClearCompletedAsync(string clearedBy = "", CancellationToken ct = default);
    Task<Result<int>> ClearAllAsync(CancellationToken ct = default);
    Task<Result<int>> RecoverExpiredLocksAsync(int recoveryDelaySeconds = 5, string recoveredBy = "", CancellationToken ct = default);

    Task<Result<QueueStatistics>> GetStatsAsync(CancellationToken ct = default);
}
