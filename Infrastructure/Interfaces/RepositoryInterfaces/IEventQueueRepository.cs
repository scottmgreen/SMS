//-----------------------------------------------------------------------
// <copyright file="IEventQueueRepository.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Repository contract for persisted EventQueue operations.
//                  Infrastructure service contract defining data access operations
//                  and external system integration interfaces.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.ValueObjects;
using SMS_Domain.Enums;

namespace SMS_Infrastructure.Interfaces;

/// <summary>
/// EventQueue Repository Interface
/// </summary>
public interface IEventQueueRepository
{
    Task<Result<QueuedEvent>> CreateAsync(QueuedEvent queuedEvent, CancellationToken ct = default);
    Task<Result<QueuedEvent>> GetByQueueCodeAsync(string queueCode, CancellationToken ct = default);
    Task<Result<QueuedEvent>> LeaseByQueueCodeAsync(string queueCode, string worker, int lockSeconds = 60, CancellationToken ct = default);
    Task<Result<List<QueuedEvent>>> GetPendingAsync(int maxResults = 100, EventCategory? eventCategory = null, CancellationToken ct = default);
    Task<Result<List<QueuedEvent>>> GetByStatusAsync(QueuedEventStatus status, int maxResults = 100, EventCategory? eventCategory = null, CancellationToken ct = default);
    Task<Result<List<QueuedEvent>>> LeaseBatchAsync(int batchSize, string worker, int lockSeconds = 60, CancellationToken ct = default);
    Task<Result<bool>> MarkProcessedAsync(string queueCode, string worker, CancellationToken ct = default);
    Task<Result<bool>> MarkFailedAsync(string queueCode, string worker, string lastError, int backoffSeconds = 30, CancellationToken ct = default);
    Task<Result<bool>> CancelAsync(string queueCode, string cancelledBy = "", CancellationToken ct = default);
    Task<Result<int>> ClearCompletedAsync(string clearedBy = "", CancellationToken ct = default);
    Task<Result<int>> ClearAllAsync(CancellationToken ct = default);
    Task<Result<int>> RecoverExpiredLocksAsync(int recoveryDelaySeconds = 5, string recoveredBy = "", CancellationToken ct = default);
    Task<Result<QueueStatistics>> GetStatsAsync(CancellationToken ct = default);
}
