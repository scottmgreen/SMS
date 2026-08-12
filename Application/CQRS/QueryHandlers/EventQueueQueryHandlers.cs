//-----------------------------------------------------------------------
// <copyright file="EventQueueQueryHandlers.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Query handlers for EventQueue retrieval and monitoring operations.
//                  Implements query handlers for processing read operations.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using SMS_Domain.ValueObjects;
using SMS_Application.Queries;

namespace SMS_Application.QueryHandlers;

public class GetQueuedEventsQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetQueuedEventsQuery, Result<IEnumerable<QueuedEvent>>>
{
    private readonly IEventQueueService _service;
    private readonly ILogger<GetQueuedEventsQueryHandler> _logger;

    public GetQueuedEventsQueryHandler(
        IEventQueueService service,
        ILogger<GetQueuedEventsQueryHandler> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<IEnumerable<QueuedEvent>>> HandleAsync(GetQueuedEventsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Processing GetQueuedEventsQuery (Status: {Status}, Type: {Type}, Max: {Max})",
                request.Status, request.EventType, request.MaxResults);

            return await _service.GetQueuedEventsAsync(request.Status, request.EventType, request.MaxResults).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("GetQueuedEventsQuery operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error while retrieving queued events", ApplicationEventIds.Error, ex);
            return Result<IEnumerable<QueuedEvent>>.Failure<IEnumerable<QueuedEvent>>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}

public class GetQueuedEventByCodeQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetQueuedEventByCodeQuery, Result<QueuedEvent>>
{
    private readonly IEventQueueService _service;
    private readonly ILogger<GetQueuedEventByCodeQueryHandler> _logger;

    public GetQueuedEventByCodeQueryHandler(
        IEventQueueService service,
        ILogger<GetQueuedEventByCodeQueryHandler> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<QueuedEvent>> HandleAsync(GetQueuedEventByCodeQuery request, CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.QueueCode))
            {
                return Result<QueuedEvent>.Failure<QueuedEvent>(DomainErrors.GeneralError.InvalidParameters);
            }

            _logger.LogApplicationInformation("Processing GetQueuedEventByCodeQuery for QueueCode: {QueueCode}", request.QueueCode);
            return await _service.GetQueuedEventAsync(request.QueueCode).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("GetQueuedEventByCodeQuery operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error while retrieving queued event by code", ApplicationEventIds.Error, ex);
            return Result<QueuedEvent>.Failure<QueuedEvent>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}

public class GetEventQueueStatisticsQueryHandler : BaseQueryBundle, IBaseRequestHandler<GetEventQueueStatisticsQuery, Result<QueueStatistics>>
{
    private readonly IEventQueueService _service;
    private readonly ILogger<GetEventQueueStatisticsQueryHandler> _logger;

    public GetEventQueueStatisticsQueryHandler(
        IEventQueueService service,
        ILogger<GetEventQueueStatisticsQueryHandler> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<QueueStatistics>> HandleAsync(GetEventQueueStatisticsQuery request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Processing GetEventQueueStatisticsQuery");
            return await _service.GetQueueStatisticsAsync().ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("GetEventQueueStatisticsQuery operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error while retrieving event queue statistics", ApplicationEventIds.Error, ex);
            return Result<QueueStatistics>.Failure<QueueStatistics>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}

