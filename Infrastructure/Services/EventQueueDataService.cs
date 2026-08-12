//-----------------------------------------------------------------------
// <copyright file="EventQueueDataService.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Data service coordinating persisted EventQueue repository operations.
//                  Infrastructure service providing external system integration
//                  and technical functionality support.
// </copyright>
//-----------------------------------------------------------------------

using SMS_Domain.Interfaces;
using SMS_Domain.ValueObjects;
using SMS_Domain.Enums;
using SMS_Infrastructure.Interfaces;

namespace SMS_Infrastructure.Services;

public sealed class EventQueueDataService : BaseDataService<EventQueueDataService>, IEventQueueDataService
{
    private readonly IEventQueueRepository _repository;
    private readonly ILogger<EventQueueDataService> _logger;
    private readonly string _logheader;

    public EventQueueDataService(
        IEventQueueRepository repository,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<EventQueueDataService> logger,
        IConfiguration configuration)
        : base(logger, serviceScopeFactory, configuration)
    {
        _logger = base.Logger;
        _logheader = base.LogHeader;
        _repository = repository;

        _logger.LogInfrastructureInformation(InfrastructureEventIds.InfrastructureEvent, $"{_logheader} {repository.GetType().Name}");
    }

    public Task<Result<QueuedEvent>> EnqueueDomainEventAsync<T>(T domainEvent, string? queuedBy = null, CancellationToken ct = default) where T : IBaseDomainEvent
    {
        var queuedEvent = QueuedEvent.FromDomainEvent(domainEvent, queuedBy);
        return _repository.CreateAsync(queuedEvent, ct);
    }

    public Task<Result<QueuedEvent>> EnqueueIntegrationEventAsync<T>(T integrationEvent, string? queuedBy = null, CancellationToken ct = default) where T : IBaseIntegrationEvent
    {
        var queuedEvent = QueuedEvent.FromIntegrationEvent(integrationEvent, queuedBy);
        return _repository.CreateAsync(queuedEvent, ct);
    }

    public Task<Result<QueuedEvent>> EnqueueUIEventAsync<T>(T uiEvent, string? queuedBy = null, CancellationToken ct = default) where T : IBaseUIEvent
    {
        var queuedEvent = QueuedEvent.FromUIEvent(uiEvent, queuedBy);
        return _repository.CreateAsync(queuedEvent, ct);
    }

    public Task<Result<QueuedEvent>> GetQueuedEventAsync(string queueCode, CancellationToken ct = default)
        => _repository.GetByQueueCodeAsync(queueCode, ct);

    public Task<Result<QueuedEvent>> LeaseQueuedEventAsync(string queueCode, string worker, int lockSeconds = 60, CancellationToken ct = default)
        => _repository.LeaseByQueueCodeAsync(queueCode, worker, lockSeconds, ct);

    public Task<Result<List<QueuedEvent>>> GetPendingEventsAsync(int maxResults = 100, EventCategory? eventCategory = null, CancellationToken ct = default)
        => _repository.GetPendingAsync(maxResults, eventCategory, ct);

    public Task<Result<List<QueuedEvent>>> GetEventsByStatusAsync(QueuedEventStatus status, int maxResults = 100, EventCategory? eventCategory = null, CancellationToken ct = default)
        => _repository.GetByStatusAsync(status, maxResults, eventCategory, ct);

    public Task<Result<List<QueuedEvent>>> LeaseBatchAsync(int batchSize, string worker, int lockSeconds = 60, CancellationToken ct = default)
        => _repository.LeaseBatchAsync(batchSize, worker, lockSeconds, ct);

    public Task<Result<bool>> MarkProcessedAsync(string queueCode, string worker, CancellationToken ct = default)
        => _repository.MarkProcessedAsync(queueCode, worker, ct);

    public Task<Result<bool>> MarkFailedAsync(string queueCode, string worker, string lastError, int backoffSeconds = 30, CancellationToken ct = default)
        => _repository.MarkFailedAsync(queueCode, worker, lastError, backoffSeconds, ct);

    public Task<Result<bool>> CancelAsync(string queueCode, string cancelledBy = "", CancellationToken ct = default)
        => _repository.CancelAsync(queueCode, cancelledBy, ct);

    public Task<Result<int>> ClearCompletedAsync(string clearedBy = "", CancellationToken ct = default)
        => _repository.ClearCompletedAsync(clearedBy, ct);

    public Task<Result<int>> ClearAllAsync(CancellationToken ct = default)
        => _repository.ClearAllAsync(ct);

    public Task<Result<int>> RecoverExpiredLocksAsync(int recoveryDelaySeconds = 5, string recoveredBy = "", CancellationToken ct = default)
        => _repository.RecoverExpiredLocksAsync(recoveryDelaySeconds, recoveredBy, ct);

    public Task<Result<QueueStatistics>> GetStatsAsync(CancellationToken ct = default)
        => _repository.GetStatsAsync(ct);
}
