//-----------------------------------------------------------------------
// <copyright file="EventQueueCommandHandlers.cs" company="SMS Safety Management System">
//     Author: SMS Development Team
//     Copyright (c) 2024 SMS Safety Management System. All rights reserved.
//     Description: Command handlers for EventQueue execution and lifecycle operations.
//                  Implements command handlers for processing write operations.
// </copyright>
//-----------------------------------------------------------------------

using Microsoft.Extensions.Logging;
using SMS_Application.Commands;

namespace SMS_Application.CommandHandlers;

public class ExecuteQueuedEventCommandHandler : BaseCommandBundle, IBaseRequestHandler<ExecuteQueuedEventCommand, Result>
{
    private readonly IEventQueueService _service;
    private readonly ILogger<ExecuteQueuedEventCommandHandler> _logger;

    public ExecuteQueuedEventCommandHandler(
        IEventQueueService service,
        ILogger<ExecuteQueuedEventCommandHandler> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result> HandleAsync(ExecuteQueuedEventCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request.EventId == Guid.Empty)
            {
                return Result.Failure(DomainErrors.GeneralError.InvalidParameters);
            }

            _logger.LogApplicationInformation("Processing ExecuteQueuedEventCommand for EventId: {EventId}", request.EventId);
            return await _service.ExecuteQueuedEventAsync(request.EventId, request.ExecutedBy).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("ExecuteQueuedEventCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error while executing queued event", ApplicationEventIds.Error, ex);
            return Result.Failure(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}

public class ExecuteAllPendingQueuedEventsCommandHandler : BaseCommandBundle, IBaseRequestHandler<ExecuteAllPendingQueuedEventsCommand, Result<int>>
{
    private readonly IEventQueueService _service;
    private readonly ILogger<ExecuteAllPendingQueuedEventsCommandHandler> _logger;

    public ExecuteAllPendingQueuedEventsCommandHandler(
        IEventQueueService service,
        ILogger<ExecuteAllPendingQueuedEventsCommandHandler> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<int>> HandleAsync(ExecuteAllPendingQueuedEventsCommand request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Processing ExecuteAllPendingQueuedEventsCommand (Type: {Type})", request.EventType);
            return await _service.ExecuteAllPendingEventsAsync(request.EventType, request.ExecutedBy).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("ExecuteAllPendingQueuedEventsCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error while executing all pending queued events", ApplicationEventIds.Error, ex);
            return Result<int>.Failure<int>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}

public class CancelQueuedEventCommandHandler : BaseCommandBundle, IBaseRequestHandler<CancelQueuedEventCommand, Result>
{
    private readonly IEventQueueService _service;
    private readonly ILogger<CancelQueuedEventCommandHandler> _logger;

    public CancelQueuedEventCommandHandler(
        IEventQueueService service,
        ILogger<CancelQueuedEventCommandHandler> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result> HandleAsync(CancelQueuedEventCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request.EventId == Guid.Empty)
            {
                return Result.Failure(DomainErrors.GeneralError.InvalidParameters);
            }

            _logger.LogApplicationInformation("Processing CancelQueuedEventCommand for EventId: {EventId}", request.EventId);
            return await _service.CancelQueuedEventAsync(request.EventId, request.CancelledBy).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("CancelQueuedEventCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error while cancelling queued event", ApplicationEventIds.Error, ex);
            return Result.Failure(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}

public class ClearCompletedQueuedEventsCommandHandler : BaseCommandBundle, IBaseRequestHandler<ClearCompletedQueuedEventsCommand, Result<int>>
{
    private readonly IEventQueueService _service;
    private readonly ILogger<ClearCompletedQueuedEventsCommandHandler> _logger;

    public ClearCompletedQueuedEventsCommandHandler(
        IEventQueueService service,
        ILogger<ClearCompletedQueuedEventsCommandHandler> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<int>> HandleAsync(ClearCompletedQueuedEventsCommand request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Processing ClearCompletedQueuedEventsCommand");
            return await _service.ClearCompletedEventsAsync().ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("ClearCompletedQueuedEventsCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error while clearing completed queued events", ApplicationEventIds.Error, ex);
            return Result<int>.Failure<int>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}

public class RebuildQueuedEmailEventCommandHandler : BaseCommandBundle, IBaseRequestHandler<RebuildQueuedEmailEventCommand, Result<Guid>>
{
    private readonly IEventQueueService _service;
    private readonly ILogger<RebuildQueuedEmailEventCommandHandler> _logger;

    public RebuildQueuedEmailEventCommandHandler(
        IEventQueueService service,
        ILogger<RebuildQueuedEmailEventCommandHandler> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<Guid>> HandleAsync(RebuildQueuedEmailEventCommand request, CancellationToken ct = default)
    {
        try
        {
            if (request.EventId == Guid.Empty)
            {
                return Result<Guid>.Failure<Guid>(DomainErrors.GeneralError.InvalidParameters);
            }

            _logger.LogApplicationInformation("Processing RebuildQueuedEmailEventCommand for EventId: {EventId}", request.EventId);
            return await _service.RebuildQueuedEmailEventAsync(request.EventId, request.RebuiltBy).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("RebuildQueuedEmailEventCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error while rebuilding queued email event", ApplicationEventIds.Error, ex);
            return Result<Guid>.Failure<Guid>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}

public class ClearAllQueuedEventsCommandHandler : BaseCommandBundle, IBaseRequestHandler<ClearAllQueuedEventsCommand, Result<int>>
{
    private readonly IEventQueueService _service;
    private readonly ILogger<ClearAllQueuedEventsCommandHandler> _logger;

    public ClearAllQueuedEventsCommandHandler(
        IEventQueueService service,
        ILogger<ClearAllQueuedEventsCommandHandler> logger)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<int>> HandleAsync(ClearAllQueuedEventsCommand request, CancellationToken ct = default)
    {
        try
        {
            _logger.LogApplicationInformation("Processing ClearAllQueuedEventsCommand");
            return await _service.ClearAllEventsAsync().ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            _logger.LogApplicationWarning("ClearAllQueuedEventsCommand operation was cancelled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogApplicationError("Unexpected error while clearing all queued events", ApplicationEventIds.Error, ex);
            return Result<int>.Failure<int>(DomainErrors.GeneralError.UnProcessableRequest);
        }
    }
}

